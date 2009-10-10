using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLife.Frames
{
    public class PlaySound : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.PlaySound; }
        }

        public int Unknown1 { get; set; }
        public uint FileNameLength { get; set; } // equal to filename length or contains padding?
        public byte[] FileName { get; set; }
        public byte[] Unknown2 { get; set; }

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(4, SeekOrigin.Current);
            uint length = stream.ReadUInt();
            stream.Seek(length + 16, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            Unknown1 = stream.ReadInt();
            FileNameLength = stream.ReadUInt();
            FileName = stream.ReadBytes((int)FileNameLength);
            Unknown2 = stream.ReadBytes(16);
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteInt(Unknown1);
            stream.WriteUInt(FileNameLength); // should be FileName.Length?
            stream.WriteBytes(FileName);
            stream.WriteBytes(Unknown2);
        }
    }
}
