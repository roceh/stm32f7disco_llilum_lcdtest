﻿using System;
using System.Collections.Generic;
using Microsoft.Zelig.Support.mbed;
using Managed.Graphics;
using Managed.SDCard;

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
        private const int TouchTrackTolerance = 50;

        /// <summary>
        /// Used to draw to the LCD screen
        /// </summary>
        private Canvas _canvas = new Canvas();

        /// <summary>
        /// Used to detect touch events
        /// </summary>
        private Touch _touch = new Touch();

        /// <summary>
        /// Default system font
        /// </summary>
        private Font _systemFont;

        /// <summary>
        /// List of touches current being tracked
        /// </summary>
        private List<TouchInfo> _activeTouches = new List<TouchInfo>();

        /// <summary>
        /// Counter for touch id
        /// </summary>
        private int _nextTouchId = 0;

        /// <summary>
        /// Default system font
        /// </summary>
        public Font SystemFont { get { return _systemFont;  } }

        /// <summary>
        /// Interface to LCD
        /// </summary>
        public Canvas Display { get { return _canvas; } }

        /// <summary>
        /// Interface to Touch controller
        /// </summary>
        public Touch Touch { get { return _touch; } }

        /// <summary>
        /// Show debug info on screen
        /// </summary>
        public bool ShowDebug { get; set; }

        /// <summary>
        /// Current time - really should be a double - but mbed = float :/
        /// </summary>
        public float FrameTime { get; set; }
        
        public void Run(Control root)
        {
            SDCardManager.Mount();

            // load in the system font
            _systemFont = Font.LoadFromFile("DEJAVU.FNT");

            var timer = new Timer();
            timer.start();

            // for fps info update
            float lastDebugTime = 0;
            float lastUpdateTime = 0;

            string infoString = "";

            while (true)
            {
                FrameTime = timer.read();

                // fixed time step - both for update and draw
                if (FrameTime - lastUpdateTime >= DeltaTime)
                {
                    int touches = Touch.GetTouchInfo();

                    for (int i = 0; i < touches; i++)
                    {
                        // near a touch we have seen recently ?
                        var previous = _activeTouches.Find(x => Math.Abs(x.Position.X - Touch.X[i]) < TouchTrackTolerance &&
                                                                Math.Abs(x.Position.Y - Touch.Y[i]) < TouchTrackTolerance);

                        if (previous == null)
                        {
                            _nextTouchId++;

                            // remember this touch
                            _activeTouches.Add(new TouchInfo { Position = new Point((int)Touch.X[i], (int)Touch.Y[i]), LastSeenTime = FrameTime, Id = _nextTouchId });

                            // new touch so send message to root control 
                            root.SendMessage(UIMessage.TouchStart, new TouchEventArgs(_nextTouchId, new Point((int)Touch.X[i], (int)Touch.Y[i])));
                        }
                        else
                        {
                            bool hasMoved = (Math.Abs(previous.Position.X - Touch.X[i]) > 0 || Math.Abs(previous.Position.Y - Touch.Y[i]) > 0);

                            // touch seen again - update for tracking
                            previous.Position = new Point((int)Touch.X[i], (int)Touch.Y[i]);
                            previous.LastSeenTime = FrameTime;

                            // moved at all ?
                            if (hasMoved)
                            {
                                root.SendMessage(UIMessage.TouchMove, new TouchEventArgs(previous.Id, previous.Position));
                            }
                        }
                    }

                    // get rid of old touches
                    for (int i = _activeTouches.Count - 1; i >= 0; i--)
                    {
                        if (FrameTime - _activeTouches[i].LastSeenTime > 0.1f)
                        {
                            root.SendMessage(UIMessage.TouchEnd, new TouchEventArgs(_activeTouches[i].Id, _activeTouches[i].Position));

                            _activeTouches.RemoveAt(i);
                        }
                    }

                    lastUpdateTime = FrameTime;                    

                    Display.Clear(0xFFFFFFFF);

                    root.Update(DeltaTime);
                    root.Draw();

                    if (ShowDebug)
                    {
                        // show debug info every couple of seconds
                        if (timer.read_ms() - lastDebugTime > 2.0f)
                        {
                            // string.format broken?
                            //infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                            //    Display.Fps,
                            //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                            //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                            infoString = "FPS: " + Display.Fps.ToString() + " MEMAVAIL: " + Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory.ToString();

                            lastDebugTime = timer.read();
                        }

                        //Display.DrawString(infoString, 0, 0);
                    }

                    // show the back buffer
                    Display.Flip();
                }
            }
        }
    }
}
