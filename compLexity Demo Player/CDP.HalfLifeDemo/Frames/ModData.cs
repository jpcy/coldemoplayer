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

        protected override void ReadContent(BinaryReader br)
        {
            uint length = br.ReadUInt32();

            if (length > 0)
            {
                Data = br.ReadBytes((int)length);
            }
        }

        protected override byte[] WriteContent()
        {
            BitWriter buffer = new BitWriter();

            if (Data == null)
            {
                buffer.WriteUInt(0);
            }
            else
            {
                buffer.WriteUInt((uint)Data.Length);
                buffer.WriteBytes(Data);
            }

            return buffer.ToArray();
        }
    }
}
