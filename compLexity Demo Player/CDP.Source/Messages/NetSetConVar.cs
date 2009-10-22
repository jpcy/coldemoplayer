using System;
using System.IO;
using System.Collections.Generic;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class NetSetConVar : Message
    {
        public class ConsoleVarible
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public ConsoleVarible(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        public override MessageIds Id
        {
            get { return MessageIds.NET_SetConVar; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.NET_SetConVar; }
        }

        public override string Name
        {
            get { return "NET_SetConVar"; }
        }

        public List<ConsoleVarible> ConsoleVariables { get; set; }

        public override void Skip(BitReader buffer)
        {
            byte nConVars = buffer.ReadByte();

            for (int i = 0; i < nConVars; i++)
            {
                buffer.SeekString();
                buffer.SeekString();
            }
        }

        public override void Read(BitReader buffer)
        {
            ConsoleVariables = new List<ConsoleVarible>();
            byte nConVars = buffer.ReadByte();

            for (int i = 0; i < nConVars; i++)
            {
                string name = buffer.ReadString();
                string value = buffer.ReadString();
                ConsoleVariables.Add(new ConsoleVarible(name, value));
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            if (ConsoleVariables != null)
            {
                log.WriteLine("Count: {0}", ConsoleVariables.Count);

                foreach (ConsoleVarible cvar in ConsoleVariables)
                {
                    log.WriteLine("Name: {0}", cvar.Name);
                    log.WriteLine("Value: {0}", cvar.Value);
                }
            }
        }
    }
}
