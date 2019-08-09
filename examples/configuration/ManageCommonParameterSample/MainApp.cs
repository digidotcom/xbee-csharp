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
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
using XBeeLibrary.Windows;

namespace Examples.Configuration.ManageCommonParameterSample
{
	/// <summary>
	/// XBee C# Library Manage Common parameters sample application.
	/// </summary>
	/// <remarks>
	/// This example shows how to manage common parameters of an XBee device. As 
	/// common parameters are split in cached and non-cached values, the application 
	/// refresh the cached values before reading them, then it sets and reads the 
	/// non-cached parameters. All the reads and configurations are performed using 
	/// the specific getters and setters provided by the XBee device object.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.
	/// </remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your module is connected to.
		private static readonly string PORT = "COM1";
		// TODO Replace with the baud rate of your module.
		private static readonly int BAUD_RATE = 9600;

		private static readonly byte[] PARAM_VALUE_PAN_ID = new byte[] { 0x01, 0x23 };
		private static readonly XBee64BitAddress PARAM_DESTINATION_ADDR = XBee64BitAddress.BROADCAST_ADDRESS;
		private static readonly PowerLevel PARAM_POWER_LEVEL = PowerLevel.LEVEL_HIGH;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-------------------------------------------------+");
			Console.WriteLine(" | XBee C# Library Manage Common parameters Sample |");
			Console.WriteLine(" +-------------------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				// Read cached parameters.
				myDevice.ReadDeviceInfo();

				Console.WriteLine(">> Cached parameters");
				Console.WriteLine("----------------------");
				Console.WriteLine(" - 64-bit address:   " + myDevice.XBee64BitAddr);
				Console.WriteLine(" - 16-bit address:   " + myDevice.XBee16BitAddr);
				Console.WriteLine(" - Node Identifier:  " + myDevice.NodeID);
				Console.WriteLine(" - Firmware version: " + myDevice.FirmwareVersion);
				Console.WriteLine(" - Hardware version: " + myDevice.HardwareVersionString);
				Console.WriteLine("");

				// Configure and read non-cached parameters.
				myDevice.SetPANID(PARAM_VALUE_PAN_ID);
				myDevice.SetDestinationAddress(PARAM_DESTINATION_ADDR);
				myDevice.SetPowerLevel(PARAM_POWER_LEVEL);

				byte[] panID = myDevice.GetPANID();
				XBee64BitAddress destinationAddress = myDevice.GetDestinationAddress();
				PowerLevel powerLevel = myDevice.GetPowerLevel();

				Console.WriteLine(">> Non-Cached parameters");
				Console.WriteLine("----------------------");
				Console.WriteLine(" - PAN ID:           " + HexUtils.ByteArrayToHexString(panID));
				Console.WriteLine(" - Destination addr: " + destinationAddress.ToString());
				Console.WriteLine(" - Power Level:      " + powerLevel.ToString());
				Console.WriteLine("");
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
	}
}
