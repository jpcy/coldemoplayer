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

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public class Event
        {
            public uint Index { get; set; }
            public uint? PacketIndex { get; set; }
            public Delta Delta { get; set; }
            public ushort? FireTime { get; set; }
        }

        public List<Event> Events { get; set; }

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint nEvents = buffer.ReadUnsignedBits(5);
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");

            for (int i = 0; i < nEvents; i++)
            {
                buffer.SeekBits(10);

                if (buffer.ReadBoolean())
                {
                    buffer.SeekBits(11);

                    if (buffer.ReadBoolean())
                    {
                        eventDeltaStructure.SkipDelta(buffer);
                    }
                }

                if (buffer.ReadBoolean())
                {
                    buffer.SeekBytes(2);
                }
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
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

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUnsignedBits((uint)Events.Count, 5);
            DeltaStructure eventDeltaStructure = demo.FindDeltaStructure("event_t");

            foreach (Event ev in Events)
            {
                buffer.WriteUnsignedBits(ev.Index, 10);

                if (ev.PacketIndex.HasValue)
                {
                    buffer.WriteBoolean(true);
                    buffer.WriteUnsignedBits((uint)ev.PacketIndex, 11);

                    if (ev.Delta == null)
                    {
                        buffer.WriteBoolean(false);
                    }
                    else
                    {
                        buffer.WriteBoolean(true);
                        eventDeltaStructure.WriteDelta(buffer, ev.Delta);
                    }                    
                }
                else
                {
                    buffer.WriteBoolean(false);
                }

                if (ev.FireTime.HasValue)
                {
                    buffer.WriteBoolean(true);
                    buffer.WriteUShort(ev.FireTime.Value);
                }
                else
                {
                    buffer.WriteBoolean(false);
                }
            }

            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num. events: {0}", Events.Count);
            // TODO
        }
    }
}
