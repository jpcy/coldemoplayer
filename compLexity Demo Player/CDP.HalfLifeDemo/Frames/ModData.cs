using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    // MOD Demo_ReadBuffer/Demo_WriteBuffer
    public class ModData : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.ModData; }
        }

        public byte[] Data { get; set; }

        public override void Skip(FastFileStream stream)
        {
            uint length = stream.ReadUInt();
            stream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            uint length = stream.ReadUInt();

            if (length > 0)
            {
                Data = stream.ReadBytes((int)length);
            }
        }

        public override void Write(FastFileStream stream)
        {
            if (Data == null)
            {
                stream.WriteUInt(0);
            }
            else
            {
                stream.WriteUInt((uint)Data.Length);
                stream.WriteBytes(Data);
            }
        }
    }
}
