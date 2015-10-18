using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Devices.Gpio;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;

namespace Managed.UI
{
    public class Application
    {
        /// <summary>
        /// How often screen should be updated
        /// </summary>
        private const float DeltaTime = 1.0f / 25.0f;

        /// <summary>
        /// How many pixels previous touch has to be within to be consider same touch
        /// </summary>
        private const int TouchTrackTolerance = 20;

        private STM32F7DiscoDisplay _display = new STM32F7DiscoDisplay();
        private STM32F7DiscoTouch _touch = new STM32F7DiscoTouch();

        private List<TouchInfo> _activeTouches = new List<TouchInfo>();

        /// <summary>
        /// Interface to LCD
        /// </summary>
        public STM32F7DiscoDisplay Display { get { return _display; } }

        /// <summary>
        /// Interface to Touch controller
        /// </summary>
        public STM32F7DiscoTouch Touch { get { return _touch; } }

        /// <summary>
        /// Show debug info on screen
        /// </summary>
        public bool ShowDebug { get; set; }
        
        public void Run(Control root)
        {
            var timer = new Timer();
            timer.start();

            // for fps info update
            float lastDebugTime = 0;
            float lastUpdateTime = 0;
            float currentTime;

            string infoString = "";

            while (true)
            {
                currentTime = timer.read();

                // fixed time step - both for update and draw
                if (currentTime - lastUpdateTime >= DeltaTime)
                {
                    int touches = Touch.GetTouchInfo();

                    for (int i = 0; i < touches; i++)
                    {
                        // near a touch we have seen recently ?
                        var previous = _activeTouches.Find(x => Math.Abs(x.Position.X - Touch.X[i]) < TouchTrackTolerance &&
                                                                Math.Abs(x.Position.Y - Touch.Y[i]) < TouchTrackTolerance);

                        if (previous == null)
                        {
                            // remember this touch
                            _activeTouches.Add(new TouchInfo { Position = new Point((int)Touch.X[i], (int)Touch.Y[i]), LastSeenTime = currentTime });

                            // new touch so send message to root control 
                            root.SendMessage(UIMessage.TouchStart, new TouchEventArgs(new Point((int)Touch.X[i], (int)Touch.Y[i])));
                        }
                        else
                        {
                            // touch seen again - update for tracking
                            previous.Position.X = (int)Touch.X[i];
                            previous.Position.Y = (int)Touch.Y[i];
                            previous.LastSeenTime = currentTime;
                        }
                    }

                    // get rid of old touches
                    for (int i = _activeTouches.Count - 1; i >= 0; i--)
                    {
                        if (currentTime - _activeTouches[i].LastSeenTime > 0.1f)
                        {
                            _activeTouches.RemoveAt(i);
                        }
                    }

                    lastUpdateTime = currentTime;                    

                    Display.Clear(STM32F7DiscoDisplay.LCD_COLOR_WHITE);

                    root.Update(DeltaTime);
                    root.Draw();

                    if (ShowDebug)
                    {
                        // show debug info every couple of seconds
                        if (timer.read_ms() - lastDebugTime > 2.0f)
                        {
                            infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                                Display.Fps,
                                Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                                Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                            lastDebugTime = timer.read();
                        }

                        Display.DrawString(infoString, 0, 0);
                    }

                    // show the back buffer
                    Display.Flip(false);
                }
            }
        }
    }
}
