using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
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

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
