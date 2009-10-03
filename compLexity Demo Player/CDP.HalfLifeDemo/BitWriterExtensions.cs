using System;
using CDP.Core;

namespace CDP.HalfLifeDemo
{
    public static class BitWriterExtensions
    {
        public static void WriteCoord(this BitWriter bitWriter, float value)
        {
            bitWriter.WriteBoolean(true); // int flag
            bitWriter.WriteBoolean(true); // fraction flag

            // sign
            if (value < 0.0f)
            {
                bitWriter.WriteBoolean(true);
            }
            else
            {
                bitWriter.WriteBoolean(false);
            }

            uint intValue = (uint)value;
            bitWriter.WriteUBits(intValue, 12);
            bitWriter.WriteUBits(0, 3); // FIXME
        }

        public static void WriteVectorCoord(this BitWriter bitWriter, float[] coord)
        {
            if (coord.Length != 3)
            {
                throw new ArgumentException("Array length must be 3.", "coord");
            }

            bitWriter.WriteBoolean(true);
            bitWriter.WriteBoolean(true);
            bitWriter.WriteBoolean(true);
            bitWriter.WriteCoord(coord[0]);
            bitWriter.WriteCoord(coord[1]);
            bitWriter.WriteCoord(coord[2]);
        }

        public static void WriteHiresAngle(this BitWriter bitWriter, float angle)
        {
            // angle / (360 / 2 ^ 16)
            bitWriter.WriteShort((short)(angle / 0.0054931640625f));
        }
    }
}
