using System;
using System.IO;

namespace CDP.HalfLife.UserMessages
{
    public class TeamInfo : UserMessage
    {
        public override string Name
        {
            get { return "TeamInfo"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte Slot { get; set; }
        public string TeamName { get; set; }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            TeamName = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Slot);
            buffer.WriteString(TeamName);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);
            log.WriteLine("TeamName: {0}", TeamName);
        }
    }
}
