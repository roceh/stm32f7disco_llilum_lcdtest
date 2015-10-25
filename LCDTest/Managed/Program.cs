namespace Managed
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Windows.Devices.Gpio;
    using Microsoft.Zelig.Support.mbed;
    using Microsoft.Zelig.DISCO_F746NG;
    using System.Text;

    /// <summary>
    /// Simple app to test LCD 
    /// </summary>
    unsafe class Program
    {
        [DllImport("C")]
        private static extern byte BSP_SDRAM_Init();

        static void Main()
        {
            try
            {
                Debug.Instance.Log("Setting up SDRAM...");

                // initialise sdram - No GC :(
                BSP_SDRAM_Init();

                // get memory manager
                var mm = Microsoft.Zelig.Runtime.MemoryManager.Instance as Microsoft.CortexM3OnMBED.MemoryManager;

                // Add around 7MB sdram to managed heap
                mm.AddExternal((UIntPtr)(0xC0000000 + (480 * 272 * 4 * 2)), (UIntPtr)(0xC0000000 + 0x800000));

                Debug.Instance.Log("SDRAM setup complete");

                //var test = new PixelTest();
                //var test = new TouchTest();
                var test = new SimpleUITest();
                //var test = new SDCardTest();
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
