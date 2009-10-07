using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcDeltaDescription : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_deltadescription; }
        }

        public override string Name
        {
            get { return "svc_deltadescription"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public DeltaStructure Structure { get; set; }
        public Delta[] Deltas { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();

            if (demo.NetworkProtocol == 43)
            {
                buffer.Endian = Core.BitReader.Endians.Big;
            }

            ushort nEntries = buffer.ReadUShort();
            DeltaStructure deltaDescription = demo.FindReadDeltaStructure("delta_description_t");

            for (int i = 0; i < nEntries; i++)
            {
                deltaDescription.SkipDelta(buffer);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            string name = buffer.ReadString();
            Structure = new DeltaStructure(name);

            if (demo.NetworkProtocol == 43)
            {
                buffer.Endian = Core.BitReader.Endians.Big;
            }

            ushort nEntries = buffer.ReadUShort();
            DeltaStructure deltaDescription = demo.FindReadDeltaStructure("delta_description_t");
            Deltas = new Delta[nEntries];

            for (int i = 0; i < nEntries; i++)
            {
                Deltas[i] = deltaDescription.CreateDelta();
                deltaDescription.ReadDelta(buffer, Deltas[i]);
                Structure.AddEntry(Deltas[i]);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(Structure.Name);
            buffer.WriteUShort((ushort)Structure.NumEntries);
            DeltaStructure deltaDescription = demo.FindWriteDeltaStructure("delta_description_t");

            // TODO: delta classes need cleaning up, should be able to write directly from the delta structure instead of storing and writing from the deltas.
            for (int i = 0; i < Structure.NumEntries; i++)
            {
                deltaDescription.WriteDelta(buffer, Deltas[i]);
            }

            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Name: {0}", Structure.Name);
            log.WriteLine("Num entries: {0}", Structure.NumEntries);
        }
    }
}
