using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.UserMessages
{
    public class TeamInfo : UserMessage
    {
        public override string Name
        {
            get { return "TeamInfo"; }
        }

        public byte Slot { get; set; }
        public string TeamName { get; set; }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            TeamName = buffer.ReadString();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
