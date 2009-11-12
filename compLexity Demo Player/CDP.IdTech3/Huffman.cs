using System;
using System.Runtime.InteropServices;

namespace CDP.IdTech3
{
    internal static class Huffman
    {
        [DllImport("huffman.dll")]
        private static extern void HuffmanInit();

        [DllImport("huffman.dll")]
        private static extern uint HuffmanReadUInt(byte[] buffer, ref int bitOffset, int nBits);

        [DllImport("huffman.dll")]
        private static extern void HuffmanWriteUInt(byte[] buffer, ref int bitOffset, uint value, int nBits);

        private static object initialisedLock = new object();
        private static bool isInitialised = false;

        public static uint ReadUInt(byte[] buffer, ref int bitOffset, int nBits)
        {
            InitialiseTree();
            return HuffmanReadUInt(buffer, ref bitOffset, nBits);
        }

        public static void WriteUInt(byte[] buffer, ref int bitOffset, uint value, int nBits)
        {
            InitialiseTree();
            HuffmanWriteUInt(buffer, ref bitOffset, value, nBits);
        }

        private static void InitialiseTree()
        {
            // The tree only needs to be built once; the only mutable state is the current bit within a byte array, which BitReader keeps track of. Initialisation needs to be thread safe.
            lock (initialisedLock)
            {
                if (!isInitialised)
                {
                    HuffmanInit();
                    isInitialised = true;
                }
            }
        }
    }
}
