using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Source.Frames
{
    public class UserCommand : Frame
    {
        public override FrameIds Id
        {
            get { return FrameIds.UserCommand; }
        }

        public override FrameIds_Protocol36 Id_Protocol36
        {
            get { return FrameIds_Protocol36.UserCommand; }
        }

        public int OutgoingSequence { get; set; }
        public byte[] Data { get; set; }

        public override void Skip(FastFileStream stream)
        {
            stream.Seek(sizeof(int), SeekOrigin.Current);
            int length = stream.ReadInt();
            stream.Seek(length, SeekOrigin.Current);
        }

        public override void Read(FastFileStream stream)
        {
            OutgoingSequence = stream.ReadInt();
            int length = stream.ReadInt();

            if (length > 0)
            {
                Data = stream.ReadBytes(length);
            }
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteInt(OutgoingSequence);

            if (Data != null && Data.Length > 0)
            {
                stream.WriteInt(Data.Length);
                stream.WriteBytes(Data);
            }
            else
            {
                stream.WriteInt(0);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Outgoing sequence: {0}", OutgoingSequence);

            if (Data != null)
            {
                log.WriteLine("Data length: {0}", Data.Length);
            }
        }
    }
}
