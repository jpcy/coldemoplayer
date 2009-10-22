using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.Source.Frames
{
    public class Signon : MessageFrame
    {
        public override FrameIds Id
        {
            get { return FrameIds.Signon; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.Signon; }
        }
    }
}
