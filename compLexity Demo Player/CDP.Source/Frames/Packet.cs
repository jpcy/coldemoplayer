using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.Source.Frames
{
    public class Packet : MessageFrame
    {
        public override FrameIds Id
        {
            get { return FrameIds.Packet; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.Packet; }
        }
    }
}