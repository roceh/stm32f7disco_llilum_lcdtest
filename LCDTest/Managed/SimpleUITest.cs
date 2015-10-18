using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Devices.Gpio;
using Microsoft.Zelig.Support.mbed;
using Microsoft.Zelig.DISCO_F746NG;
using Managed.UI;

namespace Managed
{
    /// <summary>
    /// Test for simple ui
    /// </summary>
    public class SimpleUITest
    {        
        public void Run()
        {
            var application = new Application();
            application.ShowDebug = true;
            var panel = new Panel(application) { Left = 0, Top = 0, Width = 480, Height = 272 };
            var label = new Label(application) { Left = 10, Top = 30, Width = 200, Height = 16, Text = "Hello World" };
            panel.Add(label);

            var button = new Button(application) { Left = 10, Top = 100, Width = 200, Height = 100, Text = "Click Me" };
            button.Touch += (s, e) => { label.Text = "Clicked"; };
            panel.Add(button);
            application.Run(panel);
        }
    }
}
