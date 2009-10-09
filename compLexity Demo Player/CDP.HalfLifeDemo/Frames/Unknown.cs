using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    public class Unknown : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.Unknown; }
        }

        public byte[] Data { get; set; }

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(84, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            Data = stream.ReadBytes(84);
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteBytes(Data);
        }
    }
}
