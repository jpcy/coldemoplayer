using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Source.Frames
{
    public class Stop : Frame
    {
        public override FrameIds Id
        {
            get { return FrameIds.Stop; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.Stop; }
        }

        public override void Skip(FastFileStream stream)
        {
        }

        public override void Read(FastFileStream stream)
        {
        }

        public override void Write(FastFileStream stream)
        {
        }

        public override void Log(StreamWriter log)
        {
        }
    }
}
