using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLife.Messages
{
    public class SvcWeaponAnim : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_weaponanim; }
        }

        public override string Name
        {
            get { return "svc_weaponanim"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
        }

        public override void Read(BitReader buffer)
        {
            Unknown1 = buffer.ReadByte();
            Unknown2 = buffer.ReadByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Unknown1);
            buffer.WriteByte(Unknown2);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Unknown1: {0}", Unknown1);
            log.WriteLine("Unknown2: {0}", Unknown2);
        }
    }
}
