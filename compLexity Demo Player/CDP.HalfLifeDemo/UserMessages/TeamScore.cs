using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.UserMessages
{
    public class TeamScore : HalfLifeDemo.UserMessage
    {
        public override string Name
        {
            get { return "TeamScore"; }
        }

        public string TeamName { get; set; }
        public short Score { get; set; }

        public override void Read(BitReader buffer)
        {
            TeamName = buffer.ReadString();
            Score = buffer.ReadShort();
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
