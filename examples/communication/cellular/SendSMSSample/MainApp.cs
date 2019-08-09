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
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.Cellular.SendSMSSample
{
	/// <summary>
	/// XBee C# Library Send SMS sample application.
	/// </summary>
	/// <remarks>This example sends an SMS to a phone or Cellular device.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.</remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your Cellular module is connected.
		private const string PORT = "COM1";
		// TODO Replace with the baud rate of you Cellular module.
		private const int BAUD_RATE = 9600;
		// TODO Replace with the phone number of the device to send the SMS to.
		private const string PHONE = "";
		// TODO Optionally, replace with the text of the SMS.
		private const string SMS_TEXT = "Hello from XBee Cellular!";

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-----------------------------------+");
			Console.WriteLine(" |  XBee C# Library Send SMS Sample  |");
			Console.WriteLine(" +-----------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				Console.WriteLine("Sending SMS to {1} >> '{2}'... ",
						PHONE,
						SMS_TEXT);

				myDevice.SendSMS(PHONE, SMS_TEXT);

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
