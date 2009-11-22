using System;
using System.IO;
using System.Diagnostics;

namespace CDP.Core.Extensions
{
    public static class StreamWriterExtensions
    {
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void Debug_WriteBits(this StreamWriter self, BitReader reader, int nBits)
        {
            for (int i = 0; i < nBits; i++)
            {
                self.Write("{0}", reader.ReadBoolean() ? 1 : 0);
            }

            self.WriteLine();
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void Debug_WriteTryFindString(this StreamWriter self, BitReader reader, int nBitsToTry)
        {
            int startBit = reader.CurrentBit;

            for (int i = 0; i < nBitsToTry; i++)
            {
                if (i > 0)
                {
                    self.Write("{0} bits: ", i);
                    self.Debug_WriteBits(reader, i);
                }

                self.WriteLine(reader.ReadString());
                reader.SeekBits(startBit, SeekOrigin.Begin);
            }
        }

        public static void WriteBytes(this StreamWriter self, byte[] value)
        {
            self.WriteBytes(value, b => b.ToString("X2"));
        }

        public static void WriteBytesAsChars(this StreamWriter self, byte[] value)
        {
            self.WriteBytes(value, b => (char)b);
        }

        private static void WriteBytes(this StreamWriter self, byte[] value, Func<byte, object> transform)
        {
            self.Write("Byte[]: ");

            if (value == null || value.Length == 0)
            {
                self.WriteLine("Null or empty.");
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                self.Write(transform(value[i]));
            }

            self.WriteLine();
        }

        public static void WriteVector(this StreamWriter self, string name, Vector value)
        {
            if (value == null)
            {
                self.WriteLine("{0}: null", name);
            }
            else
            {
                self.WriteLine("{0}: {1} {2} {3}", name, value.X, value.Y, value.Z);
            }
        }
    }
}
