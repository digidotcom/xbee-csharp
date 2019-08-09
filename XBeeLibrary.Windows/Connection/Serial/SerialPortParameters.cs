/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
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

namespace XBeeLibrary.Windows.Connection.Serial
{
	/// <summary>
	/// Helper class used to store serial connection parameters information.
	/// </summary>
	/// <remarks>
	/// Parameters are stored as public variables so that they can be accessed and read from
	/// any class.
	/// </remarks>
	public sealed class SerialPortParameters : IEquatable<SerialPortParameters>
	{
		// Constants.
		private const int HASH_SEED = 23;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="SerialPortParameters"/> object with
		/// the given parameters.
		/// </summary>
		/// <param name="baudrate">Serial connection baud rate.</param>
		/// <param name="dataBits">Serial connection data bits.</param>
		/// <param name="stopBits">Serial connection stop bits.</param>
		/// <param name="parity">Serial connection parity.</param>
		/// <param name="flowControl">Serial connection flow control.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="dataBits"/> <![CDATA[<]]> 0</c>.</exception>
		/// <seealso cref="StopBits"/>
		/// <seealso cref="Parity"/>
		/// <seealso cref="Handshake"/>
		public SerialPortParameters(int baudrate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
		{
			if (baudrate < 0)
				throw new ArgumentOutOfRangeException("Baudrate cannot be less than 0.");
			if (dataBits < 0)
				throw new ArgumentOutOfRangeException("Number of data bits cannot be less than 0.");

			this.BaudRate = baudrate;
			this.DataBits = dataBits;
			this.StopBits = stopBits;
			this.Parity = parity;
			this.FlowControl = flowControl;
		}

		// Properties.
		public int BaudRate { get; set; }
		public int DataBits { get; set; }
		public StopBits StopBits { get; set; }
		public Parity Parity { get; set; }
		public Handshake FlowControl { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as SerialPortParameters;

			return other != null && Equals(other);
		}

		public bool Equals(SerialPortParameters other)
		{
			return other != null
				&& other.BaudRate == BaudRate
				&& other.DataBits == DataBits
				&& other.StopBits == StopBits
				&& other.Parity == Parity
				&& other.FlowControl == FlowControl;
		}

		public override int GetHashCode()
		{
			int hash = HASH_SEED;
			hash = hash * (hash + BaudRate);
			hash = hash * (hash + DataBits);
			hash = hash * (hash + (int)StopBits);
			hash = hash * (hash + (int)Parity);
			hash = hash * (hash + (int)FlowControl);
			return hash;
		}

		public override string ToString()
		{
			return string.Format("Baud Rate: {0}, Data Bits: {1}, Stop Bits: {2}, Parity: {3}, Flow Control: {4}",
				BaudRate,
				DataBits,
				StopBits,
				Parity,
				FlowControl);
		}
	}
}