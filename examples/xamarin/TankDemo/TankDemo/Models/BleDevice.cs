/*
 * Copyright 2021, Digi International Inc.
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

using Plugin.BLE.Abstractions.Contracts;
using TankDemo.Utils;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using XBeeLibrary.Xamarin;

namespace TankDemo
{
	public class BleDevice : INotifyPropertyChanged
	{
		// Constants.
		public static string MAC_REGEX = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
		public static string MAC_REPLACE = "$1:$2:$3:$4:$5:$6";

		// Variables.
		private int rssi = RssiUtils.WORST_RSSI;

		// Properties.
		public IDevice Device { get; private set; }

		public Guid Id => Device.Id;

		public string Name => string.IsNullOrEmpty(Device.Name) ? "<Unknown>" : Device.Name;

		public string BleMac => Regex.Replace(Device.Id.ToString().Split('-')[4], MAC_REGEX, MAC_REPLACE).ToUpper();

		public string Imei { get; set; }

		public int Rssi
		{
			get { return rssi; }
			set
			{
				if (value < 0)
					rssi = value;

				RaisePropertyChangedEvent("Rssi");
				RaisePropertyChangedEvent("RssiImage");
			}
		}

		public string RssiImage => RssiUtils.GetRssiImage(rssi);

		public XBeeBLEDevice XBeeDevice { get; private set; }

		// Events.
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Class constructor. Instantiates a new <c>BleDevice</c> object with
		/// the given native device.
		/// </summary>
		/// <param name="device">The native Bluetooth device.</param>
		public BleDevice(IDevice device)
		{
			Device = device ?? throw new ArgumentNullException("Device cannot be null");
			Rssi = device.Rssi;

			XBeeDevice = new XBeeBLEDevice(device, null);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			BleDevice dev = (BleDevice)obj;
			return Id == dev.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, e);
			}
		}
	}
}
