﻿/*
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

namespace XBeeLibrary.Core.Events.Relay
{
	/// <summary>
	/// Provides the contents of the MicroPython data received event.
	/// </summary>
	public class MicroPythonDataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="MicroPythonDataReceivedEventArgs"/> object with the provided
		/// parameters.
		/// </summary>
		/// <param name="data">The received data.</param>
		public MicroPythonDataReceivedEventArgs(byte[] data)
		{
			Data = data;
		}

		// Properties.
		/// <summary>
		/// The received data.
		/// </summary>
		public byte[] Data { get; private set; }
	}
}
