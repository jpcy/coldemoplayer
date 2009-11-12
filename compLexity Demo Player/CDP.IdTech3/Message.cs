using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3
{
    public class Message
    {
        public const int MAX_MSGLEN = 16384;

        private long offset;

        public int SequenceNumber { get; set; }
        public int Length { get; set; }
        public BitReader Reader { get; set; }
        public int ReliableAck { get; set; }

        public void Read(Core.FastFileStream stream, Protocols protocol)
        {
            offset = stream.Position;
            SequenceNumber = stream.ReadInt();
            Length = stream.ReadInt();

            if (Length == -1)
            {
                // End of file.
                return;
            }

            if (Length < 0 || Length > MAX_MSGLEN)
            {
                throw new ApplicationException(string.Format("Message length \'{0}\' is out of range. Minimum is \'0\', maximum is \'{1}\'.", Length, MAX_MSGLEN));
            }

            Reader = new BitReader(stream.ReadBytes(Length), protocol >= Protocols.Protocol66);

            // Doesn't exist in protocols 43 and 45.
            if (protocol >= Protocols.Protocol48)
            {
                ReliableAck = Reader.ReadInt();
            }
        }

        public void Write(Core.FastFileStream stream)
        {
            stream.WriteInt(SequenceNumber);
            stream.WriteInt(Length);

            // End of file marker.
            if (SequenceNumber != -1 && Length != -1)
            {
                stream.WriteInt(ReliableAck);
            }
        }

        public void Log(StreamWriter log)
        {
            log.WriteLine("\n=== Message {0}, Offset: {1}, Length {2} ===", SequenceNumber, offset, Length);
            log.WriteLine("Reliable ack: {0}", ReliableAck);
        }
    }
}
