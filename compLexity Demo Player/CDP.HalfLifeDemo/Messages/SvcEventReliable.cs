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

        public uint Index { get; set; }
        public Delta Delta { get; set; }
        public float? Delay { get; set; }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 34)
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

            buffer.SkipRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Delay: {0}", Delay);
        }
#endif
    }
}
