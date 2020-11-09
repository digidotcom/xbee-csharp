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
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Rg.Plugins.Popup;
using System.Collections;
using System.Linq;
using Xamarin.Forms;

namespace AgricultureDemo.Droid
{
	[Activity(Label = "Digi XBee Agriculture", Icon = "@drawable/digi_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
		// Constants.
		private readonly int REQUEST_BLUETOOTH_ID = 0;
		private readonly string[] PERMISSIONS_BLUETOOTH =
		{
			Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation,
			Manifest.Permission.Bluetooth,
			Manifest.Permission.BluetoothAdmin
		};

		private readonly string ERROR_BLE_PERMISSIONS = "Bluetooth permissions are required in order to scan and connect to Bluetooth devices.";

		protected override void OnCreate(Bundle savedInstanceState)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			UserDialogs.Init(this);
			Popup.Init(this, savedInstanceState);

			CheckPermissions();

			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
			Xamarin.FormsMaps.Init(this, savedInstanceState);
			LoadApplication(new App());
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			
			if (requestCode != REQUEST_BLUETOOTH_ID)
				return;

			// Check if user declined any Bluetooth permission.
			bool anyPermissionDenied = false;
			foreach (var result in grantResults)
			{
				if (result == Permission.Denied)
				{
					anyPermissionDenied = true;
					break;
				}
			}
			// Display a message indicating that bluetooth permissions must be enabled.
			if (anyPermissionDenied)
				Toast.MakeText(this, ERROR_BLE_PERMISSIONS, ToastLength.Long).Show();

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			// check if the current item id 
			// is equals to the back button id
			if (item.ItemId == 16908332)
			{
				// retrieve the current xamarin forms page instance
				Page currentPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
				if (currentPage is CustomContentPage)
				{
					// check if the page has subscribed to 
					// the custom back button event
					if (((CustomContentPage)currentPage)?.CustomBackButtonAction != null)
					{
						((CustomContentPage)currentPage)?.CustomBackButtonAction.Invoke();
						// and disable the default back button action
						return false;
					}
				}

				// if its not subscribed then go ahead 
				// with the default back button action
				return base.OnOptionsItemSelected(item);
			}
			else
			{
				// since its not the back button 
				//click, pass the event to the base
				return base.OnOptionsItemSelected(item);
			}
		}

		public override void OnBackPressed()
		{
			// retrieve the current xamarin forms page instance
			Page currentPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
			if (currentPage is CustomContentPage)
			{
				// check if the page has subscribed to 
				// the custom back button event
				if (((CustomContentPage)currentPage)?.CustomBackButtonAction != null)
				{
					((CustomContentPage)currentPage)?.CustomBackButtonAction.Invoke();
				}
				else
				{
					base.OnBackPressed();
				}
			}
			else
			{
				base.OnBackPressed();
			}
		}

		/// <summary>
		/// Checks if the minimum permissions required to execute the 
		/// application are granted. If not, requests them.
		/// </summary>
		private void CheckPermissions()
		{
			if ((int)Build.VERSION.SdkInt < 23)
				return;

			var permissionsToRequest = new ArrayList();
			foreach (string permission in PERMISSIONS_BLUETOOTH)
			{
				if (CheckSelfPermission(permission) != Permission.Granted)
					permissionsToRequest.Add(permission);
			}
			// If the minimum permissions are not granted, request them to the user.
			if (permissionsToRequest.Count > 0)
				RequestPermissions(permissionsToRequest.ToArray(typeof(string)) as string[], REQUEST_BLUETOOTH_ID);
		}
	}
}