using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLife.Messages
{
    // Very similar to svc_resourcelist entries, but bits aren't packed as tightly.
    public class SvcCustomization : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_customization; }
        }

        public override string Name
        {
            get { return "svc_customization"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public enum Types : byte
        {
            Sound,
            Skin,
            Model,
            Decal,
            Generic,
            EventScript,
            World
        }

        [Flags]
        public enum FlagBits : byte
        {
            None = 0,
            FatalIfMissing = (1 << 0),
            WasMissing = (1 << 1),
            Custom = (1 << 2),
            Requested = (1 << 3),
            Precached = (1 << 4)
        }

        public byte Slot { get; set; }
        public Types Type { get; set; }
        public string FileName { get; set; }
        public ushort Index { get; set; }
        public uint DownloadSize { get; set; }
        public FlagBits Flags { get; set; }
        public byte[] Hash { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);
            buffer.SeekString();
            buffer.SeekBytes(6);

            if (((FlagBits)buffer.ReadByte() & FlagBits.Custom) == FlagBits.Custom)
            {
                buffer.SeekBytes(16);
            }
        }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            Type = (Types)buffer.ReadByte();
            FileName = buffer.ReadString();
            Index = buffer.ReadUShort();
            DownloadSize = buffer.ReadUInt();
            Flags = (FlagBits)buffer.ReadByte();

            if ((Flags & FlagBits.Custom) == FlagBits.Custom)
            {
                Hash = buffer.ReadBytes(16);
            }
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Slot);
            buffer.WriteByte((byte)Type);
            buffer.WriteString(FileName);
            buffer.WriteUShort(Index);
            buffer.WriteUInt(DownloadSize);
            buffer.WriteByte((byte)Flags);

            if ((Flags & FlagBits.Custom) == FlagBits.Custom)
            {
                buffer.WriteBytes(Hash);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);
            log.WriteLine("Type: {0}", Type);
            log.WriteLine("FileName: {0}", FileName);
            log.WriteLine("Index: {0}", Index);
            log.WriteLine("DownloadSize: {0}", DownloadSize);
            log.WriteLine("Flags: {0}", Flags);

            if (Hash != null)
            {
                log.WriteLine("Hash length: {0}", Hash.Length);
            }
        }
    }
}
