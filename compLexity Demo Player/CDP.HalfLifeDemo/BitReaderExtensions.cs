using System;
using CDP.Core;
namespace CDP.HalfLifeDemo
{
    static class BitReaderExtensions
    {
        public static float[] ReadVectorCoord(this BitReader bitReader)
        {
            bool xFlag = bitReader.ReadBoolean();
            bool yFlag = bitReader.ReadBoolean();
            bool zFlag = bitReader.ReadBoolean();

            float[] result = new float[3];

            if (xFlag)
            {
                result[0] = bitReader.ReadBitCoord();
            }

            if (yFlag)
            {
                result[1] = bitReader.ReadBitCoord();
            }

            if (zFlag)
            {
                result[2] = bitReader.ReadBitCoord();
            }

            return result;
        }

        public static float ReadCoord(this BitReader bitReader)
        {
            return bitReader.ReadShort() / 8.0f;
        }

        public static float ReadBitCoord(this BitReader bitReader)
        {
            bool intFlag = bitReader.ReadBoolean();
            bool fractionFlag = bitReader.ReadBoolean();

            float value = 0.0f;

            if (!intFlag && !fractionFlag)
            {
                return value;
            }

            bool sign = bitReader.ReadBoolean();
            uint intValue = 0;
            uint fractionValue = 0;

            if (intFlag)
            {
                intValue = bitReader.ReadUBits(12);
            }

            if (fractionFlag)
            {
                fractionValue = bitReader.ReadUBits(3);
            }

            value = intValue + ((float)fractionValue * 1.0f / 32.0f);

            if (sign)
            {
                value = -value;
            }

            return value;
        }

        public static float ReadHiresAngle(this BitReader bitReader)
        {
            // short * 360 / 2 ^ 16
            return bitReader.ReadShort() * 0.0054931640625f;
        }
    }
}
