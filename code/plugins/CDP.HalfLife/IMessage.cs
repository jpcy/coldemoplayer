using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLife
{
    public interface IMessage
    {
        byte Id { get; }
        string Name { get; }
        bool CanSkipWhenWriting { get; }
        Demo Demo { set; }
        bool Remove { get; set; }
        long Offset { get; set; }

        void Skip(BitReader buffer);
        void Read(BitReader buffer);
        void Write(BitWriter buffer);
        void Log(StreamWriter log);
    }
}
