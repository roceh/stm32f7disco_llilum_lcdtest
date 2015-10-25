using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed
{
    public class DiscoBitmap
    {
        private byte[] _data;
        private UInt32 _width;
        private UInt32 _height;

        public DiscoBitmap(UInt32 width, UInt32 height)
        {
            _data = new byte[width * height * 4];
            _width = width;
            _height = height;
        }

        public DiscoBitmap(string path, UInt32 width, UInt32 height)
        {
            _data = SDCard.ReadAllBytes(path);
            _width = width;
            _height = height;
        }

        public byte[] Data { get { return _data; } }
        public UInt32 Width { get { return _width; } }
        public UInt32 Height { get { return _height; } }
    }
}
