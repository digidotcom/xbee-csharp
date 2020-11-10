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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TankDemo.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using XBeeLibrary.Core.Events.Relay;

namespace TankDemo
{
	public class ProvisionPageViewModel : ViewModelBase
	{
		// Constants.
		private static readonly int MAX_LENGTH_COORDINATES = 15;

		private static readonly int RESPONSE_TIMEOUT = 10000;

		private static readonly string DRM_GROUP = "tanks-{0}";

		// Variables.
		private BleDevice bleDevice;

		private string installationId;
		private string deviceId;
		private string installCode;
		private Location location;

		private bool pageValid;

		// Properties.
		public string InstallationId
		{
			get { return installationId; }
			set
			{
				installationId = value.ToLower();
				RaisePropertyChangedEvent("InstallationId");
				ValidatePage();
			}
		}

		public string DeviceId
		{
			get { return deviceId; }
			set
			{
				deviceId = value;
				RaisePropertyChangedEvent("DeviceId");
				ValidatePage();
			}
		}

		public string InstallCode
		{
			get { return installCode; }
			set
			{
				installCode = value;
				RaisePropertyChangedEvent("InstallCode");
				ValidatePage();
			}
		}

		public Location Location
		{
			get { return location; }
			set
			{
				if (value == null)
					return;

				location = value;
				RaisePropertyChangedEvent("Location");
				ValidatePage();
			}
		}

		public bool IsValid
		{
			get { return pageValid; }
			set
			{
				pageValid = value;
				RaisePropertyChangedEvent("IsValid");
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>ProvisionPageViewModel</c>
		/// object with the given parameters.
		/// </summary>
		/// <param name="bleDevice">Bluetooth device.</param>
		public ProvisionPageViewModel(BleDevice bleDevice)
		{
			this.bleDevice = bleDevice;
			IsValid = false;
		}

		/// <summary>
		/// Closes the connection with the device and goes to the previous page.
		/// </summary>
		public async void DisconnectDevice()
		{
			await Task.Run(() =>
			{
				// Close the connection.
				bleDevice.XBeeDevice.Close();

				// Go to the root page.
				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
				});
			});
		}

		/// <summary>
		/// Gets the GPS location of the mobile device.
		/// </summary>
		/// <param name="map">The map object.</param>
		public async void GetLocation(Xamarin.Forms.Maps.Map map)
		{
			ShowLoadingDialog("Getting GPS location...");

			Location location = null;

			try
			{
				var request = new GeolocationRequest(GeolocationAccuracy.Best);
				location = await Geolocation.GetLocationAsync(request);

				// Move the map to the current location.
				Position position = new Position(location.Latitude, location.Longitude);
				map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(50)));

				// Add a marker to indicate the location.
				SetMarkerPosition(map, position);
			}
			catch (FeatureNotEnabledException)
			{
				HideLoadingDialog();
				await DisplayAlert("Location not enabled", "Location is disabled on the device. Please enable it to get the current position.");
			}
			catch (PermissionException)
			{
				HideLoadingDialog();
				await DisplayAlert("Location not allowed", "The app does not have permission to access location. Please add it to get the current position.");
			}
			catch (Exception)
			{
				HideLoadingDialog();
				await DisplayAlert("Error getting location", "Could not get the current location.");
			}

			HideLoadingDialog();

			Location = location;
		}

		/// <summary>
		/// Validates the page to enable or disable the provisioning button.
		/// </summary>
		public void ValidatePage()
		{
			IsValid = !string.IsNullOrEmpty(DeviceId) && !string.IsNullOrEmpty(InstallationId) && Location != null;
		}

		/// <summary>
		/// Provisions the device with the data entered.
		/// </summary>
		public void ProvisionDevice()
		{
			Task.Run(async () =>
			{
				ShowLoadingDialog("Provisining device...");

				// Generate the provisioning JSON and send it to the device.
				JObject json = JObject.Parse(@"{"
					+ "'" + JsonConstants.ITEM_OP + "': '" + JsonConstants.OP_WRITE + "',"
					+ "'" + JsonConstants.ITEM_PROP + "': {"
						+ "'" + JsonConstants.PROP_NAME + "': '" + DeviceId + "'"
					+ "}"
				+ "}");

				string response = SendData(json, JsonConstants.OP_WRITE, true);
				if (response == null || !response.Equals(""))
				{
					await ShowProvisioningError("Could not provision the device" + (response != null ? ": " + response : "") + ".");
					return;
				}

				// Register the device in Digi Remote Manager.
				try
				{
					RegisterDeviceDRM();
				}
				catch (Exception e)
				{
					await ShowProvisioningError(string.Format("Could not register the device in Digi Remote Manager: {0}", e.Message));
					return;
				}

				FinishProvisioning();

				HideLoadingDialog();

				await DisplayAlert("Provisioning finished", "The device has been provisioned successfully.");

				// Go to the root page.
				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
				});
			});
		}

		/// <summary>
		/// Shows an error with the given message.
		/// </summary>
		/// <param name="message">Message to show.</param>
		private async Task ShowProvisioningError(string message)
		{
			HideLoadingDialog();
			await DisplayAlert("Provisioning error", message);
		}

		/// <summary>
		/// Formats the given GPS coordinate.
		/// </summary>
		/// <param name="coordinate">GPS coordinate to format.</param>
		/// <returns>The formatted GPS coordinate.</returns>
		private string FormatLocation(double coordinate)
		{
			string formatCoord = coordinate.ToString().Replace(",", ".");
			if (formatCoord.Length > MAX_LENGTH_COORDINATES)
				formatCoord = formatCoord.Substring(0, MAX_LENGTH_COORDINATES);
			return formatCoord;
		}

		/// <summary>
		/// Registers the device in Digi Remote Manager.
		/// </summary>
		private void RegisterDeviceDRM()
		{
			try
			{
				string data = string.Format("<DeviceCore><devCellularModemId>{0}</devCellularModemId><dpName>{1}</dpName>" +
					"<grpPath>{2}</grpPath><dpMapLat>{3}</dpMapLat><dpMapLong>{4}</dpMapLong>{5}</DeviceCore>",
					bleDevice.Imei, NormalizeDRMName(DeviceId), string.Format(DRM_GROUP, InstallationId),
					FormatLocation(Location.Latitude), FormatLocation(Location.Longitude),
					string.IsNullOrWhiteSpace(installCode) ? "" : string.Format("<devInstallCode>{0}</devInstallCode>", installCode));
				SendDRMRequest("POST", data);
			}
			catch (WebException e)
			{
				using (var stream = e.Response.GetResponseStream())
				using (var reader = new StreamReader(stream))
				{
					string errorMessage = reader.ReadToEnd();
					Console.WriteLine(errorMessage);
					Match match = Regex.Match(errorMessage, ".*The device (.*) is already provisioned.*");
					if (match.Success)
						UpdateDeviceDRM(match.Groups[1].Value);
					else if (errorMessage.Contains("already exists"))
						throw new Exception("The device is already registered in other DRM account.");
					else if (errorMessage.Contains("401"))
						throw new Exception("Configured user and password are not correct.");
					else
						throw e;
				}
			}
			catch (Exception e)
			{
				Console.Write(e.StackTrace);
				throw e;
			}
		}

		/// <summary>
		/// Updates the device with the given ID in Digi Remote Manager.
		/// </summary>
		/// <param name="devId">DRM device ID.</param>
		private void UpdateDeviceDRM(string devId)
		{
			try
			{
				string data = string.Format("<DeviceCore><devConnectwareId>{0}</devConnectwareId><dpName>{1}</dpName>" +
					"<grpPath>{2}</grpPath><dpMapLat>{3}</dpMapLat><dpMapLong>{4}</dpMapLong></DeviceCore>",
					devId, NormalizeDRMName(DeviceId), string.Format(DRM_GROUP, InstallationId),
					FormatLocation(Location.Latitude), FormatLocation(Location.Longitude));
				SendDRMRequest("PUT", data);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		/// <summary>
		/// Sends an HTTP request to the DRM DeviceCore service with the given
		/// data.
		/// </summary>
		/// <param name="method">HTTP method.</param>
		/// <param name="data">Data to send.</param>
		private void SendDRMRequest(string method, string data)
		{
			string user = AppPreferences.GetDRMUsername();
			string pwd = AppPreferences.GetDRMPassword();
			if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pwd))
				throw new Exception("User and password not configured.");

			// Create url to the Remote Manager server for a given web service request.
			Uri url = new Uri("https://remotemanager.digi.com/ws/DeviceCore");
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

			request.Method = method;

			CredentialCache credCache = new CredentialCache();
			credCache.Add(url, "Basic", new NetworkCredential(user, pwd));

			request.Credentials = credCache;

			request.ContentType = "text/xml";
			request.Accept = "text/xml";
			StreamWriter writer = new StreamWriter(request.GetRequestStream());
			writer.Write(data);
			writer.Close();

			// Get response.
			WebResponse serverResponse = request.GetResponse();
			serverResponse.Close();
		}

		/// <summary>
		/// Normalizes the given name for the DRM provisioning.
		/// </summary>
		/// <param name="name">Name to normalize.</param>
		/// <returns>The given name normalized.</returns>
		private string NormalizeDRMName(string name)
		{
			Regex rgx = new Regex("[^a-zA-Z0-9-]");
			return rgx.Replace(name, "-");
		}

		/// <summary>
		/// Finishes the provisioning phase by sending the 'finish' message to
		/// the device.
		/// </summary>
		private void FinishProvisioning()
		{
			JObject json = JObject.Parse(@"{
				'" + JsonConstants.ITEM_OP + "': '" + JsonConstants.OP_FINISH + "'" +
			"}");

			SendData(json, JsonConstants.OP_FINISH, false);
		}

		/// <summary>
		/// Sends the given JSON message to the device.
		/// </summary>
		/// <param name="json">JSON message to send.</param>
		/// <param name="operation">Operation of the request.</param>
		/// <param name="needsResponse"><c>true</c> if the request needs a
		/// response, <c>false</c> otherwise.</param>
		/// <returns>Empty if the request was successful, the error message
		/// if it failed or <c>null</c> if the response was not received.
		/// </returns>
		private string SendData(JObject json, string operation, bool needsResponse)
		{
			byte[] dataToSend = Encoding.UTF8.GetBytes(json.ToString());
			string response = null;

			// Register an event handler for incoming data from MicroPython.
			EventHandler<MicroPythonDataReceivedEventArgs> mpHandler = (object sender, MicroPythonDataReceivedEventArgs e) =>
			{
				response = ParseResponse(operation, e.Data);
			};

			// If the request needs a response, register the appropriate callback.
			if (needsResponse)
				bleDevice.XBeeDevice.MicroPythonDataReceived += mpHandler;

			int maxRetries = needsResponse ? 3 : 1;
			int retry = 0;

			while (retry < maxRetries)
			{
				Task.Run(() => {
					// Send the data.
					bleDevice.XBeeDevice.SendMicroPythonData(dataToSend);
				});

				// If the request needs a response, wait for it.
				if (needsResponse)
				{
					long deadline = Environment.TickCount + RESPONSE_TIMEOUT;
					while (response == null && Environment.TickCount < deadline)
						Task.Delay(100);
					if (response != null)
						break;
				}

				retry += 1;
			}

			// Unregister the callback.
			if (needsResponse)
				bleDevice.XBeeDevice.MicroPythonDataReceived -= mpHandler;

			return response;
		}

		/// <summary>
		/// Parses the given response for the given operation.
		/// </summary>
		/// <param name="operation">Request operation.</param>
		/// <param name="response">Received response.</param>
		/// <returns>Empty if the request was successful, the error message
		/// if it failed or <c>null</c> if the response was not received.
		/// </returns>
		private string ParseResponse(string operation, byte[] response)
		{
			try
			{
				JObject json = JObject.Parse(Encoding.UTF8.GetString(response));
				if (!operation.Equals((string)json[JsonConstants.ITEM_OP]))
					return null;

				string status = (string)json[JsonConstants.ITEM_STATUS];
				string errorMsg = (string)json[JsonConstants.ITEM_MSG];

				if (JsonConstants.STATUS_SUCCESS.Equals(status))
					return "";
				else if (JsonConstants.STATUS_ERROR.Equals(status))
					return errorMsg;
			}
			catch (Exception)
			{
				// Do nothing.
			}
			return null;
		}

		/// <summary>
		/// Sets the location of the marker in the map.
		/// </summary>
		/// <param name="map">Map.</param>
		/// <param name="position">New position.</param>
		public void SetMarkerPosition(Xamarin.Forms.Maps.Map map, Position position)
		{
			// Remove all the pins.
			map.Pins.Clear();

			// Create and add the pin with the given position.
			Pin pin = new Pin
			{
				Label = "Tank device",
				Position = position
			};
			map.Pins.Add(pin);

			// Update the location.
			Location = new Location(position.Latitude, position.Longitude);
		}

		/// <summary>
		/// Changes the map type from street to satellite and vice-versa.
		/// </summary>
		/// <param name="map">Map.</param>
		public void ToggleMapView(Xamarin.Forms.Maps.Map map)
		{
			map.MapType = map.MapType == MapType.Street ? MapType.Satellite : MapType.Street;
		}
	}
}
