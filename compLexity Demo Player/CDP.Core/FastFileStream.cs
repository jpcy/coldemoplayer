using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CDP.Core
{
    public enum FastFileAccess
    {
        Read,
        Write
    }

    public abstract class FastFileStreamBase : IDisposable
    {
        public abstract long Position { get; }
        public abstract long Length { get; }
        public abstract long BytesLeft { get; }
        public abstract void Dispose();
        public abstract void Seek(long offset, SeekOrigin origin);
        public abstract int Read(byte[] buffer, int offset, int count);
        public abstract bool ReadBoolean();
        public abstract byte ReadByte();
        public abstract byte[] ReadBytes(int count);
        public abstract char ReadChar();
        public abstract char[] ReadChars(int count);
        public abstract decimal ReadDecimal();
        public abstract double ReadDouble();
        public abstract short ReadShort();
        public abstract int ReadInt();
        public abstract long ReadLong();
        public abstract sbyte ReadSByte();
        public abstract float ReadFloat();
        public abstract string ReadString();
        public abstract ushort ReadUShort();
        public abstract uint ReadUInt();
        public abstract ulong ReadULong();
        public abstract Vector ReadVector();
        public abstract void WriteBoolean(bool value);
        public abstract void WriteByte(byte value);
        public abstract void WriteBytes(byte[] buffer);
        public abstract void WriteChar(char ch);
        public abstract void WriteChars(char[] chars);
        public abstract void WriteDecimal(decimal value);
        public abstract void WriteDouble(double value);
        public abstract void WriteFloat(float value);
        public abstract void WriteInt(int value);
        public abstract void WriteLong(long value);
        public abstract void WriteSByte(sbyte value);
        public abstract void WriteShort(short value);
        public abstract void WriteString(string value);
        public abstract void WriteUInt(uint value);
        public abstract void WriteULong(ulong value);
        public abstract void WriteUShort(ushort value);
        public abstract void Write(byte[] buffer, int offset, int count);
        public abstract void WriteVector(Vector value);
    }

    public class FastFileStream : FastFileStreamBase, IDisposable
    {
        public class FileAccessIsReadNotWrite : InvalidOperationException
        {
            public FileAccessIsReadNotWrite()
                : base("File access mode is set to read, cannot write.")
            {
            }
        }

        public class FileAccessIsWriteNotRead : InvalidOperationException
        {
            public FileAccessIsWriteNotRead()
                : base("File access mode is set to write, cannot read.")
            {
            }
        }

        public static FastFileStreamBase Open(string fileName, FastFileAccess access)
        {
            return new FastFileStream(fileName, access);
        }

        private FileStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private long position;
        private long length;
        private FastFileAccess access;
        private bool disposed = false;

        public override long Position
        {
            get { return position; }
        }

        public override long Length
        {
            get
            {
                if (access == FastFileAccess.Read)
                {
                    return length;
                }
                else
                {
                    throw new InvalidOperationException("Cannot get stream length when writing.");
                }
            }
        }

        public override long BytesLeft
        {
            get { return Length - Position; }
        }

        private const int fileBufferSize = 4096;
        
        public FastFileStream(string fileName, FastFileAccess access)
        {
            this.access = access;

            if (access == FastFileAccess.Read)
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, fileBufferSize, FileOptions.SequentialScan);
                reader = new BinaryReader(stream);
                length = stream.Length;
            }
            else
            {
                stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, fileBufferSize, FileOptions.SequentialScan);
                writer = new BinaryWriter(stream);
            }

            position = 0;
        }

        ~FastFileStream()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                stream.Close();
                stream.Dispose();

                if (reader != null)
                {
                    reader.Close();
                }

                if (writer != null)
                {
                    writer.Close();
                }
            }

            stream = null;
            reader = null;
            writer = null;
            disposed = true;
        }

        public override void Seek(long offset, SeekOrigin origin)
        {
            position = stream.Seek(offset, origin);
        }

        #region Reading
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            int bytesRead = stream.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }

        public override bool ReadBoolean()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            bool value = reader.ReadBoolean();
            position += sizeof(bool);
            return value;
        }

        public override byte ReadByte()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            byte value = reader.ReadByte();
            position += sizeof(byte);
            return value;
        }

        public override byte[] ReadBytes(int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            byte[] value = reader.ReadBytes(count);
            position += value.Length * sizeof(byte);
            return value;
        }

        public override char ReadChar()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            char value = reader.ReadChar();
            position += sizeof(char);
            return value;
        }

        public override char[] ReadChars(int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            char[] value = reader.ReadChars(count);
            position += value.Length * sizeof(char);
            return value;
        }

        public override decimal ReadDecimal()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            decimal value = reader.ReadDecimal();
            position += sizeof(decimal);
            return value;
        }

        public override double ReadDouble()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            double value = reader.ReadDouble();
            position += sizeof(double);
            return value;
        }

        public override short ReadShort()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            short value = reader.ReadInt16();
            position += sizeof(short);
            return value;
        }

        public override int ReadInt()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            int value = reader.ReadInt32();
            position += sizeof(int);
            return value;
        }

        public override long ReadLong()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            long value = reader.ReadInt64();
            position += sizeof(long);
            return value;
        }

        public override sbyte ReadSByte()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            sbyte value = reader.ReadSByte();
            position += sizeof(sbyte);
            return value;
        }

        public override float ReadFloat()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            float value = reader.ReadSingle();
            position += sizeof(float);
            return value;
        }

        public override string ReadString()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

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

        public override ushort ReadUShort()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            ushort value = reader.ReadUInt16();
            position += sizeof(ushort);
            return value;
        }

        public override uint ReadUInt()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            uint value = reader.ReadUInt32();
            position += sizeof(uint);
            return value;
        }

        public override ulong ReadULong()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            ulong value = reader.ReadUInt64();
            position += sizeof(ulong);
            return value;
        }

        public override Vector ReadVector()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            Vector v = new Vector();
            v.X = reader.ReadSingle();
            v.Y = reader.ReadSingle();
            v.Z = reader.ReadSingle();
            position += sizeof(float) * 3;
            return v;
        }
        #endregion

        #region Writing
        public override void WriteBoolean(bool value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(bool);
        }

        public override void WriteByte(byte value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(byte);
        }

        public override void WriteBytes(byte[] buffer)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(buffer);
            position += buffer.Length * sizeof(byte);
        }

        public override void WriteChar(char ch)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(ch);
            position += sizeof(char);
        }

        public override void WriteChars(char[] chars)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(chars);
            position += chars.Length * sizeof(char);
        }

        public override void WriteDecimal(decimal value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(decimal);
        }

        public override void WriteDouble(double value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(double);
        }

        public override void WriteFloat(float value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(float);
        }

        public override void WriteInt(int value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(int);
        }

        public override void WriteLong(long value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(long);
        }

        public override void WriteSByte(sbyte value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(sbyte);
        }

        public override void WriteShort(short value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(short);
        }

        public override void WriteString(string value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            // Maximum possible encoded string length for UTF-8 is 4 bytes per character.
            BitWriter buffer = new BitWriter(value.Length * 4);
            buffer.WriteString(value);
            byte[] data = buffer.ToArray();
            WriteBytes(data);
            position += data.Length * sizeof(byte);
        }

        public override void WriteUInt(uint value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(uint);
        }

        public override void WriteULong(ulong value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(ulong);
        }

        public override void WriteUShort(ushort value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(ushort);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            stream.Write(buffer, offset, count);
            position += count;
        }

        public override void WriteVector(Vector value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            position += sizeof(float) * 3;
        }
        #endregion
    }
}