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

        protected override void ReadContent(BinaryReader br)
        {
            Core.BitReader bitReader = new Core.BitReader(br.ReadBytes(commandLength));
            Command = bitReader.ReadString();
        }

        protected override byte[] WriteContent()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(Command);
            return buffer.ToArray();
        }
    }
}
