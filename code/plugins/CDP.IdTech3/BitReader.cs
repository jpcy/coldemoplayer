using System;
using System.IO;

namespace CDP.IdTech3
{
    public class BitReader : Core.BitReader
    {
        private const int FLOAT_INT_BITS = 13;
        private const int FLOAT_INT_BIAS = (1 << (FLOAT_INT_BITS - 1));
        private bool huffman;

        public BitReader(byte[] buffer, bool huffman)
            : base(buffer)
        {
            this.huffman = huffman;
        }

        public override uint ReadUBits(int nBits)
        {
            if (!huffman)
            {
                return base.ReadUBits(nBits);
            }

            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // Don't check for overflow. Only the huffman algorithm knows how many bits need to be read (if will most likely be less than nBits).

            return ReadUBitsNotByteAligned(nBits);
        }

        public override int ReadBits(int nBits)
        {
            return (int)ReadUBits(nBits);
        }

        public override bool ReadBoolean()
        {
            return ReadUBits(1) == 1;
        }

        public override void SeekBits(int offset, SeekOrigin origin)
        {
            if (huffman)
            {
                throw new ApplicationException("Cannot seek within a huffman compressed BitReader.");
            }
            else
            {
                base.SeekBits(offset, origin);
            }
        }

        protected override uint ReadUBitsNotByteAligned(int nBits)
        {
            if (!huffman)
            {
                return base.ReadUBitsNotByteAligned(nBits);
            }

            uint result = Huffman.ReadUInt(Buffer, ref currentBit, nBits);
            return result;
        }

        protected override uint ReadUBitsByteAligned(int nBits)
        {
            if (!huffman)
            {
                return base.ReadUBitsByteAligned(nBits);
            }

            return ReadUBitsNotByteAligned(nBits);
        }

        public float ReadIntegralFloat()
        {
            return (int)ReadUBits(FLOAT_INT_BITS) - FLOAT_INT_BIAS;
        }

        public float ReadDeltaFloat()
        {
            if (ReadBoolean())
            {
                return ReadDeltaFloatOld();
            }
            else
            {
                return 0.0f;
            }
        }

        public float ReadDeltaFloatOld()
        {
            if (ReadBoolean())
            {
                return ReadFloat();
            }
            else
            {
                return ReadIntegralFloat();
            }
        }

        public int ReadDeltaBits(int nBits)
        {
            if (ReadBoolean())
            {
                return ReadBits(nBits);
            }
            else
            {
                return 0;
            }
        }

        public uint ReadDeltaUBits(int nBits)
        {
            if (ReadBoolean())
            {
                return ReadUBits(nBits);
            }
            else
            {
                return 0;
            }
        }
    }
}
