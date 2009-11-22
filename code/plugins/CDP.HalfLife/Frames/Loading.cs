using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLife.Frames
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

        private const int demoInfoLength = 436;

        public override void Read(FastFileStream stream)
        {
            if (networkProtocol <= 43)
            {
                DemoInfo = new byte[demoInfoLength];
                stream.Read(DemoInfo, 0, 28);
                stream.Seek(489, SeekOrigin.Current);
                stream.Read(DemoInfo, 421, 15);
            }
            else
            {
                DemoInfo = stream.ReadBytes(demoInfoLength);
            }

            SequenceInfo = stream.ReadBytes(28);
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteBytes(DemoInfo);
            stream.WriteBytes(SequenceInfo);
        }
    }
}
