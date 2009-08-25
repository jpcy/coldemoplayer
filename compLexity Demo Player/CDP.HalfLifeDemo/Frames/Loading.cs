using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo.Frames
{
    public class Loading : MessageFrame
    {
        public override byte Id 
        {
            get { return (byte)FrameIds.Loading; } 
        }

        // Sequence info.
        // See: Quake, client/net.h
        public int IncomingSequence { get; set; }
        public int IncomingAcknowledged { get; set; }
        public int IncomingReliableAcknowledged { get; set; }
        public int IncomingReliableSequence { get; set; }
        public int OutgoingSequence { get; set; }
        public int ReliableSequence { get; set; }
        public int LastReliableSequence { get; set; }

        protected override void ReadContent(BinaryReader br)
        {
            // Demo info.
            if (networkProtocol <= 43)
            {
                br.BaseStream.Seek(532, SeekOrigin.Current);
            }
            else
            {
                br.BaseStream.Seek(436, SeekOrigin.Current);
            }

            // Sequence info.
            IncomingSequence = br.ReadInt32();
            IncomingAcknowledged = br.ReadInt32();
            IncomingReliableAcknowledged = br.ReadInt32();
            IncomingReliableSequence = br.ReadInt32();
            OutgoingSequence = br.ReadInt32();
            ReliableSequence = br.ReadInt32();
            LastReliableSequence = br.ReadInt32();

            // Message data.
            uint messageDataLength = br.ReadUInt32(); // TODO: error if 0?
            MessageData = br.ReadBytes((int)messageDataLength);
        }

        protected override byte[] WriteContent()
        {
            throw new NotImplementedException();
        }
    }
}
