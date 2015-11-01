using Managed.Memory;
using System;
using System.Runtime.InteropServices;

namespace Managed
{
    /// <summary>
    /// Simple app to test LCD 
    /// </summary>
    unsafe class Program
    {
        static void Main()
        {
            try
            {
                // initialise sdram - No GC :(
                SDRAMInterop.BSP_SDRAM_Init();

                // get memory manager
                var mm = Microsoft.Zelig.Runtime.MemoryManager.Instance as Microsoft.CortexM3OnMBED.MemoryManager;

                // Add around 7MB sdram to managed heap
                mm.AddExternal((UIntPtr)(0xC0000000 + (480 * 272 * 4 * 2)), (UIntPtr)(0xC0000000 + 0x800000));
                
                //var test = new PixelTest();
                //var test = new TouchTest();
                var test = new SimpleUITest();
                //var test = new SDCardTest();
                //var test = new BitmapTest();
                test.Run();
            }
            catch (Exception ex)
            {
                Debug.Instance.Log(ex.ToString());
                while (true) { }
            }
        }
    }
}
