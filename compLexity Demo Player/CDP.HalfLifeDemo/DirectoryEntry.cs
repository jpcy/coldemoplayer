using System;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo
{
    public class DirectoryEntry
    {
        public static int SizeInBytes
        {
            get { return 92; }
        }

        private const int titleLength = 64;

        public int Number { get; set; }
        public string Title { get; set; }
        public int Flags { get; set; }
        public int CdTrack { get; set; }
        public float Duration { get; set; }
        public int NumFrames { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public void Read(byte[] buffer)
        {
            BitReader br = new BitReader(buffer);
            Number = br.ReadInt();
            Title = br.ReadString(titleLength);
            Flags = br.ReadInt();
            CdTrack = br.ReadInt();
            Duration = br.ReadFloat();
            NumFrames = br.ReadInt();
            Offset = br.ReadInt();
            Length = br.ReadInt();
        }
    }
}
