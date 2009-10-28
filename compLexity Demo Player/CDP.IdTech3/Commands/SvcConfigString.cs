using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3.Commands
{
    public class SvcConfigString : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_configstring; }
        }

        public override string Name
        {
            get { return "svc_configstring"; }
        }

        public override bool IsSubCommand
        {
            get { return true; }
        }

        public override bool ContainsSubCommands
        {
            get { return false; }
        }

        public short Index { get; set; }
        public string Value { get; set; }

        public override void Read(BitReader buffer)
        {
            Index = buffer.ReadShort();
            Value = buffer.ReadString();
        }

        public override void Write(Core.BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Value: {0}", Value);
        }
    }
}
