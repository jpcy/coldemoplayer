using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLife
{
    public abstract class UserMessage : IMessage
    {
        public byte Id { get; set; }
        public byte Length { get; set; }
        public abstract string Name { get; }
        public abstract bool CanSkipWhenWriting { get; }

        public Demo Demo
        {
            set { demo = value; }
        }

        public bool Remove { get; set; }
        public long Offset { get; set; }

        protected Demo demo;

        public void Skip(BitReader buffer)
        {
            buffer.SeekBytes(Length);
        }

        public abstract void Read(BitReader buffer);
        public abstract void Write(BitWriter buffer);
        public abstract void Log(StreamWriter log);
    }
}
