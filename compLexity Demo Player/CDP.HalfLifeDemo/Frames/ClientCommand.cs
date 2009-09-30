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

        public override void Skip(BinaryReader br)
        {
            br.BaseStream.Seek(commandLength, SeekOrigin.Current);
        }

        public override void Read(BinaryReader br)
        {
            Core.BitReader bitReader = new Core.BitReader(br.ReadBytes(commandLength));
            Command = bitReader.ReadString();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Command);
        }
    }
}
