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
using System.Text;
using XBeeLibrary.Core;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Utils;
using ZigBeeDevice = XBeeLibrary.Windows.ZigBeeDevice;

namespace Examples.Communication.Explicit.SendExplicitDataSample
{
	/// <summary>
	/// XBee C# Library Send Explicit Data sample application.
	/// </summary>
	/// <remarks>
	/// This example sends data in application layer (explicit) format to a 
	/// remote device whose Node Identifier is 'REMOTE'.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.
	/// </remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your sender module is connected to.
		private static readonly String PORT = "COM1";
		// TODO Replace with the baud rate of your sender module.
		private static readonly int BAUD_RATE = 9600;

		private static readonly string DATA_TO_SEND = "Hello XBee!";
		private static readonly string REMOTE_NODE_IDENTIFIER = "REMOTE";
	
		// Examples of endpoints, cluster ID and profile ID.
		private static readonly int SOURCE_ENDPOINT = 0xA0;
		private static readonly int DESTINATION_ENDPOINT = 0xA1;
		private static readonly int CLUSTER_ID = 0x1554;
		private static readonly int PROFILE_ID = 0x1234;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +---------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Send Explicit Data Sample  |");
			Console.WriteLine(" +---------------------------------------------+\n");

			ZigBeeDevice myDevice = new ZigBeeDevice(PORT, BAUD_RATE);
			byte[] dataToSend = Encoding.ASCII.GetBytes(DATA_TO_SEND);

			try
			{
				myDevice.Open();

				XBeeNetwork xbeeNetwork = myDevice.GetNetwork();
				RemoteXBeeDevice remoteDevice = xbeeNetwork.DiscoverDevice(REMOTE_NODE_IDENTIFIER);
				if (remoteDevice == null)
				{
					Console.WriteLine(">> Couldn't find the remote XBee device with '" + REMOTE_NODE_IDENTIFIER + "' Node Identifier.");
				}
				else
				{
					Console.WriteLine(">> Sending explicit data to {0} [{1} - {2} - {3} - {4}] >> {5} | {6}... ",
						remoteDevice.XBee64BitAddr,
						HexUtils.IntegerToHexString(SOURCE_ENDPOINT, 1),
						HexUtils.IntegerToHexString(DESTINATION_ENDPOINT, 1),
						HexUtils.IntegerToHexString(CLUSTER_ID, 2),
						HexUtils.IntegerToHexString(PROFILE_ID, 2),
						HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(dataToSend)),
						Encoding.ASCII.GetString(dataToSend));

					myDevice.SendExplicitData(remoteDevice, Convert.ToByte(SOURCE_ENDPOINT), Convert.ToByte(DESTINATION_ENDPOINT),
					ByteUtils.ShortToByteArray((short)CLUSTER_ID), ByteUtils.ShortToByteArray((short)PROFILE_ID), dataToSend);

					Console.WriteLine(">> Success");
				}
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
