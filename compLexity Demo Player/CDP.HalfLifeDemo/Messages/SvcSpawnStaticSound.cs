using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    /// <summary>
    /// Play an ambient sound.
    /// </summary>
    public class SvcSpawnStaticSound : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_spawnstaticsound; }
        }

        public override string Name
        {
            get { return "svc_spawnstaticsound"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public Core.Vector Position { get; set; }
        public ushort Index { get; set; }
        public byte Volume { get; set; }
        public byte Attenuation { get; set; }
        public ushort Edict { get; set; }
        public byte Pitch { get; set; }
        public byte Flags { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(14);
        }

        public override void Read(BitReader buffer)
        {
            Position = new Core.Vector();
            Position.X = buffer.ReadShort() / 8.0f;
            Position.Y = buffer.ReadShort() / 8.0f;
            Position.Z = buffer.ReadShort() / 8.0f;
            Index = buffer.ReadUShort();
            Volume = buffer.ReadByte();
            Attenuation = buffer.ReadByte();
            Edict = buffer.ReadUShort();
            Pitch = buffer.ReadByte();
            Flags = buffer.ReadByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteShort((short)(Position.X * 8.0f));
            buffer.WriteShort((short)(Position.Y * 8.0f));
            buffer.WriteShort((short)(Position.Z * 8.0f));
            buffer.WriteUShort(Index);
            buffer.WriteByte(Volume);
            buffer.WriteByte(Attenuation);
            buffer.WriteUShort(Edict);
            buffer.WriteByte(Pitch);
            buffer.WriteByte(Flags);
            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            if (Position != null)
            {
                log.WriteLine("Position: {0} {1} {2}", Position.X, Position.Y, Position.Z);
            }

            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Volume: {0}", Volume);
            log.WriteLine("Attenuation: {0}", Attenuation);
            log.WriteLine("Edict: {0}", Edict);
            log.WriteLine("Pitch: {0}", Pitch);
            log.WriteLine("Flags: {0}", Flags);
        }
    }
}
