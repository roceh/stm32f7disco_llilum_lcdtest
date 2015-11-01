using Managed.Misc;
using System;

namespace Managed.SDCard
{
    /// <summary>
    /// C# Map onto Chan's FatFS
    /// </summary>
    public unsafe static class SDCardManager 
    {
        /// <summary>
        /// Path to mounted sd card
        /// </summary>
        private static byte[] _path = new byte[5];

        /// <summary>
        /// Handle to mounted fs
        /// </summary>
        private static SDCardInterop.FATFS* _fsHandle;
        
        /// <summary>
        /// Mount the SD cards file system
        /// </summary>
        public static void Mount()
        {
            fixed (byte* path = _path)
            {
                if (SDCardInterop.FATFS_LinkDriver(SDCardInterop.GetSDDriver(), path) != 0)
                {
                    throw new Exception("Unable to initialise file system interface");
                }
                
                _fsHandle = SDCardInterop.FATFSAlloc();

                if (SDCardInterop.f_mount(_fsHandle, path, 0) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("Unable to mount SD Card");
                }
            }
        }
        
        /// <summary>
        /// Does the specified file or folder exist
        /// </summary>
        /// <param name="path">path of file or folder</param>
        /// <returns>true if exists</returns>
        public static bool Exists(string path)
        {
            if (_fsHandle != null)
            {
                fixed (byte* pathBytes = InteropHelper.GetNullTerminated(path))
                {
                    SDCardInterop.FILINFO* filinfo = SDCardInterop.FILINFOAlloc();
                    try
                    {
                        if (SDCardInterop.f_stat(pathBytes, filinfo) == SDCardInterop.FRESULT.FR_OK)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        SDCardInterop.FILINFOFree(filinfo);
                    }
                }
            }

            return false;
        }

        public static byte[] ReadAllBytes(string path)
        {
            using (var fsRead = new SDFileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                int bytesRead;

                byte[] result = new byte[fsRead.Length];

                int i = 0;

                // read to end of file
                while ((bytesRead = fsRead.Read(result, i, 4096)) > 0)
                {
                    i += bytesRead;
                }

                return result;
            }

            throw new Exception("Unable to load file");
        }
    }
}
