using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Source.Frames
{
    public class ConsoleCommand : Frame
    {
        public override FrameIds Id
        {
            get { return FrameIds.ConsoleCommand; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.ConsoleCommand; }
        }

        public int CommandLength { get; set; }
        public string Command { get; set; }

        public override void Skip(FastFileStream stream)
        {
            int length = stream.ReadInt();
            stream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            CommandLength = stream.ReadInt();
            Command = stream.ReadString();

            if (CommandLength != Command.Length + 1)
            {
                throw new ApplicationException("Console command length doesn't match actual command string length.");
            }
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteInt(CommandLength);
            stream.WriteString(Command);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine(Command);
        }
    }
}
