// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#ifndef IOT_CONFIGS_H
#define IOT_CONFIGS_H

/**
 * WiFi setup
 */
#define IOT_CONFIG_WIFI_SSID            "jpfeiffer"
#define IOT_CONFIG_WIFI_PASSWORD        "Fxdbones54"

/**
 * Find under Microsoft Azure IoT Suite -> DEVICES -> <your device> -> Device Details and Authentication Keys
 * String containing Hostname, Device Id & Device Key in the format:
 *  "HostName=<host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"    
 */
#define IOT_CONFIG_CONNECTION_STRING    "HostName=EpiFibHub.azure-devices.net;DeviceId=Feather_001;SharedAccessKey=BUuucPHPWNVTBWd+FKlOZA=="

/** 
 * Choose the transport protocol
 */
#define IOT_CONFIG_MQTT              // uncomment this line for MQTT
//#define IOT_CONFIG_HTTP              // uncomment this line for HTTP

#endif /* IOT_CONFIGS_H */