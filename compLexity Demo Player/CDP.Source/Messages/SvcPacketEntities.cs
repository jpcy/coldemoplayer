using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcPacketEntities : Message
    {
        [Flags]
        private enum DeltaHeaderUpdateFlags
        {
	        Zero			= 0x0000,
	        LeavePvs		= 0x0001,
	        Delete			= 0x0002,
	        EnterPvs		= 0x0004,
	        ForceRecreate	= 0x0008
        }

        private enum UpdateTypes
        {
            EnterPvs,
            LeavePvs,
            DeltaEnt,
            PreserveEnt
        }

        public override MessageIds Id
        {
            get { return MessageIds.SVC_PacketEntities; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_PacketEntities; }
        }

        public override string Name
        {
            get { return "SVC_PacketEntities"; }
        }

        public uint Max { get; set; }
        public bool DeltaBit { get; set; }
        public int Delta { get; set; }
        public bool Baseline { get; set; }
        public uint Changed { get; set; }
        public uint NumBits { get; set; }
        public bool Unknown1 { get; set; } // always 0

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(11);

            if (buffer.ReadBoolean())
            {
                buffer.SeekBytes(4);
            }

            buffer.SeekBits(12);
            uint nBits = buffer.ReadUBits(20);
            buffer.SeekBits(1 + nBits);
        }

        public override void Read(BitReader buffer)
        {
            Max = buffer.ReadUBits(11);
            DeltaBit = buffer.ReadBoolean();

            if (DeltaBit)
            {
                Delta = buffer.ReadInt();
            }
            else
            {
                Delta = -1;
            }

            Baseline = buffer.ReadBoolean();
            Changed = buffer.ReadUBits(11);
            NumBits = buffer.ReadUBits(20);
            Unknown1 = buffer.ReadBoolean();
            buffer.SeekBits(NumBits);
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Max: {0}", Max);
            log.WriteLine("Delta: {0}", Delta);
            log.WriteLine("Baseline: {0}", Baseline);
            log.WriteLine("Changed: {0}", Changed);
            log.WriteLine("Num bits: {0}", NumBits);
            log.WriteLine("Unknown1: {0}\n", Unknown1);
        }
    }
}
