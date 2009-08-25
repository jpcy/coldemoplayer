using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    public class PlaySound : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.PlaySound; }
        }

        protected override void ReadContent(BinaryReader br)
        {
            br.BaseStream.Seek(4, SeekOrigin.Current); // signed int, unknown
            uint length = br.ReadUInt32();
            br.BaseStream.Seek(length, SeekOrigin.Current); // sound filename
            br.BaseStream.Seek(16, SeekOrigin.Current); // unknown
        }

        protected override byte[] WriteContent()
        {
            return null;
        }
    }
}
