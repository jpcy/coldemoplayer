using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CDP.Core;

namespace CDP.Source
{
    public abstract class MessageFrame : Frame
    {
        public override bool HasMessages
        {
            get { return true; }
        }

        public int Flags { get; set; }
        public Vector ViewOrigin { get; set; }
        public Vector ViewAngles { get; set; }
        public Vector LocalViewAngles { get; set; }
        public Vector ViewOriginResampled { get; set; }
        public Vector ViewAnglesResampled { get; set; }
        public Vector LocalViewAnglesResampled { get; set; }
        public int SequenceIn { get; set; }
        public int SequenceOut { get; set; }

        private const int length = 84;
        private const int lengthProtocol36 = 312;

        public override void Skip(FastFileStream stream)
        {
            if (Demo.NetworkProtocol >= 36)
            {
                stream.Seek(lengthProtocol36, SeekOrigin.Current);
            }
            else
            {
                stream.Seek(length, SeekOrigin.Current);
            }
        }

        public override void Read(FastFileStream stream)
        {
            Flags = stream.ReadInt();
            ViewOrigin = stream.ReadVector();
            ViewAngles = stream.ReadVector();
            LocalViewAngles = stream.ReadVector();
            ViewOriginResampled = stream.ReadVector();
            ViewAnglesResampled = stream.ReadVector();
            LocalViewAnglesResampled = stream.ReadVector();

            if (Demo.NetworkProtocol >= 36)
            {
                stream.Seek(lengthProtocol36 - length, SeekOrigin.Current);
            }

            SequenceIn = stream.ReadInt();
            SequenceOut = stream.ReadInt();
        }

        public override void Write(FastFileStream stream)
        {
            stream.WriteInt(Flags);
            stream.WriteVector(ViewOrigin);
            stream.WriteVector(ViewAngles);
            stream.WriteVector(LocalViewAngles);
            stream.WriteVector(ViewOriginResampled);
            stream.WriteVector(ViewAnglesResampled);
            stream.WriteVector(LocalViewAnglesResampled);
            stream.WriteInt(SequenceIn);
            stream.WriteInt(SequenceOut);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Flags: {0}", Flags);
            log.WriteLine("View origin: {0}", ViewOrigin);
            log.WriteLine("View angles: {0}", ViewAngles);
            log.WriteLine("Local view angles: {0}", LocalViewAngles);
            log.WriteLine("View origin (resampled): {0}", ViewOriginResampled);
            log.WriteLine("View angles (resampled): {0}", ViewAnglesResampled);
            log.WriteLine("Local view angles (resampled): {0}", LocalViewAnglesResampled);
            log.WriteLine("Sequence in: {0}", SequenceIn);
            log.WriteLine("Sequence out: {0}", SequenceOut);
        }
    }
}
