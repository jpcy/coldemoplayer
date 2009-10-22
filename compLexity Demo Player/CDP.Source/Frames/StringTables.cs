using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Frames
{
    public class StringTables : Frame
    {
        public class Entry
        {
            public string Name { get; set; }
            public byte[] ExtraData { get; set; }

            public void Read(BitReader buffer)
            {
                Name = buffer.ReadString();

                if (buffer.ReadBoolean())
                {
                    ushort extraDataLength = buffer.ReadUShort();
                    ExtraData = buffer.ReadBytes(extraDataLength);
                }
            }
        }

        public class Table
        {
            public string Name { get; set; }
            public List<Entry> Entries { get; set; }
            public List<Entry> ExtraEntries { get; set; }

            public Table()
            {
                Entries = new List<Entry>();
                ExtraEntries = new List<Entry>();
            }
        }

        public override FrameIds Id
        {
            get { return FrameIds.StringTables; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.StringTables; }
        }

        public List<Table> Tables { get; set; }

        public StringTables()
        {
            Tables = new List<Table>();
        }

        public override void Skip(FastFileStream stream)
        {
            int length = stream.ReadInt();
            stream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            int length = stream.ReadInt();

            if (length == 0)
            {
                return;
            }

            BitReader buffer = new BitReader(stream.ReadBytes(length));
            byte nTables = buffer.ReadByte();

            for (int i = 0; i < nTables; i++)
            {
                Table table = new Table();
                Tables.Add(table);
                table.Name = buffer.ReadString();
                ushort nEntries = buffer.ReadUShort();

                for (int j = 0; j < nEntries; j++)
                {
                    Entry entry = new Entry();
                    table.Entries.Add(entry);
                    entry.Read(buffer);
                }

                if (buffer.ReadBoolean())
                {
                    nEntries = buffer.ReadUShort();

                    for (int j = 0; j < nEntries; j++)
                    {
                        Entry entry = new Entry();
                        table.ExtraEntries.Add(entry);
                        entry.Read(buffer);
                    }
                }
            }
        }

        public override void Write(FastFileStream stream)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num. tables: {0}", Tables.Count);

            foreach (Table table in Tables)
            {
                log.WriteLine("Table: {0}", table.Name);
                log.WriteLine("Num. entries: {0}", table.Entries.Count);

                foreach (Entry entry in table.Entries)
                {
                    log.WriteLine("\tEntry name: {0}", entry.Name);

                    if (entry.ExtraData != null)
                    {
                        log.WriteBytesAsChars(entry.ExtraData);
                    }

                    log.WriteLine();
                }

                log.WriteLine("Num. extra entries: {0}", table.ExtraEntries.Count);

                foreach (Entry entry in table.ExtraEntries)
                {
                    log.WriteLine("\tExtra entry name: {0}", entry.Name);

                    if (entry.ExtraData != null)
                    {
                        log.WriteBytesAsChars(entry.ExtraData);
                    }

                    log.WriteLine();
                }
            }
        }
    }
}
