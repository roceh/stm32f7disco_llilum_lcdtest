using System;
using System.IO;

namespace Managed
{
    public unsafe class SDFileStream : Stream
    {
        SDInterop.FIL* _fp = null;
                
        public SDFileStream(string path, FileMode mode, FileAccess access) : base()
        {
            _fp = SDInterop.FILAlloc();

            byte fatfsMode = 0;

            switch (mode)
            {
                case FileMode.CreateNew:
                    fatfsMode |= SDInterop.FA_CREATE_NEW;
                    break;
                case FileMode.Create:
                    fatfsMode |= SDInterop.FA_CREATE_ALWAYS;
                    break;
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                    fatfsMode |= SDInterop.FA_OPEN_ALWAYS;
                    break;
                case FileMode.Open:
                case FileMode.Truncate:
                    fatfsMode |= SDInterop.FA_OPEN_EXISTING;
                    break;
            }

            if ((access & FileAccess.Read) != 0)
            {
                fatfsMode |= SDInterop.FA_READ;
            }

            if ((access & FileAccess.Write) != 0)
            {
                fatfsMode |= SDInterop.FA_WRITE;
            }
            
            fixed (byte * pathBytes = InteropHelper.GetNullTerminated(path))
            {
                if (SDInterop.f_open(_fp, pathBytes, fatfsMode) != SDInterop.FRESULT.FR_OK)
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
                if (SDInterop.f_truncate(_fp) != SDInterop.FRESULT.FR_OK)
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
                return SDInterop.GetFileSize(_fp);
            }
        }

        public override long Position
        {
            get
            {
                return SDInterop.GetFilePosition(_fp);
            }

            set
            {
                if (SDInterop.f_lseek(_fp, (UInt32)value) != SDInterop.FRESULT.FR_OK)
                {
                    throw new Exception("SD card seek error");
                }
            }
        }

        public override void Flush()
        {
            SDInterop.f_sync(_fp);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            UInt32 bytesRead;

            fixed (byte *bufferPtr = buffer)
            {
                if (SDInterop.f_read(_fp, bufferPtr + offset, (UInt32) count, &bytesRead) != SDInterop.FRESULT.FR_OK)
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
                    if (SDInterop.f_lseek(_fp, (UInt32)offset) != SDInterop.FRESULT.FR_OK)
                    {
                        throw new Exception("SD card seek error");
                    }
                    break;
                case SeekOrigin.Current:
                    if (SDInterop.f_lseek(_fp, (UInt32) (Position + offset)) != SDInterop.FRESULT.FR_OK)
                    {
                        throw new Exception("SD card seek error");
                    }
                    break;
                case SeekOrigin.End:
                    if (SDInterop.f_lseek(_fp, (UInt32) (Length - offset)) != SDInterop.FRESULT.FR_OK)
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
                if (SDInterop.f_truncate(_fp) != SDInterop.FRESULT.FR_OK)
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
                        if (SDInterop.f_write(_fp, bufferPtr, (UInt32) Math.Min(Length - value, PadSize), &bytesWritten) != SDInterop.FRESULT.FR_OK)
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
                if (SDInterop.f_write(_fp, bufferPtr + offset, (UInt32)count, &bytesWritten) != SDInterop.FRESULT.FR_OK)
                {
                    throw new Exception("SD card file write error");
                }
            }
        }

        public override void Close()
        {
            if (SDInterop.f_close(_fp) != SDInterop.FRESULT.FR_OK)
            {
                throw new Exception("SD card file close error");
            }

            SDInterop.FILFree(_fp);

            _fp = null;

            base.Close();
        }
    }
}
