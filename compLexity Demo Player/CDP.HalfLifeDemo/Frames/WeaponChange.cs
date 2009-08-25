using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    // Something to do with weapon change animations.
    // Non-critical -- can be removed, which results in no weapon change animations (like HLTV/HLTV models).
    public class WeaponChange : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.WeaponChange; }
        }

        protected override void ReadContent(BinaryReader br)
        {
            br.BaseStream.Seek(8, SeekOrigin.Current);
        }
    }
}
