using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Devices.Gpio;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;

namespace Managed
{
    /// <summary>
    /// Drawing bitmaps
    /// </summary>
    public class BitmapTest
    {
        private DiscoBitmap _background;
        private DiscoBitmap _sprite;

        public class Sprite
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int Dx;
            public int Dy;
            
            public void Step(int sw, int sh)
            {
                X += Dx;
                Y += Dy;

                if (X < 0 || X + Width > sw)
                {
                    Dx = -Dx;
                    X = Math.Min(Math.Max(0, X), sw - Width);
                }

                if (Y < 0 || Y + Height > sh)
                {
                    Dy = -Dy;
                    Y = Math.Min(Math.Max(0, Y), sh - Height);
                }
            }
        }

        public unsafe void Run()
        {
            var timer = new Timer();
            timer.start();

            // for fps info update
            int lastTick = timer.read_ms();

            // create double buffered display
            var display = new STM32F7DiscoDisplay();

            string infoString = "";

            SDCard.Mount();

            _background = new DiscoBitmap("BACK.DAT", 480, 272);
            _sprite = new DiscoBitmap("SPRITE.DAT", 64, 64);


            var sprites = new List<Sprite>();

            var r = new Random();

            for (int i = 0; i < 10; i++)
            {
                sprites.Add(new Sprite
                {
                    X = r.Next(480 - 64),
                    Y = r.Next(272 - 64),
                    Width = (int)_sprite.Width,
                    Height = (int)_sprite.Height,
                    Dx = r.Next(2) == 0 ? 2 : -2,
                    Dy = r.Next(2) == 0 ? 2 : -2
                });
            }

            while (true)
            {
                display.Clear(_background);

                foreach (var sprite in sprites)
                {
                    display.DrawBitmap(_sprite, 0, 0, sprite.X, sprite.Y, 64, 64);
                    sprite.Step(STM32F7DiscoDisplay.ScreenWidth, STM32F7DiscoDisplay.ScreenHeight);
                }

                // show info every couple of seconds
                if (timer.read_ms() - lastTick > 2000)
                {
                    // string.format broken?
                    //infoString = String.Format("FPS: {0} MEMAVAIL: {1} MEMALOC: {2}",
                    //    display.Fps,
                    //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory,
                    //    Microsoft.Zelig.Runtime.MemoryManager.Instance.AllocatedMemory);

                    infoString = "FPS: " + display.Fps.ToString();
                    //+ " MEMAVAIL: " + Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory.ToString();

                    lastTick = timer.read_ms();
                }

                display.DrawString(infoString, 0, 0);

                // show the back buffer - true to lock fps
                display.Flip(true);
            }
        }
    }
}
