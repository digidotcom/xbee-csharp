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

using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using AgricultureDemo.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Utils;
using System.Text.RegularExpressions;

namespace AgricultureDemo
{
	public class ProvisionPageViewModel : ViewModelBase
	{
		// Constants.
		private static readonly int MIN_NETWORK_ID = 1;
		private static readonly int MAX_NETWORK_ID = 65534;

		private static readonly int MAX_LENGTH_NETWORK_ID = 4;
		private static readonly int MAX_LENGTH_NETWORK_PASSWORD = 32;
		private static readonly int MAX_LENGTH_COORDINATES = 15;

		private static readonly string AT_ASSOCIATION = "AI";

		private static readonly int ASSOCIATION_TIMEOUT = 60000;
		private static readonly int RESPONSE_TIMEOUT = 10000;

		private static readonly string DRM_GROUP = "agri-{0}";

		// Variables.
		private BleDevice bleDevice;

		private string farmId;
		private string deviceId;
		private string installCode;
		private string networkId;
		private string networkPassword;
		private Location location;

		private bool isController;
		private bool isMainController;
		private bool pageValid;

		// Properties.
		public string FarmId
		{
			get { return farmId; }
			set
			{
				farmId = value.ToLower();
				RaisePropertyChangedEvent("FarmId");
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

		public string NetworkId
		{
			get { return networkId; }
			set
			{
				networkId = IsValidHex(value, MAX_LENGTH_NETWORK_ID) ? value : networkId;
				RaisePropertyChangedEvent("NetworkId");
				ValidatePage();
			}
		}

		public string NetworkPassword
		{
			get { return networkPassword; }
			set
			{
				networkPassword = IsValidHex(value, MAX_LENGTH_NETWORK_PASSWORD) ? value : networkPassword;
				RaisePropertyChangedEvent("NetworkPassword");
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

		public bool IsController
		{
			get { return isController; }
			set
			{
				isController = value;
				RaisePropertyChangedEvent("IsController");
			}
		}

		public bool IsMainController
		{
			get { return isMainController; }
			set
			{
				isMainController = value;
				RaisePropertyChangedEvent("IsMainController");
				ValidatePage();
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
			IsController = bleDevice.IsController;
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
		/// Generates a random network identifier.
		/// </summary>
		public void GenerateNetworkId()
		{
			Random random = new Random();
			NetworkId = random.Next(MIN_NETWORK_ID, MAX_NETWORK_ID).ToString("X");
		}

		/// <summary>
		/// Opens the page to select one of the used network identifiers.
		/// </summary>
		public async void SearchNetworkId()
		{
			NetworksLogPage page = new NetworksLogPage();
			await PopupNavigation.Instance.PushAsync(page);
			page.Disappearing += (object sender, EventArgs e) =>
			{
				if (page.SelectedItem != null)
					NetworkId = page.SelectedItem;
			};
		}

		/// <summary>
		/// Returns whether the given value is a valid hexadecimal string.
		/// </summary>
		/// <param name="newValue">Value to check.</param>
		/// <param name="maxLength">Maximum length allowed.</param>
		/// <returns><c>true</c> if the value is valid, <c>false</c> otherwise.
		/// </returns>
		public bool IsValidHex(string newValue, int maxLength)
		{
			bool isValid = true;

			if (newValue.Length > 1)
			{
				// Check the length.
				if (newValue.Length > maxLength)
					isValid = false;

				// Check the hex format.
				try
				{
					int.Parse(newValue, System.Globalization.NumberStyles.HexNumber);
				}
				catch (FormatException)
				{
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Validates the page to enable or disable the provisioning button.
		/// </summary>
		public void ValidatePage()
		{
			IsValid = !string.IsNullOrEmpty(DeviceId) && (!isController || !string.IsNullOrEmpty(FarmId)) 
				&& !string.IsNullOrEmpty(NetworkId) && Location != null;
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
						+ "'" + JsonConstants.PROP_NAME + "': '" + DeviceId + "',"
						+ (isController && isMainController ? "'" + JsonConstants.PROP_MAIN_CONTROLLER + "': 'true'," : "")
						+ "'" + JsonConstants.PROP_LATITUDE + "': '" + FormatLocation(Location.Latitude) + "',"
						+ "'" + JsonConstants.PROP_LONGITUDE + "': '" + FormatLocation(Location.Longitude) + "',"
						+ (Location.Altitude.HasValue ? "'" + JsonConstants.PROP_ALTITUDE + "': '" + FormatLocation(Math.Round(Location.Altitude.Value)) + "'," : "")
						+ "'" + JsonConstants.PROP_PAN_ID + "': '" + NetworkId + "',"
						+ (!string.IsNullOrEmpty(NetworkPassword) ? "'" + JsonConstants.PROP_PASS + "': '" + NetworkPassword + "'" : "")
					+ "}"
				+ "}");

				string response = SendData(json, JsonConstants.OP_WRITE, true);
				if (response == null || !response.Equals(""))
				{
					await ShowProvisioningError("Could not provision the device" + (response != null ? ": " + response : "") + ".");
					return;
				}

				// If the device is a controller, store the newtork ID and register it in Digi Remote Manager.
				if (isController)
				{
					AppPreferences.AddNetworkId(NetworkId);
					try
					{
						RegisterDeviceDRM();
					}
					catch (Exception e)
					{
						await ShowProvisioningError(string.Format("Could not register the device in Digi Remote Manager: {0}", e.Message));
						return;
					}
				}
				// If the device is a station, wait until it is joined to the network.
				else
				{
					HideLoadingDialog();
					ShowLoadingDialog("Waiting for device to join the network...");

					long deadline = Environment.TickCount + ASSOCIATION_TIMEOUT;
					int ai = -1;
					// Wait until the device is associated or the timeout elapses.
					while (Environment.TickCount < deadline && ai != 0)
					{
						ai = int.Parse(HexUtils.ByteArrayToHexString(bleDevice.XBeeDevice.GetParameter(AT_ASSOCIATION)), System.Globalization.NumberStyles.HexNumber);
						await Task.Delay(1000);
					}

					if (ai != 0)
					{
						await ShowProvisioningError(string.Format("The device could not associate to the configured network. " +
							"Please make sure the irrigation controller with network ID '{0}' is powered on.", NetworkId));
						return;
					}
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
		/// Registers the irrigation controller in Digi Remote Manager.
		/// </summary>
		private void RegisterDeviceDRM()
		{
			try
			{
				string data = string.Format("<DeviceCore><devMac>{0}</devMac><dpName>{1}</dpName><grpPath>{2}</grpPath>" +
					"<dpMapLat>{3}</dpMapLat><dpMapLong>{4}</dpMapLong>{5}{6}</DeviceCore>",
					bleDevice.Mac, NormalizeDRMName(DeviceId), string.Format(DRM_GROUP, FarmId),
					FormatLocation(Location.Latitude), FormatLocation(Location.Longitude),
					isMainController ? "<dpTags>main_controller</dpTags>" : "",
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
		/// Updates the irrigation controller with the given ID in Digi Remote
		/// Manager.
		/// </summary>
		/// <param name="devId">DRM device ID.</param>
		private void UpdateDeviceDRM(string devId)
		{
			try
			{
				string data = string.Format("<DeviceCore><devConnectwareId>{0}</devConnectwareId><dpName>{1}</dpName>" +
					"<grpPath>{2}</grpPath><dpMapLat>{3}</dpMapLat><dpMapLong>{4}</dpMapLong>{5}</DeviceCore>",
					devId, NormalizeDRMName(DeviceId), string.Format(DRM_GROUP, FarmId),
					FormatLocation(Location.Latitude), FormatLocation(Location.Longitude),
					isMainController ? "<dpTags>main_controller</dpTags>" : "");
				SendDRMRequest("PUT", data);
			}
			catch (Exception e)
			{
				Console.Write(e.StackTrace);
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
			Uri url = new Uri(string.Format("{0}/ws/DeviceCore", AppPreferences.GetDRMServer()));
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

			// Register event handlers for incoming data from both interfaces.
			EventHandler<SerialDataReceivedEventArgs> serialHandler = (object sender, SerialDataReceivedEventArgs e) =>
			{
				response = ParseResponse(operation, e.Data);
			};
			EventHandler<MicroPythonDataReceivedEventArgs> mpHandler = (object sender, MicroPythonDataReceivedEventArgs e) =>
			{
				response = ParseResponse(operation, e.Data);
			};

			// If the request needs a response, register the appropriate callbacks.
			if (needsResponse)
			{
				if (isController)
					bleDevice.XBeeDevice.SerialDataReceived += serialHandler;
				else
					bleDevice.XBeeDevice.MicroPythonDataReceived += mpHandler;
			}

			int maxRetries = needsResponse ? 3 : 1;
			int retry = 0;

			while (retry < maxRetries)
			{
				Task.Run(() => {
					// Send the data based on the device type.
					if (isController)
						bleDevice.XBeeDevice.SendSerialData(dataToSend);
					else
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

			// Unregister the callbacks.
			if (needsResponse)
			{
				if (isController)
					bleDevice.XBeeDevice.SerialDataReceived -= serialHandler;
				else
					bleDevice.XBeeDevice.MicroPythonDataReceived -= mpHandler;
			}

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
				Label = "Irrigation device",
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
