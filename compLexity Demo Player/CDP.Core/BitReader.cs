using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.Core
{
    /// <summary>
    /// Reads various data types from a byte array, down to the bit level.
    /// </summary>
    public class BitReader
    {
        public class OutOfRangeException : Exception
        {
            public OutOfRangeException()
            {
            }
        }

        public enum Endians
        {
            Little,
            Big
        }

        public Endians Endian { get; set; }
        public byte[] Buffer { get; private set; }

        public int Length
        {
            get
            {
                return Buffer.Length;
            }
        }

        public int CurrentBit
        {
            get
            {
                return currentBit;
            }
        }

        public int CurrentByte
        {
            get
            {
                return currentBit / 8;
            }
        }

        public int BitsLeft
        {
            get
            {
                return (Length * 8) - currentBit;
            }
        }

        public int BytesLeft
        {
            get
            {
                return Length - CurrentByte;
            }
        }

        private int currentBit = 0;

        public BitReader(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            Buffer = buffer;
            Endian = Endians.Little;
        }

        public void SeekBits(int count)
        {
            SeekBits(count, SeekOrigin.Current);
        }

        public void SeekBits(uint count)
        {
            SeekBits((int)count, SeekOrigin.Current);
        }

        public void SeekBits(int offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Current)
            {
                currentBit += offset;
            }
            else if (origin == SeekOrigin.Begin)
            {
                currentBit = offset;
            }
            else if (origin == SeekOrigin.End)
            {
                currentBit = (Length * 8) - offset;
            }

            if (currentBit < 0 || currentBit > Length * 8)
            {
                throw new OutOfRangeException();
            }
        }

        public void SeekBytes(int count)
        {
            SeekBits(count * 8);
        }

        public void SeekBytes(int offset, SeekOrigin origin)
        {
            SeekBits(offset * 8, origin);
        }

        public void SeekString()
        {
            while (ReadByte() != 0)
            {
            }
        }

        public void SeekRemainingBitsInCurrentByte()
        {
            int bitOffset = currentBit % 8;

            if (bitOffset != 0)
            {
                SeekBits(8 - bitOffset);
            }
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

        private uint ReadUBitsLittleEndianByteAligned(int nBits)
        {
            if (nBits % 8 != 0)
            {
                throw new ArgumentException("Must be a multiple of 8.", "nBits");
            }

            if (currentBit % 8 != 0)
            {
                throw new InvalidOperationException("Current bit is not byte-aligned.");
            }

            uint result = 0;

            for (int i = 0; i < nBits / 8; i++)
            {
                result += (uint)(Buffer[CurrentByte] << (i * 8));
                currentBit += 8;
            }

            return result;
        }

        private uint ReadUBitsLittleEndian(int nBits)
        {
            /* Example:
             * 
             * You want to seek 4 bits than read a 22-bit unsigned value.
             * First 4 bits are 0. The value is 3,141,592. 6 bits, all 1's follow.
             * Value: 11011000 11101111 00101111.
             * Bitstream: 10000000 11111101 11111110 11111110
             * Bitstream (LSB first): 00000001101111110111111101111111.
             * 
             * bitOffset = 4
             * nBitsToRead = 26
             * nBytesToRead = 4 (nBitsToRead rounded up to the nearest byte)
             * currentValue = bitstream above
             * 
             * currentValue >>= bitOffset
             * The value above, with 6 trailing bits (all 1's in this case). Shifting to the right removes bitOffset bits (in this case, the 4 leading bits that are all 0's).
             * Bitstream (LSB first): 0001101111110111111101111111
             * 
             * currentValue &= (uint)(((long)1 << nBits) - 1)
             * Logical AND currentValue with 2^22-1, which give only the number of bits we want (in this case, 22).
             * Bitstream (LSB first): 0001101111110111111101
             * 
             * */
            int bitOffset = currentBit % 8;
            int nBitsToRead = bitOffset + nBits;
            int nBytesToRead = nBitsToRead / 8 + (nBitsToRead % 8 != 0 ? 1 : 0);

            // get bytes we need
            ulong currentValue = 0;
            for (int i = 0; i < nBytesToRead; i++)
            {
                byte b = Buffer[CurrentByte + i];
                currentValue += (ulong)((ulong)b << (i * 8));
            }

            // get bits we need from bytes
            currentValue >>= bitOffset;
            currentValue &= (uint)(((ulong)1 << nBits) - 1);

            // increment current bit
            currentBit += nBits;

            return (uint)currentValue;
        }

        public uint ReadUBits(int nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // Check for overflow.
            if (currentBit + nBits > Length * 8)
            {
                throw new OutOfRangeException();
            }

            if (Endian == Endians.Little)
            {
                if (currentBit % 8 == 0 && nBits % 8 == 0)
                {
                    return ReadUBitsLittleEndianByteAligned(nBits);
                }
                else
                {
                    return ReadUBitsLittleEndian(nBits);
                }
            }
            else
            {
                return ReadUBitsBigEndian(nBits);
            }
        }

        public uint ReadUBits(uint nBits)
        {
            return ReadUBits((int)nBits);
        }

        public int ReadBits(int nBits)
        {
            int result = (int)ReadUBits(nBits - 1);

            if (ReadBoolean())
            {
                result = -((1 << (nBits - 1)) - result);
            }

            return result;
        }

        public int ReadBits(uint nBits)
        {
            return ReadBits((int)nBits);
        }

        public bool ReadBoolean()
        {
            // check for overflow
            if (currentBit + 1 > Length * 8)
            {
                throw new OutOfRangeException();
            }

            int currentByte = currentBit / 8;
            int bitOffset = currentBit % 8;
            bool result = (Buffer[currentByte] & ((Endian == Endians.Little ? 1 << bitOffset : 128 >> bitOffset))) == 0 ? false : true;
            currentBit++;
            return result;
        }

        public byte ReadByte()
        {
            return (byte)ReadUBits(8);
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadBits(8);
        }

        public byte[] ReadBytes(uint nBytes)
        {
            return ReadBytes((int)nBytes);
        }

        public byte[] ReadBytes(int nBytes)
        {
            if (nBytes <= 0)
            {
                throw new ArgumentOutOfRangeException("Must be a positive integer.", "nBytes");
            }

            byte[] result = new byte[nBytes];

            for (int i = 0; i < nBytes; i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }

        public char[] ReadChars(int nChars)
        {
            char[] result = new char[nChars];

            for (int i = 0; i < nChars; i++)
            {
                result[i] = (char)ReadByte(); // not unicode
            }

            return result;
        }

        public short ReadShort()
        {
            return (short)ReadBits(16);
        }

        public ushort ReadUShort()
        {
            return (ushort)ReadUBits(16);
        }

        public int ReadInt()
        {
            return ReadBits(32);
        }

        public uint ReadUInt()
        {
            return ReadUBits(32);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        /// <summary>
        /// Read a null-terminated string, then skip any remaining bytes to make up length bytes.
        /// </summary>
        /// <param name="length">The total number of bytes to read.</param>
        /// <returns></returns>
        public string ReadString(int length)
        {
            int startBit = currentBit;
            string s = ReadString();
            int seek = length * 8 - (currentBit - startBit);

            if (seek > 0)
            {
                SeekBits(seek);
            }

            return s;
        }

        public string ReadString()
        {
            List<byte> bytes = new List<byte>();

            while (true)
            {
                byte b = ReadByte();

                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}