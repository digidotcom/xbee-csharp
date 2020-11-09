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

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using AgricultureDemo.Utils;
using System;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;

namespace AgricultureDemo
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DrmCredentialsPage : PopupPage
	{
		public static readonly string SERVER_PRODUCTION = "https://remotemanager.digi.com";
		public static readonly string SERVER_TEST = "https://test.idigi.com";

		/// <summary>
		/// Class constructor. Instantiates a new <c>DrmCredentialsPage</c>
		/// object.
		/// </summary>
		public DrmCredentialsPage()
		{
			InitializeComponent();
			BindingContext = this;
			usernameEntry.Text = AppPreferences.GetDRMUsername();
			passwordEntry.Text = AppPreferences.GetDRMPassword();
			serverPicker.ItemsSource = new List<string>
			{
				"Production",
				"Test"
			};
			serverPicker.SelectedIndex = AppPreferences.GetDRMServer().Equals(SERVER_PRODUCTION) ? 0 : 1;
		}

		protected override bool OnBackgroundClicked()
		{
			return false;
		}

		/// <summary>
		/// Method called when the 'OK' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void OkClicked(object sender, EventArgs e)
		{
			// Save the DRM username and password in the preferences.
			AppPreferences.SetDRMUsername(usernameEntry.Text);
			AppPreferences.SetDRMPassword(passwordEntry.Text);
			AppPreferences.SetDRMServer(serverPicker.SelectedIndex == 0 ? SERVER_PRODUCTION : SERVER_TEST);
			ClosePopup();
		}

		/// <summary>
		/// Method called when the 'Cancel' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void CancelClicked(object sender, EventArgs e)
		{
			ClosePopup();
		}

		/// <summary>
		/// Closes the popup.
		/// </summary>
		private async void ClosePopup()
		{
			await PopupNavigation.Instance.PopAsync();
		}
	}
}