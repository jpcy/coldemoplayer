using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    public class ClientCommand : Frame
    {
        public override byte Id
        {
            get { return (byte)FrameIds.ClientCommand; }
        }

        public string Command { get; set; }

        private const int commandLength = 64;

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(commandLength, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            BitReader bitReader = new BitReader(stream.ReadBytes(commandLength));
            Command = bitReader.ReadString();
        }

        public override void Write(FastFileStream stream)
        {
            BitWriter bitWriter = new BitWriter();
            bitWriter.WriteString(Command, commandLength);
            stream.WriteBytes(bitWriter.ToArray());
        }
    }
}
