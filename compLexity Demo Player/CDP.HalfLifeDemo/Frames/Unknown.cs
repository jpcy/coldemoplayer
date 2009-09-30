using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    public class Unknown : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.Unknown; }
        }

        public byte[] Data { get; set; }

        public override void Skip(BinaryReader br)
        {
            br.BaseStream.Seek(84, SeekOrigin.Current);
        }

        public override void Read(BinaryReader br)
        {
            Data = br.ReadBytes(84);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Data);
        }
    }
}
