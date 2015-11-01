using System;

namespace Managed.Graphics
{
    public unsafe class Touch
    {
        /// <summary>
        /// Width of the screen
        /// </summary>
        private const int ScreenWidth = 480;

        /// <summary>
        /// Height of the screen
        /// </summary>
        private const int ScreenHeight = 272;

        private UInt32[] _x = new UInt32[5];
        private UInt32[] _y = new UInt32[5];

        public Touch()
        {
            DisplayInterop.BSP_TS_Init(ScreenWidth, ScreenHeight);
        }
                
        /// <summary>
        /// Returns number of touches detected
        /// </summary>
        /// <returns>number of touches</returns>
        public byte GetTouchInfo()
        {
            byte touches;

            fixed (UInt32* x = _x)
            fixed (UInt32* y = _y)
            {
                touches = DisplayInterop.BSP_TS_GetTouchInfo(x, y);
            }

            return touches;
        }

        /// <summary>
        /// X values of touch
        /// </summary>
        public UInt32[] X { get { return _x; } }

        /// <summary>
        /// Y values of touch
        /// </summary>
        public UInt32[] Y { get { return _y; } }
        

    }
}
