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
using System.Net.Sockets;
using System.Text;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using CellularDevice = XBeeLibrary.Windows.CellularDevice;

namespace ConsoleApp.Communication.IP.KnockKnockSample
{
	/// <summary>
	/// XBee C# Library Knock Knock sample application.
	/// </summary>
	/// <remarks>This example starts a simple web server and connects to it by sending a
	/// message to start a Knock Knock joke.
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

		private const int SERVER_PORT = 9750;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +--------------------------------------+");
			Console.WriteLine(" |  XBee C# Library Knock Knock Sample  |");
			Console.WriteLine(" +--------------------------------------+\n");

			CellularDevice myDevice = new CellularDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				myDevice.IPDataReceived += MyDevice_IPDataReceived;

				IPAddress[] ad = Dns.GetHostAddresses(Dns.GetHostName());
				IPAddress hostAddress = null;
				foreach (IPAddress ip in ad)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						hostAddress = ip;
						break;
					}
				}
				hostAddress = IPAddress.Parse("10.101.2.193");
				WebServer.Start(hostAddress, SERVER_PORT);

				myDevice.SendIPData(hostAddress, SERVER_PORT, IPProtocol.TCP,
					Encoding.UTF8.GetBytes("\n"));

				string line;
				while (!(line = Console.ReadLine()).ToLower().Equals("bye."))
					myDevice.SendIPData(hostAddress, SERVER_PORT, IPProtocol.TCP,
						Encoding.UTF8.GetBytes(line + "\n"));
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
			Console.WriteLine(e.IPDataReceived.DataString);
		}
	}
}
