using System;
using System.IO;

namespace CDP.HalfLife.UserMessages
{
    public class TeamScore : HalfLife.UserMessage
    {
        public override string Name
        {
            get { return "TeamScore"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public string TeamName { get; set; }
        public short Score { get; set; }

        public override void Read(BitReader buffer)
        {
            TeamName = buffer.ReadString();
            Score = buffer.ReadShort();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteString(TeamName);
            buffer.WriteShort(Score);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Team name: {0}", TeamName);
            log.WriteLine("Score: {0}", Score);
        }
    }
}
