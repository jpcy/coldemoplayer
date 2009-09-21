using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Core
{
    public class BitWriter
    {
        private List<byte> data;
        private int currentBit = 0;

        public BitWriter()
        {
            data = new List<byte>();
        }

        public byte[] ToArray()
        {
            return data.ToArray();
        }

        public void WriteUnsignedBits(uint value, int nBits)
        {
            if (nBits < 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            int currentByte = currentBit / 8;
            int bitOffset = currentBit - (currentByte * 8);

            // calculate how many bits need to be written to the current byte
            int bitsToWriteToCurrentByte = 8 - bitOffset;
            if (bitsToWriteToCurrentByte > nBits)
            {
                bitsToWriteToCurrentByte = nBits;
            }

            // calculate how many bytes need to be added to the list
            int bytesToAdd = 0;

            if (nBits > bitsToWriteToCurrentByte)
            {
                int temp = nBits - bitsToWriteToCurrentByte;
                bytesToAdd = temp / 8;

                if ((temp % 8) != 0)
                {
                    bytesToAdd++;
                }
            }

            if (bitOffset == 0)
            {
                bytesToAdd++;
            }

            // add new bytes if needed
            for (int i = 0; i < bytesToAdd; i++)
            {
                data.Add(new byte());
            }

            int nBitsWritten = 0;

            // write bits to the current byte
            byte b = (byte)(value & ((1 << bitsToWriteToCurrentByte) - 1));
            b <<= bitOffset;
            b += data[currentByte];
            data[currentByte] = b;

            nBitsWritten += bitsToWriteToCurrentByte;
            currentByte++;

            // write bits to all the newly added bytes
            while (nBitsWritten < nBits)
            {
                bitsToWriteToCurrentByte = nBits - nBitsWritten;
                if (bitsToWriteToCurrentByte > 8)
                {
                    bitsToWriteToCurrentByte = 8;
                }

                b = (byte)((value >> nBitsWritten) & ((1 << bitsToWriteToCurrentByte) - 1));
                data[currentByte] = b;

                nBitsWritten += bitsToWriteToCurrentByte;
                currentByte++;
            }

            // set new current bit
            currentBit += nBits;
        }

        public void WriteBits(int value, int nBits)
        {
            WriteUnsignedBits((uint)value, nBits - 1);
            WriteUnsignedBits(value < 0 ? 1u : 0u, 1);
        }

        public void WriteBoolean(bool value)
        {
            int currentByte = currentBit / 8;

            if (currentByte > data.Count - 1)
            {
                data.Add(new byte());
            }

            if (value)
            {
                data[currentByte] += (byte)(1 << currentBit % 8);
            }

            currentBit++;
        }

        public void WriteByte(byte value)
        {
            WriteUnsignedBits((uint)value, 8);
        }

        public void WriteSByte(sbyte value)
        {
            WriteBits((int)value, 8);
        }

        public void WriteBytes(byte[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                WriteByte(values[i]);
            }
        }

        public void WriteChars(char[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                // ascii
                WriteByte((byte)values[i]);
            }
        }

        public void WriteShort(short value)
        {
            WriteBits((int)value, 16);
        }

        public void WriteUShort(ushort value)
        {
            WriteUnsignedBits((uint)value, 16);
        }

        public void WriteInt(int value)
        {
            WriteBits(value, 32);
        }

        public void WriteUInt(uint value)
        {
            WriteUnsignedBits(value, 32);
        }

        public void WriteFloat(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteString(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                // ascii
                WriteByte((byte)value[i]);
            }

            // null terminator
            WriteByte(0);
        }

        public void WriteString(string value, int length)
        {
            if (length < value.Length + 1)
            {
                throw new ArgumentException("String longer that specified length.", "length");
            }

            WriteString(value);

            // pad to length bytes
            for (int i = 0; i < length - (value.Length + 1); i++)
            {
                WriteByte(0);
            }
        }

        public void WriteVectorCoord(bool goldSrc, float[] coord)
        {
            WriteBoolean(true);
            WriteBoolean(true);
            WriteBoolean(true);
            WriteCoord(goldSrc, coord[0]);
            WriteCoord(goldSrc, coord[1]);
            WriteCoord(goldSrc, coord[2]);
        }

        public void WriteCoord(bool goldSrc, float value)
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

            if (goldSrc)
            {
                WriteUnsignedBits(intValue, 12);
                WriteUnsignedBits(0, 3); // FIXME
            }
            else
            {
                WriteUnsignedBits(intValue - 1, 14);
                WriteUnsignedBits(0, 5); // FIXME
            }
        }
    }
}
