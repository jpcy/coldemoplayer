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
            DeltaStructure eventDeltaStructure = demo.FindReadDeltaStructure("event_t");
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

            Index = buffer.ReadUBits(10);
            DeltaStructure eventDeltaStructure = demo.FindReadDeltaStructure("event_t");
            Delta = eventDeltaStructure.CreateDelta();
            eventDeltaStructure.ReadDelta(buffer, Delta);

            if (buffer.ReadBoolean())
            {
                Delay = buffer.ReadUBits(16) / 100.0f;
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUBits(Index, 10);
            DeltaStructure eventDeltaStructure = demo.FindWriteDeltaStructure("event_t");
            eventDeltaStructure.WriteDelta(buffer, Delta);

            if (Delay == null)
            {
                buffer.WriteBoolean(false);
            }
            else
            {
                buffer.WriteBoolean(true);
                buffer.WriteUBits((uint)(Delay * 100.0f), 16);
            }

            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Delay: {0}", Delay);
        }
    }
}
