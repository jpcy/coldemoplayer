using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    public class Unknown : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.Unknown; }
        }

        public byte[] Data { get; set; }

        protected override void ReadContent(BinaryReader br)
        {
            Data = br.ReadBytes(84);
        }

        protected override byte[] WriteContent()
        {
            return Data;
        }
    }
}
