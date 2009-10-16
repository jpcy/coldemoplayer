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

    public class FastFileStream : IDisposable
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

        private FileStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private long position;
        private long length;
        private FastFileAccess access;
        private bool disposed = false;

        public long Position
        {
            get { return position; }
        }

        public long Length
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

        public void Dispose()
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

        public void Seek(long offset, SeekOrigin origin)
        {
            position = stream.Seek(offset, origin);
        }

        #region Reading
        public int Read(byte[] buffer, int offset, int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            int bytesRead = stream.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }

        public bool ReadBoolean()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            bool value = reader.ReadBoolean();
            position += sizeof(bool);
            return value;
        }

        public byte ReadByte()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            byte value = reader.ReadByte();
            position += sizeof(byte);
            return value;
        }

        public byte[] ReadBytes(int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            byte[] value = reader.ReadBytes(count);
            position += value.Length * sizeof(byte);
            return value;
        }

        public char ReadChar()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            char value = reader.ReadChar();
            position += sizeof(char);
            return value;
        }

        public char[] ReadChars(int count)
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            char[] value = reader.ReadChars(count);
            position += value.Length * sizeof(char);
            return value;
        }

        public decimal ReadDecimal()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            decimal value = reader.ReadDecimal();
            position += sizeof(decimal);
            return value;
        }

        public double ReadDouble()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            double value = reader.ReadDouble();
            position += sizeof(double);
            return value;
        }

        public short ReadShort()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            short value = reader.ReadInt16();
            position += sizeof(short);
            return value;
        }

        public int ReadInt()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            int value = reader.ReadInt32();
            position += sizeof(int);
            return value;
        }

        public long ReadLong()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            long value = reader.ReadInt64();
            position += sizeof(long);
            return value;
        }

        public sbyte ReadSByte()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            sbyte value = reader.ReadSByte();
            position += sizeof(sbyte);
            return value;
        }

        public float ReadFloat()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            float value = reader.ReadSingle();
            position += sizeof(float);
            return value;
        }

        public string ReadString()
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

        public ushort ReadUShort()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            ushort value = reader.ReadUInt16();
            position += sizeof(ushort);
            return value;
        }

        public uint ReadUInt()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            uint value = reader.ReadUInt32();
            position += sizeof(uint);
            return value;
        }

        public ulong ReadULong()
        {
            if (access != FastFileAccess.Read)
            {
                throw new FileAccessIsWriteNotRead();
            }

            ulong value = reader.ReadUInt64();
            position += sizeof(ulong);
            return value;
        }

        public Vector ReadVector()
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
        public void WriteBoolean(bool value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(bool);
        }

        public void WriteByte(byte value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(byte);
        }

        public void WriteBytes(byte[] buffer)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(buffer);
            position += buffer.Length * sizeof(byte);
        }

        public void WriteChar(char ch)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(ch);
            position += sizeof(char);
        }

        public void WriteChars(char[] chars)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(chars);
            position += chars.Length * sizeof(char);
        }

        public void WriteDecimal(decimal value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(decimal);
        }

        public void WriteDouble(double value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(double);
        }

        public void WriteFloat(float value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(float);
        }

        public void WriteInt(int value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(int);
        }

        public void WriteLong(long value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(long);
        }

        public void WriteSByte(sbyte value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(sbyte);
        }

        public void WriteShort(short value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(short);
        }

        public void WriteString(string value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            BitWriter buffer = new BitWriter();
            buffer.WriteString(value);
            byte[] data = buffer.ToArray();
            WriteBytes(data);
            position += data.Length * sizeof(byte);
        }

        public void WriteUInt(uint value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(uint);
        }

        public void WriteULong(ulong value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(ulong);
        }

        public void WriteUShort(ushort value)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            writer.Write(value);
            position += sizeof(ushort);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (access != FastFileAccess.Write)
            {
                throw new FileAccessIsReadNotWrite();
            }

            stream.Write(buffer, offset, count);
            position += count;
        }

        public void WriteVector(Vector value)
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