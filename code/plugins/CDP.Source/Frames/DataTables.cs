using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Frames
{
    public class DataTables : Frame
    {
        public class ClassInfo
        {
            public short Index { get; set; }
            public string ClassName { get; set; }
            public string DataTableName { get; set; }
        }

        public override FrameIds Id
        {
            get { return FrameIds.DataTables; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.DataTables; }
        }

        public List<DataTable> SendTables { get; set; }
        public List<ClassInfo> ClassInfos { get; set; }

        public override void Skip(FastFileStream stream)
        {
            int length = stream.ReadInt();
            stream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            int length = stream.ReadInt();
            BitReader buffer = new BitReader(stream.ReadBytes(length));

            // Read Send Tables.
            SendTables = new List<DataTable>();

            while (buffer.ReadBoolean())
            {
                DataTable table = new DataTable();
                SendTables.Add(table);
                table.Unknown1 = buffer.ReadBoolean();
                table.Name = buffer.ReadString();

                int nPropsBits = 9;

                if (Demo.NetworkProtocol >= 8)
                {
                    nPropsBits = 10;
                }

                uint nProps = buffer.ReadUBits(nPropsBits);

                for (int i = 0; i < nProps; i++)
                {
                    Prop prop = new Prop();
                    table.PropDefinitions.Add(prop);
                    prop.Type = (Prop.Types)buffer.ReadUBits(5);

                    // Looks like an extra type was inserted in the middle.
                    // FIXME
                    if (Demo.NetworkProtocol == 15)
                    {
                        if ((uint)prop.Type == 6)
                        {
                            prop.Type = Prop.Types.DPT_DataTable;
                        }
                        else if (prop.Type == Prop.Types.DPT_DataTable)
                        {
                            prop.Type = Prop.Types.DPT_Array;
                        }
                        else if (prop.Type == Prop.Types.DPT_Array)
                        {
                            prop.Type = Prop.Types.DPT_String;
                        }
                    }

                    if (!Enum.IsDefined(typeof(Prop.Types), prop.Type))
                    {
                        throw new ApplicationException("Unknown prop type\'{0}\'".Args(prop.Type));
                    }

                    prop.Name = buffer.ReadString();
                    int nPropFlagsBits = 13;

                    if (Demo.NetworkProtocol >= 8)
                    {
                        nPropFlagsBits = 16;
                    }

                    prop.Flags = (Prop.FlagBits)buffer.ReadUBits(nPropFlagsBits);

                    if ((prop.Flags & Prop.FlagBits.Exclude) == Prop.FlagBits.Exclude)
                    {
                        prop.ExcludeProp = buffer.ReadString();
                    }
                    else if (prop.Type == Prop.Types.DPT_Array)
                    {
                        prop.NumElements = buffer.ReadUBits(10);
                    }
                    else if (prop.Type == Prop.Types.DPT_DataTable)
                    {
                        prop.DataTableName = buffer.ReadString();
                    }
                    else
                    {
                        // TODO: Verify. What's the difference between ReadBitFloat and ReadFloat in bf_read?
                        prop.Low = buffer.ReadFloat();
                        prop.High = buffer.ReadFloat();

                        int propNumBitsBits = 6;

                        if (Demo.NetworkProtocol == 15)
                        {
                            propNumBitsBits = 7;
                        }

                        prop.nBits = buffer.ReadUBits(propNumBitsBits);
                    }
                }
            }

            // Read ClassInfos.
            ClassInfos = new List<ClassInfo>();
            short nClassInfos = buffer.ReadShort();

            for (int i = 0; i < nClassInfos; i++)
            {
                ClassInfos.Add(new ClassInfo
                {
                    Index = buffer.ReadShort(),
                    ClassName = buffer.ReadString(),
                    DataTableName = buffer.ReadString()
                });
            }

            if (buffer.BitsLeft >= 8)
            {
                throw new ApplicationException("Failed to completely read frame data.");
            }
        }

        public override void Write(FastFileStream stream)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            if (SendTables != null)
            {
                log.WriteLine("Num sendtables: {0}", SendTables.Count);

                foreach (DataTable table in SendTables)
                {
                    log.WriteLine("uk1: {0}", table.Unknown1);
                    log.WriteLine("Name: {0}", table.Name);
                    log.WriteLine("Num props: {0}", table.PropDefinitions.Count);

                    foreach (Prop prop in table.PropDefinitions)
                    {
                        log.WriteLine("\tName: {0}", prop.Name);
                        log.WriteLine("\tType: {0}", prop.Type);
                        log.WriteLine("\tFlags: {0}", prop.Flags);

                        if (prop.Type == Prop.Types.DPT_DataTable)
                        {
                            log.WriteLine("\tData table: {0}", prop.DataTableName);
                        }

                        log.WriteLine();
                    }
                }
            }

            if (ClassInfos != null)
            {
                log.WriteLine("Num classinfos: {0}", ClassInfos.Count);

                foreach (ClassInfo classInfo in ClassInfos)
                {
                    log.WriteLine("Index: {0}", classInfo.Index);
                    log.WriteLine("Name: {0}", classInfo.ClassName);
                    log.WriteLine("SendTable: {0}", classInfo.DataTableName);
                }
            }
        }
    }
}
