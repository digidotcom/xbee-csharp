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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AgricultureDemo
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NetworksLogPage : PopupPage
	{
		// Properties.
		public string SelectedItem { get; private set; }

		/// <summary>
		/// Class constructor. Instantiates a new <c>NetworksLogPage</c> object.
		/// </summary>
		public NetworksLogPage()
		{
			InitializeComponent();

			networksList.ItemsSource = AppPreferences.GetNetworkIds();

			var template = new DataTemplate(typeof(TextCell));
			template.SetValue(TextCell.TextColorProperty, Color.Black);
			template.SetBinding(TextCell.TextProperty, ".");
			networksList.ItemTemplate = template;
		}

		protected override bool OnBackgroundClicked()
		{
			return false;
		}

		/// <summary>
		/// Method called when a list item is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void ListItemClicked(object sender, SelectedItemChangedEventArgs e)
		{
			SelectedItem = (string)e.SelectedItem;
		}

		/// <summary>
		/// Method called when the 'OK' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void OkClicked(object sender, EventArgs e)
		{
			ClosePopup();
		}

		/// <summary>
		/// Method called when the 'Cancel' button is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void CancelClicked(object sender, EventArgs e)
		{
			SelectedItem = null;
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