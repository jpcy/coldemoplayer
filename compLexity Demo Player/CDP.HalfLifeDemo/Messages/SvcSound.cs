using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcSound : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_sound; }
        }

        public override string Name
        {
            get { return "svc_sound"; }
        }

        [Flags]
        private enum FlagBits : uint
        {
            None = 0,
            Volume = (1 << 0),
            Attenuation = (1 << 1),
            Index = (1 << 2),
            Pitch = (1 << 3)
        }

        public byte? Volume { get; set; }
        public byte? Attenuation { get; set; }
        public uint Channel { get; set; }
        public uint Edict { get; set; }
        public ushort Index { get; set; }
        public Core.Vector Position { get; set; }
        public byte? Pitch;

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            FlagBits flags = (FlagBits)buffer.ReadUnsignedBits(9);

            if ((flags & FlagBits.Volume) == FlagBits.Volume)
            {
                Volume = buffer.ReadByte();
            }

            if ((flags & FlagBits.Attenuation) == FlagBits.Attenuation)
            {
                Attenuation = buffer.ReadByte();
            }

            Channel = buffer.ReadUnsignedBits(3);
            Edict = buffer.ReadUnsignedBits(11);

            if ((flags & FlagBits.Index) == FlagBits.Index)
            {
                Index = buffer.ReadUShort();
            }
            else
            {
                Index = buffer.ReadByte();
            }

            Position = new Core.Vector(buffer.ReadVectorCoord(true));

            if ((flags & FlagBits.Pitch) == FlagBits.Pitch)
            {
                Pitch = buffer.ReadByte();
            }

            buffer.SkipRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Volume: {0}", Volume);
            log.WriteLine("Attenuation: {0}", Attenuation);
            log.WriteLine("Channel: {0}", Channel);
            log.WriteLine("Edict: {0}", Edict);
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("Position: {0} {1} {2}", Position.X, Position.Y, Position.Z);
            log.WriteLine("Pitch: {0}", Pitch);
        }
#endif
    }
}
