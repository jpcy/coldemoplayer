using System;
using System.IO;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcEvent : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_event; }
        }

        public override string Name
        {
            get { return "svc_event"; }
        }

        public class Event
        {
            public uint Index { get; set; }
            public uint? PacketIndex { get; set; }
            public Delta Delta { get; set; }
            public ushort? FireTime { get; set; }
        }

        public List<Event> Events { get; set; }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 34)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint nEvents = buffer.ReadUnsignedBits(5);
            Events = new List<Event>();
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");

            for (int i = 0; i < nEvents; i++)
            {
                Event ev = new Event();
                ev.Index = buffer.ReadUnsignedBits(10);

                if (buffer.ReadBoolean())
                {
                    ev.PacketIndex = buffer.ReadUnsignedBits(11);

                    if (buffer.ReadBoolean())
                    {
                        ev.Delta = eventDeltaStructure.CreateDelta();
                        eventDeltaStructure.ReadDelta(buffer, ev.Delta);
                    }
                }

                if (buffer.ReadBoolean())
                {
                    ev.FireTime = buffer.ReadUShort();
                }

                Events.Add(ev);
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
            log.WriteLine("Num. events: {0}", Events.Count);
        }
#endif
    }
}
