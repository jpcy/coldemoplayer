using System;
using System.IO;
using CDP.Core;

namespace CDP.Source.Messages
{
    public class SvcServerInfo : Message
    {
        public override MessageIds Id
        {
            get { return MessageIds.SVC_ServerInfo; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_ServerInfo; }
        }

        public override string Name
        {
            get { return "SVC_ServerInfo"; }
        }

        public short NetworkProtocol { get; set; }
        public int SpawnCount { get; set; }
        public bool Unknown1 { get; set; } // dedicated?
        public bool Unknown2 { get; set; } // dedicated?
        public int Unknown3 { get; set; }
        public int Unknown6 { get; set; }
        public ushort MaxClasses { get; set; }
        public int Unknown4 { get; set; }
        public byte Unknown5 { get; set; } // password?
        public byte MaxClients { get; set; }
        public float TimeDeltaPerTick { get; set; }
        public char ServerType { get; set; } // l: linux, w: windows
        public string GameFolder { get; set; }
        public string MapName { get; set; }
        public string SkyName { get; set; }
        public string ServerName { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBits(186);

            if (Demo.NetworkProtocol >= 36)
            {
                buffer.SeekBytes(4);
            }

            buffer.SeekString();
            buffer.SeekString();
            buffer.SeekString();
            buffer.SeekString();
        }

        public override void Read(BitReader buffer)
        {
            NetworkProtocol = buffer.ReadShort();
            SpawnCount = buffer.ReadInt();
            Unknown1 = buffer.ReadBoolean();
            Unknown2 = buffer.ReadBoolean();
            Unknown3 = buffer.ReadInt();

            if (Demo.NetworkProtocol >= 36)
            {
                Unknown6 = buffer.ReadInt();
            }

            MaxClasses = buffer.ReadUShort();
            Unknown4 = buffer.ReadInt();
            Unknown5 = buffer.ReadByte();
            MaxClients = buffer.ReadByte();
            TimeDeltaPerTick = buffer.ReadFloat();
            ServerType = (char)buffer.ReadByte();
            GameFolder = buffer.ReadString();
            MapName = buffer.ReadString();
            SkyName = buffer.ReadString();
            ServerName = buffer.ReadString();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteShort(NetworkProtocol);
            buffer.WriteInt(SpawnCount);
            buffer.WriteBoolean(Unknown1);
            buffer.WriteBoolean(Unknown2);
            buffer.WriteInt(Unknown3);

            if (Demo.NetworkProtocol >= 36)
            {
                buffer.WriteInt(Unknown6);
            }

            buffer.WriteUShort(MaxClasses);
            buffer.WriteInt(Unknown4);
            buffer.WriteByte(Unknown5);
            buffer.WriteByte(MaxClients);
            buffer.WriteFloat(TimeDeltaPerTick);
            buffer.WriteByte((byte)ServerType);
            buffer.WriteString(GameFolder);
            buffer.WriteString(MapName);
            buffer.WriteString(SkyName);
            buffer.WriteString(ServerName);
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Network protocol: {0}", NetworkProtocol);
            log.WriteLine("Spawn count: {0}", SpawnCount);
            log.WriteLine("UK1: {0}", Unknown1);
            log.WriteLine("UK2: {0}", Unknown2);
            log.WriteLine("UK3: {0}", Unknown3);

            if (Demo.NetworkProtocol >= 36)
            {
                log.WriteLine("UK6: {0}", Unknown6);
            }

            log.WriteLine("Max classes: {0}", MaxClasses);
            log.WriteLine("UK4: {0}", Unknown4);
            log.WriteLine("UK5: {0}", Unknown5);
            log.WriteLine("Max clients: {0}", MaxClients);
            log.WriteLine("Time delta per tick: {0}", TimeDeltaPerTick);
            log.WriteLine("Server type: {0}", ServerType);
            log.WriteLine("Game folder: {0}", GameFolder);
            log.WriteLine("Map name: {0}", MapName);
            log.WriteLine("Sky name: {0}", SkyName);
            log.WriteLine("Server name: {0}", ServerName);
        }
    }
}
