using System;
using System.IO;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Messages
{
    public class SvcBspDecal : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_BSPDecal; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_BSPDecal; }
        }

        public override string Name
        {
            get { return "SVC_BSPDecal"; }
        }

        public Core.Vector Origin { get; set; }
        public uint Tex { get; set; }
        public bool Unknown1 { get; set; }
        public uint Ent { get; set; }
        public uint Mod { get; set; }
        public bool LowPriority { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBitVectorCoord();
            buffer.SeekBits(9);

            if (buffer.ReadBoolean())
            {
                buffer.SeekBits(22);
            }

            buffer.SeekBits(1);
        }

        public override void Read(BitReader buffer)
        {
            Origin = buffer.ReadBitVectorCoord();
            Tex = buffer.ReadUBits(9);
            Unknown1 = buffer.ReadBoolean();

            if (Unknown1)
            {
                Ent = buffer.ReadUBits(11);
                Mod = buffer.ReadUBits(11);
            }

            LowPriority = buffer.ReadBoolean();
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteVector("Origin", Origin);
            log.WriteLine("Tex: {0}", Tex);
            log.WriteLine("Unknown1: {0}", Unknown1);

            if (Unknown1)
            {
                log.WriteLine("Ent: {0}", Ent);
                log.WriteLine("Mod: {0}", Mod);
            }

            log.WriteLine("Low priority: {0}", LowPriority);
        }
    }
}
