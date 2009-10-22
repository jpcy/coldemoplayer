using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CDP.Source
{
    public class Header
    {
        public static int SizeInBytes
        {
            get { return 1072; }
        }

        public string Magic { get; set; }
        public int DemoProtocol { get; set; }
        public int NetworkProtocol { get; set; }
        public string ServerName { get; set; }
        public string RecorderName { get; set; }
        public string MapName { get; set; }
        public string GameFolder { get; set; }
        public float DurationInSeconds { get; set; }
        public int Ticks { get; set; }
        public int Frames { get; set; }
        public int SignonLength { get; set; }

        private const int magicLength = 8;
        private const int serverNameLength = 260;
        private const int recorderNameLength = 260;
        private const int mapNameLength = 260;
        private const int gameFolderLength = 260;

        public void Read(byte[] buffer)
        {
            Core.BitReader br = new Core.BitReader(buffer);
            Magic = br.ReadString(magicLength);
            DemoProtocol = br.ReadInt();

            /*if (demoProtocol != 3)
            {
                throw new ApplicationException(String.Format("Unknown demo protocol \"{0}\", should be 3.", demoProtocol));
            }*/

            NetworkProtocol = br.ReadInt();

            /*if (networkProtocol != 7)
            {
                throw new ApplicationException(String.Format("Unknown network protocol \"{0}\", should be 7.", networkProtocol));
            }*/

            ServerName = br.ReadString(serverNameLength);
            RecorderName = br.ReadString(recorderNameLength);
            MapName = br.ReadString(mapNameLength);
            GameFolder = br.ReadString(gameFolderLength);
            DurationInSeconds = br.ReadFloat();
            Ticks = br.ReadInt();
            Frames = br.ReadInt();
            SignonLength = br.ReadInt();
        }

        public byte[] Write()
        {
            Core.BitWriter bw = new Core.BitWriter();
            bw.WriteString(Magic, magicLength);
            bw.WriteInt(DemoProtocol);
            bw.WriteInt(NetworkProtocol);
            bw.WriteString(ServerName, serverNameLength);
            bw.WriteString(RecorderName, recorderNameLength);
            bw.WriteString(MapName, mapNameLength);
            bw.WriteString(GameFolder, gameFolderLength);
            bw.WriteFloat(DurationInSeconds);
            bw.WriteInt(Ticks);
            bw.WriteInt(Frames);
            bw.WriteInt(SignonLength);
            return bw.ToArray();
        }

        public void Log(StreamWriter log)
        {
            log.WriteLine("Magic: {0}", Magic);
            log.WriteLine("Demo protocol: {0}", DemoProtocol);
            log.WriteLine("Network protocol: {0}", NetworkProtocol);
            log.WriteLine("Server name: {0}", ServerName);
            log.WriteLine("Recorder name: {0}", RecorderName);
            log.WriteLine("Map name: {0}", MapName);
            log.WriteLine("Game folder: {0}", GameFolder);
            log.WriteLine("Duration: {0} seconds", DurationInSeconds);
            log.WriteLine("Num. ticks: {0}", Ticks);
            log.WriteLine("Num. frames: {0}", Frames);
            log.WriteLine("Signon length: {0}", SignonLength);
        }
    }
}
