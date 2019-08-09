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
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.IP.ConnectToEchoServerSample
{
	/// <summary>
	/// XBee C# Library Connect to Echo Server sample application.
	/// </summary>
	/// <remarks>This example connects to an echo server, sends data to it and reads the 
	/// echoed data.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.</remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your sender module is connected to.
		private const string PORT = "COM1";
		// TODO Replace with the baud rate of your sender module.
		private const int BAUD_RATE = 9600;
		// TODO Optionally, replace with the text you want to send to the server.
		private const string TEXT = "Hello XBee!";
	
		private const string ECHO_SERVER = "52.43.121.77";
	
		private const int ECHO_SERVER_PORT = 9001;

		private const IPProtocol PROTOCOL_TCP = IPProtocol.TCP;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-------------------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Connect to Echo Server Sample  |");
			Console.WriteLine(" +-------------------------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);
			byte[] dataToSend = Encoding.UTF8.GetBytes(TEXT);

			try
			{
				myDevice.Open();

				Console.WriteLine(">> Sending text to " + ECHO_SERVER + ":"
					+ ECHO_SERVER_PORT + " >> '" + TEXT + "'... ");
				myDevice.SendIPData(IPAddress.Parse(ECHO_SERVER),
						ECHO_SERVER_PORT, PROTOCOL_TCP, dataToSend);

				Console.WriteLine(">> Success");

				IPMessage response = myDevice.ReadIPData();
				if (response == null)
				{
					Console.WriteLine(">> Echo response was not received from the server.");
				}
				else
				{
					Console.WriteLine(">> Echo response received from " + response.IPAddress + ":"
						+ response.SourcePort + " >> '" + response.DataString);
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
