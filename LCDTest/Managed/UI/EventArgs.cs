using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public class TouchEventArgs : EventArgs
    {
        public Point Position;

        public TouchEventArgs(Point position)
        {
            Position = position;
        }
    }
}
