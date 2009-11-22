using System;

namespace CDP.HalfLife
{
    public class BitReader : Core.BitReader
    {
        public enum Endians
        {
            Little,
            Big
        }

        public Endians Endian { get; set; }

        public BitReader(byte[] buffer)
            : base(buffer)
        {
            Endian = Endians.Little;
        }

        // HL 1.1.0.6 bit reading (big endian byte and bit order)
        private uint ReadUBitsBigEndian(int nBits)
        {
            int bitOffset = currentBit % 8;
            int nBitsToRead = bitOffset + nBits;
            int nBytesToRead = nBitsToRead / 8 + (nBitsToRead % 8 != 0 ? 1 : 0);

            // get bytes we need
            ulong currentValue = 0;
            for (int i = 0; i < nBytesToRead; i++)
            {
                byte b = Buffer[CurrentByte + (nBytesToRead - 1) - i];
                currentValue += (ulong)((ulong)b << (i * 8));
            }

            // get bits we need from bytes
            currentValue >>= ((nBytesToRead * 8 - bitOffset) - nBits);
            currentValue &= (uint)(((ulong)1 << nBits) - 1);

            // increment current bit
            currentBit += nBits;

            return (uint)currentValue;
        }

        public virtual bool ReadBooleanBigEndian()
        {
            // check for overflow
            if (currentBit + 1 > Length * 8)
            {
                throw new OutOfRangeException();
            }

            int currentByte = currentBit / 8;
            int bitOffset = currentBit % 8;
            bool result = (Buffer[currentByte] & (128 >> bitOffset)) == 0 ? false : true;
            currentBit++;
            return result;
        }

        protected override uint ReadUBitsByteAligned(int nBits)
        {
            if (Endian == Endians.Little)
            {
                return base.ReadUBitsByteAligned(nBits);
            }
            else
            {
                return ReadUBitsBigEndian(nBits);
            }
        }

        protected override uint ReadUBitsNotByteAligned(int nBits)
        {
            if (Endian == Endians.Little)
            {
                return base.ReadUBitsNotByteAligned(nBits);
            }
            else
            {
                return ReadUBitsBigEndian(nBits);
            }
        }

        public override bool ReadBoolean()
        {
            if (Endian == Endians.Little)
            {
                return base.ReadBoolean();
            }
            else
            {
                return ReadBooleanBigEndian();
            }
        }

        public float[] ReadVectorCoord()
        {
            bool xFlag = ReadBoolean();
            bool yFlag = ReadBoolean();
            bool zFlag = ReadBoolean();

            float[] result = new float[3];

            if (xFlag)
            {
                result[0] = ReadBitCoord();
            }

            if (yFlag)
            {
                result[1] = ReadBitCoord();
            }

            if (zFlag)
            {
                result[2] = ReadBitCoord();
            }

            return result;
        }

        public float ReadCoord()
        {
            return ReadShort() / 8.0f;
        }

        public float ReadBitCoord()
        {
            bool intFlag = ReadBoolean();
            bool fractionFlag = ReadBoolean();

            float value = 0.0f;

            if (!intFlag && !fractionFlag)
            {
                return value;
            }

            bool sign = ReadBoolean();
            uint intValue = 0;
            uint fractionValue = 0;

            if (intFlag)
            {
                intValue = ReadUBits(12);
            }

            if (fractionFlag)
            {
                fractionValue = ReadUBits(3);
            }

            value = intValue + ((float)fractionValue * 1.0f / 32.0f);

            if (sign)
            {
                value = -value;
            }

            return value;
        }

        public float ReadHiresAngle()
        {
            // short * 360 / 2 ^ 16
            return ReadShort() * 0.0054931640625f;
        }
    }
}
