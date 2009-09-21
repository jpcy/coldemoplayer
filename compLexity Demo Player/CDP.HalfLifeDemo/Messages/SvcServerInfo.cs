using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcServerInfo : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_serverinfo; }
        }

        public override string Name
        {
            get { return "svc_serverinfo"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return false; }
        }

        public uint NetworkProtocol { get; set; }
        public uint ProcessCount { get; set; }
        public uint MungedMapChecksum { get; set; }
        public byte[] ClientDllChecksum { get; set; }
        public byte MaxClients { get; set; }
        public byte RecorderSlot { get; set; }
        public byte Deathmatch { get; set; }
        public string GameFolder { get; set; }
        public string ServerName { get; set; }
        public string MapName { get; set; }
        public string MapCycle { get; set; }
        public byte ExtraInfoFlag { get; set; }
        public byte[] ExtraInfo { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(31);
            buffer.SeekString();

            if (demo.NetworkProtocol > 43)
            {
                buffer.SeekString();
            }

            buffer.SeekString();

            if (demo.NetworkProtocol == 45)
            {
                // Peek and see if the next message ID is svc_sendextrainfo.
                byte nextMessageId = buffer.ReadByte();
                buffer.SeekBytes(-1);

                if (nextMessageId == (byte)EngineMessageIds.svc_sendextrainfo)
                {
                    return;
                }
            }

            buffer.SeekString();

            if (demo.NetworkProtocol > 43)
            {
                ExtraInfoFlag = buffer.ReadByte();

                if (ExtraInfoFlag > 0)
                {
                    int length = demo.NetworkProtocol == 45 ? 36 : 21;
                    buffer.SeekBytes(length);
                }
            }
        }

        public override void Read(BitReader buffer)
        {
            NetworkProtocol = buffer.ReadUInt();
            ProcessCount = buffer.ReadUInt();
            MungedMapChecksum = buffer.ReadUInt();
            ClientDllChecksum = buffer.ReadBytes(16);
            MaxClients = buffer.ReadByte();
            RecorderSlot = buffer.ReadByte();
            Deathmatch = buffer.ReadByte();
            GameFolder = buffer.ReadString();

            if (demo.NetworkProtocol > 43)
            {
                ServerName = buffer.ReadString();
            }
            else
            {
                ServerName = "Unknown";
            }

            MapName = buffer.ReadString();

            if (demo.NetworkProtocol == 45)
            {
                // Peek and see if the next message ID is svc_sendextrainfo.
                byte nextMessageId = buffer.ReadByte();
                buffer.SeekBytes(-1);

                if (nextMessageId == (byte)EngineMessageIds.svc_sendextrainfo)
                {
                    return;
                }
            }

            MapCycle = buffer.ReadString();

            if (demo.NetworkProtocol > 43)
            {
                ExtraInfoFlag = buffer.ReadByte();

                if (ExtraInfoFlag > 0)
                {
                    int length = demo.NetworkProtocol == 45 ? 36 : 21;
                    ExtraInfo = buffer.ReadBytes(length);
                }
            }
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUInt(Demo.NewestNetworkProtocol);
            buffer.WriteUInt(ProcessCount);
            buffer.WriteUInt(MungedMapChecksum);
            buffer.WriteBytes(ClientDllChecksum);
            buffer.WriteByte(MaxClients);
            buffer.WriteByte(RecorderSlot);
            buffer.WriteByte(Deathmatch);
            buffer.WriteString(GameFolder);
            buffer.WriteString(ServerName);
            buffer.WriteString(MapName);
            buffer.WriteString(MapCycle);
            buffer.WriteByte(0); // extra flag
            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Network protocol: {0}", NetworkProtocol);
            log.WriteLine("Process count: {0}", ProcessCount);
            log.WriteLine("Map checksum: {0}", MungedMapChecksum);
            log.Write("Client DLL checksum:");

            for (int i = 0; i < ClientDllChecksum.Length; i++)
            {
                log.Write(" {0}", ClientDllChecksum[i].ToString("X2"));
            }

            log.WriteLine();
            log.WriteLine("Max client: {0}", MaxClients);
            log.WriteLine("Recorder slot: {0}", RecorderSlot);
            log.WriteLine("uk1: {0}", Deathmatch);
            log.WriteLine("Game folder: {0}", GameFolder);
            log.WriteLine("Server name: {0}", ServerName);
            log.WriteLine("Map name: {0}", MapName);
            log.WriteLine("Map cycle: {0}", MapCycle);
            log.WriteLine("Extra info flag: {0}", ExtraInfoFlag);

            log.Write("Extra info:");

            if (ExtraInfo == null)
            {
                log.Write(" none");
            }
            else
            {
                for (int i = 0; i < ExtraInfo.Length; i++)
                {
                    log.Write(" {0}", ExtraInfo[i].ToString("X2"));
                }
            }

            log.WriteLine();
        }
    }
}
