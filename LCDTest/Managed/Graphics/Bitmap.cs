using Managed.SDCard;
using System;

namespace Managed.Graphics
{
    public class Bitmap
    {
        private byte[] _data;
        private int _width;
        private int _height;

        public Bitmap(int width, int height)
        {
            _data = new byte[width * height * 4];
            _width = width;
            _height = height;
        }

        public Bitmap(string path, int width, int height)
        {
            _data = SDCardManager.ReadAllBytes(path);
            _width = width;
            _height = height;

            Debug.Instance.Log("Loaded " + path + " Length " + _data.Length.ToString() + 
                " MemAvail " + Microsoft.Zelig.Runtime.MemoryManager.Instance.AvailableMemory.ToString());
        }

        public byte[] Data { get { return _data; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
    }
}
