using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;

namespace Managed
{
    public unsafe class STM32F7DiscoTouch
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

        public STM32F7DiscoTouch()
        {
            BSP_TS_Init(ScreenWidth, ScreenHeight);
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
                touches = BSP_TS_GetTouchInfo(x, y);
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
        
        [DllImport("C")]
        internal static extern byte BSP_TS_Init(UInt16 sizeX, UInt16 sizeY);

        [DllImport("C")]
        internal static extern byte BSP_TS_DeInit();

        [DllImport("C")]
        internal static extern byte BSP_TS_GetTouchInfo(UInt32* x, UInt32* y);
    }
}
