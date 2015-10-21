using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public class TouchEventArgs : EventArgs
    {
        public int Id;
        public Point Position;

        public TouchEventArgs(int id, Point position)
        {
            Id = id;
            Position = position;
        }
    }
}
