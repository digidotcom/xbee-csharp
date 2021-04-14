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

using Xamarin.Essentials;

namespace TankDemo.Utils
{
	class AppPreferences
	{
		// Application preferences.
		private const string DRM_USERNAME = "DRMUsername";
		private const string DRM_PASSWORD = "DRMPassword";

		// Default preferences values.
		private const string DEFAULT_DRM_USERNAME = "";
		private const string DEFAULT_DRM_PASSWORD = "";

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
		/// Clears (removes) all the preferences and their values.
		/// </summary>
		public static void ClearPreferences()
		{
			Preferences.Clear();
		}
	}
}
