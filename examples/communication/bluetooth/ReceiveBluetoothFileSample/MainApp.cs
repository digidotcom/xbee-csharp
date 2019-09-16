/*
 * Copyright 2019, Digi International Inc.
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

using System;
using System.IO;
using System.Linq;
using System.Text;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Utils;
using XBeeDevice = XBeeLibrary.Windows.XBeeDevice;

namespace Examples.Communication.Bluetooth.ReceiveBluetoothFileSample
{
	/// <summary>
	/// XBee C# Library Receive Bluetooth File sample application.
	/// </summary>
	/// <remarks>
	/// This example registers a callback to be executed when new data from the
	/// Bluetooth Low Energy interface of the XBee device is received and stores
	/// it in a file.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.
	/// </remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your receiver module is connected.
		private static readonly string PORT = "COM1";
		// TODO Replace with the baud rate of you receiver module.
		private static readonly int BAUD_RATE = 9600;

		private static readonly string SEPARATOR = "@@@";

		private static readonly string MSG_START = "START" + SEPARATOR;
		private static readonly string MSG_END = "END";
		private static readonly string MSG_ACK = "OK";

		private static XBeeDevice myDevice;

		private static FileInfo receivedFile;

		private static FileStream stream;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-------------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Receive Bluetooth File Sample  |");
			Console.WriteLine(" +-------------------------------------------------+\n");

			myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();
				myDevice.BluetoothDataReceived += MyDevice_BluetoothDataReceived;
				Console.WriteLine(">> Waiting for file from the Bluetooth interface...");
			}
			catch (XBeeException e)
			{
				Console.WriteLine("ERROR: " + e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				Console.WriteLine(">> (Press any key to exit)");
				Console.ReadKey(true);
				myDevice.Close();
			}
		}

		/// <summary>
		/// Method called to manage received data from the Bluetooth interface
		/// of the XBee device.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyDevice_BluetoothDataReceived(object sender, BluetoothDataReceivedEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data);

			// Check if the data is 'START' or 'END'.
			if (data.StartsWith(MSG_START))
			{
				// Get the file name.
				string fileName = data.Split(new string[] { SEPARATOR }, StringSplitOptions.None)[1];

				// Create the file.
				receivedFile = new FileInfo(fileName);

				// If the file exists, remove it.
				if (receivedFile.Exists)
					receivedFile.Delete();

				if (stream != null)
					stream.Dispose();
				stream = receivedFile.Create();
				
				Console.WriteLine(">> START message received, saving data to file...");
				SendAck();
			}
			else if (data.Equals(MSG_END))
			{
				// Close the stream.
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
					stream = null;
				}
				Console.WriteLine(">> END message received, file '{0}'\n\n", receivedFile.FullName);
				SendAck();
			}
			else if (stream != null)
			{
				byte[] payload = e.Data.Take(e.Data.Length - 1).ToArray();
				int checksum = ByteUtils.ByteToInt(e.Data[e.Data.Length - 1]);

				// Validate the checksum.
				XBeeChecksum chk = new XBeeChecksum();
				chk.Add(payload);
				if (chk.Generate() == checksum)
				{
					// Write block to file.
					try
					{
						stream.Write(payload, 0, payload.Length);
					}
					catch (Exception ex)
					{
						Console.WriteLine("ERROR: " + ex.Message);
						Console.WriteLine(ex.StackTrace);
					}
					SendAck();
				}
			}
		}

		/// <summary>
		/// Sends the ACK message to the given XBee device.
		/// </summary>
		private static void SendAck()
		{
			try
			{
				myDevice.SendBluetoothData(Encoding.UTF8.GetBytes(MSG_ACK));
			}
			catch (XBeeException e)
			{
				Console.WriteLine("ERROR: " + e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
	}
}
