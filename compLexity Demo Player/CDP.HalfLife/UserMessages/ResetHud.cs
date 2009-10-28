using System;
using System.IO;

namespace CDP.HalfLife.UserMessages
{
    public class ResetHud : UserMessage
    {
        public override string Name
        {
            get { return "ResetHUD"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public byte[] Data { get; set; }

        public override void Read(BitReader buffer)
        {
            if (Length > 0)
            {
                Data = buffer.ReadBytes(Length);
            }
        }

        public override void Write(BitWriter buffer)
        {
            if (Data != null)
            {
                buffer.WriteBytes(Data);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Data length: {0}", (Data == null ? 0 : Data.Length));
        }
    }
}
