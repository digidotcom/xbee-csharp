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
using System.Collections.ObjectModel;
using Xamarin.Essentials;

namespace AgricultureDemo.Utils
{
	class AppPreferences
	{
		// Application preferences.
		private const string DRM_USERNAME = "DRMUsername";
		private const string DRM_PASSWORD = "DRMPassword";
		private const string DRM_SERVER = "DRMServer";
		private const string NETWORK_IDS = "NetworkIds";

		// Default preferences values.
		private const string DEFAULT_DRM_USERNAME = "";
		private const string DEFAULT_DRM_PASSWORD = "";
		private const string DEFAULT_NETWORK_IDS = "";

		private const string SEPARATOR = "@@@";

		/// <summary>
		/// Returns the DRM username preference.
		/// </summary>
		/// <returns>The DRM username preference.</returns>
		public static string GetDRMUsername()
		{
			return Preferences.Get(DRM_USERNAME, DEFAULT_DRM_USERNAME);
		}

		/// <summary>
		/// Sets the DRM username preference.
		/// </summary>
		/// <param name="username">The new DRM username.</param>
		public static void SetDRMUsername(string username)
		{
			Preferences.Set(DRM_USERNAME, username);
		}

		/// <summary>
		/// Returns the DRM password preference.
		/// </summary>
		/// <returns>The DRM password preference.</returns>
		public static string GetDRMPassword()
		{
			return Preferences.Get(DRM_PASSWORD, DEFAULT_DRM_PASSWORD);
		}

		/// <summary>
		/// Sets the DRM password preference.
		/// </summary>
		/// <param name="username">The new DRM password.</param>
		public static void SetDRMPassword(string password)
		{
			Preferences.Set(DRM_PASSWORD, password);
		}

		/// <summary>
		/// Returns the DRM server preference.
		/// </summary>
		/// <returns>The DRM server preference.</returns>
		public static string GetDRMServer()
		{
			return Preferences.Get(DRM_SERVER, DrmCredentialsPage.SERVER_PRODUCTION);
		}

		/// <summary>
		/// Sets the DRM server preference.
		/// </summary>
		/// <param name="server">The new DRM server.</param>
		public static void SetDRMServer(string server)
		{
			Preferences.Set(DRM_SERVER, server);
		}

		/// <summary>
		/// Returns the stored list of network IDs.
		/// </summary>
		/// <returns>The list of network IDs.</returns>
		public static ObservableCollection<string> GetNetworkIds()
		{
			ObservableCollection<string> list = new ObservableCollection<string>();
			string networkIds = Preferences.Get(NETWORK_IDS, DEFAULT_NETWORK_IDS);
			if (string.IsNullOrEmpty(networkIds))
				return list;

			foreach (string networkId in networkIds.Split(new string[] { SEPARATOR }, StringSplitOptions.None))
			{
				if (string.IsNullOrEmpty(networkId))
					continue;

				list.Add(networkId);
			}

			return list;
		}

		/// <summary>
		/// Adds the given network ID to the list.
		/// </summary>
		/// <param name="networkId">The network ID to add.</param>
		public static void AddNetworkId(string networkId)
		{
			ObservableCollection<string> list = GetNetworkIds();
			if (list.Contains(networkId))
			{
				list.Remove(networkId);
				list.Insert(0, networkId);
			}
			else
			{
				list.Add(networkId);
			}

			Preferences.Set(NETWORK_IDS, string.Join(SEPARATOR, list));
		}

		/// <summary>
		/// Clears (removes) all the preferences and their values.
		/// </summary>
		public static void ClearPreferences()
		{
			Preferences.Clear();
		}
	}
}
