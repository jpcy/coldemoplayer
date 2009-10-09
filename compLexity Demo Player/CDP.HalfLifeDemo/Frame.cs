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

        protected Demo demo;
        public Demo Demo
        {
            set { demo = value; }
        }

        public bool Remove { get; set; }

        protected uint networkProtocol;

        public void ReadHeader(FastFileStream stream, uint networkProtocol)
        {
            this.networkProtocol = networkProtocol;
            Timestamp = stream.ReadFloat();
            Number = stream.ReadUInt();
        }

        public void WriteHeader(FastFileStream stream)
        {
            stream.WriteByte(Id);
            stream.WriteFloat(Timestamp);
            stream.WriteUInt(Number);
        }

        public virtual void Skip(FastFileStream stream) { }
        public virtual void Read(FastFileStream stream) { }
        public virtual void Write(FastFileStream stream) { }
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
