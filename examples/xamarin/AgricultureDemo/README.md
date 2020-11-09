End to End Agriculture IoT Sample Application
=============================================

This example is part of the Digi's End-to-End Agriculture IoT demo. It
demonstrates how to use the XBee Gateway and the XBee 3 modules to exchange
data, communicate with Digi Remote Manager and use the BLE interface to talk to
mobile apps applied to the agriculture vertical.

This application is used to make the initial provisioning of the different
irrigation devices of the installation: controllers (XBee Gateways) and
stations (XBee 3 modules).

Read the demo documentation for more information:
https://www.digi.com/resources/documentation/digidocs/90002422/.

Demo requirements
-----------------

To run this example you need:

* An XBee Gateway device.
* At least one XBee 3 radio module with MicroPython support running the
  corresponding MicroPython application of the demo and its corresponding
  carrier board (XBIB-C board).
* A smartphone (Android or iOS) with access to the Internet.
* A Digi Remote Manager account. Go to https://myaccount.digi.com/ to create it
  if you do not have one.

Demo setup
----------

Make sure the hardware is set up correctly:

1. The mobile device is powered on.
2. The XBee Gateways and XBee 3 modules are properly set up and running their
   corresponding Python/MicroPython application.

Demo run
--------

The example is already configured, so all you need to do is to build and launch
the project.

The first page of the application lists the available irrigation devices near
to you. Before starting the provisioning, make sure you configure your Digi
Remote Manager credentials from the menu. 

To provision a device, tap on it on the list and enter its default Bluetooth
password. If the connection is successful, the Provisioning page appears. Enter
the required information about the device you want to provisioning and tap
**Provision device**.


Compatible with
---------------

* Android devices
* iOS devices

License
-------

Copyright 2020, Digi International Inc.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, you can obtain one at http://mozilla.org/MPL/2.0/.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.