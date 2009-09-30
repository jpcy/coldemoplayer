using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
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

        public override void Skip(BinaryReader br)
        {
            br.BaseStream.Seek(4, SeekOrigin.Current);
            uint length = br.ReadUInt32();
            br.BaseStream.Seek(length + 16, SeekOrigin.Current);
        }

        public override void Read(BinaryReader br)
        {
            Unknown1 = br.ReadInt32();
            FileNameLength = br.ReadUInt32();
            FileName = br.ReadBytes((int)FileNameLength);
            Unknown2 = br.ReadBytes(16);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(FileNameLength); // should be FileName.Length?
            bw.Write(FileName);
            bw.Write(Unknown2);
        }
    }
}
