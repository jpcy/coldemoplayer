using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    public class Loading : Frame
    {
        public override byte Id 
        {
            get { return (byte)FrameIds.Loading; } 
        }

        public override bool HasMessages
        {
            get { return true; }
        }

        public override bool CanSkip
        {
            get { return false; }
        }

        public byte[] DemoInfo { get; set; }

        // Sequence info.
        // See: Quake, client/net.h
        public byte[] SequenceInfo { get; set; }

        public override void Read(BinaryReader br)
        {
            int demoInfoLength;

            if (networkProtocol <= 43)
            {
                demoInfoLength = 532;
            }
            else
            {
                demoInfoLength = 436;
            }

            DemoInfo = br.ReadBytes(demoInfoLength);
            SequenceInfo = br.ReadBytes(28);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(DemoInfo);
            bw.Write(SequenceInfo);
        }
    }
}
