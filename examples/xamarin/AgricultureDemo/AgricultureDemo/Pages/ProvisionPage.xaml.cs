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

using System;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace AgricultureDemo
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProvisionPage : CustomContentPage
	{
		private ProvisionPageViewModel provisionPageViewModel;

		/// <summary>
		/// Class constructor. Instantiates a new <c>ProvisionPage</c> object
		/// with the given parameter.
		/// </summary>
		/// <param name="bleDevice">Bluetooth device.</param>
		public ProvisionPage(BleDevice bleDevice)
		{
			InitializeComponent();
			provisionPageViewModel = new ProvisionPageViewModel(bleDevice);
			BindingContext = provisionPageViewModel;

			// Register the back button action.
			if (EnableBackButtonOverride)
			{
				CustomBackButtonAction = async () =>
				{
					// Ask the user if wants to close the connection.
					if (await DisplayAlert("Disconnect device", "Do you want to disconnect the irrigation device?", "Yes", "No"))
						provisionPageViewModel.DisconnectDevice();
				};
			}

			UpdateLocation();
		}

		/// <summary>
		/// Updates the GPS location.
		/// </summary>
		private void UpdateLocation()
		{
			provisionPageViewModel.GetLocation(map);
		}

		/// <summary>
		/// Method called when the 'Provision device' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void ProvisionDeviceClicked(object sender, EventArgs e)
		{
			provisionPageViewModel.ProvisionDevice();
		}

		/// <summary>
		/// Method called when the 'Refresh location' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void RefreshLocationClicked(object sender, EventArgs e)
		{
			UpdateLocation();
		}

		/// <summary>
		/// Method called when the 'Generate Network ID' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void GenerateNetworkIdClicked(object sender, EventArgs e)
		{
			provisionPageViewModel.GenerateNetworkId();
		}

		/// <summary>
		/// Method called when the 'Search network' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void SearchNetworkIdClicked(object sender, EventArgs e)
		{
			provisionPageViewModel.SearchNetworkId();
		}

		/// <summary>
		/// Method called when the map is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void MapClicked(object sender, MapClickedEventArgs e)
		{
			provisionPageViewModel.SetMarkerPosition(map, e.Position);
		}

		/// <summary>
		/// Method called when the button to change the view is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void ToggleMapViewClicked(object sender, EventArgs e)
		{
			provisionPageViewModel.ToggleMapView(map);
		}
	}
}