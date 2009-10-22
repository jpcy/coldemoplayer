using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcPrefetch : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_Prefetch; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_Prefetch; }
        }

        public override string Name
        {
            get { return "SVC_Prefetch"; }
        }

        public uint Unknown { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(13);
        }

        public override void Read(BitReader buffer)
        {
            Unknown = buffer.ReadUBits(13);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Unknown: {0}", Unknown);
        }
    }
}
