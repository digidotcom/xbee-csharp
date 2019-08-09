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
using System.IO.Ports;
using XBeeLibrary.Windows.Connection.Serial;

namespace XBeeLibrary.Windows
{
	/// <summary>
	/// This class represents an XBee ZigBee device.
	/// </summary>
	/// <seealso cref="CellularDevice"/>
	/// <seealso cref="DigiMeshDevice"/>
	/// <seealso cref="DigiPointDevice"/>
	/// <seealso cref="Raw802Device"/>
	public class ZigBeeDevice : Core.ZigBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ZigBeeDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="port">Serial port name where ZigBee device is attached to.</param>
		/// <param name="baudRate">Serial port baud rate to communicate with the device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> 
		/// <![CDATA[>]]> 0</c>.</exception>
		public ZigBeeDevice(string port, int baudRate)
			: base(XBee.CreateConnectiontionInterface(port, baudRate)) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ZigBeeDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="port">Serial port name where ZigBee device is attached to.</param>
		/// <param name="baudRate">Serial port baud rate to communicate with the device.</param>
		/// <param name="dataBits">Serial port data bits.</param>
		/// <param name="stopBits">Serial port stop bits.</param>
		/// <param name="parity">Serial port parity.</param>
		/// <param name="flowControl">Serial port flow control.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="dataBits"/> <![CDATA[<]]> 0</c>.</exception>
		/// <seealso cref="StopBits"/>
		/// <seealso cref="Parity"/>
		/// <seealso cref="Handshake"/>
		public ZigBeeDevice(string port, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
			: this(port, new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl)) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ZigBeeDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="port">Serial port name where ZigBee device is attached to.</param>
		/// <param name="serialPortParameters">Object containing the serial port parameters.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>
		/// or if <c><paramref name="serialPortParameters"/> == null</c>.</exception>
		/// <seealso cref="SerialPortParameters"/>
		public ZigBeeDevice(string port, SerialPortParameters serialPortParameters)
			: base(XBee.CreateConnectiontionInterface(port, serialPortParameters)) { }
	}
}
