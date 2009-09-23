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

        protected override void ReadContent(BinaryReader br)
        {
            Unknown1 = br.ReadInt32();
            FileNameLength = br.ReadUInt32();
            FileName = br.ReadBytes((int)FileNameLength);
            Unknown2 = br.ReadBytes(16);
        }

        protected override byte[] WriteContent()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteInt(Unknown1);
            buffer.WriteUInt(FileNameLength); // should be FileName.Length?
            buffer.WriteBytes(FileName);
            buffer.WriteBytes(Unknown2);
            return buffer.ToArray();
        }
    }
}
