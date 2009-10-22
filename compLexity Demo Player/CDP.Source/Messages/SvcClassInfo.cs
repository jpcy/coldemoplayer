using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcClassInfo : Message
    {
        public class Class
        {
            // TODO: Probably the same as ClassInfos in the DataTables frame, needs verifying.
            public uint Index { get; set; }
            public string Unknown1 { get; set; }
            public string Unknown2 { get; set; }
        }

        public override MessageIds Id
        {
            get { return MessageIds.SVC_ClassInfo; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_ClassInfo; }
        }

        public override string Name
        {
            get { return "SVC_ClassInfo"; }
        }

        public short NumClasses { get; set; }
        public List<Class> Classes { get; set; }

        public override void Skip(BitReader buffer)
        {
            short num = buffer.ReadShort();

            if (!buffer.ReadBoolean()) // bit: "use client classes"
            {
                uint indexBits = Core.Math.LogBase2((uint)num) + 1;

                for (int i = 0; i < num; i++)
                {
                    buffer.SeekBits(indexBits);
                    buffer.SeekString();
                    buffer.SeekString();
                }
            }
        }

        public override void Read(BitReader buffer)
        {
            NumClasses = buffer.ReadShort();

            if (!buffer.ReadBoolean()) // bit: "use client classes"
            {
                Classes = new List<Class>();
                uint indexBits = Core.Math.LogBase2((uint)NumClasses) + 1;

                for (int i = 0; i < NumClasses; i++)
                {
                    Classes.Add(new Class
                    {
                        Index = buffer.ReadUBits(indexBits),
                        Unknown1 = buffer.ReadString(),
                        Unknown2 = buffer.ReadString()
                    });
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num classes: {0}", NumClasses);

            if (Classes != null)
            {
                foreach (Class c in Classes)
                {
                    log.WriteLine("Class {0}: \'{1}\' \'{2}\'", c.Index, c.Unknown1, c.Unknown2);
                }
            }
        }
    }
}
