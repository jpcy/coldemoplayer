using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    // Something to do with weapon change animations.
    // Non-critical -- can be removed, which results in no weapon change animations (like HLTV/HLTV models).
    public class WeaponChange : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.WeaponChange; }
        }

        public byte[] Data { get; set; }

        public override void Skip(BinaryReader br)
        {
            br.BaseStream.Seek(8, SeekOrigin.Current);
        }

        public override void Read(BinaryReader br)
        {
            Data = br.ReadBytes(8);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Data);
        }
    }
}
