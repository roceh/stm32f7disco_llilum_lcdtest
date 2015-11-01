using Managed.Graphics;
using System;

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
