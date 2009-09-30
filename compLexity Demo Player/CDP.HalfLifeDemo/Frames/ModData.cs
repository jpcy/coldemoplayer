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

        public override void Skip(BinaryReader br)
        {
            uint length = br.ReadUInt32();
            br.BaseStream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(BinaryReader br)
        {
            uint length = br.ReadUInt32();

            if (length > 0)
            {
                Data = br.ReadBytes((int)length);
            }
        }

        public override void Write(BinaryWriter bw)
        {
            if (Data == null)
            {
                bw.Write(0u);
            }
            else
            {
                bw.Write((uint)Data.Length);
                bw.Write(Data);
            }
        }
    }
}
