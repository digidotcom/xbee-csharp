/*
 * Copyright 2020, Digi International Inc.
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

using Acr.UserDialogs;
using Newtonsoft.Json.Linq;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using AgricultureDemo.Utils;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Xamarin;

namespace AgricultureDemo
{
	public class MainPageViewModel : ViewModelBase
	{
		// Constants.
		private const string BLE_NAME_PREFIX = "IRRIGATION";

		private static readonly int DEV_INIT_TIMEOUT = 10000;

		// Variables.
		private IAdapter adapter;
		private ObservableCollection<BleDevice> devices;

		private bool isScanning = false;

		// Properties.
		public ObservableCollection<BleDevice> Devices
		{
			get { return devices; }
			set
			{
				devices = value;
				RaisePropertyChangedEvent("Devices");
			}
		}

		public BleDevice SelectedDevice { get; set; }

		public bool IsScanning
		{
			get { return isScanning; }
			set
			{
				isScanning = value;
				RaisePropertyChangedEvent("IsScanning");
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>MainPageViewModel</c> object.
		/// </summary>
		public MainPageViewModel()
		{
			devices = new ObservableCollection<BleDevice>();

			// Initialize Bluetooth stuff.
			adapter = CrossBluetoothLE.Current.Adapter;

			// Subscribe to device advertisements.
			adapter.DeviceAdvertised += (object sender, DeviceEventArgs e) =>
			{
				// Add only the devices whose name matches the configured prefix.
				if (e.Device == null || e.Device.Name == null || !e.Device.Name.ToLower().StartsWith(BLE_NAME_PREFIX.ToLower()))
					return;

				BleDevice advertisedDevice = new BleDevice(e.Device);
				// If the device is not discovered, add it to the list. Otherwise update its RSSI value.
				if (!Devices.Contains(advertisedDevice))
					Devices.Add(advertisedDevice);
				else
					Devices[Devices.IndexOf(advertisedDevice)].Rssi = advertisedDevice.Rssi;
			};

			// Subscribe to device connection lost.
			adapter.DeviceConnectionLost += (object sender, DeviceErrorEventArgs e) =>
			{
				if (SelectedDevice == null || CrossBluetoothLE.Current.State != BluetoothState.On)
					return;

				// Close the connection with the device.
				SelectedDevice.XBeeDevice.Close();
			};
		}

		/// <summary>
		/// Starts the Bluetooth scan process.
		/// </summary>
		public void StartScan()
		{
			CheckBluetooth();

			if (IsScanning)
				return;

			IsScanning = true;
			Devices.Clear();
			RestartScanning();

			// Timer task to re-start the scan process every 10 seconds.
			Device.StartTimer(TimeSpan.FromSeconds(10), () =>
			{
				RestartScanning();
				return IsScanning;
			});
		}

		/// <summary>
		/// Re-starts the Bluetooth scan process.
		/// </summary>
		private async void RestartScanning()
		{
			if (IsScanning)
			{
				await adapter.StopScanningForDevicesAsync();
				await adapter.StartScanningForDevicesAsync();
			}
		}

		/// <summary>
		/// Stops the Bluetooth scan process.
		/// </summary>
		public async void StopScan()
		{
			IsScanning = false;

			if (adapter.IsScanning)
				await adapter.StopScanningForDevicesAsync();
		}

		/// <summary>
		/// Asks the user for the Bluetooth password and tries to connect to the selected device.
		/// </summary>
		/// <param name="authFailed"><c>true</c> if the first authentication attempt failed,
		/// <c>false</c> otherwise.</param>
		public void AskForPassword(bool authFailed)
		{
			UserDialogs.Instance.Prompt(new PromptConfig
			{
				Title = "Enter Bluetooth password",
				// If the first authentication attempt failed, show an error message.
				Message = authFailed ? "Invalid password, please try again" : null,
				InputType = InputType.Password,
				OkText = "OK",
				CancelText = "Cancel",
				OnAction = args =>
				{
					if (args.Ok)
					{
						// Set the device password.
						SelectedDevice.XBeeDevice.SetBluetoothPassword(args.Value);
						// Connect to the selected device.
						ConnectToDevice();
					}
					else
					{
						// Restart the scan.
						StartScan();
					}
				}
			});
		}

		/// <summary>
		/// Connects to the selected device.
		/// </summary>
		private void ConnectToDevice()
		{
			// The connection process blocks the UI interface, so it must be done in a different thread.
			Task.Run(() =>
			{
				// Show a progress dialog while connecting to the device.
				ShowLoadingDialog("Connecting to device...");
				try
				{
					// Open the connection with the device.
					SelectedDevice.XBeeDevice.Open();

					// Ask the irrigation device for its type.
					string type = DetermineDeviceType(SelectedDevice);
					if (type == null)
					{
						HideLoadingDialog();
						DisplayAlert("Error connecting to device", "The irrigation device did not respond to the connection request. " +
							"Make sure the correct application is running on it and try again.");
						SelectedDevice.XBeeDevice.Close();
						return;
					}
					else
					{
						SelectedDevice.Type = type;
					}

					// If the open method did not throw an exception, the connection is open.
					Device.BeginInvokeOnMainThread(() =>
					{
						// Load the Provision page.
						Application.Current.MainPage.Navigation.PushAsync(new ProvisionPage(SelectedDevice));
					});
				}
				catch (BluetoothAuthenticationException)
				{
					// There was a problem in the Bluetooth authentication process, so ask for the password again.
					HideLoadingDialog();
					AskForPassword(true);
				}
				catch (XBeeException e)
				{
					HideLoadingDialog();
					DisplayAlert("Error connecting to device", e.Message);
				}
				finally
				{
					HideLoadingDialog();
				}
			});
		}

		/// <summary>
		/// Checks whether the bluetooth adapter is on or off. In case it is
		/// off, shows a dialog asking the user to turn it on.
		/// </summary>
		private async void CheckBluetooth()
		{
			var state = await GetBluetoothState();
			if (state == BluetoothState.Off)
				await DisplayAlert("Bluetooth disabled", "Bluetooth is required to work with the XBee devices, please turn it on.");
		}

		/// <summary>
		/// Returns the current bluetooth state.
		/// </summary>
		/// <returns>BluetoothState</returns>
		private Task<BluetoothState> GetBluetoothState()
		{
			IBluetoothLE ble = CrossBluetoothLE.Current;
			var tcs = new TaskCompletionSource<BluetoothState>();

			// In some cases the bluetooth state is unknown the first time it is read.
			if (ble.State == BluetoothState.Unknown)
			{
				// Listen for changes in the bluetooth adapter.
				EventHandler<BluetoothStateChangedArgs> handler = null;
				handler = (o, e) =>
				{
					CrossBluetoothLE.Current.StateChanged -= handler;
					tcs.SetResult(e.NewState);
				};
				CrossBluetoothLE.Current.StateChanged += handler;
			}
			else
			{
				tcs.SetResult(ble.State);
			}

			return tcs.Task;
		}

		/// <summary>
		/// Determines the type of the given irrigation device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		/// <returns>The type of the irrigation device.</returns>
		private string DetermineDeviceType(BleDevice device)
		{
			XBeeBLEDevice xbeeDevice = device.XBeeDevice;
			string type = null;

			// Register an event handler for incoming data from the serial interface of the
			// Bluetooth device (XBee Gateway).
			EventHandler<SerialDataReceivedEventArgs> serialHandler = (object sender, SerialDataReceivedEventArgs e) =>
			{
				type = ParseResponse(device, e.Data);
			};
			xbeeDevice.SerialDataReceived += serialHandler;

			// Register an event handler for incoming data from the MicroPython interface of the
			// Bluetooth device (XBee module).
			EventHandler<MicroPythonDataReceivedEventArgs> mpHandler = (object sender, MicroPythonDataReceivedEventArgs e) =>
			{
				type = ParseResponse(device, e.Data);
			};
			xbeeDevice.MicroPythonDataReceived += mpHandler;

			JObject json = JObject.Parse(@"{
				'" + JsonConstants.ITEM_OP + "': '" + JsonConstants.OP_ID + "'" +
			"}");

			// Send the ID request to both interfaces, only one will respond.
			xbeeDevice.SendSerialData(Encoding.UTF8.GetBytes(json.ToString()));
			xbeeDevice.SendMicroPythonData(Encoding.UTF8.GetBytes(json.ToString()));

			// Wait until the type is received or the timeout elapses.
			long deadline = Environment.TickCount + DEV_INIT_TIMEOUT;
			while (type == null && Environment.TickCount < deadline)
				Task.Delay(100);

			// Unregister the callbacks.
			xbeeDevice.SerialDataReceived -= serialHandler;
			xbeeDevice.MicroPythonDataReceived -= mpHandler;

			return type;
		}

		/// <summary>
		/// Parses the given response from the given Bluetooth device and
		/// returns its type.
		/// </summary>
		/// <param name="device">Bluetooth device that received the response.
		/// </param>
		/// <param name="response">Response received.</param>
		/// <returns>The type of the Bluetooth device.</returns>
		private string ParseResponse(BleDevice device, byte[] response)
		{
			try
			{
				// Parse the JSON of the response.
				JObject json = JObject.Parse(Encoding.UTF8.GetString(response));
				if (!JsonConstants.OP_ID.Equals((string)json[JsonConstants.ITEM_OP])
					|| !JsonConstants.STATUS_SUCCESS.Equals((string)json[JsonConstants.ITEM_STATUS]))
					return null;

				// Get the device type and MAC address.
				string type = (string)json[JsonConstants.ITEM_VALUE];
				string mac = (string)json[JsonConstants.ITEM_MAC];

				if (type.Equals(BleDevice.TYPE_CONTROLLER))
					device.Mac = mac;

				return type;
			}
			catch (Exception)
			{
				// Do nothing.
			}
			return null;
		}
	}
}
