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
using XBeeLibrary.Core;
using XBeeLibrary.Core.Exceptions;
using XBeeDevice = XBeeLibrary.Windows.XBeeDevice;

namespace Examples.Network.DiscoverDevicesSample
{
	/// <summary>
	/// XBee C# Library Discover Devices sample application.
	/// </summary>
	/// <remarks>
	/// This example retrieves the XBee network from the local XBee device and 
	/// performs a remote device discovery.
	/// 
	/// For a complete description on the example, refer to the 'ReadMe.txt' file
	/// included in the root directory.
	/// </remarks>
	public class MainApp
	{
		/* Constants */

		// TODO Replace with the serial port where your module is connected to.
		private static readonly string PORT = "COM1";
		// TODO Replace with the baud rate of your module.
		private static readonly int BAUD_RATE = 9600;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-----------------------------------------+");
			Console.WriteLine(" | XBee C# Library Discover Devices Sample |");
			Console.WriteLine(" +-----------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();
				XBeeNetwork myXBeeNetwork = myDevice.GetNetwork();
				myXBeeNetwork.SetDiscoveryTimeout(15000);
				myXBeeNetwork.DeviceDiscovered += MyXBeeNetwork_DeviceDiscovered;
				myXBeeNetwork.DiscoveryFinished += MyXBeeNetwork_DiscoveryFinished;
				myXBeeNetwork.DiscoveryError += MyXBeeNetwork_DiscoveryError;
				Console.WriteLine(">> Discovering remote XBee devices...");
				myXBeeNetwork.StartNodeDiscoveryProcess();
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
		/// Method called when a new device is discovered.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyXBeeNetwork_DeviceDiscovered(object sender, XBeeLibrary.Core.Events.DeviceDiscoveredEventArgs e)
		{
			Console.WriteLine(">> Device discovered: " + e.DiscoveredDevice.ToString());
		}

		/// <summary>
		/// Method called when the discovery process has finished.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyXBeeNetwork_DiscoveryFinished(object sender, XBeeLibrary.Core.Events.DiscoveryFinishedEventArgs e)
		{
			if (e.Error == null)
			{
				Console.WriteLine(">> Discovery process finished successfully.");
			}
			else
			{
				Console.WriteLine(">> Discovery process finished due to the following error: " + e.Error);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Method called when the discovery process has failed.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private static void MyXBeeNetwork_DiscoveryError(object sender, XBeeLibrary.Core.Events.DiscoveryErrorEventArgs e)
		{
			Console.WriteLine(">> There was an error discovering devices: " + e.Error);
		}
	}
}
