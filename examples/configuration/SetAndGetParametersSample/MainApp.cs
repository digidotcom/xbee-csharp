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
using XBeeLibrary.Core.Utils;
using XBeeLibrary.Windows;

namespace Examples.Configuration.SetAndGetParametersSample
{
	/// <summary>
	/// XBee C# Library Set and Get parameters sample application.
	/// </summary>
	/// <remarks>
	/// This example sets and gets the value of 4 parameters with different 
	/// value types. Then it reads them from the device verifying the read values 
	/// are the same as the values that were set.
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

		private static readonly string PARAM_NODE_ID = "NI";
		private static readonly string PARAM_PAN_ID = "ID";
		private static readonly string PARAM_DEST_ADDRESS_H = "DH";
		private static readonly string PARAM_DEST_ADDRESS_L = "DL";
		private static readonly string PARAM_VALUE_NODE_ID = "Yoda";
		private static readonly byte[] PARAM_VALUE_PAN_ID = new byte[] { 0x12, 0x34 };
		private static readonly int PARAM_VALUE_DEST_ADDRESS_H = 0x00;
		private static readonly int PARAM_VALUE_DEST_ADDRESS_L = 0xFFFF;

		/// <summary>
		/// Application main method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			Console.WriteLine(" +-------------------------------------------+");
			Console.WriteLine(" | XBee C# Library Set/Get parameters Sample |");
			Console.WriteLine(" +-------------------------------------------+\n");

			XBeeDevice myDevice = new XBeeDevice(PORT, BAUD_RATE);

			try
			{
				myDevice.Open();

				// Set parameters.
				myDevice.SetParameter(PARAM_NODE_ID, Encoding.ASCII.GetBytes(PARAM_VALUE_NODE_ID));
				myDevice.SetParameter(PARAM_PAN_ID, PARAM_VALUE_PAN_ID);
				myDevice.SetParameter(PARAM_DEST_ADDRESS_H, ByteUtils.IntToByteArray(PARAM_VALUE_DEST_ADDRESS_H));
				myDevice.SetParameter(PARAM_DEST_ADDRESS_L, ByteUtils.IntToByteArray(PARAM_VALUE_DEST_ADDRESS_L));

				// Get parameters
				byte[] paramValueNI = myDevice.GetParameter(PARAM_NODE_ID);
				byte[] paramValueID = myDevice.GetParameter(PARAM_PAN_ID);
				byte[] paramValueDH = myDevice.GetParameter(PARAM_DEST_ADDRESS_H);
				byte[] paramValueDL = myDevice.GetParameter(PARAM_DEST_ADDRESS_L);

				// Compare the read parameter values with the values that were set.
				if (!Encoding.ASCII.GetString(paramValueNI).Equals(PARAM_VALUE_NODE_ID))
				{
					Console.WriteLine(">> NI parameter was not set correctly.");
					myDevice.Close();
					return;
				}
				if (ByteUtils.ByteArrayToLong(paramValueID) != ByteUtils.ByteArrayToLong(PARAM_VALUE_PAN_ID))
				{
					Console.WriteLine(">> ID parameter was not set correctly.");
					myDevice.Close();
					return;
				}
				if (ByteUtils.ByteArrayToInt(paramValueDH) != PARAM_VALUE_DEST_ADDRESS_H)
				{
					Console.WriteLine(">> DH parameter was not set correctly.");
					myDevice.Close();
					return;
				}
				if (ByteUtils.ByteArrayToInt(paramValueDL) != PARAM_VALUE_DEST_ADDRESS_L)
				{
					Console.WriteLine(">> DL parameter was not set correctly.");
					myDevice.Close();
					return;
				}
				Console.WriteLine("All parameters were set correctly!");
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
