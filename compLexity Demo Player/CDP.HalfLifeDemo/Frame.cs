using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

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
        public virtual bool CanSkip
        {
            get { return true; }
        }

        public bool Remove { get; set; }

        protected uint networkProtocol;

        public void ReadHeader(BinaryReader br, uint networkProtocol)
        {
            this.networkProtocol = networkProtocol;
            Timestamp = br.ReadSingle();
            Number = br.ReadUInt32();
        }

        public void WriteHeader(BinaryWriter bw)
        {
            bw.Write(Id);
            bw.Write(Timestamp);
            bw.Write(Number);
        }

        public virtual void Skip(BinaryReader br) { }
        public virtual void Read(BinaryReader br) { }
        public virtual void Write(BinaryWriter bw) { }
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
