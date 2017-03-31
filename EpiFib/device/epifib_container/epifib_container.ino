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
#endif /* ARDUINO_SAMD_FEATHER_M0 */

#include "epifib_container.h"
#include "NTPClient.h"
#include <AzureIoTHub.h>
#if defined(IOT_CONFIG_MQTT)
#include <AzureIoTProtocol_MQTT.h>
#elif defined(IOT_CONFIG_HTTP)
#include <AzureIoTProtocol_HTTP.h>
#endif /* defined(IOT_CONFIG_MQTT) */

#ifdef ARDUINO_SAMD_FEATHER_M0
#define WINC_CS   8
#define WINC_IRQ  7
#define WINC_RST  4
#define WINC_EN   2
// Setup the WINC1500 connection with the pins above and the default hardware SPI.
Adafruit_WINC1500 WiFi(WINC_CS, WINC_IRQ, WINC_RST);
#endif /* ARDUINO_SAMD_FEATHER_M0 */

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

/*******************************************************************************
 * Speaker
 ******************************************************************************/
 #define SPEAKER_PIN 13

/*******************************************************************************
 * Screen
 ******************************************************************************/
 
#include "SPI.h"
#include "Adafruit_GFX.h"
#include "Adafruit_ILI9341.h"
#include "TouchScreen.h"

#define TFT_RST 13
#define TFT_DC 12
#define TFT_CS 11
#define TFT_MOSI 10
#define TFT_MISO 9 
#define TFT_CLK 6 
#define YP A1
#define XM A2 
#define YM A3  
#define XP 11  
#define BOXSIZE 40
#define PENRADIUS 3
#define TFT_ORIENTATION 1
#define MINPRESSURE 10
#define MAXPRESSURE 1000

Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC, TFT_MOSI, TFT_CLK, TFT_RST, TFT_MISO);
TouchScreen ts = TouchScreen(XP, YP, XM, YM, 300);
int oldcolor, currentcolor;

/*******************************************************************************
 * Program
 ******************************************************************************/
void setup() {
    delay(1000);
    initSerial();

#ifdef WINC_EN
    pinMode(WINC_EN, OUTPUT);
    digitalWrite(WINC_EN, HIGH);
#endif /* WINC_EN */
    
    initScreen();
    initHallEffect();
    initLedStrip();
    initSpeaker();
    initWifi();
    initTime();
}

void loop()
{
    epifib_container_run(&door, &toggleLeds, &toggleSound, &toggleScreen, &toggleAlarm, &checkPress);
}

void initSerial()
{
    Serial.begin(115200);
    //Serial.setDebugOutput(true);
}

void initWifi()
{
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
    
    while (WiFi.status() != WL_CONNECTED)
    {
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
#endif /* ARDUINO_SAMD_FEATHER_M0 */

    time_t epochTime = (time_t)-1;

    NTPClient ntpClient;
    ntpClient.begin();

    while (true)
    {
        epochTime = ntpClient.getEpochTime("0.pool.ntp.org");

        if (epochTime == (time_t)-1)
        {
            Serial.println("Fetching NTP epoch time failed! Waiting 5 seconds to retry.");
            delay(5000);
        } else
        {
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
void initHallEffect() 
{
    pinMode(HALL_INTERRUPT_PIN, INPUT_PULLUP);
    attachInterrupt(digitalPinToInterrupt(HALL_INTERRUPT_PIN), doorOpen, RISING);
}

void doorOpen()
{
    door = !door;
    toggleLeds(door == 1 ? 1 : 0);
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initLedStrip()
{
    FastLED.addLeds<LED_TYPE, DATA_PIN, CLK_PIN, COLOR_ORDER>(leds, NUM_LEDS);
    toggleLeds(0);
}

void toggleLeds(int type)
{
    int color;
    switch (type)
    {
       case 1:
          color = CRGB::Red;
          break;
       case 2:
          color = CRGB::Green;
          break;
       case 3:
          color = CRGB::Blue;
          break;
       case 4:
          if (color != CRGB::Black)
          {
              color = CRGB::Black;
          }
          else
          {
              color = CRGB::Red;
          }
          
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
void initSpeaker()
{
    pinMode(SPEAKER_PIN, OUTPUT);
    toggleSound(0);
}

void toggleSound(int on)
{
    digitalWrite(SPEAKER_PIN, on ? HIGH : LOW);
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initScreen()
{
    tft.begin();
    uint8_t x = tft.readcommand8(ILI9341_RDMODE);
    Serial.print("Display Power Mode: 0x"); Serial.println(x, HEX);
    x = tft.readcommand8(ILI9341_RDMADCTL);
    Serial.print("MADCTL Mode: 0x"); Serial.println(x, HEX);
    x = tft.readcommand8(ILI9341_RDPIXFMT);
    Serial.print("Pixel Format: 0x"); Serial.println(x, HEX);
    x = tft.readcommand8(ILI9341_RDIMGFMT);
    Serial.print("Image Format: 0x"); Serial.println(x, HEX);
    x = tft.readcommand8(ILI9341_RDSELFDIAG);
    Serial.print("Self Diagnostic: 0x"); Serial.println(x, HEX);

    tft.setRotation(TFT_ORIENTATION);
    tft.fillScreen(ILI9341_BLACK);

    tft.setTextColor(ILI9341_RED);
    tft.setTextSize(5);
    tft.println("EpiFib");
}

void toggleScreen(int type)
{
    switch (type)
    {
        case 1:
            tft.setTextSize(1);
            tft.println("SOMEONE IS LOOKING FOR THIS BOX!");
            break;
        default:
            tft.setTextSize(1);
            tft.println("EpiFib");
    }
}

void checkPress()
{
    TSPoint p = ts.getPoint();
    
    // we have some minimum pressure we consider 'valid'
    // pressure of 0 means no pressing!
    if (p.z < MINPRESSURE || p.z > MAXPRESSURE)
    {
        return;
    } 
    else
    {
        toggleAlarm(0);
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////
void initAlarm()
{
}

void toggleAlarm(int type)
{
    toggleLeds(type);
    toggleSound(type);
    toggleScreen(type);
}

