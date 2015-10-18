using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Devices.Gpio;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;

namespace Managed
{
    /// <summary>
    /// Touch scren test
    /// </summary>
    public class TouchTest
    {
        public void Run()
        {  
            var timer = new Timer();
            timer.start();

            // for fps info update
            int lastTick = timer.read_ms();

            // create double buffered display
            var display = new STM32F7DiscoDisplay();
            var touch = new STM32F7DiscoTouch();

            string infoString = "";

            while (true)
            {
                int touches = touch.GetTouchInfo();

                display.Clear(STM32F7DiscoDisplay.LCD_COLOR_GREEN);

                for (int i = 0; i < touches; i++)
                {
                    display.DrawCircle((UInt16)Math.Max(50, Math.Min(480 - 50 - 1, touch.X[i])), (UInt16)Math.Max(50, Math.Min(272 - 50 - 1, touch.Y[i])), 50);
                }
                
                // show info every couple of seconds
                if (timer.read_ms() - lastTick > 2000)
                {
                    infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                        display.Fps,
                        Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                        Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                    lastTick = timer.read_ms();
                }

                display.DrawString(infoString, 0, 0);

                // show the back buffer - true to lock fps
                display.Flip(true);
            }
        }
    }
}
