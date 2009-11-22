using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcGameEventList : Message
    {
        public class Event
        {
            public class Entry
            {
                public uint Type { get; set; } // TODO: enum
                public string Name { get; set; }
            }

            public uint Id { get; set; }
            public string Name { get; set; }
            public List<Entry> Entries { get; private set; }

            public Event()
            {
                Entries = new List<Entry>();
            }
        }

        public override MessageIds Id
        {
            get { return MessageIds.SVC_GameEventList; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_GameEventList; }
        }

        public override string Name
        {
            get { return "SVC_GameEventList"; }
        }

        public uint NumEvents { get; set; }
        public uint NumBits { get; set; }
        public List<Event> Events { get; private set; }

        public SvcGameEventList()
        {
            Events = new List<Event>();
        }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(9);
            uint nBits = buffer.ReadUBits(20);
            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            NumEvents = buffer.ReadUBits(9);
            NumBits = buffer.ReadUBits(20);

            for (int i = 0; i < NumEvents; i++)
            {
                Event ev = new Event
                {
                    Id = buffer.ReadUBits(9),
                    Name = buffer.ReadString()
                };

                Events.Add(ev);

                while (true)
                {
                    uint entryType = buffer.ReadUBits(3);

                    if (entryType == 0)
                    {
                        break;
                    }

                    Event.Entry entry = new Event.Entry
                    {
                        Type = entryType,
                        Name = buffer.ReadString()
                    };

                    ev.Entries.Add(entry);
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num events: {0}", NumEvents);
            log.WriteLine("Num bits: {0}", NumBits);

            foreach (Event ev in Events)
            {
                log.WriteLine("\tEvent {0} [{1}]", ev.Name, ev.Id);

                foreach (Event.Entry entry in ev.Entries)
                {
                    log.WriteLine("\t\tEntry {0} {1}", entry.Name, entry.Type);
                }
            }
        }
    }
}
