namespace Managed.Graphics
{
    public struct Rect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int Left { get { return X;  } }
        public int Top { get { return Y; } }
        public int Right { get { return X + Width; } }
        public int Bottom { get { return Y + Height; } }

        public Rect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
