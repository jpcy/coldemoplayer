using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.IdTech3
{
    public class BitWriter : Core.BitWriter
    {
        public BitWriter()
            : base(Message.MAX_MSGLEN)
        {
        }
    }
}
