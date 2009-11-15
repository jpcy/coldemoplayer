using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3.Commands
{
    public class SvcServerCommand : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_servercommand; }
        }

        public override string Name
        {
            get { return "svc_servercommand"; }
        }

        public override bool IsSubCommand
        {
            get { return false; }
        }

        public override bool ContainsSubCommands
        {
            get { return false; }
        }

        public int Sequence { get; set; }
        public string Command { get; set; }

        public override void Read(BitReader buffer)
        {
            Sequence = buffer.ReadInt();
            Command = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteInt(Sequence);
            buffer.WriteString(Command);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Sequence: {0}", Sequence);
            log.WriteLine("Command: {0}", Command);
        }
    }
}
