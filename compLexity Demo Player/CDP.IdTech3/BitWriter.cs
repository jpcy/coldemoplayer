using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.IdTech3
{
    public class BitWriter : Core.BitWriter
    {
        private const int FLOAT_INT_BITS = 13;
        private const int FLOAT_INT_BIAS = (1 << (FLOAT_INT_BITS - 1));

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
            WriteUBits((uint)value, nBits);
        }

        public override void WriteBoolean(bool value)
        {
            WriteUBits(value ? 1u : 0u, 1);
        }

        public override void PadRemainingBitsInCurrentByte()
        {
            throw new InvalidOperationException();
        }

        public void WriteIntegralFloat(float value)
        {
            WriteUBits((uint)(value + FLOAT_INT_BIAS), FLOAT_INT_BITS);
        }

        /// <summary>
        /// Writes an integral float if possible, otherwise a full-precision float is written. A leading bit determines whether the following value is integral (false) or full precision (true).
        /// </summary>
        /// <param name="value"></param>
        public void WriteIntegralFloatMaybe(float value)
        {
            int trunc = (int)value;

            if (trunc == value && trunc + FLOAT_INT_BIAS >= 0 && trunc + FLOAT_INT_BIAS < (1 << FLOAT_INT_BITS))
            {
                WriteBoolean(false);
                WriteIntegralFloat(value);
            }
            else
            {
                WriteBoolean(true);
                WriteFloat(value);
            } 
        }

        public void WriteDeltaFloat(float value)
        {
            if (value != 0.0f)
            {
                WriteBoolean(true);
                WriteIntegralFloatMaybe(value);
            }
            else
            {
                WriteBoolean(false);
            }
        }

        public void WriteDeltaBits(int value, int nBits)
        {
            if (value != 0)
            {
                WriteBoolean(true);
                WriteBits(value, nBits);
            }
            else
            {
                WriteBoolean(false);
            }
        }

        public void WriteDeltaUBits(uint value, int nBits)
        {
            if (value != 0)
            {
                WriteBoolean(true);
                WriteUBits(value, nBits);
            }
            else
            {
                WriteBoolean(false);
            }
        }
    }
}
