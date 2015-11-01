using System;
using System.Runtime.InteropServices;

namespace Managed.Graphics
{
    public unsafe static class DisplayInterop
    {        
        [DllImport("C")]
        internal static extern void LCD_Init(UInt32* activeAddress, UInt32 *drawAddress);

        [DllImport("C")]
        internal static extern void LCD_DeInit();

        [DllImport("C")]
        internal static extern void LCD_DrawImageWithAlpha(UInt32* image, int srcX, int srcY, int srcWidth, int dstX, int dstY, int dstWidth, int dstHeight);

        [DllImport("C")]
        internal static extern void LCD_SetActiveAddress(UInt32 *address);

        [DllImport("C")]
        internal static extern void LCD_SetDrawAddress(UInt32* address);

        [DllImport("C")]
        internal static extern byte BSP_TS_Init(UInt16 sizeX, UInt16 sizeY);

        [DllImport("C")]
        internal static extern byte BSP_TS_DeInit();

        [DllImport("C")]
        internal static extern byte BSP_TS_GetTouchInfo(UInt32* x, UInt32* y);
    }
}
