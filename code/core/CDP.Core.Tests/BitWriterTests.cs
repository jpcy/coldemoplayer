using System;
using System.Text;
using NUnit.Framework;
using CDP.Core;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class BitWriterTests
    {
        private BitWriter writer;
        
        // For WriteUnsignedBits and WriteBits.
        private const int nTestBits = 13;
        private const uint testUBits = 5197;
        private const int testBits = -3149;

        private const byte testByte = 204;
        private const sbyte testSByte = -100;
        private readonly byte[] testBytes = { 204, 100, 42 };
        private readonly char[] testChars = { 'A', 'B', 'C' };
        private const short testShort = (short)-25195;
        private const ushort testUShort = (ushort)41579;
        private const int testInt = -1943226037;
        private const uint testUInt = 3016967861;
        private const float testFloat = 3.141592653589f;
        private readonly string testString = "abcdefg";

        /// <summary>
        /// Calculates the number of bytes required to store the specified number of bits.
        /// </summary>
        /// <param name="nBits">The number of bits.</param>
        /// <returns>The number of bytes.</returns>
        private int CalculateNumberOfBytes(int nBits)
        {
            return (nBits / 8 + (nBits % 8 == 0 ? 0 : 1));
        }

        [SetUp]
        public void SetUp()
        {
            writer = new BitWriter(1024);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteUnsignedBits_nBitsTooLow()
        {
            writer.WriteUBits(0, -1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteUnsignedBits_nBitsTooHigh()
        {
            writer.WriteUBits(0, 33);
        }

        [Test]
        public void WriteUnsignedBits_ByteAligned()
        {
            writer.WriteUBits(testUBits, nTestBits);
            Assert.That(writer.ToArray().Length, Is.EqualTo(CalculateNumberOfBytes(nTestBits)));
            BitReader reader = new BitReader(writer.ToArray());
            Assert.That(reader.ReadUBits(nTestBits), Is.EqualTo(testUBits));
        }

        [Test]
        public void WriteUnsignedBits_NotByteAligned()
        {
            const int nDummyBits = 2;
            writer.WriteUBits(0, nDummyBits);
            writer.WriteUBits(testUBits, nTestBits);
            Assert.That(writer.ToArray().Length, Is.EqualTo(CalculateNumberOfBytes(nDummyBits + nTestBits)));
            BitReader reader = new BitReader(writer.ToArray());
            reader.SeekBits(nDummyBits);
            Assert.That(reader.ReadUBits(nTestBits), Is.EqualTo(testUBits));
        }

        [Test]
        public void WriteBits()
        {
            writer.WriteBits(testBits, nTestBits);
            Assert.That(writer.ToArray().Length, Is.EqualTo(CalculateNumberOfBytes(nTestBits)));

            BitReader reader = new BitReader(writer.ToArray());
            Assert.That(reader.ReadBits(nTestBits), Is.EqualTo(testBits));
        }

        [Test]
        public void WriteBoolean_ByteAligned()
        {
            writer.WriteBoolean(true);
            Assert.That(writer.ToArray().Length, Is.EqualTo(1));
            Assert.That((writer.ToArray()[0] & 1) != 0, Is.EqualTo(true));
        }

        [Test]
        public void WriteBoolean_NotByteAligned()
        {
            const int nDummyBits = 2;
            
            // Needs to fit in a byte or the test breaks.
            Assert.That(nDummyBits, Is.LessThanOrEqualTo(7));

            writer.WriteBits(0, nDummyBits);
            writer.WriteBoolean(true);
            Assert.That(writer.ToArray().Length, Is.EqualTo(1));
            Assert.That((writer.ToArray()[0] & (1 << nDummyBits)) != 0, Is.EqualTo(true));
        }

        [Test]
        public void WriteByte()
        {
            writer.WriteByte(testByte);
            Assert.That(writer.ToArray().Length, Is.EqualTo(1));
            Assert.That(writer.ToArray()[0], Is.EqualTo(testByte));
        }

        [Test]
        public void WriteSByte()
        {
            writer.WriteSByte(testSByte);
            Assert.That(writer.ToArray().Length, Is.EqualTo(1));
            Assert.That((sbyte)writer.ToArray()[0], Is.EqualTo(testSByte));
        }

        [Test]
        public void WriteBytes()
        {
            writer.WriteBytes(testBytes);
            Assert.That(writer.ToArray().Length, Is.EqualTo(testBytes.Length));
            Assert.That(writer.ToArray(), Is.EqualTo(testBytes));
        }

        [Test]
        public void WriteChars()
        {
            writer.WriteChars(testChars);
            Assert.That(writer.ToArray().Length, Is.EqualTo(testChars.Length));
            byte[] temp = new byte[testChars.Length];

            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = (byte)testChars[i];
            }

            Assert.That(writer.ToArray(), Is.EqualTo(temp));
        }

        [Test]
        public void WriteShort()
        {
            writer.WriteShort(testShort);
            Assert.That(writer.ToArray().Length, Is.EqualTo(2));
            Assert.That(writer.ToArray(), Is.EqualTo(BitConverter.GetBytes(testShort)));
        }

        [Test]
        public void WriteUShort()
        {
            writer.WriteUShort(testUShort);
            Assert.That(writer.ToArray().Length, Is.EqualTo(2));
            Assert.That(writer.ToArray(), Is.EqualTo(BitConverter.GetBytes(testUShort)));
        }

        [Test]
        public void WriteInt()
        {
            writer.WriteInt(testInt);
            Assert.That(writer.ToArray().Length, Is.EqualTo(4));
            Assert.That(writer.ToArray(), Is.EqualTo(BitConverter.GetBytes(testInt)));
        }

        [Test]
        public void WriteUInt()
        {
            writer.WriteUInt(testUInt);
            Assert.That(writer.ToArray().Length, Is.EqualTo(4));
            Assert.That(writer.ToArray(), Is.EqualTo(BitConverter.GetBytes(testUInt)));
        }

        [Test]
        public void WriteFloat()
        {
            writer.WriteFloat(testFloat);
            Assert.That(writer.ToArray().Length, Is.EqualTo(4));
            Assert.That(writer.ToArray(), Is.EqualTo(BitConverter.GetBytes(testFloat)));
        }

        [Test]
        public void WriteString()
        {
            writer.WriteString(testString);
            Assert.That(writer.ToArray().Length, Is.EqualTo(testString.Length + 1));
            byte[] temp = new byte[testString.Length + 1];
            Array.Copy(Encoding.ASCII.GetBytes(testString), temp, testString.Length);
            Assert.That(writer.ToArray(), Is.EqualTo(temp));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteString_Padded_PaddedLengthTooShort()
        {
            writer.WriteString(testString, testString.Length / 2);
        }

        [Test]
        public void WriteString_Padded()
        {
            const int padding = 10;
            int totalLength = testString.Length + 1 + padding; // 1 for the null-terminator
            writer.WriteString(testString, totalLength);
            byte[] temp = new byte[totalLength];
            Array.Copy(Encoding.ASCII.GetBytes(testString), temp, testString.Length);
            Assert.That(writer.ToArray(), Is.EqualTo(temp));
        }
    }
}
