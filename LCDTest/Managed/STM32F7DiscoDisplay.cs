using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;

namespace Managed
{
    /// <summary>
    /// Interface to the LCD on the STM32F7 disco board
    /// </summary>
    public unsafe class STM32F7DiscoDisplay
    {
        private const UInt32 LCD_FB_START_ADDRESS = 0xC0000000;

        public const UInt32 LCD_COLOR_BLUE = 0xFF0000FF;
        public const UInt32 LCD_COLOR_GREEN = 0xFF00FF00;
        public const UInt32 LCD_COLOR_RED = 0xFFFF0000;
        public const UInt32 LCD_COLOR_CYAN = 0xFF00FFFF;
        public const UInt32 LCD_COLOR_MAGENTA = 0xFFFF00FF;
        public const UInt32 LCD_COLOR_YELLOW = 0xFFFFFF00;
        public const UInt32 LCD_COLOR_LIGHTBLUE = 0xFF8080FF;
        public const UInt32 LCD_COLOR_LIGHTGREEN = 0xFF80FF80;
        public const UInt32 LCD_COLOR_LIGHTRED = 0xFFFF8080;
        public const UInt32 LCD_COLOR_LIGHTCYAN = 0xFF80FFFF;
        public const UInt32 LCD_COLOR_LIGHTMAGENTA = 0xFFFF80FF;
        public const UInt32 LCD_COLOR_LIGHTYELLOW = 0xFFFFFF80;
        public const UInt32 LCD_COLOR_DARKBLUE = 0xFF000080;
        public const UInt32 LCD_COLOR_DARKGREEN = 0xFF008000;
        public const UInt32 LCD_COLOR_DARKRED = 0xFF800000;
        public const UInt32 LCD_COLOR_DARKCYAN = 0xFF008080;
        public const UInt32 LCD_COLOR_DARKMAGENTA = 0xFF800080;
        public const UInt32 LCD_COLOR_DARKYELLOW = 0xFF808000;
        public const UInt32 LCD_COLOR_WHITE = 0xFFFFFFFF;
        public const UInt32 LCD_COLOR_LIGHTGRAY = 0xFFD3D3D3;
        public const UInt32 LCD_COLOR_GRAY = 0xFF808080;
        public const UInt32 LCD_COLOR_DARKGRAY = 0xFF404040;
        public const UInt32 LCD_COLOR_BLACK = 0xFF000000;
        public const UInt32 LCD_COLOR_BROWN = 0xFFA52A2A;
        public const UInt32 LCD_COLOR_ORANGE = 0xFFFFA500;
        public const UInt32 LCD_COLOR_TRANSPARENT = 0xFF000000;

        /// <summary>
        /// Found any FPS around 30fps would cause glitches - i.e. the controller
        /// had not finnished dma'ing out the previous buffer to the LCD
        /// </summary>
        private const int MinFrameTime = 1000 / 25;

        /// <summary>
        /// Width of the screen
        /// </summary>
        private const int ScreenWidth = 480;

        /// <summary>
        /// Height of the screen
        /// </summary>
        private const int ScreenHeight = 272;

        private int _lastFlipTime = 0;
        private int _activeBuffer = 0;
        private int _backBuffer = 1;
        private UInt32[] _buffers;
        private Timer _timer;
        private int _fps = 0;
        private int _inProgressFps = 0;
        private int _lastFpsTrackTick = 0;

        public STM32F7DiscoDisplay()
        {
            BSP_LCD_Init();

            _buffers = new UInt32[] { LCD_FB_START_ADDRESS, LCD_FB_START_ADDRESS + (ScreenWidth * ScreenHeight * 4) };

            _activeBuffer = 0;
            _backBuffer = 1;

            BSP_LCD_LayerDefaultInit(0, _buffers[0]);

            // clear both buffers
            BSP_LCD_SetDrawAddress(_buffers[_activeBuffer]);
            BSP_LCD_Clear(LCD_COLOR_BLACK);
            BSP_LCD_SetDrawAddress(_buffers[_backBuffer]);
            BSP_LCD_Clear(LCD_COLOR_BLACK);

            BSP_LCD_DisplayOn();

            var font16 = BSP_LCD_GetFontBySize(16);
            BSP_LCD_SetFont(font16);

            BSP_LCD_SetBackColor(LCD_COLOR_WHITE);
            BSP_LCD_SetTextColor(LCD_COLOR_DARKBLUE);

            _timer = new Timer();
            _timer.start();
        }

        public UInt32 GetBackBufferAddress()
        {
            return _buffers[_backBuffer];
        }

        /// <summary>
        /// Draw a string at the given position
        /// </summary>
        /// <param name="text">text string</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public void DrawString(string text, int x, int y)
        {
            var textArray = new byte[text.Length + 1];

            for (int i = 0; i < text.Length; i++)
            {
                textArray[i] = (byte)text[i];
            }

            textArray[text.Length] = 0;

            fixed (byte* textPointer = textArray)
            {
                BSP_LCD_DisplayStringAt((UInt16)x, (UInt16)y, textPointer, 0);
            }
        }

        /// <summary>
        /// Draw a string at the given position
        /// </summary>
        /// <param name="text">text string</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public void DrawCenteredString(string text, int x, int y, int width, int height)
        {
            var fWidth = BSP_LCD_GetFontWidth();
            var fHeight = BSP_LCD_GetFontHeight();

            var textArray = new byte[text.Length + 1];

            for (int i = 0; i < text.Length; i++)
            {
                textArray[i] = (byte)text[i];
            }

            textArray[text.Length] = 0;

            fixed (byte* textPointer = textArray)
            {
                BSP_LCD_DisplayStringAt((UInt16)(x + (width/2) - ((fWidth * text.Length)/2)), (UInt16)(y + (height / 2) - (fHeight / 2)), textPointer, 0);
            }
        }

        /// <summary>
        /// Set pixel at the given x and y
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="color">color to set pixel</param>
        public void SetPixel(UInt16 x, UInt16 y, UInt32 color)
        {            
            *((UInt32*)(_buffers[_backBuffer] + (4 * (y * ScreenWidth + x)))) = color;
        }

        /// <summary>
        /// Draws a circle at the given position
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="radius">radius of circle</param>
        public void DrawCircle(int x, int y, int radius)
        {
            BSP_LCD_DrawCircle((UInt16)x, (UInt16)y, (UInt16)radius);
        }

        /// <summary>
        /// Draw rectangle at the given position
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        public void DrawRectangle(int x, int y, int width, int height)
        {
            BSP_LCD_DrawRect((UInt16) x, (UInt16)y, (UInt16)width, (UInt16)height);
        }

        /// <summary>
        /// Clear the screen with given color
        /// </summary>
        /// <param name="color">color to clear with</param>
        public void Clear(UInt32 color)
        {
            BSP_LCD_Clear(color);
        }

        /// <summary>
        /// Flip the back and active buffers
        /// </summary>
        /// <param name="lockFps">fix fps at 25fps</param>
        public void Flip(bool lockFps)
        {
            _inProgressFps++;

            var currentTick = _timer.read_ms();

            // calculate the fps
            if (currentTick - _lastFpsTrackTick >= 1000)
            {
                _lastFpsTrackTick = currentTick;

                _fps = _inProgressFps;

                _inProgressFps = 0;
            }

            // flip the active and back buffers
            _activeBuffer = (_activeBuffer == 0) ? 1 : 0;
            _backBuffer = (_backBuffer == 0) ? 1 : 0;

            // i believe this does not actually switch the address until any current dma'ing has finished
            BSP_LCD_SetLayerAddress(0, _buffers[_activeBuffer]);
            BSP_LCD_SetDrawAddress(_buffers[_backBuffer]);

            // wait a bit to maintain FPS
            while (lockFps && _timer.read_ms() - _lastFlipTime < MinFrameTime)
            { }

            // remember last time we completed the flip
            _lastFlipTime = _timer.read_ms();
        }

        /// <summary>
        /// Current fps - will be zero for first second
        /// </summary>
        public int Fps { get { return _fps; } }
        
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
        internal static extern void BSP_LCD_LayerRgb565Init(UInt16 LayerIndex, UInt32 FB_Address);

        [DllImport("C")]
        internal static extern void BSP_LCD_SelectLayer(UInt32 layerIndex);

        [DllImport("C")]
        internal static extern void BSP_LCD_Clear(UInt32 color);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetFont(IntPtr font);

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

        [DllImport("C")]
        internal static extern unsafe void BSP_LCD_DisplayStringAt(UInt16 xpos, UInt16 ypos, byte* text, byte mode);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetLayerAddress(UInt32 layerIndex, UInt32 address);

        [DllImport("C")]
        internal static extern void BSP_LCD_DrawCircle(UInt16 xpos, UInt16 ypos, UInt16 Radius);

        [DllImport("C")]
        internal static extern void BSP_LCD_SetDrawAddress(UInt32 address);

        [DllImport("C")]
        internal static extern void BSP_LCD_DrawRect(UInt16 xpos, UInt16 ypos, UInt16 width, UInt16 height);

        [DllImport("C")]
        internal static extern UInt16 BSP_LCD_GetFontWidth();

        [DllImport("C")]
        internal static extern UInt16 BSP_LCD_GetFontHeight();

        [DllImport("C")]
        internal static extern IntPtr BSP_LCD_GetFontBySize(int size);
    }
}
