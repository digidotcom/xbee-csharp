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
using System.Net;
using System.Text;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.IP.SendUDPDataSample
{
	/// <summary>
	/// XBee C# Library Send UDP Data sample application.
	/// </summary>
	/// <remarks>This example sends UDP data to the specified IP address and port number.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.</remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your module is connected to.
		private const string PORT = "COM1";
		// TODO Replace with the baud rate of your module.
		private const int BAUD_RATE = 9600;
		// TODO Replace with the destination IP address.
		private const string DEST_IP_ADDRESS = "192.168.1.2";
		// TODO Replace with the destination port number (in decimal format).
		private const int DEST_PORT = 9750;

		private const IPProtocol PROTOCOL = IPProtocol.UDP;
		private const string DATA_TO_SEND = "Hello XBee!";

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +----------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Send UDP Data Sample  |");
			Console.WriteLine(" +----------------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);
			byte[] dataToSend = Encoding.UTF8.GetBytes(DATA_TO_SEND);

			try
			{
				myDevice.Open();

				if (!myDevice.IsConnected())
				{
					Console.WriteLine(">> ERROR: the device is not connected to the network");
				}
				else
				{
					Console.WriteLine("Sending data to {0}:{1} >> {2} | {3}... ",
						DEST_IP_ADDRESS,
						DEST_PORT,
						HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(dataToSend)),
						DATA_TO_SEND);

				myDevice.SendIPData(IPAddress.Parse(DEST_IP_ADDRESS), DEST_PORT,
					PROTOCOL, dataToSend);

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
