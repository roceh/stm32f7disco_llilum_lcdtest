using System;
using Microsoft.Zelig.Support.mbed;
using Managed.Graphics;

namespace Managed
{
    /// <summary>
    /// Fade in/out
    /// </summary>
    public class PixelTest
    {
        public unsafe void Run()
        {
            var timer = new Timer();
            timer.start();

            // for fps info update
            int lastTick = timer.read_ms();
            int color = 0;
            int dir = 1;

            // create double buffered display
            var canvas = new Canvas();

            string infoString = "";

            while (true)
            {                
                // fill screen with pixels
                for (UInt16 x = 0; x < 480; x++)
                {
                    for (UInt16 y = 0; y < 272; y++)
                    {
                        canvas.SetPixel(x, y, (UInt32)(0xFF000000 | (color << 16)));
                    }
                }

                // show info every couple of seconds
                if (timer.read_ms() - lastTick > 2000)
                {
                    // string.format broken?
                    //infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                    //    display.Fps,
                    //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                    //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                    infoString = "FPS: " + canvas.Fps.ToString() + " MEMAVAIL: " + Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory.ToString();

                    lastTick = timer.read_ms();
                }

                //canvas.DrawString(infoString, 0, 0);

                // fade in/out the pixel fill
                color += dir;

                if (color == 255)
                {
                    dir = -1;
                }
                else if (color == 0)
                {
                    dir = +1;
                }

                // show the back buffer
                canvas.Flip();
            }
        }
    }
}
