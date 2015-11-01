using Managed.Misc;
using System;
using System.IO;

namespace Managed.SDCard
{
    public unsafe class SDFileStream : Stream
    {
        SDCardInterop.FIL* _fp = null;
                
        public SDFileStream(string path, FileMode mode, FileAccess access) : base()
        {
            _fp = SDCardInterop.FILAlloc();

            byte fatfsMode = 0;

            switch (mode)
            {
                case FileMode.CreateNew:
                    fatfsMode |= SDCardInterop.FA_CREATE_NEW;
                    break;
                case FileMode.Create:
                    fatfsMode |= SDCardInterop.FA_CREATE_ALWAYS;
                    break;
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                    fatfsMode |= SDCardInterop.FA_OPEN_ALWAYS;
                    break;
                case FileMode.Open:
                case FileMode.Truncate:
                    fatfsMode |= SDCardInterop.FA_OPEN_EXISTING;
                    break;
            }

            if ((access & FileAccess.Read) != 0)
            {
                fatfsMode |= SDCardInterop.FA_READ;
            }

            if ((access & FileAccess.Write) != 0)
            {
                fatfsMode |= SDCardInterop.FA_WRITE;
            }
            
            fixed (byte * pathBytes = InteropHelper.GetNullTerminated(path))
            {
                if (SDCardInterop.f_open(_fp, pathBytes, fatfsMode) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("Unable to open sdcard file");
                }               
            }

            if (Length > 0)
            {
                if (mode == FileMode.Append)
                {
                    Position = Length;
                }
                else
                {
                    Position = 0;
                }
            }

            if (mode == FileMode.Truncate)
            {
                if (SDCardInterop.f_truncate(_fp) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("Unable to truncate sdcard file");
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return SDCardInterop.GetFileSize(_fp);
            }
        }

        public override long Position
        {
            get
            {
                return SDCardInterop.GetFilePosition(_fp);
            }

            set
            {
                if (SDCardInterop.f_lseek(_fp, (UInt32)value) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("SD card seek error");
                }
            }
        }

        public override void Flush()
        {
            SDCardInterop.f_sync(_fp);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            UInt32 bytesRead;

            fixed (byte *bufferPtr = buffer)
            {
                if (SDCardInterop.f_read(_fp, bufferPtr + offset, (UInt32) count, &bytesRead) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("SD card file read error");
                }
            }

            return (int) bytesRead;
        } 

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (SDCardInterop.f_lseek(_fp, (UInt32)offset) != SDCardInterop.FRESULT.FR_OK)
                    {
                        throw new Exception("SD card seek error");
                    }
                    break;
                case SeekOrigin.Current:
                    if (SDCardInterop.f_lseek(_fp, (UInt32) (Position + offset)) != SDCardInterop.FRESULT.FR_OK)
                    {
                        throw new Exception("SD card seek error");
                    }
                    break;
                case SeekOrigin.End:
                    if (SDCardInterop.f_lseek(_fp, (UInt32) (Length - offset)) != SDCardInterop.FRESULT.FR_OK)
                    {
                        throw new Exception("SD card seek error");
                    }
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            // save current position
            var position = Position;

            if (value < Length)
            {
                // seek to position
                Position = value;

                // truncate the file down
                if (SDCardInterop.f_truncate(_fp) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("Unable to truncate sdcard file");
                }
            }
            else
            {  
                const UInt32 PadSize = 32;

                var pad = new byte[PadSize];

                UInt32 bytesWritten;

                // move to end
                Position = Length;

                fixed (byte* bufferPtr = pad)
                {
                    // loop until file is length required
                    while (Length < value)
                    {
                        if (SDCardInterop.f_write(_fp, bufferPtr, (UInt32) Math.Min(Length - value, PadSize), &bytesWritten) != SDCardInterop.FRESULT.FR_OK)
                        {
                            throw new Exception("SD card file write error");
                        }
                    }
                }
            }
            
            // restore original position (or to truncated end if we have shrunk the file)
            Position = Math.Min(position, Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            UInt32 bytesWritten;

            fixed (byte* bufferPtr = buffer)
            {
                if (SDCardInterop.f_write(_fp, bufferPtr + offset, (UInt32)count, &bytesWritten) != SDCardInterop.FRESULT.FR_OK)
                {
                    throw new Exception("SD card file write error");
                }
            }
        }

        public override void Close()
        {
            if (SDCardInterop.f_close(_fp) != SDCardInterop.FRESULT.FR_OK)
            {
                throw new Exception("SD card file close error");
            }

            SDCardInterop.FILFree(_fp);

            _fp = null;

            base.Close();
        }
    }
}
