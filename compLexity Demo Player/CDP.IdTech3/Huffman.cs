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

        private static bool isInitialised = false;

        public static uint ReadUInt(byte[] buffer, ref int bitOffset, int nBits)
        {
            if (!isInitialised)
            {
                HuffmanInit();
            }

            return HuffmanReadUInt(buffer, ref bitOffset, nBits);
        }
    }
}
