using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class NetStringCmd : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.NET_StringCmd; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.NET_StringCmd; }
        }

        public override string Name
        {
            get { return "NET_StringCmd"; }
        }

        public string Command { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            Command = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Command: {0}", Command);
        }
    }
}
