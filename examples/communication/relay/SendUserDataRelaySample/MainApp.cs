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
using XBeeDevice = XBeeLibrary.Windows.XBeeDevice;

namespace Examples.Communication.Relay.SendUserDataRelaySample
{
	/// <summary>
	/// XBee C# Library Send User Data Relay sample application.
	/// </summary>
	/// <remarks>
	/// This example sends a message to the Bluetooth Low Energy interface of
	/// the device.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.
	/// </remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your sender module is connected to.
		private static readonly string PORT = "COM1";
		// TODO Replace with the baud rate of your sender module.
		private static readonly int BAUD_RATE = 9600;

		private static readonly XBeeLocalInterface DEST_INTERFACE = XBeeLocalInterface.BLUETOOTH;
		private static readonly string DATA_TO_SEND = "Hello from the serial interface (#{0})";

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-----------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Send User Data Relay Sample  |");
			Console.WriteLine(" +-----------------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				string data = "";

				for (int i = 0; i < 10; i++)
				{
					data = string.Format(DATA_TO_SEND, i + 1);
					Console.Write(">> Sending User Data Relay to {0} >> '{1}'... ",
						DEST_INTERFACE.GetDescription(), data);
					myDevice.SendUserDataRelay(DEST_INTERFACE, Encoding.ASCII.GetBytes(data));
					Console.WriteLine("Success");

					Task.Delay(1000).Wait();
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
