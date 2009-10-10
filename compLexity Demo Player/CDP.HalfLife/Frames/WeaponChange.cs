using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLife.Frames
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

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(8, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            Data = stream.ReadBytes(8);
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteBytes(Data);
        }
    }
}
