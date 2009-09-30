using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo
{
    public interface IMessage
    {
        byte Id { get; }
        string Name { get; }
        bool CanSkipWhenWriting { get; }
        Demo Demo { set; }
        bool Remove { get; set; }

        void Skip(Core.BitReader buffer);
        void Read(Core.BitReader buffer);
        void Write(Core.BitWriter buffer);
        void Log(StreamWriter log);
    }
}
