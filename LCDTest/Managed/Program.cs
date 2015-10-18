namespace Managed
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Windows.Devices.Gpio;
    using Microsoft.Zelig.Support.mbed;
    using Microsoft.Zelig.DISCO_F746NG;

    /// <summary>
    /// Simple app to test LCD 
    /// </summary>
    unsafe class Program
    {        
        static void Main()
        {
            //var test = new PixelTest();
            //var test = new TouchTest();
            var test = new SimpleUITest();

            test.Run();
        }
    }
}
