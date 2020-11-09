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

namespace AgricultureDemo.Utils
{
	public class JsonConstants
	{
		public static readonly string ITEM_OP = "operation";
		public static readonly string ITEM_STATUS = "status";
		public static readonly string ITEM_MSG = "error_message";
		public static readonly string ITEM_VALUE = "value";
		public static readonly string ITEM_MAC = "mac";
		public static readonly string ITEM_PROP = "properties";

		public static readonly string PROP_LATITUDE = "latitude";
		public static readonly string PROP_LONGITUDE = "longitude";
		public static readonly string PROP_ALTITUDE = "altitude";
		public static readonly string PROP_NAME = "name";
		public static readonly string PROP_PAN_ID = "pan_id";
		public static readonly string PROP_PASS = "password";
		public static readonly string PROP_MAIN_CONTROLLER = "main_controller";

		public static readonly string OP_ID = "id";
		public static readonly string OP_READ = "read";
		public static readonly string OP_WRITE = "write";
		public static readonly string OP_FINISH = "finish";

		public static readonly string STATUS_SUCCESS = "success";
		public static readonly string STATUS_ERROR = "error";
	}
}
