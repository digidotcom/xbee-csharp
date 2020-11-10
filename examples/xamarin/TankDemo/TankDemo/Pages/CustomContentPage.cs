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

using System;
using Xamarin.Forms;

namespace TankDemo
{
	public class CustomContentPage : ContentPage
	{
		/// <summary>
		/// Gets or Sets the Back button click overriden custom action
		/// </summary>
		public Action CustomBackButtonAction { get; set; }

		public static readonly BindableProperty EnableBackButtonOverrideProperty =
			BindableProperty.Create(nameof(EnableBackButtonOverride), typeof(bool), typeof(CustomContentPage), false);

		/// <summary>
		/// Gets or Sets Custom Back button overriding state
		/// </summary>
		public bool EnableBackButtonOverride
		{
			get
			{
				return (bool)GetValue(EnableBackButtonOverrideProperty);
			}
			set
			{
				SetValue(EnableBackButtonOverrideProperty, value);
			}
		}
	}
}
