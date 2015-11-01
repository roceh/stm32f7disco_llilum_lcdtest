using Managed.Graphics;

namespace Managed.UI
{
    public class TouchInfo
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public float LastSeenTime { get; set; }
    }
}
