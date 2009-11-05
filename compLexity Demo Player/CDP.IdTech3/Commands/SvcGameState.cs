using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3.Commands
{
    public class SvcGameState : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_gamestate; }
        }

        public override string Name
        {
            get { return "svc_gamestate"; }
        }

        public override bool IsSubCommand
        {
            get { return false; }
        }

        public override bool ContainsSubCommands
        {
            get { return true; }
        }

        public override bool HasFooter
        {
            get { return true; }
        }

        public int ServerCommandSequence { get; set; }
        public int ClientNumber { get; set; }
        public int Checksum { get; set; }

        public override void Read(BitReader buffer)
        {
            ServerCommandSequence = buffer.ReadInt();
        }

        public override void Write(Core.BitWriter buffer)
        {
            buffer.WriteInt(ServerCommandSequence);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Server command sequence: {0}", ServerCommandSequence);
        }

        public override void ReadFooter(BitReader buffer)
        {
            if (demo.Protocol >= Protocols.Protocol66)
            {
                ClientNumber = buffer.ReadInt();
                Checksum = buffer.ReadInt();
            }
        }

        public override void WriteFooter(Core.BitWriter buffer)
        {
            buffer.WriteInt(ClientNumber);
            buffer.WriteInt(Checksum);
        }

        public override void LogFooter(StreamWriter log)
        {
            log.WriteLine("Client number: {0}", ClientNumber);
            log.WriteLine("Checksum: {0}", Checksum);
        }
    }
}
