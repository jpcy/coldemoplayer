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
        public sbyte Length { get; set; }
        public abstract string Name { get; }

        public Demo Demo
        {
            set { demo = value; }
        }

        protected Demo demo;

        public abstract void Read(Core.BitReader buffer);
        public abstract byte[] Write();
#if DEBUG
        public abstract void Log(StreamWriter log);
#endif
    }
}
