using System;

namespace CDP.HalfLife
{
    public class BitWriter : Core.BitWriter
    {
        public void WriteCoord(float value)
        {
            WriteBoolean(true); // int flag
            WriteBoolean(true); // fraction flag

            // sign
            if (value < 0.0f)
            {
                WriteBoolean(true);
            }
            else
            {
                WriteBoolean(false);
            }

            uint intValue = (uint)value;
            WriteUBits(intValue, 12);
            WriteUBits(0, 3); // FIXME
        }

        public void WriteVectorCoord(float[] coord)
        {
            if (coord.Length != 3)
            {
                throw new ArgumentException("Array length must be 3.", "coord");
            }

            WriteBoolean(true);
            WriteBoolean(true);
            WriteBoolean(true);
            WriteCoord(coord[0]);
            WriteCoord(coord[1]);
            WriteCoord(coord[2]);
        }

        public void WriteHiresAngle(float angle)
        {
            // angle / (360 / 2 ^ 16)
            WriteShort((short)(angle / 0.0054931640625f));
        }
    }
}
