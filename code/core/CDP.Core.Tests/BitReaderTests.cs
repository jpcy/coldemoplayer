using System;
using NUnit.Framework;
using CDP.Core;
using System.IO;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class BitReaderTests
    {
        private BitReader blankReader;
        private byte[] blankBuffer;
        private const int blankBufferLength = 10;

        private const uint uintTestValue = 1234567890;

        [SetUp]
        public void SetUp()
        {
            blankBuffer = new byte[blankBufferLength];
            blankReader = new BitReader(blankBuffer);
        }

        [Test]
        public void SeekBits_Relative()
        {
            blankReader.SeekBits(5, SeekOrigin.Begin);
            blankReader.SeekBits(5, SeekOrigin.Current);
            Assert.That(blankReader.CurrentBit, Is.EqualTo(10));
        }

        [Test]
        public void SeekBits_Begin()
        {
            blankReader.SeekBits(5, SeekOrigin.Begin);
            Assert.That(blankReader.CurrentBit, Is.EqualTo(5));
        }

        [Test]
        public void SeekBits_End()
        {
            blankReader.SeekBits(5, SeekOrigin.End);
            Assert.That(blankReader.CurrentBit, Is.EqualTo(blankBuffer.Length * 8 - 5));
        }

        [Test]
        [ExpectedException(typeof(BitReader.OutOfRangeException))]
        public void SeekBits_Underflow()
        {
            blankReader.SeekBits(-1);
        }

        [Test]
        [ExpectedException(typeof(BitReader.OutOfRangeException))]
        public void SeekBits_Overflow()
        {
            blankReader.SeekBits(blankBuffer.Length * 8 + 1);
        }

        [Test]
        public void SkipRemainingBitsInCurrentByte_NotByteAligned()
        {
            blankReader.SeekBits(4);
            blankReader.SeekRemainingBitsInCurrentByte();
            Assert.That(blankReader.CurrentBit, Is.EqualTo(8));
        }

        [Test]
        public void SkipRemainingBitsInCurrentByte_ByteAligned()
        {
            blankReader.SeekBits(8);
            blankReader.SeekRemainingBitsInCurrentByte();
            Assert.That(blankReader.CurrentBit, Is.EqualTo(8));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadUnsignedBits_NegativeNumberOfBits()
        {
            blankReader.ReadUBits(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadUnsignedBits_NumberOfBitsTooHigh()
        {
            blankReader.ReadUBits(33);
        }

        [Test]
        [ExpectedException(typeof(BitReader.OutOfRangeException))]
        public void ReadUnsignedBits_Overflow()
        {
            BitReader reader = new BitReader(new byte[4]);
            reader.SeekBits(1);
            reader.ReadUBits(32);
        }

        [Test]
        public void ReadUnsignedBits_NotByteAligned()
        {
            ulong value = (ulong)uintTestValue << 1;
            BitReader reader = new BitReader(BitConverter.GetBytes(value));
            reader.SeekBits(1);
            Assert.That(reader.ReadUBits(32), Is.EqualTo(uintTestValue));
            Assert.That(reader.CurrentBit, Is.EqualTo(33));
        }

        [Test]
        public void ReadUnsignedBits_ByteAligned()
        {
            BitReader reader = new BitReader(BitConverter.GetBytes(uintTestValue));
            Assert.That(reader.ReadUBits(32), Is.EqualTo(uintTestValue));
            Assert.That(reader.CurrentBit, Is.EqualTo(32));
        }

        [Test]
        public void ReadBits_Ok()
        {
            const int value = -1;
            BitReader reader = new BitReader(BitConverter.GetBytes(value));
            Assert.That(reader.ReadBits(32), Is.EqualTo(value));
        }

        [Test]
        [ExpectedException(typeof(BitReader.OutOfRangeException))]
        public void ReadBoolean_Overflow()
        {
            blankReader.SeekBytes(blankReader.Length);
            blankReader.ReadBoolean();
        }

        [Test]
        public void ReadBoolean_Ok()
        {
            const int value = 1 << 1; // offset of 1
            BitReader reader = new BitReader(BitConverter.GetBytes(value));
            reader.SeekBits(1);
            Assert.That(reader.ReadBoolean(), Is.True);
        }
    }
}
