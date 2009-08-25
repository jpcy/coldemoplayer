using System;
using System.IO;
using BitReader = CDP.Core.BitReader;

namespace CDP.HalfLifeDemo.Messages
{
    class UnregisteredUserMessage : UserMessage
    {
        public override string Name
        {
            get { return name; }
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

        public override byte[] Write()
        {
            return data;
        }

#if DEBUG
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
#endif
    }
}
