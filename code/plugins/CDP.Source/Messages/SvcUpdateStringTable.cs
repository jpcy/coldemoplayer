using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcUpdateStringTable : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_UpdateStringTable; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_UpdateStringTable; }
        }

        public override string Name
        {
            get { return "SVC_UpdateStringTable"; }
        }

        public uint TableId { get; set; }
        public ushort? NumEntries { get; set; }
        public uint NumBits { get; set; }

        public override void Skip(BitReader buffer)
        {
            // Table ID.
            if (Demo.DemoProtocol <= 2)
            {
                buffer.SeekBits(4);
            }
            else
            {
                buffer.SeekBits(5);
            }

            if (buffer.ReadBoolean())
            {
                buffer.SeekBits(16); // NumEntries
            }

            uint nBits;

            if (Demo.NetworkProtocol >= 8)
            {
                nBits = buffer.ReadUBits(20);
            }
            else
            {
                nBits = buffer.ReadUBits(16);
            }

            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            if (Demo.DemoProtocol <= 2)
            {
                TableId = buffer.ReadUBits(4);
            }
            else
            {
                TableId = buffer.ReadUBits(5);
            }

            if (buffer.ReadBoolean())
            {
                NumEntries = buffer.ReadUShort();
            }

            if (Demo.NetworkProtocol >= 8)
            {
                NumBits = buffer.ReadUBits(20);
            }
            else
            {
                NumBits = buffer.ReadUBits(16);
            }

            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Table ID: {0}", TableId);
            log.WriteLine("Num. entries: {0}", NumEntries);
            log.WriteLine("Num. bits: {0}", NumBits);
        }
    }
}
