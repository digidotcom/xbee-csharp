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
using Rg.Plugins.Popup.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AgricultureDemo
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		// Constants.
		private const string BUTTON_OK = "OK";

		private const string DOCUMENTATION_URL = "https://www.digi.com/resources/documentation/digidocs/90002422/";

		// Events.
		public event PropertyChangedEventHandler PropertyChanged;

		// Properties.
		/// <summary>
		/// URL of app's online documentation.
		/// </summary>
		public string URLDocumentation => DOCUMENTATION_URL;

		// Commands.
		/// <summary>
		/// Opens the URL specified in the parameter.
		/// </summary>
		public ICommand OpenURLCommand { get; private set; }
		/// <summary>
		/// Command used to open the DRM credentials page.
		/// </summary>
		public ICommand DrmCredentialsCommand { get; private set; }

		/// <summary>
		/// Class constructor. Instantiates a new <c>ViewModelBase</c> object.
		/// </summary>
		public ViewModelBase()
		{
			OpenURLCommand = new Command<string>(OpenURL);
			DrmCredentialsCommand = new Command(OpenDrmCredentialsPage);
		}

		/// <summary>
		/// Raises a property changed event for the given property name.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, e);
			}
		}

		/// <summary>
		/// Shows a loading dialog with the given text.
		/// </summary>
		/// <param name="text">Text to display.</param>
		protected void ShowLoadingDialog(string text)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.ShowLoading(text);
			});
		}

		/// <summary>
		/// Hides the loading dialog.
		/// </summary>
		protected void HideLoadingDialog()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.HideLoading();
			});
		}

		/// <summary>
		/// Displays an alert with the given title and message.
		/// </summary>
		/// <param name="alertTitle">Title of the alert to display.</param>
		/// <param name="alertMessage">Message of the alert to display.</param>
		protected async Task DisplayAlert(string alertTitle, string alertMessage)
		{
			await DisplayAlert(alertTitle, alertMessage, BUTTON_OK);
		}

		/// <summary>
		/// Displays an alert with the given title and message.
		/// </summary>
		/// <param name="alertTitle">Title of the alert to display.</param>
		/// <param name="alertMessage">Message of the alert to display.</param>
		/// <param name="buttonText">Text to display in the button of the alert dialog.</param>
		protected async Task DisplayAlert(string alertTitle, string alertMessage, string buttonText)
		{
			Page currentPage = GetCurrentPage();
			if (currentPage == null)
				return;

			await Device.InvokeOnMainThreadAsync(async () => {
				await currentPage.DisplayAlert(alertTitle, alertMessage, buttonText);
			});
		}

		/// <summary>
		/// Returns the current page being displayed.
		/// </summary>
		/// <returns>The current <c>Page</c> being displayed.</returns>
		protected Page GetCurrentPage()
		{
			return Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
		}

		/// <summary>
		/// Opens the specified HTML page in the default WEB browser of the 
		/// phone.
		/// </summary>
		/// <param name="urlPage">The HTML page to open.</param>
		public void OpenURL(string urlPage)
		{
			Device.OpenUri(new Uri(urlPage));
		}

		/// <summary>
		/// Opens the 'DRM credentials' page.
		/// </summary>
		public async void OpenDrmCredentialsPage()
		{
			DrmCredentialsPage page = new DrmCredentialsPage();
			await PopupNavigation.Instance.PushAsync(page);
		}
	}
}
