using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    // MOD Demo_ReadBuffer/Demo_WriteBuffer
    public class ModData : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.ModData; }
        }

        protected override void ReadContent(BinaryReader br)
        {
            uint length = br.ReadUInt32();
            br.BaseStream.Seek(length, SeekOrigin.Current);
        }

        protected override byte[] WriteContent()
        {
            return null;
        }
    }
}
