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
        Demo Demo { set; }

        void Read(Core.BitReader buffer);
        byte[] Write();
#if DEBUG
        void Log(StreamWriter log);
#endif
    }
}
