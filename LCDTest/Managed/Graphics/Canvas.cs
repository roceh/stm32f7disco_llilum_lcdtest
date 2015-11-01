using System;
using Microsoft.Zelig.Support.mbed;

namespace Managed.Graphics
{
    /// <summary>
    /// Interface to draw on the frame buffer
    /// </summary>
    public unsafe class Canvas
    {
        /// <summary>
        /// Location of the framebuffers
        /// </summary>
        private const UInt32 FrameBufferBase = 0xC0000000;

        /// <summary>
        /// Found any FPS around 30fps would cause glitches - i.e. the controller
        /// had not finnished dma'ing out the previous buffer to the LCD
        /// </summary>
        private const int MinFrameTime = 1000 / 25;

        /// <summary>
        /// Width of the screen
        /// </summary>
        public const int ScreenWidth = 480;

        /// <summary>
        /// Height of the screen
        /// </summary>
        public const int ScreenHeight = 272;
        
        /// <summary>
        /// Current index in buffer array that is address of screen being dma'd to lcd
        /// </summary>
        private int _activeBuffer = 0;

        /// <summary>
        /// Current index in buffer array that is address that is being drawn to 
        /// </summary>
        private int _backBuffer = 1;

        /// <summary>
        /// List of address for two screen buffers
        /// </summary>
        private UInt32[] _buffers;

        /// <summary>
        /// Timer to measure fps
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Frames per second being achieved
        /// </summary>
        private int _fps = 0;

        /// <summary>
        /// Fps being calculated
        /// </summary>
        private int _inProgressFps = 0;

        /// <summary>
        /// Used to calculate when we have reached a second for fps
        /// </summary>
        private int _lastFpsTrackTimeMs = 0;

        /// <summary>
        /// Clipping rectangle of screen
        /// </summary>
        private Rect _screenClip;

        /// <summary>
        /// Current clipping rectangle
        /// </summary>
        private Rect _clip;

        /// <summary>
        /// Current fps - will be zero for first second
        /// </summary>
        public int Fps { get { return _fps; } }

        /// <summary>
        /// Initialise the class
        /// </summary>
        public Canvas()
        {
            _buffers = new UInt32[] { FrameBufferBase, FrameBufferBase + (ScreenWidth * ScreenHeight * 4) };

            _activeBuffer = 0;
            _backBuffer = 1;

            DisplayInterop.LCD_Init((UInt32*)_buffers[_activeBuffer], (UInt32*)_buffers[_backBuffer]);

            // initial clip
            _screenClip = new Rect(0, 0, ScreenWidth, ScreenHeight);
            _clip = _screenClip;

            _timer = new Timer();
            _timer.start();
        }

        /// <summary>
        /// Get address of the current back buffer
        /// </summary>
        public UInt32 GetBackBufferAddress()
        {
            return _buffers[_backBuffer];
        }

        /// <summary>
        /// Set pixel at the given x and y
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="color">color to set pixel</param>
        public void SetPixel(int x, int y, UInt32 color)
        {
            *((UInt32*)(_buffers[_backBuffer] + (4 * (y * ScreenWidth + x)))) = color;
        }
        
        /// <summary>
        /// Clear the screen with given color
        /// </summary>
        /// <param name="color">color to clear with</param>
        public void Clear(UInt32 color)
        {
            UInt32* back = (UInt32*)GetBackBufferAddress();

            UInt32 pixels = ScreenWidth * ScreenHeight;

            for (UInt32 i = 0; i < pixels; i++)
            {
                *back++ = color;
            }
        }

        /// <summary>
        /// Calculate size of string
        /// </summary>
        /// <param name="text">text to calculate size of</param>
        /// <param name="font">font to use to calculate size</param>
        /// <returns>size of string</returns>
        public Size MeasureString(string text, Font font)
        {
            int maxWidth = 0, maxHeight = 0;

            Font.Character fontCharacter;

            int spacingFromPrevious;

            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    // first chracter no spacing
                    spacingFromPrevious = 0;
                }
                //else if (font.Kernings.TryGetValue(new Font.Kerning(text[i - 1], text[i]), out spacingFromPrevious))
                else
                {
                    // no kerning found - default to 1 pixel
                    spacingFromPrevious = 1;
                }

                maxWidth += spacingFromPrevious;

                if (font.Characters.TryGetValue(text[i], out fontCharacter))
                {
                    maxWidth += fontCharacter.Rect.Width;
                    maxHeight = Math.Max(maxHeight, fontCharacter.Offset.Y + fontCharacter.Rect.Height);
                }
            }

            return new Size(maxWidth, maxHeight);
        }

        /// <summary>
        /// Draw a text string 
        /// </summary>
        /// <param name="text">text string to draw</param>
        /// <param name="x">x position to draw</param>
        /// <param name="y">y position to draw</param>
        /// <param name="font">font to draw text with</param>
        public void DrawString(string text, int x, int y, Font font)
        {
            Font.Character fontCharacter;
            Font.Page fontPage;
            int spacingFromPrevious;

            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    // first chracter no spacing
                    spacingFromPrevious = 0;
                }
                //else if (font.Kernings.TryGetValue(new Font.Kerning(text[i - 1], text[i]), out spacingFromPrevious))
                else
                {
                    // no kerning found - default to 1 pixel
                    spacingFromPrevious = 1;
                }

                // adjust the spacing to draw the character
                x += spacingFromPrevious;

                // character found ?
                if (font.Characters.TryGetValue(text[i], out fontCharacter))
                {
                    if (font.Pages.TryGetValue(fontCharacter.Page, out fontPage))
                    {
                        DrawBitmap(fontPage.Texture,
                            fontCharacter.Rect.X, fontCharacter.Rect.Y, x, y + fontCharacter.Offset.Y,
                            fontCharacter.Rect.Width, fontCharacter.Rect.Height);                       
                    }

                    x += fontCharacter.Rect.Width;
                }
            }
        }

        public void DrawCenteredString(string text, int x, int y, int width, int height, Font font)
        {
            Size size = MeasureString(text, font);
            DrawString(text, x + (width / 2) - (size.Width / 2), y + (height / 2) - (size.Height / 2), font);
        }

        public void DrawCircle(int x, int y, int radius, UInt32 color)
        {
            int xx = radius;
            int yy = 0;
            int decisionOver2 = 1 - xx;   // Decision criterion divided by 2 evaluated at x=r, y=0

            while (yy <= xx)
            {
                SetPixel(xx + x, yy + y, color); // Octant 1
                SetPixel(yy + x, xx + y, color); // Octant 2
                SetPixel(-xx + x, yy + y, color); // Octant 4
                SetPixel(-yy + x, xx + y, color); // Octant 3
                SetPixel(-xx + x, -yy + y, color); // Octant 5
                SetPixel(-yy + x, -xx + y, color); // Octant 6
                SetPixel(xx + x, -yy + y, color); // Octant 8
                SetPixel(yy + x, -xx + y, color); // Octant 7
                yy++;
                if (decisionOver2 <= 0)
                {
                    decisionOver2 += 2 * yy + 1;   // Change in decision criterion for y -> y+1
                }
                else
                {
                    xx--;
                    decisionOver2 += 2 * (yy - xx) + 1;   // Change for y -> y+1, x -> x-1
                }
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, UInt32 color)
        {
            DrawLine(x, y, x + width - 1, y, color);
            DrawLine(x + width - 1, y, x + width - 1, y + height - 1, color);
            DrawLine(x + width - 1, y + height - 1, x, y + height - 1, color);
            DrawLine(x, y + height - 1, x, y, color);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, UInt32 color)
        {
            var clippedLine = CohenSutherland.CohenSutherlandLineClip(_clip, x1, y1, x2, y2);

            if (clippedLine != null && clippedLine.Count == 2)
            {
                int x3 = clippedLine[0].X;
                int y3 = clippedLine[0].Y;
                int x4 = clippedLine[1].X;
                int y4 = clippedLine[1].Y;

                int w = x4 - x3;
                int h = y4 - y3;
                int dx3 = 0, dy3 = 0, dx4 = 0, dy4 = 0;
                if (w < 0) dx3 = -1; else if (w > 0) dx3 = 1;
                if (h < 0) dy3 = -1; else if (h > 0) dy3 = 1;
                if (w < 0) dx4 = -1; else if (w > 0) dx4 = 1;
                int longest = Math.Abs(w);
                int shortest = Math.Abs(h);
                if (!(longest > shortest))
                {
                    longest = Math.Abs(h);
                    shortest = Math.Abs(w);
                    if (h < 0) dy4 = -1; else if (h > 0) dy4 = 1;
                    dx4 = 0;
                }
                int numerator = longest >> 1;
                for (int i = 0; i <= longest; i++)
                {
                    SetPixel(x3, y3, color);
                    numerator += shortest;
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        x3 += dx3;
                        y3 += dy3;
                    }
                    else
                    {
                        x3 += dx4;
                        y3 += dy4;
                    }
                }
            }
        }

        /// <summary>
        /// Clear the screen with given bitmap 
        /// </summary>
        /// <param name="bitmap">bitmap to clear with</param>
        public void Clear(Bitmap bitmap)
        {
            if (bitmap.Width != ScreenWidth || bitmap.Height != ScreenHeight)
            {
                throw new Exception("Bitmap has to match screen size");
            }
            
            int pixels = bitmap.Width * bitmap.Height;
            
            fixed (byte* ptr = bitmap.Data)
            {
                UInt32* back = (UInt32*)GetBackBufferAddress();
                UInt32* pixel = (UInt32*)ptr;

                UInt32 i;

                for (i = 0; i < pixels; i++)
                {
                    *back = *pixel;

                    // *back++ = *pixel++ bugged?
                    back++;
                    pixel++;
                }
            }
        }

        /// <summary>
        /// Alpha blend the two ARGB8888 colors together
        /// </summary>
        /// <param name="p1">background</param>
        /// <param name="p2">foreground</param>
        /// <returns>blended color</returns>
        private UInt32 AlphaBlend(UInt32 p1, UInt32 p2)
        {
            const UInt32 AMASK = 0xFF000000;
            const UInt32 RBMASK = 0x00FF00FF;
            const UInt32 GMASK = 0x0000FF00;
            const UInt32 AGMASK = AMASK | GMASK;
            const UInt32 ONEALPHA = 0x01000000;
            UInt32 a = (p2 & AMASK) >> 24;

            if (a == 255)
            { return p2; }

            if (a == 0)
            { return p1; }
            
            UInt32 na = 255 - a;
            UInt32 rb = ((na * (p1 & RBMASK)) + (a * (p2 & RBMASK))) >> 8;
            UInt32 ag = (na * ((p1 & AGMASK) >> 8)) + (a * (ONEALPHA | ((p2 & GMASK) >> 8)));
            return ((rb & RBMASK) | (ag & AGMASK));
        }


        /// <summary>
        /// Set the clipping rectangle
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void SetClip(int x, int y, int width, int height)
        {
            int sx = 0, sy = 0;
            
            // make sure we are still in the screen clip rect
            Clip(ref x, ref width, ref sx, _screenClip.X, _screenClip.Width);
            Clip(ref y, ref height, ref sy, _screenClip.Y, _screenClip.Height);

            _clip.X = x;
            _clip.Y = y;
            _clip.Width = width;
            _clip.Height = height;
        }

        /// <summary>
        /// Reset clipping rect to screen boundary
        /// </summary>
        public void ResetClip()
        {
            _clip.X = 0;
            _clip.Y = 0;
            _clip.Width = ScreenWidth;
            _clip.Height = ScreenHeight;
        }
        
        /// <summary>
        /// Clip the cordinates
        /// </summary>
        /// <param name="dmin">destination min</param>
        /// <param name="dsize">destination size</param>
        /// <param name="smin">source min</param>
        /// <param name="cmin">clipping min</param>
        /// <param name="csize">clipping size</param>
        private void Clip(ref int dmin, ref int dsize, ref int smin, int cmin, int csize)
        {
            int dmax = dmin + dsize - 1;
            int cmax = cmin + csize - 1;

            if (dmin < cmin)
            {
                smin += cmin - dmin;
                dmin = cmin;
            }

            dmax = Math.Min(dmax, cmax);

            dsize = dmax - dmin + 1;
        }

        /// <summary>
        /// Draws a bitmap to the screen
        /// </summary>
        /// <param name="bitmap">bitmap to draw</param>
        /// <param name="sx">source x</param>
        /// <param name="sy">source y</param>
        /// <param name="dx">destination x</param>
        /// <param name="dy">destination y</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void DrawBitmap(Bitmap bitmap, int sx, int sy, int dx, int dy, int width, int height)
        {
            // clip rectangles
            Clip(ref dx, ref width, ref sx, _clip.X, _clip.Width);
            Clip(ref dy, ref height, ref sy, _clip.Y, _clip.Height);

            if (width > 0 && height > 0)
            {
                fixed (byte* ptr = bitmap.Data)
                {
                    DisplayInterop.LCD_DrawImageWithAlpha((UInt32*)ptr, sx, sy, bitmap.Width, dx, dy, width, height);
                }

                /*fixed (byte* ptr = bitmap.Data)
                {
                    UInt32* backBuffer = (UInt32*)GetBackBufferAddress();

                    UInt32* source = (UInt32*)ptr + (sx + sy * bitmap.Width);
                    UInt32* destination = backBuffer + (dx + dy * ScreenWidth);

                    int sourceRemain = bitmap.Width - width;
                    int destinationRemain = ScreenWidth - width;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *destination = AlphaBlend(*destination, *source);
                            destination++;
                            source++;
                        }

                        source += sourceRemain;
                        destination += destinationRemain;
                    }
                }*/
            }
        }

        /// <summary>
        /// Flip the back and active buffers
        /// </summary>
        /// <param name="lockFps">fix fps at 25fps</param>
        public void Flip()
        {
            _inProgressFps++;

            var currentTick = _timer.read_ms();

            // calculate the fps
            if (currentTick - _lastFpsTrackTimeMs >= 1000)
            {
                _lastFpsTrackTimeMs = currentTick;

                _fps = _inProgressFps;

                _inProgressFps = 0;
            }

            // flip the active and back buffers
            _activeBuffer = (_activeBuffer == 0) ? 1 : 0;
            _backBuffer = (_backBuffer == 0) ? 1 : 0;

            // i believe this does not actually switch the address until any current dma'ing has finished
            DisplayInterop.LCD_SetActiveAddress((UInt32*) _buffers[_activeBuffer]);
            DisplayInterop.LCD_SetDrawAddress((UInt32*) _buffers[_backBuffer]);
        }

    }
}
