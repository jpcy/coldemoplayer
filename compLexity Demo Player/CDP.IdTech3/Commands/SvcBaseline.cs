using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3.Commands
{
    public class SvcBaseline : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_baseline; }
        }

        public override string Name
        {
            get { return "svc_baseline"; }
        }

        public override bool IsSubCommand
        {
            get { return true; }
        }

        public override bool ContainsSubCommands
        {
            get { return false; }
        }

        public Entity Entity { get; set; }

        public override void Read(BitReader buffer)
        {
            Entity = new Entity(demo.Protocol);
            Entity.Number = buffer.ReadUBits(Entity.GENTITYNUM_BITS);
            Entity.Read(buffer);
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUBits(Entity.Number, Entity.GENTITYNUM_BITS);

            if (demo.Protocol == demo.ConvertTargetProtocol)
            {
                Entity.Write(buffer);
            }
            else
            {
                Entity newEntity = new Entity(demo.ConvertTargetProtocol, Entity);
                newEntity.Write(buffer);
            }
        }

        public override void Log(StreamWriter log)
        {
            if (Entity != null)
            {
                Entity.Log(log);
            }
        }
    }
}
