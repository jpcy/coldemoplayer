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

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
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

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            FlagBits flags = (FlagBits)buffer.ReadUBits(9);

            if ((flags & FlagBits.Volume) == FlagBits.Volume)
            {
                buffer.SeekBytes(1);
            }

            if ((flags & FlagBits.Attenuation) == FlagBits.Attenuation)
            {
                buffer.SeekBytes(1);
            }

            buffer.SeekBits(14);

            if ((flags & FlagBits.Index) == FlagBits.Index)
            {
                buffer.SeekBytes(2);
            }
            else
            {
                buffer.SeekBytes(1);
            }

            buffer.ReadVectorCoord();

            if ((flags & FlagBits.Pitch) == FlagBits.Pitch)
            {
                buffer.SeekBytes(1);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            FlagBits flags = (FlagBits)buffer.ReadUBits(9);

            if ((flags & FlagBits.Volume) == FlagBits.Volume)
            {
                Volume = buffer.ReadByte();
            }

            if ((flags & FlagBits.Attenuation) == FlagBits.Attenuation)
            {
                Attenuation = buffer.ReadByte();
            }

            Channel = buffer.ReadUBits(3);
            Edict = buffer.ReadUBits(11);

            if ((flags & FlagBits.Index) == FlagBits.Index)
            {
                Index = buffer.ReadUShort();
            }
            else
            {
                Index = buffer.ReadByte();
            }

            Position = new Core.Vector(buffer.ReadVectorCoord());

            if ((flags & FlagBits.Pitch) == FlagBits.Pitch)
            {
                Pitch = buffer.ReadByte();
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            FlagBits flags = FlagBits.None;

            if (Volume.HasValue)
            {
                flags |= FlagBits.Volume;
            }

            if (Attenuation.HasValue)
            {
                flags |= FlagBits.Attenuation;
            }

            if (Index > byte.MaxValue)
            {
                flags |= FlagBits.Index;
            }

            if (Pitch.HasValue)
            {
                flags |= FlagBits.Pitch;
            }

            buffer.WriteUBits((uint)flags, 9);

            if ((flags & FlagBits.Volume) == FlagBits.Volume)
            {
                buffer.WriteByte(Volume.Value);
            }

            if ((flags & FlagBits.Attenuation) == FlagBits.Attenuation)
            {
                buffer.WriteByte(Attenuation.Value);
            }

            buffer.WriteUBits(Channel, 3);
            buffer.WriteUBits(Edict, 11);

            if ((flags & FlagBits.Index) == FlagBits.Index)
            {
                buffer.WriteUShort(Index);
            }
            else
            {
                buffer.WriteByte((byte)Index);
            }

            buffer.WriteVectorCoord(Position.ToArray());

            if ((flags & FlagBits.Pitch) == FlagBits.Pitch)
            {
                buffer.WriteByte(Pitch.Value);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Volume: {0}", Volume);
            log.WriteLine("Attenuation: {0}", Attenuation);
            log.WriteLine("Channel: {0}", Channel);
            log.WriteLine("Edict: {0}", Edict);
            log.WriteLine("Index: {0}", Index);

            if (Position != null)
            {
                log.WriteLine("Position: {0} {1} {2}", Position.X, Position.Y, Position.Z);
            }

            log.WriteLine("Pitch: {0}", Pitch);
        }
    }
}
