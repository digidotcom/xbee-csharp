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
using System.Threading.Tasks;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
using XBeeLibrary.Windows;

namespace Examples.Communication.Explicit.ReceiveExplicitDataPollingSample
{
	/// <summary>
	/// XBee C# Library Receive Explicit Data polling sample application.
	/// </summary>
	/// <remarks>
	/// This example configures a ZigBee device to read data in application layer
	/// (explicit) mode from the ZigBee device using the polling mechanism.
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

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +------------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Explicit Data Polling Sample  |");
			Console.WriteLine(" +------------------------------------------------+\n");

			ZigBeeDevice myZigBeeDevice = new ZigBeeDevice(PORT, BAUD_RATE);

			try
			{
				myZigBeeDevice.Open();
				myZigBeeDevice.APIOutputMode = APIOutputMode.MODE_EXPLICIT;
				Task.Run( () =>
				{
					while (true)
					{
						ExplicitXBeeMessage explicitXBeeMessage = myZigBeeDevice.ReadExplicitData();
						if (explicitXBeeMessage != null)
						{
							Console.WriteLine(">> From " + explicitXBeeMessage.Device.XBee64BitAddr + explicitXBeeMessage.Device.NodeID + ": " + Encoding.ASCII.GetString(explicitXBeeMessage.Data));
							Console.WriteLine(" - Source endpoint: " + HexUtils.ByteToHexString(explicitXBeeMessage.SourceEndpoint));
							Console.WriteLine(" - Destination endpoint: " + HexUtils.ByteToHexString(explicitXBeeMessage.DestEndpoint));
							Console.WriteLine(" - Cluster ID: " + HexUtils.ByteArrayToHexString(explicitXBeeMessage.ClusterID));
							Console.WriteLine(" - Profile ID: " + HexUtils.ByteArrayToHexString(explicitXBeeMessage.ProfileID));
						}
					}
				});
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
				myZigBeeDevice.Close();
			}
		}
	}
}
