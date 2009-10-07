using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.CounterStrikeDemo.UserMessages
{
    public class ClCorpse : CDP.HalfLifeDemo.UserMessage
    {
        public override string Name
        {
            get { return "ClCorpse"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return ((CounterStrikeDemo.Demo)demo).Version == CounterStrikeDemo.Demo.Versions.CounterStrike16; }
        }

        public string Model { get; set; }
        public byte[] Data1 { get; set; }
        public byte Sequence { get; set; }
        public byte[] Data2 { get; set; }

        public override void Read(BitReader buffer)
        {
            Model = buffer.ReadString();
            Data1 = buffer.ReadBytes(22);
            Sequence = buffer.ReadByte();
            Data2 = buffer.ReadBytes(3);
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Model);
            buffer.WriteBytes(Data1);
            buffer.WriteByte(Sequence);
            buffer.WriteBytes(Data2);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Model: {0}", Model);
            log.WriteLine("Sequence: {0}", Sequence);
        }
    }
}
