using System;
using System.IO;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcEventReliable : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_event_reliable; }
        }

        public override string Name
        {
            get { return "svc_event_reliable"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public uint Index { get; set; }
        public Delta Delta { get; set; }
        public float? Delay { get; set; }

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            buffer.SeekBits(10);
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");
            eventDeltaStructure.SkipDelta(buffer);

            if (buffer.ReadBoolean())
            {
                buffer.SeekBytes(2);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            Index = buffer.ReadUnsignedBits(10);
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");
            Delta = eventDeltaStructure.CreateDelta();
            eventDeltaStructure.ReadDelta(buffer, Delta);

            if (buffer.ReadBoolean())
            {
                Delay = buffer.ReadUnsignedBits(16) / 100.0f;
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUnsignedBits(Index, 10);
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");
            eventDeltaStructure.WriteDelta(buffer, Delta);

            if (Delay == null)
            {
                buffer.WriteBoolean(false);
            }
            else
            {
                buffer.WriteBoolean(true);
                buffer.WriteUnsignedBits((uint)(Delay * 100.0f), 16);
            }

            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Delay: {0}", Delay);
        }
    }
}
