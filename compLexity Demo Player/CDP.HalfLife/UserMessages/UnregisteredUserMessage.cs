using System;
using System.IO;

namespace CDP.HalfLife.UserMessages
{
    public class UnregisteredUserMessage : UserMessage
    {
        public override string Name
        {
            get { return name; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        private string name;
        private byte[] data;

        public UnregisteredUserMessage(string name)
        {
            this.name = name;
        }

        public override void Read(BitReader buffer)
        {
            if (Length > 0)
            {
                data = buffer.ReadBytes(Length);
            }
            else if (Length != 0)
            {
                throw new ApplicationException(string.Format("Bad user message length \"{0}\".", Length));
            }
        }

        public override void Write(BitWriter buffer)
        {
        }

        public override void Log(StreamWriter log)
        {
            if (data == null)
            {
                return;
            }

            log.Write("Data:");

            for (int i = 0; i < data.Length; i++)
            {
                log.Write(" {0}", data[i].ToString("X2"));
            }

            log.WriteLine();
        }
    }
}
