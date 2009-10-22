using System;
using System.Collections.Generic;
using System.Linq;
using CDP.Core;

namespace CDP.Source
{
    static internal class BitReaderExtensions
    {
        private const int COORD_INTEGER_BITS = 14;
        private const int COORD_FRACTIONAL_BITS = 5;
        private const int COORD_DENOMINATOR = 1 << COORD_FRACTIONAL_BITS;
        private const float COORD_RESOLUTION = 1.0f / COORD_DENOMINATOR;

        public static int ReadUBitVar(this BitReader self)
        {
            int nBits = 0;

            while (!self.ReadBoolean())
            {
                nBits++;
            }

            int data = (1 << nBits) - 1;

            if (nBits > 0)
            {
                data += (int)self.ReadUBits(nBits);
            }

            return data;
        }

        public static float ReadBitAngle(this BitReader self, int nBits)
        {
            return self.ReadUBits(nBits) * (360.0f / (float)(1 << nBits));
        }

        public static Vector ReadBitVectorCoord(this BitReader self)
        {
            bool xFlag = self.ReadBoolean();
            bool yFlag = self.ReadBoolean();
            bool zFlag = self.ReadBoolean();

            return new Vector
            {
                X = (xFlag ? self.ReadBitCoord() : 0.0f),
                Y = (yFlag ? self.ReadBitCoord() : 0.0f),
                Z = (zFlag ? self.ReadBitCoord() : 0.0f),
            };
        }

        public static float ReadBitCoord(this BitReader self)
        {
            bool intFlag = self.ReadBoolean();
            bool fractFlag = self.ReadBoolean();
            float value = 0.0f;

            if (intFlag || fractFlag)
            {
                bool negative = self.ReadBoolean();

                if (intFlag)
                {
                    value += self.ReadUBits(COORD_INTEGER_BITS) + 1;
                }

                if (fractFlag)
                {
                    value += self.ReadUBits(COORD_FRACTIONAL_BITS) * COORD_RESOLUTION;
                }

                if (negative)
                {
                    value = -value;
                }
            }

            return value;
        }

        public static void SeekBitVectorCoord(this BitReader self)
        {
            int numCoordsToSkip = 0;

            for (int i = 0; i < 3; i++)
            {
                numCoordsToSkip += self.ReadBoolean() ? 1 : 0;
            }

            for (int i = 0; i < numCoordsToSkip; i++)
            {
                self.SeekBitCoord();
            }
        }

        public static void SeekBitCoord(this BitReader self)
        {
            bool intFlag = self.ReadBoolean();
            bool fractFlag = self.ReadBoolean();

            if (intFlag || fractFlag)
            {
                self.SeekBits(1);

                if (intFlag)
                {
                    self.SeekBits(COORD_INTEGER_BITS);
                }

                if (fractFlag)
                {
                    self.SeekBits(COORD_FRACTIONAL_BITS);
                }
            }
        }
    }
}
