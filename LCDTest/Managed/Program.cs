//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define LPC1768
//#define K64F
#define DISCO_F746NG

namespace Managed
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Windows.Devices.Gpio;
    using Microsoft.Zelig.Support.mbed;
    using Microsoft.Zelig.DISCO_F746NG;
    
    class Program
    {
        const UInt32 LCD_FB_START_ADDRESS = 0xC0000000;

        const UInt32 LCD_COLOR_BLUE          = 0xFF0000FF;
        const UInt32 LCD_COLOR_GREEN         = 0xFF00FF00;
        const UInt32 LCD_COLOR_RED           = 0xFFFF0000;
        const UInt32 LCD_COLOR_CYAN          = 0xFF00FFFF;
        const UInt32 LCD_COLOR_MAGENTA       = 0xFFFF00FF;
        const UInt32 LCD_COLOR_YELLOW        = 0xFFFFFF00;
        const UInt32 LCD_COLOR_LIGHTBLUE     = 0xFF8080FF;
        const UInt32 LCD_COLOR_LIGHTGREEN    = 0xFF80FF80;
        const UInt32 LCD_COLOR_LIGHTRED      = 0xFFFF8080;
        const UInt32 LCD_COLOR_LIGHTCYAN     = 0xFF80FFFF;
        const UInt32 LCD_COLOR_LIGHTMAGENTA  = 0xFFFF80FF;
        const UInt32 LCD_COLOR_LIGHTYELLOW   = 0xFFFFFF80;
        const UInt32 LCD_COLOR_DARKBLUE      = 0xFF000080;
        const UInt32 LCD_COLOR_DARKGREEN     = 0xFF008000;
        const UInt32 LCD_COLOR_DARKRED       = 0xFF800000;
        const UInt32 LCD_COLOR_DARKCYAN      = 0xFF008080;
        const UInt32 LCD_COLOR_DARKMAGENTA   = 0xFF800080;
        const UInt32 LCD_COLOR_DARKYELLOW    = 0xFF808000;
        const UInt32 LCD_COLOR_WHITE         = 0xFFFFFFFF;
        const UInt32 LCD_COLOR_LIGHTGRAY     = 0xFFD3D3D3;
        const UInt32 LCD_COLOR_GRAY          = 0xFF808080;
        const UInt32 LCD_COLOR_DARKGRAY      = 0xFF404040;
        const UInt32 LCD_COLOR_BLACK         = 0xFF000000;
        const UInt32 LCD_COLOR_BROWN         = 0xFFA52A2A;
        const UInt32 LCD_COLOR_ORANGE        = 0xFFFFA500;
        const UInt32 LCD_COLOR_TRANSPARENT   = 0xFF000000;


        static void Main()
        {
            var controller = GpioController.GetDefault();

            GpioPin pin = controller.OpenPin((int)PinName.LED1);

            // Start with all LEDs on.
            pin.Write(GpioPinValue.High);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            // just to make sure we are running
            for (int i = 0; i < 2; i++)
            {
                pin.Write(GpioPinValue.Low);
                Timer.wait_ms(100);
                pin.Write(GpioPinValue.High);
                Timer.wait_ms(100);
            }

            BSP_LCD_Init();

            BSP_LCD_LayerDefaultInit(0, LCD_FB_START_ADDRESS);
            BSP_LCD_LayerDefaultInit(1, LCD_FB_START_ADDRESS + (BSP_LCD_GetXSize() * BSP_LCD_GetYSize() * 4));

            BSP_LCD_DisplayOn();

            BSP_LCD_SelectLayer(0);
            BSP_LCD_Clear(LCD_COLOR_BLACK);

            BSP_LCD_SelectLayer(1);
            BSP_LCD_Clear(LCD_COLOR_BLACK);

            //BSP_LCD_SetFont(&LCD_DEFAULT_FONT);

            BSP_LCD_SetBackColor(LCD_COLOR_WHITE);
            BSP_LCD_SetTextColor(LCD_COLOR_DARKBLUE);

            Random r = new Random();

            while (true)
            {
                UInt32 red = (UInt32) (r.Next(255) << 16);
                UInt32 green = (UInt32) (r.Next(255) << 8);
                UInt32 blue = (UInt32) r.Next(255);
                
                BSP_LCD_SetTextColor(0xFF000000 | red | green | blue);
                BSP_LCD_FillRect((UInt16) r.Next(380), (UInt16)r.Next(170), 100, 100);
            }
        }

        [DllImport("C")]
        internal static extern byte BSP_LCD_Init();

        [DllImport("C")]
        internal static extern byte BSP_LCD_DeInit();

        [DllImport("C")]
        internal static extern void BSP_LCD_DisplayOn();

        [DllImport("C")]
        internal static extern void BSP_LCD_DisplayOff();

        [DllImport("C")]
        internal static extern void BSP_LCD_LayerDefaultInit(UInt16 layerIndex, UInt32 FBAddress);
        
        [DllImport("C")]
        internal static extern void BSP_LCD_SelectLayer(UInt32 layerIndex);

        [DllImport("C")]
        internal static extern void BSP_LCD_Clear(UInt32 color);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetFont(IntPtr font);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetFont(UInt32 color);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetBackColor(UInt32 color);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetTextColor(UInt32 color);

        [DllImport("C")]
        internal static extern void BSP_LCD_DrawPixel(UInt16 xpos, UInt16 ypos, UInt32 RGBCode);

        [DllImport("C")]
        internal static extern void BSP_LCD_FillRect(UInt16 xpos, UInt16 ypos, Int16 width, UInt16 geight);

        [DllImport("C")]
        internal static extern UInt32 BSP_LCD_GetXSize();

        [DllImport("C")]
        internal static extern UInt32 BSP_LCD_GetYSize();

    }
}
