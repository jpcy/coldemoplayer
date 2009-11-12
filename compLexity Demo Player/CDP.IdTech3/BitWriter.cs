using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.IdTech3
{
    public class BitWriter : Core.BitWriter
    {
        private bool huffman;

        public BitWriter(bool huffman)
            : base(Message.MAX_MSGLEN)
        {
            this.huffman = huffman;
        }

        public override void WriteUBits(uint value, int nBits)
        {
            if (!huffman)
            {
                base.WriteUBits(value, nBits);
            }
            else
            {
                if (nBits < 0 || nBits > 32)
                {
                    throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
                }

                Huffman.WriteUInt(data, ref currentBit, value, nBits);
            }
        }

        public override void WriteBits(int value, int nBits)
        {
            // Convert to uint but keep the sign bit in the right place.
            uint newValue = (value < 0 ? (uint)-value : (uint)value);
            newValue |= (uint)(1 << (nBits - 1));
            WriteUBits(newValue, nBits);
        }

        public override void WriteBoolean(bool value)
        {
            WriteUBits(value ? 1u : 0u, 1);
        }

        public override void PadRemainingBitsInCurrentByte()
        {
            throw new InvalidOperationException();
        }
    }
}
