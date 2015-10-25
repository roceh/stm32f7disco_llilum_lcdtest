using System;
using System.IO;
using System.Runtime.InteropServices;


namespace Managed
{
    /// <summary>
    /// C# Map onto Chan's FatFS
    /// </summary>
    public unsafe static class SDCard 
    {
        /// <summary>
        /// Path to mounted sd card
        /// </summary>
        private static byte[] _path = new byte[5];

        /// <summary>
        /// Handle to mounted fs
        /// </summary>
        private static SDInterop.FATFS* _fsHandle;
        
        /// <summary>
        /// Mount the SD cards file system
        /// </summary>
        public static void Mount()
        {
            fixed (byte* path = _path)
            {
                if (SDInterop.FATFS_LinkDriver(SDInterop.GetSDDriver(), path) != 0)
                {
                    throw new Exception("Unable to initialise file system interface");
                }
                
                _fsHandle = SDInterop.FATFSAlloc();

                if (SDInterop.f_mount(_fsHandle, path, 0) != SDInterop.FRESULT.FR_OK)
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
                    SDInterop.FILINFO* filinfo = SDInterop.FILINFOAlloc();
                    try
                    {
                        if (SDInterop.f_stat(pathBytes, filinfo) == SDInterop.FRESULT.FR_OK)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        SDInterop.FILINFOFree(filinfo);
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
