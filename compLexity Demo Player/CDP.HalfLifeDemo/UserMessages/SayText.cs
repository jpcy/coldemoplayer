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

        public override bool CanSkipWhenWriting
        {
            get { return true; }
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

                // FIXME: this will cause problems when writing.
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
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(Slot);

            foreach (String s in Strings)
            {
                buffer.WriteString(s);
            }

            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);

            if (Strings != null)
            {
                foreach (String s in Strings)
                {
                    log.WriteLine("\"{0}\"", s);
                }
            }
        }
    }
}
