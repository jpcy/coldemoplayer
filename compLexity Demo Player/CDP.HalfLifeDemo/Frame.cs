using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo
{
    public abstract class Frame
    {
        public abstract byte Id { get; }
        public float Timestamp { get; private set; }
        public uint Number { get; private set; }
        public virtual bool HasMessages
        {
            get { return false; }
        }

        protected uint networkProtocol;

        public void Read(BinaryReader br, uint networkProtocol)
        {
            this.networkProtocol = networkProtocol;
            Timestamp = br.ReadSingle();
            Number = br.ReadUInt32();
            ReadContent(br);
        }

        public byte[] Write()
        {
            // TODO
            return null;
        }

        protected virtual void ReadContent(BinaryReader br) { }
        protected virtual byte[] WriteContent() { return null; }
    }

    public enum FrameIds : byte
    {
        Loading,
        Playback,
        PlaybackSegmentStart,
        ClientCommand,
        ClientData,
        EndOfSegment,
        Unknown,
        WeaponChange,
        PlaySound,
        ModData
    }
}
