using Managed.SDCard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Managed.Graphics
{
    public class Font
    {
        public struct Character
        {
            public int Channel;
            public Rect Rect;
            public Point Offset;
            public char Id;
            public int Page;
            public int XAdvance;
        }

        public struct Page
        {
            public Bitmap Texture;
            public string FileName;
            public int Id;

            public Page(int id, string fileName, Bitmap texture)
            {
                FileName = fileName;
                Id = id;
                Texture = texture;
            }
        }

        public Font()
        {
        }

        private static bool GetVool(string[] items, string name)
        {
            return GetInt(items, name) != 0;
        }

        private static int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static int GetInt(string[] items, string name)
        {
            var value = GetString(items, name);

            if (value.Length == 0)
            {
                throw new Exception("Cannot convert to int");
            }

            // int parse not supported :/

            int j = 0;

            int result = 0;
            
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (value[j] >= 0x30 && value[j] <= 0x39)
                    result += (value[j] - 0x30) * IntPow(10, (uint) i);

                j++;
            }

            return result;
        }

        private static string GetString(string[] items, string name)
        {
            string result = "";

            name = name.ToLowerInvariant();

            foreach (string item in items)
            {
                int index = -1;

                // indexof not supported :/
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[i] == '=')
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    string itemName = item.Substring(0, index).ToLowerInvariant();
                    string itemValue = item.Substring(index + 1);

                    if (itemName == name)
                    {
                        if (itemValue.StartsWith("\"", StringComparison.Ordinal) && itemValue.EndsWith("\"", StringComparison.Ordinal))
                        {
                            itemValue = itemValue.Substring(1, itemValue.Length - 2);
                        }

                        result = itemValue;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Load in font template file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>loaded font</returns>
        public static Font LoadFromFile(string fileName)
        {
            Font font = new Font();
            
            Dictionary<int, Page> pages = new Dictionary<int, Page>();
            Dictionary<char, Character> characters = new Dictionary<char, Character>();

            byte[] data = SDCardManager.ReadAllBytes(fileName);

            int i = 0;
            int k;
            int l = 0;

            char[] lineChars = new char[255];

            string line;

            int commonWidth = 0, commonHeight = 0;

            while (i < data.Length)
            {
                k = i;

                l = 0;

                while (k < data.Length && data[k] != 10)
                {
                    if (data[k] != 10 && data[k] != 13)
                    {
                        lineChars[l] = (char)data[k];
                        l++;
                    }
                    k++;
                }

                line = new string(lineChars, 0, l);

                i = k + 1;

                string[] items = line.Split(' ');

                if (items.Length != 0)
                {
                    switch (items[0])
                    {
                        case "common":
                            commonWidth = GetInt(items, "scaleW");
                            commonHeight = GetInt(items, "scaleH");
                            break;
                        case "page":
                            int id = GetInt(items, "id");
                            string file = GetString(items, "file").Trim('"');
                            Bitmap texture = new Bitmap(file, commonWidth, commonHeight);
                            pages.Add(id, new Page(id, file, texture));
                            break;
                        case "char":
                            var charData = new Character
                            {
                                Id = (char)GetInt(items, "id"),
                                Rect = new Rect(GetInt(items, "x"), GetInt(items, "y"), GetInt(items, "width"), GetInt(items, "height")),
                                Offset = new Point(GetInt(items, "xoffset"), GetInt(items, "yoffset")),
                                XAdvance = GetInt(items, "xadvance"),
                                Page = GetInt(items, "page"),
                                Channel = GetInt(items, "chnl")
                            };

                            characters.Add(charData.Id, charData);
                            break;
                    }
                }                
            }

            font.Pages = pages;
            font.Characters = characters;
                                    
            return font;
        }

        public Dictionary<char, Character> Characters { get; set; }
        public Dictionary<int, Page> Pages { get; set; }
    }
}
