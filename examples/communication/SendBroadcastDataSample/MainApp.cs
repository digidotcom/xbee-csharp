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
using XBeeLibrary.Core.Exceptions;
using XBeeDevice = XBeeLibrary.Windows.XBeeDevice;

namespace Examples.Communication.SendBroadcastDataSample
{
	/// <summary>
	/// XBee C# Library Send Broadcast Data sample application.
	/// </summary>
	/// <remarks>
	/// This example sends data to all remote devices on the same network.
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
		private static readonly string DATA_TO_SEND = "Hello XBee World!";

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +----------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Send Broadcast Data Sample  |");
			Console.WriteLine(" +----------------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);
			byte[] dataToSend = Encoding.ASCII.GetBytes(DATA_TO_SEND);

			try
			{
				myDevice.Open();
				Console.WriteLine(">> Sending broadcast data. Message: " + DATA_TO_SEND);
				myDevice.SendBroadcastData(dataToSend);
				Console.WriteLine(">> Success");
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
