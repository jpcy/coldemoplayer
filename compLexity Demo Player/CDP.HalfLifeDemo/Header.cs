using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLifeDemo
{
    public class Header
    {
        public static int SizeInBytes
        {
            get { return 544; }
        }

        public string Magic { get; private set; }
        public uint DemoProtocol { get; private set; }
        public uint NetworkProtocol { get; private set; }
        public string MapName { get; private set; }
        public string GameFolderName { get; private set; }
        public uint MapChecksum { get; private set; }
        public uint DirectoryEntriesOffset { get; private set; }

        private const int magicLength = 8;
        private const int mapNameLength = 260;
        private const int gameFolderLength = 260;
        private const int expectedDemoProtocol = 5;
        private const int minimumNetworkProtocol = 43;

        public void Read(byte[] buffer)
        {
            BitReader br = new BitReader(buffer);
            Magic = br.ReadString(magicLength);
            DemoProtocol = br.ReadUInt();

            if (DemoProtocol != expectedDemoProtocol)
            {
                throw new ApplicationException(string.Format("Unknown demo protocol \"{0}\", should be {1}.", DemoProtocol, expectedDemoProtocol));
            }

            NetworkProtocol = br.ReadUInt();

            if (NetworkProtocol < minimumNetworkProtocol)
            {
                throw new ApplicationException(string.Format("Unsupported network protocol \"{0}\", should be {1} or higher.", NetworkProtocol, minimumNetworkProtocol));
            }

            MapName = br.ReadString(mapNameLength);
            GameFolderName = br.ReadString(gameFolderLength);
            MapChecksum = br.ReadUInt();
            DirectoryEntriesOffset = br.ReadUInt();
        }

        public byte[] Write()
        {
            return null;
        }
    }
}
