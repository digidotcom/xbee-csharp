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
using System.Threading;
using XBeeLibrary.Core.Connection;

namespace XBeeLibrary.Windows.Connection.Serial
{
	/// <summary>
	/// Class that provides common functionality to work with serial ports.
	/// </summary>
	public class WinSerialPort : IConnectionInterface
	{
		// Constants.
		/// <summary>
		/// Default receive timeout: 10 milliseconds.
		/// </summary>
		/// <remarks>
		/// When the specified number of milliseconds have elapsed, read will return immediately.
		/// </remarks>
		public const int DEFAULT_PORT_TIMEOUT = 10;

		/// <summary>
		/// Default number of data bits: 8.
		/// </summary>
		public const int DEFAULT_DATA_BITS = 8;

		/// <summary>
		/// Default number of stop bits: 1.
		/// </summary>
		public const StopBits DEFAULT_STOP_BITS = StopBits.One;

		/// <summary>
		/// Default parity: None.
		/// </summary>
		public const Parity DEFAULT_PARITY = Parity.None;

		/// <summary>
		/// Default flow control: None.
		/// </summary>
		public const Handshake DEFAULT_FLOW_CONTROL = Handshake.None;

		protected const int FLOW_CONTROL_HW = 3;

		protected string port;
		protected int baudRate;
		protected int receiveTimeout;

		protected SerialPortParameters parameters;

		private SerialPort serialPort;

		private DataStream stream;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WinSerialPort"/> object with the given
		/// parameters.
		/// </summary>
		/// <param name="port">COM port name to use.</param>
		/// <param name="parameters">Serial port connection parameters.</param>
		/// <seealso cref="SerialPortParameters"/>
		public WinSerialPort(string port, SerialPortParameters parameters)
			: this(port, parameters, DEFAULT_PORT_TIMEOUT) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WinSerialPort"/> object with the given
		/// parameters.
		/// </summary>
		/// <param name="port">COM port name to use.</param>
		/// <param name="baudRate">Serial connection baud rate.</param>
		public WinSerialPort(string port, int baudRate)
			: this(port, new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL), DEFAULT_PORT_TIMEOUT) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WinSerialPort"/> object with the given
		/// parameters.
		/// </summary>
		/// <param name="port">COM port name to use.</param>
		/// <param name="baudRate">Serial connection baud rate.</param>
		/// <param name="receiveTimeout">Receive timeout in milliseconds.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> <![CDATA[<]]> 0</c>
		///or if <c><paramref name="receiveTimeout"/> <![CDATA[<]]> 0</c>.</exception>
		public WinSerialPort(string port, int baudRate, int receiveTimeout)
			: this(port, new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL), receiveTimeout) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="WinSerialPort"/> object with the given
		/// parameters.
		/// </summary>
		/// <param name="port">COM port name to use.</param>
		/// <param name="parameters">Serial port connection parameters.</param>
		/// <param name="receiveTimeout">Receive timeout in milliseconds.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="port"/> == null</c>
		/// or if <c><paramref name="serialPortParameters"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="receiveTimeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <seealso cref="SerialPortParameters"/>
		public WinSerialPort(string port, SerialPortParameters parameters, int receiveTimeout)
		{
			if (parameters == null)
				throw new ArgumentNullException("Serial port parameters cannot be null");
			if (receiveTimeout < 0)
				throw new ArgumentOutOfRangeException("Receive timeout cannot be less than 0");

			this.port = port ?? throw new ArgumentNullException("Serial port cannot be null");
			this.baudRate = parameters.BaudRate;
			this.receiveTimeout = receiveTimeout;
			this.parameters = parameters;
		}

		public void Open()
		{
			stream = new DataStream();

			serialPort = new SerialPort(port, baudRate);
			serialPort.DataBits = parameters.DataBits;
			serialPort.StopBits = parameters.StopBits;
			serialPort.Parity = parameters.Parity;

			serialPort.Handshake = parameters.FlowControl;

			serialPort.ReadTimeout = receiveTimeout;
			serialPort.DataReceived += SerialPortDataReceived;

			serialPort.Open();
		}

		public void Close()
		{
			if (serialPort != null)
			{
				serialPort.DataReceived -= SerialPortDataReceived;
				serialPort.Close();
			}
		}

		public void WriteData(byte[] data)
		{
			if (serialPort != null && serialPort.IsOpen)
				serialPort.Write(data, 0, data.Length);
		}

		public void WriteData(byte[] data, int offset, int length)
		{
			if (serialPort != null && serialPort.IsOpen)
				serialPort.Write(data, offset, length);
		}

		public int ReadData(byte[] data)
		{
			int readBytes = 0;
			if (stream != null)
				readBytes = stream.Read(data, 0, data.Length);
			return readBytes;
		}

		public int ReadData(byte[] data, int offset, int length)
		{
			int readBytes = 0;
			if (stream != null)
				readBytes = stream.Read(data, offset, length);
			return readBytes;
		}

		public bool IsOpen
		{
			get
			{
				if (serialPort != null)
					return serialPort.IsOpen;

				return false;
			}
		}

		public DataStream Stream => stream;

		public ConnectionType GetConnectionType()
		{
			return ConnectionType.SERIAL;
		}

		public void SetEncryptionKeys(byte[] key, byte[] txNonce, byte[] rxNonce)
		{
			// Not implemented.
		}

		/// <summary>
		/// Sets the new parameters of the serial port.
		/// </summary>
		/// <param name="baudRate">The new value of baud rate.</param>
		/// <param name="dataBits">The new value of data bits.</param>
		/// <param name="stopBits">The new value of stop bits.</param>
		/// <param name="parity">The new value of parity.</param>
		/// <param name="flowControl">The new value of flow control.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="baudRate"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="dataBits"/> <![CDATA[<]]> 0</c>.</exception>
		/// <seealso cref="StopBits"/>
		/// <seealso cref="Parity"/>
		/// <seealso cref="Handshake"/>
		public void SetPortParameters(int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
		{
			SerialPortParameters parameters = new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl);
			SetPortParameters(parameters);
		}

		/// <summary>
		/// Sets the new parameters of the serial port.
		/// </summary>
		/// <param name="parameters"></param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="parameters"/> == null</c>.</exception>
		/// <seealso cref="SerialPortParameters"/>
		public void SetPortParameters(SerialPortParameters parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException("Serial port parameters cannot be null.");

			baudRate = parameters.BaudRate;
			this.parameters = parameters;
			if (IsOpen)
			{
				serialPort.Close();
				serialPort.Open();
			}
		}

		private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				int available = serialPort.BytesToRead;

				if (available > 0)
				{
					lock (this)
					{
						byte[] buffer = new byte[available];
						serialPort.Read(buffer, 0, available);
						stream.Write(buffer, 0, available);
						Monitor.Pulse(this);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public override string ToString()
		{
			if (parameters != null)
			{
				String parity = "N";
				String flowControl = "N";
				if (parameters.Parity == Parity.Odd)
					parity = "O";
				else if (parameters.Parity == Parity.Even)
					parity = "E";
				else if (parameters.Parity == Parity.Mark)
					parity = "M";
				else if (parameters.Parity == Parity.Space)
					parity = "S";
				if (parameters.FlowControl == Handshake.RequestToSend)
					flowControl = "H";
				else if (parameters.FlowControl == Handshake.XOnXOff)
					flowControl = "S";
				return string.Format("[{0} - {1}/{2}/{3}/{4}/{5}] ",
					port,
					baudRate,
					parameters.DataBits,
					parity,
					parameters.StopBits,
					flowControl);
			}
			else
				return string.Format("[{0} - {1}/8/N/1/N] ", port, baudRate);
		}
	}
}