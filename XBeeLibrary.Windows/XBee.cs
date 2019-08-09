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
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Windows.Connection.Serial;

namespace XBeeLibrary.Windows
{
	/// <summary>
	/// Helper class used to create a serial port connection interface.
	/// </summary>
	public class XBee
	{
		/// <summary>
		/// Retrieves a serial port connection interface for the provided port with the given baud
		/// rate.
		/// </summary>
		/// <param name="port">Serial port name.</param>
		/// <param name="baudRate">Serial port baud rate.</param>
		/// <returns>The serial port connection interface.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> 
		/// <![CDATA[>]]> 0</c>.</exception>
		/// <seealso cref="IConnectionInterface"/>
		public static IConnectionInterface CreateConnectiontionInterface(string port, int baudRate)
		{
			IConnectionInterface connectionInterface = new WinSerialPort(port, baudRate);
			return connectionInterface;
		}

		/// <summary>
		/// Retrieves a serial port connection interface for the provided port with the given
		/// serial port parameters.
		/// </summary>
		/// <param name="port">Serial port name.</param>
		/// <param name="serialPortParameters">Serial port parameters.</param>
		/// <returns>The serial port connection interface.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>
		/// or if <c><paramref name="serialPortParameters"/> == null</c>.</exception>
		/// <seealso cref="IConnectionInterface"/>
		/// <seealso cref="SerialPortParameters"/>
		public static IConnectionInterface CreateConnectiontionInterface(string port, SerialPortParameters serialPortParameters)
		{
			IConnectionInterface connectionInterface = new WinSerialPort(port, serialPortParameters);
			return connectionInterface;
		}
	}
}