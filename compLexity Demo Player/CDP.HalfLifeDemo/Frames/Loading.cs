using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo.Frames
{
    public class Loading : MessageFrame
    {
        public override byte Id 
        {
            get { return (byte)FrameIds.Loading; } 
        }

        public byte[] DemoInfo { get; set; }

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
            int demoInfoLength;

            if (networkProtocol <= 43)
            {
                demoInfoLength = 532;
            }
            else
            {
                demoInfoLength = 436;
            }

            DemoInfo = br.ReadBytes(demoInfoLength);

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

            if (messageDataLength > 0)
            {
                MessageData = br.ReadBytes((int)messageDataLength);
            }
        }

        protected override byte[] WriteContent()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteBytes(DemoInfo);
            buffer.WriteInt(IncomingSequence);
            buffer.WriteInt(IncomingAcknowledged);
            buffer.WriteInt(IncomingReliableAcknowledged);
            buffer.WriteInt(IncomingReliableSequence);
            buffer.WriteInt(OutgoingSequence);
            buffer.WriteInt(ReliableSequence);
            buffer.WriteInt(LastReliableSequence);

            if (MessageData == null)
            {
                buffer.WriteUInt(0);
            }
            else
            {
                buffer.WriteUInt((uint)MessageData.Length);
                buffer.WriteBytes(MessageData);
            }

            return buffer.ToArray();
        }
    }
}
