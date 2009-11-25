using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.HalfLife
{
    public class Header
    {
        public static int SizeInBytes
        {
            get { return 544; }
        }

        public string Magic { get; set; }
        public uint DemoProtocol { get; set; }
        public uint NetworkProtocol { get; set; }
        public string MapName { get; set; }
        public string GameFolderName { get; set; }
        public uint MapChecksum { get; set; }
        public uint DirectoryEntriesOffset { get; set; }

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
                throw new ApplicationException(string.Format(Strings.UnsupportedDemoProtocol, DemoProtocol, expectedDemoProtocol));
            }

            NetworkProtocol = br.ReadUInt();

            if (NetworkProtocol < minimumNetworkProtocol)
            {
                throw new ApplicationException(string.Format(Strings.UnsupportedNetworkProtocol, NetworkProtocol, minimumNetworkProtocol));
            }

            MapName = br.ReadString(mapNameLength);
            GameFolderName = br.ReadString(gameFolderLength);
            MapChecksum = br.ReadUInt();
            DirectoryEntriesOffset = br.ReadUInt();
        }

        public byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(Magic, magicLength);
            buffer.WriteUInt(DemoProtocol);
            buffer.WriteUInt(NetworkProtocol);
            buffer.WriteString(MapName, mapNameLength);
            buffer.WriteString(GameFolderName, gameFolderLength);
            buffer.WriteUInt(MapChecksum);
            buffer.WriteUInt(DirectoryEntriesOffset);
            return buffer.ToArray();
        }

        public void Log(StreamWriter log)
        {
            log.WriteLine("Magic: {0}", magicLength);
            log.WriteLine("Demo protocol: {0}", DemoProtocol);
            log.WriteLine("Network protocol: {0}", NetworkProtocol);
            log.WriteLine("Map: {0}", MapName);
            log.WriteLine("Game folder: {0}", GameFolderName);
            log.WriteLine("Map checksum: {0}", MapChecksum);
            log.WriteLine("Directory entries offset: {0}", DirectoryEntriesOffset);
        }
    }
}
