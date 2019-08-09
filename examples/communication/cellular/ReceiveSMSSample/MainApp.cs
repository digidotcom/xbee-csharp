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
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.Cellular.ReceiveSMSSample
{
	/// <summary>
	/// XBee C# Library Receive SMS sample application.
	/// </summary>
	/// <remarks>This example configures a Cellular device to read SMS (sent from a phone 
	/// or Cellular device) using a callback that is executed when new SMS is 
	/// received.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.</remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your Cellular module is connected.
		private const string PORT = "COM1";
		// TODO Replace with the baud rate of your Cellular module.
		private const int BAUD_RATE = 9600;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +--------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Receive SMS Sample  |");
			Console.WriteLine(" +--------------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				myDevice.SMSReceived += MyDevice_SMSReceived;

				Console.WriteLine(">> Waiting for SMS...");
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
		/// Method called when a new SMS is received.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyDevice_SMSReceived(object sender, SMSReceivedEventArgs e)
		{
			Console.WriteLine("Received SMS from {0} >> '{1}'", e.SMSReceived.PhoneNumber, e.SMSReceived.Data);
		}
	}
}
