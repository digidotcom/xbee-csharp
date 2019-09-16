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
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
using XBeeDevice = XBeeLibrary.Windows.XBeeDevice;

namespace Examples.Communication.Relay.ReceiveUserDataRelaySample
{
	/// <summary>
	/// XBee C# Library Receive User Data Relay sample application.
	/// </summary>
	/// <remarks>
	/// This example registers a callback to be executed when new User Data Relay
	/// messages are received.
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
			Console.WriteLine(" +--------------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Receive User Data Relay Sample  |");
			Console.WriteLine(" +--------------------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();
				myDevice.UserDataRelayReceived += MyDevice_UserDataRelayReceived;
				Console.WriteLine(">> Waiting for data...");
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
		/// Method called to manage received User Data Relay messages that were sent
		/// by another interface of the XBee device.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyDevice_UserDataRelayReceived(object sender, UserDataRelayReceivedEventArgs e)
		{
			Console.WriteLine("Relay data received from {0} >> {1} | {2}",
				e.UserDataRelayMessage.SourceInterface.GetDescription(),
				HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(e.UserDataRelayMessage.Data)),
				Encoding.ASCII.GetString(e.UserDataRelayMessage.Data));
		}
	}
}
