using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.IdTech3
{
    public class NetField
    {
        public string Name { get; private set; }
        public int Bits { get; private set; }
        public bool Signed { get; private set; }

        public NetField(string name, int bits)
            : this(name, bits, false)
        {
        }
        
        public NetField(string name, int bits, bool signed)
        {
            Name = name;
            Bits = bits;
            Signed = signed;
        }
    }
}
