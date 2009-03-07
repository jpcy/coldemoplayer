using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics; // Assert
using System.Collections; // ArrayList
using System.IO;

namespace compLexity_Demo_Player
{
    [Serializable]
    public class BitBufferOutOfRangeException : Exception
    {
        public BitBufferOutOfRangeException()
        {
        }
    }

    public class BitBuffer
    {
        public enum EndianType
        {
            Little,
            Big
        }

        #region Properties
        /// <summary>
        /// Data length in bytes.
        /// </summary>
        public Int32 Length
        {
            get
            {
                return data.Count;
            }
        }

        public Int32 CurrentBit
        {
            get
            {
                return currentBit;
            }
        }

        public Int32 CurrentByte
        {
            get
            {
                return (currentBit - (currentBit % 8)) / 8;
            }
        }

        public Int32 BitsLeft
        {
            get
            {
                return (data.Count * 8) - currentBit;
            }
        }

        public Int32 BytesLeft
        {
            get
            {
                return data.Count - CurrentByte;
            }
        }

        public Byte[] Data
        {
            get
            {
                return data.ToArray();
            }
        }

        public EndianType Endian
        {
            get;
            set;
        }
        #endregion

        private List<Byte> data = null;
        private Int32 currentBit = 0;

        public BitBuffer(Byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "Value cannot be null.");
            }

            this.data = new List<Byte>(data);
            Endian = EndianType.Little;
        }

        public void SeekBits(Int32 count)
        {
            SeekBits(count, SeekOrigin.Current);
        }

        public void SeekBits(Int32 offset, SeekOrigin origin)
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
                currentBit = (data.Count * 8) - offset;
            }

            if (currentBit < 0 || currentBit > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }
        }

        public void SeekBytes(Int32 count)
        {
            SeekBits(count * 8);
        }

        public void SeekBytes(Int32 offset, SeekOrigin origin)
        {
            SeekBits(offset * 8, origin);
        }

        /// <summary>
        /// Seeks past the remaining bits in the current byte.
        /// </summary>
        public void SkipRemainingBits()
        {
            Int32 bitOffset = currentBit % 8;

            if (bitOffset != 0)
            {
                SeekBits(8 - bitOffset);
            }
        }

        // HL 1.1.0.6 bit reading (big endian byte and bit order)
        private UInt32 ReadUnsignedBitsBigEndian(Int32 nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // check for overflow
            if (currentBit + nBits > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            Int32 currentByte = currentBit / 8;
            Int32 bitOffset = currentBit - (currentByte * 8);
            Int32 nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            UInt64 currentValue = 0;
            for (Int32 i = 0; i < nBytesToRead; i++)
            {
                Byte b = data[currentByte + (nBytesToRead - 1) - i];
                currentValue += (UInt64)((UInt64)b << (i * 8));
            }

            // get bits we need from bytes
            currentValue >>= ((nBytesToRead * 8 - bitOffset) - nBits);
            currentValue &= (UInt32)(((Int64)1 << nBits) - 1);

            // increment current bit
            currentBit += nBits;

            return (UInt32)currentValue;
        }

        private UInt32 ReadUnsignedBitsLittleEndian(Int32 nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // check for overflow
            if (currentBit + nBits > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            Int32 currentByte = currentBit / 8;
            Int32 bitOffset = currentBit - (currentByte * 8);
            Int32 nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            UInt64 currentValue = 0;
            for (Int32 i = 0; i < nBytesToRead; i++)
            {
                Byte b = data[currentByte + i];
                currentValue += (UInt64)((UInt64)b << (i * 8));
            }

            // get bits we need from bytes
            currentValue >>= bitOffset;
            currentValue &= (UInt32)(((Int64)1 << nBits) - 1);

            // increment current bit
            currentBit += nBits;

            return (UInt32)currentValue;
        }

        public UInt32 ReadUnsignedBits(Int32 nBits)
        {
            if (Endian == EndianType.Little)
            {
                return ReadUnsignedBitsLittleEndian(nBits);
            }
            else
            {
                return ReadUnsignedBitsBigEndian(nBits);
            }
        }

        public Int32 ReadBits(Int32 nBits)
        {
            Int32 result = (Int32)ReadUnsignedBits(nBits - 1);
            Int32 sign = (ReadBoolean() ? 1 : 0);

            if (sign == 1)
            {
                result = -((1 << (nBits - 1)) - result);
            }

            return result;
        }

        public Boolean ReadBoolean()
        {
            // check for overflow
            if (currentBit + 1 > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            Boolean result = (data[currentBit / 8] & ((Endian == EndianType.Little ? 1 << currentBit % 8 : 128 >> currentBit % 8))) == 0 ? false : true;
            currentBit++;
            return result;
        }

        public Byte ReadByte()
        {
            return (Byte)ReadUnsignedBits(8);
        }

        public SByte ReadSByte()
        {
            return (SByte)ReadBits(8);
        }

        public Byte[] ReadBytes(Int32 nBytes)
        {
            Byte[] result = new Byte[nBytes];

            for (Int32 i = 0; i < nBytes; i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }

        public Char[] ReadChars(Int32 nChars)
        {
            Char[] result = new Char[nChars];

            for (Int32 i = 0; i < nChars; i++)
            {
                result[i] = (Char)ReadByte(); // not unicode
            }

            return result;
        }

        public Int16 ReadInt16()
        {
            return (Int16)ReadBits(16);
        }

        public UInt16 ReadUInt16()
        {
            return (UInt16)ReadUnsignedBits(16);
        }

        public Int32 ReadInt32()
        {
            return ReadBits(32);
        }

        public UInt32 ReadUInt32()
        {
            return ReadUnsignedBits(32);
        }

        public Single ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        /// <summary>
        /// Read a null-terminated string, then skip any remaining bytes to make up length bytes.
        /// </summary>
        /// <param name="length">The total number of bytes to read.</param>
        /// <returns></returns>
        public String ReadString(Int32 length)
        {
            Int32 startBit = currentBit;
            String s = ReadString();
            SeekBits(length * 8 - (currentBit - startBit));
            return s;
        }

        public String ReadString()
        {
            List<Byte> bytes = new List<Byte>();

            while (true)
            {
                Byte b = ReadByte();

                if (b == 0x00)
                {
                    break;
                }

                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public Single[] ReadVectorCoord()
        {
            return ReadVectorCoord(false);
        }

        public Single[] ReadVectorCoord(Boolean goldSrc)
        {
            Boolean xFlag = ReadBoolean();
            Boolean yFlag = ReadBoolean();
            Boolean zFlag = ReadBoolean();

            Single[] result = new Single[3];

            if (xFlag)
            {
                result[0] = ReadCoord(goldSrc);
            }

            if (yFlag)
            {
                result[1] = ReadCoord(goldSrc);
            }

            if (zFlag)
            {
                result[2] = ReadCoord(goldSrc);
            }

            return result;
        }

        public Single ReadCoord()
        {
            return ReadCoord(false);
        }

        public Single ReadCoord(Boolean goldSrc)
        {
            Boolean intFlag = ReadBoolean();
            Boolean fractionFlag = ReadBoolean();

            Single value = 0.0f;

            if (!intFlag && !fractionFlag)
            {
                return value;
            }

            Boolean sign = ReadBoolean();
            UInt32 intValue = 0;
            UInt32 fractionValue = 0;

            if (intFlag)
            {
                if (goldSrc)
                {
                    intValue = ReadUnsignedBits(12);
                }
                else
                {
                    intValue = ReadUnsignedBits(14) + 1;
                }
            }

            if (fractionFlag)
            {
                if (goldSrc)
                {
                    fractionValue = ReadUnsignedBits(3);
                }
                else
                {
                    fractionValue = ReadUnsignedBits(5);
                }
            }

            value = intValue + ((Single)fractionValue * 1.0f / 32.0f);

            if (sign)
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Sets all bits to zero, starting with the current bit and up to nBits.
        /// Used for Fade to Black removal.
        /// </summary>
        /// <param name="nBits"></param>
        public void ZeroOutBits(Int32 nBits)
        {
            for (Int32 i = 0; i < nBits; i++)
            {
                Int32 currentByte = currentBit / 8;
                Int32 bitOffset = currentBit - (currentByte * 8);

                Byte temp = data[currentByte];
                temp -= (Byte)(data[currentByte] & (1 << bitOffset));
                data[currentByte] = temp;

                currentBit++;
            }
        }

        public void PrintBits(StreamWriter writer, Int32 nBits)
        {
            if (writer == null || nBits == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            for (Int32 i = 0; i < nBits; i++)
            {
                sb.AppendFormat("{0}", (ReadBoolean() ? 1 : 0));
            }

            writer.Write(sb.ToString() + "\n");
        }

        public void InsertBytes(Byte[] insertData)
        {
            if (insertData.Length == 0)
            {
                return;
            }

            if (currentBit % 8 != 0)
            {
                throw new ApplicationException("InsertBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            data.InsertRange(CurrentByte, insertData);
            currentBit += insertData.Length * 8;
        }

        public void RemoveBytes(Int32 count)
        {
            if (count == 0)
            {
                return;
            }

            if (currentBit % 8 != 0)
            {
                throw new ApplicationException("RemoveBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            if (CurrentByte + count > this.Length)
            {
                throw new BitBufferOutOfRangeException();
            }

            data.RemoveRange(CurrentByte, count);
        }
    }

    public class BitWriter
    {
        private List<Byte> data = null;
        private Int32 currentBit = 0;

        public Byte[] Data
        {
            get
            {
                return data.ToArray();
            }
        }

        public BitWriter()
        {
            data = new List<Byte>();
        }

        public void WriteUnsignedBits(UInt32 value, Int32 nBits)
        {
            Int32 currentByte = currentBit / 8;
            Int32 bitOffset = currentBit - (currentByte * 8);

            // calculate how many bits need to be written to the current byte
            Int32 bitsToWriteToCurrentByte = 8 - bitOffset;
            if (bitsToWriteToCurrentByte > nBits)
            {
                bitsToWriteToCurrentByte = nBits;
            }

            // calculate how many bytes need to be added to the list
            Int32 bytesToAdd = 0;

            if (nBits > bitsToWriteToCurrentByte)
            {
                Int32 temp = nBits - bitsToWriteToCurrentByte;
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
            for (Int32 i = 0; i < bytesToAdd; i++)
            {
                data.Add(new Byte());
            }

            Int32 nBitsWritten = 0;

            // write bits to the current byte
            Byte b = (Byte)(value & ((1 << bitsToWriteToCurrentByte) - 1));
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

                b = (Byte)((value >> nBitsWritten) & ((1 << bitsToWriteToCurrentByte) - 1));
                data[currentByte] = b;

                nBitsWritten += bitsToWriteToCurrentByte;
                currentByte++;
            }

            // set new current bit
            currentBit += nBits;
        }

        public void WriteBits(Int32 value, Int32 nBits)
        {
            WriteUnsignedBits((UInt32)value, nBits - 1);

            UInt32 sign = (value < 0 ? 1u : 0u);
            WriteUnsignedBits(sign, 1);
        }

        public void WriteBoolean(Boolean value)
        {
            Int32 currentByte = currentBit / 8;

            if (currentByte > data.Count - 1)
            {
                data.Add(new Byte());
            }

            if (value)
            {
                data[currentByte] += (Byte)(1 << currentBit % 8);
            }

            currentBit++;
        }

        public void WriteByte(Byte value)
        {
            WriteUnsignedBits((UInt32)value, 8);
        }

        public void WriteSByte(SByte value)
        {
            WriteBits((Int32)value, 8);
        }

        public void WriteBytes(Byte[] values)
        {
            for (Int32 i = 0; i < values.Length; i++)
            {
                WriteByte(values[i]);
            }
        }

        public void WriteChars(Char[] values)
        {
            for (Int32 i = 0; i < values.Length; i++)
            {
                WriteByte((Byte)values[i]);
            }
        }

        public void WriteInt16(Int16 value)
        {
            WriteBits((Int32)value, 16);
        }

        public void WriteUInt16(UInt16 value)
        {
            WriteUnsignedBits((UInt32)value, 16);
        }

        public void WriteInt32(Int32 value)
        {
            WriteBits(value, 32);
        }

        public void WriteUInt32(UInt32 value)
        {
            WriteUnsignedBits(value, 32);
        }

        public void WriteString(String value)
        {
            for (Int32 i = 0; i < value.Length; i++)
            {
                WriteByte((Byte)value[i]);
            }

            // null terminator
            WriteByte(0);
        }

        public void WriteString(String value, Int32 length)
        {
            if (length < value.Length + 1)
            {
                throw new ApplicationException("String length longer than specified length.");
            }

            WriteString(value);

            // write padding 0's
            for (Int32 i = 0; i < length - (value.Length + 1); i++)
            {
                WriteByte(0);
            }
        }

        public void WriteVectorCoord(Boolean goldSrc, Single[] coord)
        {
            WriteBoolean(true);
            WriteBoolean(true);
            WriteBoolean(true);
            WriteCoord(goldSrc, coord[0]);
            WriteCoord(goldSrc, coord[1]);
            WriteCoord(goldSrc, coord[2]);
        }

        public void WriteCoord(Boolean goldSrc, Single value)
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

            UInt32 intValue = (UInt32)value;

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
