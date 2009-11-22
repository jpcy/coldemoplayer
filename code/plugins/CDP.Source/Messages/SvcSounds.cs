using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcSounds :Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_Sounds; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_Sounds; }
        }

        public override string Name
        {
            get { return "SVC_Sounds"; }
        }

        public bool Reliable { get; set; }
        public uint Number { get; set; } // sound number?

        public override void Skip(BitReader buffer)
        {
            int nBitsLength = 8;

            if (!buffer.ReadBoolean())
            {
                buffer.SeekBits(8);
                nBitsLength = 16;
            }

            uint nBits = buffer.ReadUBits(nBitsLength);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            Reliable = buffer.ReadBoolean();
            int nBitsLength = 8;

            if (!Reliable)
            {
                Number = buffer.ReadUBits(8);
                nBitsLength = 16;
            }
            else
            {
                Number = 1;
            }

            uint nBits = buffer.ReadUBits(nBitsLength);
            // TODO: see SoundInfo_t::WriteDelta
            buffer.SeekBits(nBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Reliable: {0}", Reliable);
            log.WriteLine("Number: {0}", Number);
        }
    }
}
