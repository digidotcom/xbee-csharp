# XBee C# Library [ ![NuGet](https://img.shields.io/nuget/v/XBeeLibrary.Core)](https://www.nuget.org/packages/XBeeLibrary.Core/) [ ![NuGet](https://img.shields.io/nuget/v/XBeeLibrary.Xamarin)](https://www.nuget.org/packages/XBeeLibrary.Xamarin/)

This project contains the source code of the XBee C# Library, an easy-to-use 
API developed in C# that allows you to interact with Digi International's
[XBee](http://www.digi.com/xbee/) radio frequency (RF) modules. This source has 
been contributed by [Digi International](http://www.digi.com) from the original
work of Sébastien Rault.

The XBee C# library has two modules: **XBeeLibrary.Core**, which contains all
the common code for any platform, and **XBeeLibrary.Xamarin**, which contains
the necessary APIs to develop multi-platform mobile applications with Xamarin
to communicate with XBee devices over Bluetooth Low Energy.

> [!WARNING]
> Official support for Xamarin ended on May 1, 2024. So, XBee Library for
> Xamarin has been deprecated in favor of **Digi IoT Library for .NET MAUI**.
> To develop multi-platform mobile applications that communicate with Digi XBee
> devices over Bluetooth Low Energy, use our new
> [Digi IoT Library for .NET MAUI](https://github.com/digidotcom/digi-iot-maui).
> 
> For more information about developing Bluetooth Low Energy Mobile apps for
> Digi XBee devices, see the [Digi IoT Mobile SDK User Guide](https://www.digi.com/resources/documentation/digidocs/90002568)

The project includes the C# source code and multiple examples that show how to
use the available APIs. The examples are also available in source code format.

The main features of the library include:

* Support for ZigBee, 802.15.4, DigiMesh, Point-to-Multipoint, Cellular and
BLU XBee devices.
* Support for API and API escaped operating modes.
* Support for communicating with XBee devices over Bluetooth Low Energy
(XBeeLibrary.Xamarin).
* Management of local (attached to the host) and remote XBee device objects.
* Discovery of remote XBee devices associated with the same network as the 
local device.
* Configuration of local and remote XBee devices:
  * Configure common parameters with specific setters and getters.
  * Configure any other parameter with generic methods.
  * Execute AT commands.
  * Apply configuration changes.
  * Write configuration changes.
  * Reset the device.
* Transmission of data to all the XBee devices on the network or to a specific 
device.
* Reception of data from remote XBee devices:
  * Data polling.
  * Data reception callback.
* Transmission and reception of IP and SMS messages.
* Reception of network status changes related to the local XBee device.
* IO lines management:
  * Configure IO lines.
  * Set IO line value.
  * Read IO line value.
  * Receive IO data samples from any remote XBee device on the network.
* Support for explicit frames and application layer fields (Source endpoint, 
Destination endpoint, Profile ID, and Cluster ID).
* Support for User Data Relay frames, allowing the communication between
different interfaces (Serial, Bluetooth Low Energy and MicroPython).


## Start Here

The best place to get started is the 
[XBee C# Library documentation](http://www.digi.com/resources/documentation/digidocs/90002359/).


## How to Contribute

The contributing guidelines are in the 
[CONTRIBUTING.md](https://github.com/digidotcom/xbee-csharp/blob/master/CONTRIBUTING.md) 
document.


## License

Copyright 2019-2024, Digi International Inc.
Copyright 2014-2015, Sébastien RAULT. 

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
