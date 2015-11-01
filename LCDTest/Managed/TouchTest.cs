using System;
using Microsoft.Zelig.Support.mbed;
using Managed.Graphics;

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
            var canvas = new Canvas();
            var touch = new Touch();

            string infoString = "";

            while (true)
            {
                int touches = touch.GetTouchInfo();

                canvas.Clear(0xFFFFFFFF);

                for (int i = 0; i < touches; i++)
                {
                    canvas.DrawCircle((UInt16)Math.Max(50, Math.Min(480 - 50 - 1, touch.X[i])), (UInt16)Math.Max(50, Math.Min(272 - 50 - 1, touch.Y[i])), 50, 0xFFFF0000);
                }
                
                // show info every couple of seconds
                if (timer.read_ms() - lastTick > 2000)
                {
                    infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                        canvas.Fps,
                        Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                        Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                    lastTick = timer.read_ms();
                }

                //canvas.DrawString(infoString, 0, 0);

                // show the back buffer
                canvas.Flip();
            }
        }
    }
}
