using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo
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

        protected Demo demo;

        public void Skip(Core.BitReader buffer)
        {
            buffer.SeekBytes(Length);
        }

        public abstract void Read(Core.BitReader buffer);
        public abstract void Write(Core.BitWriter buffer);
        public abstract void Log(StreamWriter log);
    }
}
