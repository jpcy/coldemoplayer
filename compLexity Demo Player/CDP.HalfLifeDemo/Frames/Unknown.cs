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

        protected override void ReadContent(BinaryReader br)
        {
            br.BaseStream.Seek(84, SeekOrigin.Current);
        }
    }
}
