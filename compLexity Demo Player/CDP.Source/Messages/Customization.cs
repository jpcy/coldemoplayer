using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    // Only encountered in demo protocol 2 demos.
    // FIXME: Need old binaries to get the correct name.
    public class Customization : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.Customization; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.Customization; }
        }

        public override string Name
        {
            get { return "Customization"; }
        }

        public uint Unknown1 { get; set; }
        public string FileName { get; set; }
        public bool Unknown2 { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(4);
            buffer.SeekString();
            buffer.SeekBits(1);
        }

        public override void Read(BitReader buffer)
        {
            Unknown1 = buffer.ReadUInt();
            FileName = buffer.ReadString();
            Unknown2 = buffer.ReadBoolean();
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Unknown1: {0}", Unknown1);
            log.WriteLine("FileName: {0}", FileName);
            log.WriteLine("Unknown2: {0}", Unknown2);
        }
    }
}
