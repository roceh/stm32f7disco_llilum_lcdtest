using System;
using System.Collections.Generic;
using Microsoft.Zelig.Support.mbed;
using Managed.Graphics;
using Managed.SDCard;

namespace Managed
{
    /// <summary>
    /// Drawing bitmaps
    /// </summary>
    public class BitmapTest
    {
        private Bitmap _background;
        private Bitmap _sprite;
        private Font _font;

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
                        
            string infoString = "";

            SDCardManager.Mount();

            _background = new Bitmap("BACK.DAT", 480, 272);
            _sprite = new Bitmap("SPRITE.DAT", 64, 64);
            _font = Font.LoadFromFile("DEJAVU.FNT");

            // create double buffered display
            var canvas = new Canvas();

            var sprites = new List<Sprite>();

            var r = new Random();

            for (int i = 0; i < 20; i++)
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
                canvas.Clear(_background);

                foreach (var sprite in sprites)
                {
                    canvas.DrawBitmap(_sprite, 0, 0, sprite.X, sprite.Y, 64, 64);
                    sprite.Step(Canvas.ScreenWidth, Canvas.ScreenHeight);
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
                                
                canvas.DrawString(infoString, 0, 0, _font);

                // show the back buffer
                canvas.Flip();
            }
        }
    }
}
