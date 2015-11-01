using System;
using System.Runtime.InteropServices;

namespace Managed.SDCard
{
    public unsafe static class SDCardInterop
    {
        public enum FRESULT : byte
        {
            FR_OK = 0,              /* (0) Succeeded */
            FR_DISK_ERR,            /* (1) A hard error occurred in the low level disk I/O layer */
            FR_INT_ERR,             /* (2) Assertion failed */
            FR_NOT_READY,           /* (3) The physical drive cannot work */
            FR_NO_FILE,             /* (4) Could not find the file */
            FR_NO_PATH,             /* (5) Could not find the path */
            FR_INVALID_NAME,        /* (6) The path name format is invalid */
            FR_DENIED,              /* (7) Access denied due to prohibited access or directory full */
            FR_EXIST,               /* (8) Access denied due to prohibited access */
            FR_INVALID_OBJECT,      /* (9) The file/directory object is invalid */
            FR_WRITE_PROTECTED,     /* (10) The physical drive is write protected */
            FR_INVALID_DRIVE,       /* (11) The logical drive number is invalid */
            FR_NOT_ENABLED,         /* (12) The volume has no work area */
            FR_NO_FILESYSTEM,       /* (13) There is no valid FAT volume */
            FR_MKFS_ABORTED,        /* (14) The f_mkfs() aborted due to any parameter error */
            FR_TIMEOUT,             /* (15) Could not get a grant to access the volume within defined period */
            FR_LOCKED,              /* (16) The operation is rejected according to the file sharing policy */
            FR_NOT_ENOUGH_CORE,     /* (17) LFN working buffer could not be allocated */
            FR_TOO_MANY_OPEN_FILES, /* (18) Number of open files > _FS_SHARE */
            FR_INVALID_PARAMETER    /* (19) Given parameter is invalid */
        };

        public const byte FA_READ = 0x01;
        public const byte FA_OPEN_EXISTING = 0x00;        
        public const byte FA_WRITE = 0x02;
        public const byte FA_CREATE_NEW = 0x04;
        public const byte FA_CREATE_ALWAYS = 0x08;
        public const byte FA_OPEN_ALWAYS = 0x10;
        public const byte FA__WRITTEN = 0x20;
        public const byte FA__DIRTY = 0x40;

        internal unsafe struct DriveType
        { }

        internal unsafe struct FATFS
        { }

        internal unsafe struct FIL
        { }

        internal unsafe struct FILINFO
        { }

        [DllImport("C")]
        internal static extern DriveType* GetSDDriver();

        [DllImport("C")]
        internal static extern UInt32 GetFileSize(FIL* fil);

        [DllImport("C")]
        internal static extern UInt32 GetFilePosition(FIL* fil);

        [DllImport("C")]
        internal static extern FATFS* FATFSAlloc();

        [DllImport("C")]
        internal static extern void FATFSFree(FATFS* fatfs);

        [DllImport("C")]
        internal static extern FILINFO* FILINFOAlloc();

        [DllImport("C")]
        internal static extern void FILINFOFree(FILINFO* filinfo);

        [DllImport("C")]
        internal static extern FIL* FILAlloc();

        [DllImport("C")]
        internal static extern void FILFree(FIL* fil);

        [DllImport("C")]
        internal static extern byte FATFS_LinkDriver(DriveType* drv, byte* path);

        [DllImport("C")]
        internal static extern FRESULT f_mount(FATFS* fs, byte* path, byte opt);

        [DllImport("C")]
        internal static extern FRESULT f_stat(byte* path, FILINFO* fno);

        [DllImport("C")]
        internal static extern FRESULT f_open(FIL* fp, byte* path, byte mode);

        [DllImport("C")]
        internal static extern FRESULT f_close(FIL* fp);

        [DllImport("C")]
        internal static extern FRESULT f_read(FIL* fp, void* buff, UInt32 btr, UInt32* br);

        [DllImport("C")]
        internal static extern FRESULT f_write(FIL* fp, byte* buff, UInt32 btw, UInt32* bw);

        [DllImport("C")]
        internal static extern FRESULT f_lseek(FIL* fp, UInt32 ofs);

        [DllImport("C")]
        internal static extern FRESULT f_sync(FIL* fp);

        [DllImport("C")]
        internal static extern FRESULT f_truncate(FIL* fp);
    }
}
