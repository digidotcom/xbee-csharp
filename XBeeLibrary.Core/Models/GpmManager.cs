/*
 * Copyright 2023,2024, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	public class GpmManager
	{
		// Constants.
		private static readonly byte COMMAND_GPM_INFO_REQUEST = (byte)0x00;
		private static readonly byte COMMAND_GPM_INFO_RESPONSE = (byte)0x80;
		private static readonly byte COMMAND_GPM_ERASE_REQUEST = (byte)0x01;
		private static readonly byte COMMAND_GPM_ERASE_RESPONSE = (byte)0x81;
		private static readonly byte COMMAND_GPM_WRITE_REQUEST = (byte)0x02;
		private static readonly byte COMMAND_GPM_WRITE_RESPONSE = (byte)0x82;
		private static readonly byte COMMAND_GPM_VERIFY_REQUEST = (byte)0x05;
		private static readonly byte COMMAND_GPM_VERIFY_RESPONSE = (byte)0x85;
		private static readonly byte COMMAND_GPM_VERIFY_INSTALL_REQUEST = (byte)0x06;
		private static readonly byte COMMAND_GPM_VERIFY_INSTALL_RESPONSE = (byte)0x86;
		private static readonly int ENDPOINT_DIGI_DEVICE = 0xE6;
		private static readonly int BROADCAST_RADIUS_MAXIMUM = 0x00;
		private static readonly int TRANSMIT_OPTIONS_NONE = 0x00;

		private static readonly byte[] ZERO_SHORT_VALUE = new byte[] { 0x00, 0x00 };

		private static readonly byte[] CLUSTER_GPM = new byte[] { 0x00, 0x23 };
		private static readonly byte[] PROFILE_DIGI = new byte[] { (byte)0xC1, 0x05 };

		private static readonly string ERROR_TX_STATUS = "Transmit status was not received.";
		public static readonly string ERROR_RX_TIMEOUT = "Timeout waiting for GPM response.";
		private static readonly string ERROR_SEND_PACKET_EXTENDED = "Could not send GPM packet > ";
		private static readonly string ERROR_INVALID_ANSWER = "Invalid GPM packet answer";
		private static readonly string ERROR_ERASE_ERROR_PROCESS = "An error occurred in the erase process.";
		private static readonly string ERROR_WRITE_DATA_PROCESS = "An error occurred writing data to device.";
		private static readonly string ERROR_VERIFY_INSTALL_ERROR_PROCESS = "Image transferred to device is not valid.";
		private static readonly string ERROR_PLATFORM_INFO_ERROR_PROCESS = "An error occurred getting the platform information.";
		private static readonly string ERROR_NO_MODEM_RESET = "No modem reset frame detected after {0} seconds.";
		private static readonly string ERROR_PREFIX_TIMEOUT = "timeout";

		public static readonly string TASK_GET_PLATFORM_INFO = "Getting platform info";
		public static readonly string TASK_ERASE = "Erasing flash";
		public static readonly string TASK_TRANSFER = "Transferring firmware";
		public static readonly string TASK_VERIFY = "Verifying flash image";
		public static readonly string TASK_VERIFY_AND_INSTALL = "Verifying and installing flash image";
		public static readonly string TASK_WAIT_FOR_UPDATE = "Waiting for device to be upgraded";

		private static readonly int GPM_WRITE_RETRIES = 3;
		private static readonly int MAX_UPGRADE_TIME = 90000;
		internal static readonly int DEFAULT_TIMEOUT = 30000;
		private static readonly int API_EXPLICIT_OVERHEAD = 24;
		private static readonly int GPM_PROTOCOL_OVERHEAD = 8;
		private static readonly int MTU_OVERHEAD = 3;

		// Variables.
		private readonly AbstractXBeeDevice device;

		private readonly Stream firmwareBinaryStream;

		private readonly object gpmLock = new object();
		private readonly object modemStatusLock = new object();

		private int gpmBlocks;
		private int gpmBytesPerBlock;
		private int attMTU;

		private bool gpmPacketSent = false;
		private bool gpmPacketReceived = false;
		private bool modemStatusReceived = false;

		private byte[] gpmAnswerPayload;

		private readonly int timeout = DEFAULT_TIMEOUT;

		private readonly ILog logger;

		public GpmManager(AbstractXBeeDevice device, Stream firmwareBinaryStream)
			: this(device, firmwareBinaryStream, DEFAULT_TIMEOUT) { }

		public GpmManager(AbstractXBeeDevice device, Stream firmwareBinaryStream, int timeout)
		: this(device, firmwareBinaryStream, DEFAULT_TIMEOUT, -1) { }

		public GpmManager(AbstractXBeeDevice device, Stream firmwareBinaryStream, int timeout, int attMTU)
		{
			this.device = device;
			this.firmwareBinaryStream = firmwareBinaryStream;
			this.timeout = timeout;
			this.attMTU = attMTU;

			logger = LogManager.GetLogger<GpmManager>();
		}

		/// <summary>
		/// Handler for GPM update events.
		/// </summary>
		public event EventHandler<GpmUpdateEventArgs> GpmUpdateEventHandler;

		/// <summary>
		/// Updates the firmware of the configured device.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		public void UpdateFirmware()
		{
			logger.Info("Updating device firmware");

			// Step 1: Calculate the page size.
			int maxCmdLen = ByteUtils.ByteArrayToInt(device.GetParameter("NP"));
			int pageSize = maxCmdLen - GPM_PROTOCOL_OVERHEAD;
			// Optimize the page size based on the provided ATT_MTU. In order to do so, 
			// the ATT_MTU must be greater than all the API and GPM overheads.
			if (attMTU != -1 && (attMTU - MTU_OVERHEAD) > (API_EXPLICIT_OVERHEAD + GPM_PROTOCOL_OVERHEAD))
			{
				int totalPacketSize = API_EXPLICIT_OVERHEAD + GPM_PROTOCOL_OVERHEAD + pageSize;
				if (totalPacketSize > (attMTU - MTU_OVERHEAD))
				{
					// Check how many BLE packets are necessary to send 'pageSize' bytes.
					int numBLEPackets = (totalPacketSize / (attMTU - MTU_OVERHEAD)) + ((totalPacketSize % (attMTU - MTU_OVERHEAD)) != 0 ? 1 : 0);

					// Now check if the number of effective bytes sent in the same number of BLE packets adjusting the page size to the MTU is greater.
					int newPageSize = attMTU - MTU_OVERHEAD - API_EXPLICIT_OVERHEAD - GPM_PROTOCOL_OVERHEAD;
					int totalEffectiveBytes = newPageSize * numBLEPackets;

					if (totalEffectiveBytes > pageSize)
						pageSize = newPageSize;
				}
			}

			// Step 2: Get the platform information (GPM info) from the device.
			GetPlatformInfo();

			// Step 3: Erase the GPM.
			EraseFlash();

			// Step 4: Send the firmware binary to the device.
			TransferFirmware(pageSize);

			// Step 5: Verify and install the transferred firmware image.
			try
			{
				VerifyAndInstallFlashImage();
			}
			catch (GpmException e)
			{
				// The image could be correct on the target device. So, verify it
				// and retry the install.
				if (!e.Message.Contains(ERROR_RX_TIMEOUT))
				{
					VerifyFlashImage();
					VerifyAndInstallFlashImage();
				}
				else
				{
					// A timeout exception means that the device rebooted to install the firmware,
					// which is expected.
					// TODO: This should just return, or return a boolean (true). The code calling
					//       this method should not be checking the type of exception thrown to
					//       determine if the update was success or not.
					throw e;
				}
			}

			// Step 6: Wait for the device to reboot after the firmware update. In Bluetooth this
			//         step is never reached as the BLE connection is lost when the device is reset
			//         to install the firmware.
			WaitForDeviceToUpgrade();

			logger.Info("Device firmware updated successfully");
		}

		/// <summary>
		/// Retrieves the GMP information of the XBee device. Used to get the number of blocks and bytes per block of
		/// the GPM.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		private void GetPlatformInfo()
		{
			NotifyEvent(TASK_GET_PLATFORM_INFO, null);

			// Initialize and fill the erase platform info packet.
			byte[] platformInfoPayload = new byte[8];
			platformInfoPayload[0] = COMMAND_GPM_INFO_REQUEST; // Platform info command ID.
			platformInfoPayload[1] = 0; // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, platformInfoPayload, 2, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, platformInfoPayload, 4, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, platformInfoPayload, 6, 2); // Unused field.

			// Create the GPM platform info packet.
			ExplicitAddressingPacket platformInfoPacket = new ExplicitAddressingPacket(1, device.XBee64BitAddr,
				device.XBee16BitAddr, (byte)ENDPOINT_DIGI_DEVICE, (byte)ENDPOINT_DIGI_DEVICE, CLUSTER_GPM, PROFILE_DIGI,
				(byte)BROADCAST_RADIUS_MAXIMUM, (byte)TRANSMIT_OPTIONS_NONE, platformInfoPayload);
			// Send the packet.
			string errorString = SendGPMAPIPacket(platformInfoPacket, timeout);
			if (errorString != null)
				errorString = ERROR_SEND_PACKET_EXTENDED + errorString;
			else if (gpmAnswerPayload.Length < 8)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if (gpmAnswerPayload[0] != COMMAND_GPM_INFO_RESPONSE)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if ((gpmAnswerPayload[1] & 0x01) == 1)
				errorString = ERROR_PLATFORM_INFO_ERROR_PROCESS;
			if (errorString != null)
			{
				logger.Error("Error getting platform info: " + errorString);
				throw new GpmException(errorString);
			}
			
			// Get the number of blocks from the response.
			byte[] gpmBlocksArray = new byte[2];
			Array.Copy(gpmAnswerPayload, 2, gpmBlocksArray, 0, 2);
			gpmBlocks = ByteUtils.ByteArrayToInt(gpmBlocksArray);

			// Get the number of bytes per block from the response.
			byte[] gpmBytesPerBlockArray = new byte[2];
			Array.Copy(gpmAnswerPayload, 4, gpmBytesPerBlockArray, 0, 2);
			gpmBytesPerBlock = ByteUtils.ByteArrayToInt(gpmBytesPerBlockArray);
		}

		/// <summary>
		/// Sends the GPM packet to erase the GPM blocks of the XBee device.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		private void EraseFlash()
		{
			NotifyEvent(TASK_ERASE, null);

			// Initialize and fill the erase flash packet.
			byte[] erasePayload = new byte[8];
			erasePayload[0] = COMMAND_GPM_ERASE_REQUEST; // Erase command ID.
			erasePayload[1] = 0; // No options (0x00).
			Array.Copy(ZERO_SHORT_VALUE, 0, erasePayload, 2, 2); // Block index. If all blocks are erased, this field is ignored.
			Array.Copy(ZERO_SHORT_VALUE, 0, erasePayload, 4, 2); // Unused, set to 0x0000.
			Array.Copy(ZERO_SHORT_VALUE, 0, erasePayload, 6, 2); // Erase all GPM flash blocks, set to 0x0000.

			// Create the GPM erase packet.
			ExplicitAddressingPacket erasePacket = new ExplicitAddressingPacket(1, device.XBee64BitAddr,
				device.XBee16BitAddr, (byte)ENDPOINT_DIGI_DEVICE, (byte)ENDPOINT_DIGI_DEVICE, CLUSTER_GPM, PROFILE_DIGI,
				(byte)BROADCAST_RADIUS_MAXIMUM, (byte)TRANSMIT_OPTIONS_NONE, erasePayload);
			// Send the packet.
			string errorString = SendGPMAPIPacket(erasePacket, timeout);
			if (errorString != null)
				errorString = ERROR_SEND_PACKET_EXTENDED + errorString;
			else if (gpmAnswerPayload.Length < 8)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if (gpmAnswerPayload[0] != COMMAND_GPM_ERASE_RESPONSE)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if ((gpmAnswerPayload[1] & 0x01) == 1)
				errorString = ERROR_ERASE_ERROR_PROCESS;
			if (errorString != null)
			{
				logger.Error("Error erasing flash: " + errorString);
				throw new GpmException(errorString);
			}
		}

		/// <summary>
		/// Transfers the firmware image to the device.
		/// </summary>
		/// <param name="pageSize">Page size.</param>
		/// <exception cref="GpmException"></exception>
		private void TransferFirmware(int pageSize)
		{
			NotifyEvent(TASK_TRANSFER, 0);

			FirmwareFile firmwareFile = new FirmwareFile(firmwareBinaryStream, pageSize);
			List<MemPage> memPages = firmwareFile.GetMemPages();
			int currentBlock = 0;
			int currentIndex = 0;
			int percentage = 0;
			byte[] pageBytesRest = new byte[0];
			for (int i = 0; i < memPages.Count; i++)
			{
				percentage = (i + 1) * 100 / memPages.Count;
				byte[] pageData = memPages[i].PageBytes;
				// Send a complete page.
				int pageBytesSent = 0;
				while (pageBytesSent < pageData.Length)
				{
					byte[] dataToSend;
					if (currentIndex + pageBytesRest.Length + pageData.Length - pageBytesSent > gpmBytesPerBlock)
					{
						// Here we have to send only a part of the memPage, and the rest in the following block.
						int l = Math.Min(gpmBytesPerBlock - currentIndex, pageSize);
						dataToSend = new byte[l];
					}
					else
					{
						// If it is not the last page, store the bytes to be send with part of the next page.
						if (pageBytesRest.Length + pageData.Length - pageBytesSent < pageSize
								&& i < memPages.Count - 1)
						{
							byte[] aux = new byte[pageBytesRest.Length + pageData.Length - pageBytesSent];
							Array.Copy(pageBytesRest, 0, aux, 0, pageBytesRest.Length);
							Array.Copy(pageData, pageBytesSent, aux, pageBytesRest.Length, pageData.Length - pageBytesSent);
							pageBytesRest = aux;
							break;
						}
						int l = Math.Min(pageBytesRest.Length + pageData.Length - pageBytesSent, pageSize);
						dataToSend = new byte[l];
					}
					// Copy bytes of the chunck to be sent.
					Array.Copy(pageBytesRest, 0, dataToSend, 0, pageBytesRest.Length);
					Array.Copy(pageData, pageBytesSent, dataToSend, pageBytesRest.Length, dataToSend.Length - pageBytesRest.Length);

					// Send the firmware chunk.
					WriteData(currentBlock, currentIndex, dataToSend, GPM_WRITE_RETRIES);

					// Update the number of bytes sent.
					pageBytesSent = pageBytesSent + dataToSend.Length - pageBytesRest.Length;
					pageBytesRest = new byte[0];

					// Calculate current index and block number.
					currentIndex += dataToSend.Length;
					if (currentIndex >= gpmBytesPerBlock)
					{
						currentIndex = 0;
						currentBlock += 1;
					}

					// If there are no more blocks exit.
					if (currentBlock >= gpmBlocks)
						break;
				}
				NotifyEvent(null, percentage);
			}
		}

		/// <summary>
		/// Writes the given data bytes in the specified block index and byte index of the GPM flash.
		/// </summary>
		/// <param name="blockIndex">The GPM block index where the data should be written.</param>
		/// <param name="byteIndex">The byte index within the specified block where the data should be written.</param>
		/// <param name="data">The array of data bytes to be written.</param>
		/// <param name="retries">The number of retries to perform if the write process failed.</param>
		/// <exception cref="GpmException"></exception>
		private void WriteData(int blockIndex, int byteIndex, byte[] data, int retries)
		{
			// Initialize packet values.
			byte[] blockIndexArray = ByteUtils.ShortToByteArray((short)blockIndex);
			byte[] byteIndexArray = ByteUtils.ShortToByteArray((short)byteIndex);
			byte[] dataLengthArray = ByteUtils.ShortToByteArray((short)data.Length);

			// Declare and fill the write packet.
			byte[] writePayload = new byte[data.Length + 8];
			writePayload[0] = COMMAND_GPM_WRITE_REQUEST; // Write command ID.
			writePayload[1] = 0; // No options defined yet for this command.
			Array.Copy(blockIndexArray, 0, writePayload, 2, 2); // Block index of the GPM to be written.
			Array.Copy(byteIndexArray, 0, writePayload, 4, 2); // Byte index within the block to be written.
			Array.Copy(dataLengthArray, 0, writePayload, 6, 2); // Number of data bytes.

			// Fill the write packet with the data to be written.
			for (int i = 0; i < data.Length; i++)
				writePayload[i + 8] = data[i];

			// Create the GPM write packet.
			ExplicitAddressingPacket writePacket = new ExplicitAddressingPacket(1, device.XBee64BitAddr,
				device.XBee16BitAddr, (byte)ENDPOINT_DIGI_DEVICE, (byte)ENDPOINT_DIGI_DEVICE, CLUSTER_GPM, PROFILE_DIGI,
				(byte)BROADCAST_RADIUS_MAXIMUM, (byte)TRANSMIT_OPTIONS_NONE, writePayload);
			// Send the packet.
			string errorString = null;
			while (retries > 0)
			{
				// Send the packet.
				errorString = SendGPMAPIPacket(writePacket, timeout);
				if (errorString != null)
					errorString = ERROR_SEND_PACKET_EXTENDED + errorString;
				else if (gpmAnswerPayload.Length < 8)
					errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
				else if (gpmAnswerPayload[0] != COMMAND_GPM_WRITE_RESPONSE)
					errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
				else if ((gpmAnswerPayload[1] & 0x01) == 1)
					errorString = ERROR_WRITE_DATA_PROCESS;
				if (errorString == null)
					break;
				else
					retries -= 1;
			}

			if (errorString != null)
			{
				logger.Error("Error writing data: " + errorString);
				throw new GpmException(errorString);
			}
		}

		/// <summary>
		/// Sends the GPM packet to verify the firmware image in the XBee device.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		private void VerifyFlashImage()
		{
			NotifyEvent(TASK_VERIFY, null);

			// Declare and fill the verify and install packet.
			byte[] verifyAndInstallPayload = new byte[8];
			verifyAndInstallPayload[0] = COMMAND_GPM_VERIFY_REQUEST; // Verify and install command ID.
			verifyAndInstallPayload[1] = 0; // No options defined yet for this command.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 2, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 4, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 6, 2); // Unused field.

			// Create the GPM verify and install packet.
			ExplicitAddressingPacket verifyInstallPacket = new ExplicitAddressingPacket(1, device.XBee64BitAddr,
				device.XBee16BitAddr, (byte)ENDPOINT_DIGI_DEVICE, (byte)ENDPOINT_DIGI_DEVICE, CLUSTER_GPM, PROFILE_DIGI,
				(byte)BROADCAST_RADIUS_MAXIMUM, (byte)TRANSMIT_OPTIONS_NONE, verifyAndInstallPayload);
			// Send the packet.
			string errorString = SendGPMAPIPacket(verifyInstallPacket, timeout);
			if (errorString != null)
				errorString = ERROR_SEND_PACKET_EXTENDED + errorString;
			else if (gpmAnswerPayload.Length < 8)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if (gpmAnswerPayload[0] != COMMAND_GPM_VERIFY_RESPONSE)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if ((gpmAnswerPayload[1] & 0x01) == 1)
				errorString = ERROR_VERIFY_INSTALL_ERROR_PROCESS;
			if (errorString != null)
			{
				logger.Error("Error verifying flash image: " + errorString);
				throw new GpmException(errorString);
			}
		}

		/// <summary>
		/// Sends the GPM packet to verify and install the firmware image in the XBee device.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		private void VerifyAndInstallFlashImage()
		{
			NotifyEvent(TASK_VERIFY_AND_INSTALL, null);

			// Declare and fill the verify and install packet.
			byte[] verifyAndInstallPayload = new byte[8];
			verifyAndInstallPayload[0] = COMMAND_GPM_VERIFY_INSTALL_REQUEST; // Verify and install command ID.
			verifyAndInstallPayload[1] = 0; // No options defined yet for this command.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 2, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 4, 2); // Unused field.
			Array.Copy(ZERO_SHORT_VALUE, 0, verifyAndInstallPayload, 6, 2); // Unused field.

			// Create the GPM verify and install packet.
			ExplicitAddressingPacket verifyInstallPacket = new ExplicitAddressingPacket(1, device.XBee64BitAddr,
				device.XBee16BitAddr, (byte)ENDPOINT_DIGI_DEVICE, (byte)ENDPOINT_DIGI_DEVICE, CLUSTER_GPM, PROFILE_DIGI,
				(byte)BROADCAST_RADIUS_MAXIMUM, (byte)TRANSMIT_OPTIONS_NONE, verifyAndInstallPayload);
			// Send the packet.
			string errorString = SendGPMAPIPacket(verifyInstallPacket, timeout);
			// When the connection with the device is via Bluetooth, the device could reset before
			// receiving the ACK for the write characteristic. This causes a timeout when writing to
			// the TX characteristic. The firmware is being applied, but we need to wait some time
			// for it to start and make the firmware update process think there was a timeout waiting
			// for the GPM response, which would be the expected behavior.
			if (errorString != null
				&& errorString.ToLower().StartsWith(ERROR_PREFIX_TIMEOUT)
				&& !errorString.Equals(ERROR_RX_TIMEOUT))
			{
				Task.Delay(timeout).Wait();
				errorString = ERROR_RX_TIMEOUT;
			}
			if (errorString != null)
				errorString = ERROR_SEND_PACKET_EXTENDED + errorString;
			else if (gpmAnswerPayload.Length < 8)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if (gpmAnswerPayload[0] != COMMAND_GPM_VERIFY_INSTALL_RESPONSE)
				errorString = ERROR_SEND_PACKET_EXTENDED + ERROR_INVALID_ANSWER;
			else if ((gpmAnswerPayload[1] & 0x01) == 1)
				errorString = ERROR_VERIFY_INSTALL_ERROR_PROCESS;
			if (errorString != null)
			{
				logger.Error("Error verifying and installing flash image: " + errorString);
				throw new GpmException(errorString);
			}
		}

		/// <summary>
		/// Waits for the device to be upgraded.
		/// </summary>
		/// <exception cref="GpmException"></exception>
		private void WaitForDeviceToUpgrade()
		{
			NotifyEvent(TASK_WAIT_FOR_UPDATE, null);

			modemStatusReceived = false;
			device.PacketReceived += ReceiveModemStatusPacket;
			lock (modemStatusLock)
			{
				Monitor.Wait(modemStatusLock, MAX_UPGRADE_TIME);
			}
			device.PacketReceived -= ReceiveModemStatusPacket;
			if (!modemStatusReceived)
			{
				logger.Error("Did not receive the reset modem status");
				throw new GpmException(string.Format(ERROR_NO_MODEM_RESET, MAX_UPGRADE_TIME / 1000));
			}
		}

		/// <summary>
		/// Sends the given GPM API packet (actually it is an XBee packet) and waits for the answer or until timeout is
		/// reached.
		/// </summary>
		/// <param name="packet">The packet to send.</param>
		/// <param name="timeout">The time to wait for answer.</param>
		/// <returns>The error message (if any), <c>null</c> if everything was correct.</returns>
		private string SendGPMAPIPacket(XBeePacket packet, int timeout)
		{
			gpmPacketSent = false;
			gpmPacketReceived = false;
			gpmAnswerPayload = null;
			string errorString = null;

			// Add the receive listener to the local XBee device.
			device.PacketReceived += ReceiveGpmPacket;
			try
			{
				// Send the packet.
				device.SendPacketAsync(packet);
				// Wait for response or timeout.
				// Check first if the answer to the packet was not already received. Interfaces
				// such as Bluetooth can take some time to execute the write operation and, by
				// the time that process finishes, the answer could have been already received.
				if (!gpmPacketReceived)
				{
					lock (gpmLock)
					{
						Monitor.Wait(gpmLock, Math.Max(timeout, AbstractXBeeDevice.DEFAULT_RECEIVE_TIMETOUT));
					}
				}

				if (!gpmPacketSent)
					errorString = ERROR_TX_STATUS;
				else if (!gpmPacketReceived)
					errorString = ERROR_RX_TIMEOUT;
			}
			catch (XBeeException e)
			{
				errorString = e.Message;
			}
			finally
			{
				device.PacketReceived -= ReceiveGpmPacket;
			}
			return errorString;
		}

		/// <summary>
		/// Method called when a GPM packet is received.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void ReceiveGpmPacket(object sender, PacketReceivedEventArgs e)
		{
			XBeePacket receivedPacket = e.ReceivedPacket;
			if (receivedPacket is TransmitStatusPacket transmitStatusPacket)
			{
				// This is our packet, check status to verify the packet has been sent.
				if (transmitStatusPacket.TransmitStatus == XBeeTransmitStatus.SUCCESS
					|| transmitStatusPacket.TransmitStatus == XBeeTransmitStatus.SELF_ADDRESSED)
				{
					gpmPacketSent = true;

					// Sometimes the transmit status is received after the GPM frame.
					// So notify if the GPM frame was also received.
					if (gpmPacketReceived)
					{
						// Remove listener to avoid reception of more frames.
						device.PacketReceived -= ReceiveGpmPacket;
						// Continue execution by notifying the lock object.
						lock (gpmLock)
						{
							Monitor.Pulse(gpmLock);
						}
					}
				}
				else
				{
					// Remove listener to avoid reception of more frames.
					device.PacketReceived -= ReceiveGpmPacket;

					// Sometimes the transmit status is received after the GPM frame.
					// In case the GPM frame was received, remove the data.
					if (gpmPacketReceived)
					{
						gpmPacketReceived = false;
						gpmAnswerPayload = null;
					}
					// Notify about transmit packet error.
					lock (gpmLock)
					{
						Monitor.Pulse(gpmLock);
					}
				}
			}
			else if (receivedPacket is ExplicitRxIndicatorPacket packet)
			{
				// If frame was not successfully sent or we already have received the Rx indicator response, ignore this packet.
				if (gpmPacketReceived)
					return;
				// Check the 64-bit source address, the profile, cluster and source/destination end points.
				if (!packet.SourceAddress64.Equals(device.XBee64BitAddr)
						|| !packet.ProfileID.SequenceEqual(PROFILE_DIGI)
						|| !packet.ClusterID.SequenceEqual(CLUSTER_GPM)
						|| packet.SourceEndpoint != ENDPOINT_DIGI_DEVICE
						|| packet.DestEndpoint != ENDPOINT_DIGI_DEVICE)
				{
					return;
				}

				gpmAnswerPayload = packet.RFData;
				gpmPacketReceived = true;

				// Sometimes the transmit status is received after the GPM frame.
				// So only notify if the transmit status was also received.
				if (gpmPacketSent)
				{
					// Remove listener to avoid reception of more frames.
					device.PacketReceived -= ReceiveGpmPacket;
					// Continue execution by notifying the lock object.
					lock (gpmLock)
					{
						Monitor.Pulse(gpmLock);
					}
				}
			}
		}

		/// <summary>
		/// Method called when a packet is received.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void ReceiveModemStatusPacket(object sender, PacketReceivedEventArgs e)
		{
			try
			{
				ModemStatusPacket modemStatusPacket = ModemStatusPacket.CreatePacket(e.ReceivedPacket.GenerateByteArray());
				if (modemStatusPacket.Status == ModemStatusEvent.STATUS_HARDWARE_RESET)
				{
					logger.Info("Reset modem status received");
					modemStatusReceived = true;
					lock (modemStatusLock)
					{
						Monitor.Pulse(modemStatusLock);
					}
				}
			}
			catch (Exception)
			{
				// Do nothing, an error will be thrown by the process if the modem status
				// packet is not received in a specific time window.
			}
		}

		/// <summary>
		/// Notifies an event with the following message and progress to the event handlers.
		/// </summary>
		/// <param name="message">Event message.</param>
		/// <param name="progress">Event progress (optional).</param>
		private void NotifyEvent(string message, int? progress)
		{
			if (message != null)
				logger.Info(message);
			if (progress != null)
				logger.Info(string.Format("{0}%", progress));
			lock (GpmUpdateEventHandler)
			{
				var handler = GpmUpdateEventHandler;
				if (handler != null)
				{
					var args = new GpmUpdateEventArgs(message, progress);
					handler.GetInvocationList().AsParallel().ForAll((action) => action.DynamicInvoke(this, args));
				}
			}
		}
	}
}
