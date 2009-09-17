using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.UserMessages
{
    public class SayText : UserMessage
    {
        public override string Name
        {
            get { return "SayText"; }
        }

        public byte Slot { get; set; }
        public List<string> Strings { get; set; }

        public override void Read(BitReader buffer)
        {
            int endOffset = buffer.CurrentByte + Length;
            Slot = buffer.ReadByte();
            Strings = new List<string>();

            while (true)
            {
                string s = buffer.ReadString();

                if (!string.IsNullOrEmpty(s))
                {
                    Strings.Add(s);
                }

                if (buffer.CurrentByte == endOffset)
                {
                    break;
                }
            }
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
