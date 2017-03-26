// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Please Use Arduino IDE 1.6.8 or later.

/*******************************************************************************
 * Communication
 ******************************************************************************/
#include "iot_configs.h"

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <time.h>
#include <sys/time.h>
#include <SPI.h>
#ifdef ARDUINO_SAMD_FEATHER_M0
#include <Adafruit_WINC1500.h>
#include <Adafruit_WINC1500Client.h>
#include <Adafruit_WINC1500Server.h>
#include <Adafruit_WINC1500SSLClient.h>
#include <Adafruit_WINC1500Udp.h>
#elif defined(ARDUINO_SAMD_ZERO) || defined(ARDUINO_SAMD_MKR1000)
#include <WiFi101.h>
#endif

#include "epifib_container.h"
#include "NTPClient.h"

#include <AzureIoTHub.h>
#if defined(IOT_CONFIG_MQTT)
    #include <AzureIoTProtocol_MQTT.h>
#elif defined(IOT_CONFIG_HTTP)
    #include <AzureIoTProtocol_HTTP.h>
#endif

#ifdef ARDUINO_SAMD_FEATHER_M0
#define WINC_CS   8
#define WINC_IRQ  7
#define WINC_RST  4
#define WINC_EN   2
// Setup the WINC1500 connection with the pins above and the default hardware SPI.
Adafruit_WINC1500 WiFi(WINC_CS, WINC_IRQ, WINC_RST);
#endif

static char ssid[] = IOT_CONFIG_WIFI_SSID;
static char pass[] = IOT_CONFIG_WIFI_PASSWORD;

int status = WL_IDLE_STATUS;

/*******************************************************************************
 * Hall effect
 ******************************************************************************/
#define HALL_INTERRUPT_PIN 5

volatile byte door = LOW;

/*******************************************************************************
 * LED Strip
 ******************************************************************************/
#include <FastLED.h>
#define LED_TYPE APA102
#define NUM_LEDS 60
#define CLK_PIN A5
#define DATA_PIN A4
#define COLOR_ORDER BGR

CRGB leds[NUM_LEDS];
void (*updateLeds)(int);

/*******************************************************************************
 * Speaker
 ******************************************************************************/
 #define SPEAKER_PIN 10
int ledPin = 13;
int speakerOut = 10;               
byte names[] = { 'c', 'd', 'e', 'f', 'g', 'a', 'b', 'C' };  
int tones[] = { 1915, 1700, 1519, 1432, 1275, 1136, 1014, 956 };
byte melody[] = "2d2a1f2c2d2a2d2c2f2d2a2c2d2a1f2c2d2a2a2g2p8p8p8p";
// count length: 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0
//                                10                  20                  30
int count = 0;
int count2 = 0;
int count3 = 0;
int MAX_COUNT = 5;
int statePin = LOW;
void (*soundAlarm)(void);

/*******************************************************************************
 * Program
 ******************************************************************************/
void setup() {
    delay(1000);
    
    initSerial();

#ifdef WINC_EN
    pinMode(WINC_EN, OUTPUT);
    digitalWrite(WINC_EN, HIGH);
#endif
    
    initWifi();
    initTime();
    initHallEffect();
    initLedStrip();
    initSpeaker();
}

void loop() {
    // Run the Remote Monitoring from the Azure IoT Hub C SDK
    // You must set the device id, device key, IoT Hub name and IotHub suffix in
    // epifib_container.c
    epifib_container_run(&door, updateLeds, soundAlarm);
}

void initSerial() {
    // Start serial and initialize stdout
    Serial.begin(115200);
    //Serial.setDebugOutput(true);
}

void initWifi() {
    // Attempt to connect to Wifi network:
    Serial.print("\r\n\r\nAttempting to connect to SSID: ");
    Serial.println(ssid);

    // Connect to network
    if (strlen(pass) > 0)
    {
        status = WiFi.begin(ssid, pass);
    }
    else
    {
        status = WiFi.begin(ssid);
    }
    
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }

    Serial.println("\r\nConnected to wifi");
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initTime() {
#ifdef ARDUINO_SAMD_FEATHER_M0
    Adafruit_WINC1500UDP     _udp;
#elif defined(ARDUINO_SAMD_ZERO) || defined(ARDUINO_SAMD_MKR1000)
    WiFiUDP     _udp;
#endif

    time_t epochTime = (time_t)-1;

    NTPClient ntpClient;
    ntpClient.begin();

    while (true) {
        epochTime = ntpClient.getEpochTime("0.pool.ntp.org");

        if (epochTime == (time_t)-1) {
            Serial.println("Fetching NTP epoch time failed! Waiting 5 seconds to retry.");
            delay(5000);
        } else {
            Serial.print("Fetched NTP epoch time is: ");
            Serial.println(epochTime);
            break;
        }
    }
    
    ntpClient.end();

    struct timeval tv;
    tv.tv_sec = epochTime;
    tv.tv_usec = 0;

    settimeofday(&tv, NULL);
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initHallEffect() {
  pinMode(HALL_INTERRUPT_PIN, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(HALL_INTERRUPT_PIN), doorOpen, RISING);
}

void doorOpen() {
  door = !door;
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initLedStrip() {
  FastLED.addLeds<LED_TYPE, DATA_PIN, CLK_PIN, COLOR_ORDER>(leds, NUM_LEDS);
  updateLeds = &changeLeds;
}

void changeLeds(int num)
{
    int color;
    switch (num)
    {
       case 1:
          color = CRGB::Green;
          break;
       default:
          color = CRGB::Black;
          break;
    }

    for (int i=0; i<NUM_LEDS; i++)
    {
      leds[i] = color;
    }
    
    FastLED.show();
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initSpeaker() {
    //pinMode(SPEAKER_PIN, OUTPUT);
    soundAlarm = &playAlarm;
}

void playAlarm() {
    analogWrite(speakerOut, 0);     
    for (count = 0; count < MAX_COUNT; count++) {
      statePin = !statePin;
      digitalWrite(ledPin, statePin);
      for (count3 = 0; count3 <= (melody[count*2] - 48) * 30; count3++) {
        for (count2=0;count2<8;count2++) {
          if (names[count2] == melody[count*2 + 1]) {       
            analogWrite(speakerOut,500);
            delayMicroseconds(tones[count2]);
            analogWrite(speakerOut, 0);
            delayMicroseconds(tones[count2]);
          } 
          if (melody[count*2 + 1] == 'p') {
            // make a pause of a certain size
            analogWrite(speakerOut, 0);
            delayMicroseconds(500);
          }
        }
      }
    }
}

