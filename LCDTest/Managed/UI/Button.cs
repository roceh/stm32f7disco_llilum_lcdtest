using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public class Button : Control
    {
        public string Text { get; set; }

        public Button(Application application) : base(application)
        { }

        public override void Draw()
        {
            base.Draw();

            Application.Display.DrawRectangle(AbsoluteLeft, AbsoluteTop, Width, Height);
            Application.Display.DrawCenteredString(Text, AbsoluteLeft, AbsoluteTop, Width, Height);
        }
    }
}
