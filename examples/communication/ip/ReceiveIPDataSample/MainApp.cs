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
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Utils;
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.IP.ReceiveIPDataSample
{
	/// <summary>
	/// XBee C# Library Receive IP Data sample application.
	/// </summary>
	/// <remarks>This example registers a listener to manage the received IP data.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.</remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your receiver module is connected.
		private const string PORT = "COM1";
		// TODO Replace with the baud rate of you receiver module.
		private const int BAUD_RATE = 9600;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Receive IP Data Sample  |");
			Console.WriteLine(" +------------------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				if (!myDevice.IsConnected())
				{
					Console.WriteLine(">> ERROR: the device is not connected to the network");
				}
				else
				{
					Console.WriteLine(">> Waiting for data...");
					myDevice.IPDataReceived += MyDevice_IPDataReceived;
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

		/// <summary>
		/// Method called when new IP data is received.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyDevice_IPDataReceived(object sender, IPDataReceivedEventArgs e)
		{
			Console.WriteLine("From {1} >> {2} | {3}",
							e.IPDataReceived.IPAddress,
							HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(e.IPDataReceived.Data)),
							e.IPDataReceived.DataString);
		}
	}
}
