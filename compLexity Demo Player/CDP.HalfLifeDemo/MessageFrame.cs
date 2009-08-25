using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.HalfLifeDemo
{
    public abstract class MessageFrame : Frame
    {
        public override bool HasMessages
        {
            get { return true; }
        }
        public byte[] MessageData { get; set; }
    }
}
