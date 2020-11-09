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

using Xamarin.Forms;

namespace AgricultureDemo
{
	public partial class MainPage : CustomContentPage
	{
		private MainPageViewModel mainPageViewModel;

		/// <summary>
		/// Class constructor. Instantiates a new <c>MainPage</c> object.
		/// </summary>
		public MainPage()
		{
			InitializeComponent();
			mainPageViewModel = new MainPageViewModel();
			BindingContext = mainPageViewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			mainPageViewModel.StartScan();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			mainPageViewModel.StopScan();
		}

		/// <summary>
		/// Method called when a list item is selected.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			BleDevice selectedDevice = e.SelectedItem as BleDevice;

			// Clear selection.
			((ListView)sender).SelectedItem = null;

			if (selectedDevice == null)
				return;

			mainPageViewModel.StopScan();
			mainPageViewModel.SelectedDevice = selectedDevice;
			mainPageViewModel.AskForPassword(false);
		}
	}
}
