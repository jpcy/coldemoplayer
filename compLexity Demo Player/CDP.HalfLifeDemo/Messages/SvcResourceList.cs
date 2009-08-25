using System;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcResourceList : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_resourcelist; }
        }

        public override string Name
        {
            get { return "svc_resourcelist"; }
        }

        public class Resource
        {
            public enum Types : uint
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
            public enum FlagBits : uint
            {
                None = 0,
                FatalIfMissing = (1<<0),
                WasMissing = (1<<1),
                Custom = (1<<2),
                Requested = (1<<3),
                Precached = (1<<4)
            }

            public Types Type { get; set; }
            public string Name { get; set; }
            public uint Index { get; set; }
            public uint DownloadSize { get; set; }
            public FlagBits Flags { get; set; }
            public byte[] Hash { get; set; }
            public byte[] Reserved { get; set; }
        }

        public List<Resource> Resources { get; set; }
        public List<uint> ConsistencyList { get; set; }

        private const int hashLength = 16;
        private const int reservedLength = 32;

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint nEntries = buffer.ReadUnsignedBits(12);
            Resources = new List<Resource>((int)nEntries);

            for (int i = 0; i < nEntries; i++)
            {
                Resource resource = new Resource();
                resource.Type = (Resource.Types)buffer.ReadUnsignedBits(4);
                resource.Name = buffer.ReadString();
                resource.Index = buffer.ReadUnsignedBits(12);
                resource.DownloadSize = buffer.ReadUnsignedBits(24);
                resource.Flags = (Resource.FlagBits)buffer.ReadUnsignedBits(3);

                if ((resource.Flags & Resource.FlagBits.Custom) == Resource.FlagBits.Custom)
                {
                    resource.Hash = buffer.ReadBytes(hashLength);
                }

                if (buffer.ReadBoolean())
                {
                    resource.Reserved = buffer.ReadBytes(reservedLength);
                }

                Resources.Add(resource);
            }

            // Consistency list.
            // Indicies of resources to force consistency upon. Delta compressed from the last index, starting with 0.
            if (buffer.ReadBoolean())
            {
                ConsistencyList = new List<uint>();
                uint currentIndex = 0;

                while (buffer.ReadBoolean())
                {
                    uint nBits = (buffer.ReadBoolean() ? 5u : 10u);
                    uint index = buffer.ReadUnsignedBits(nBits) + currentIndex;
                    ConsistencyList.Add(index);
                    currentIndex = index;
                }
            }

            buffer.SkipRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUnsignedBits((uint)Resources.Count, 12);

            foreach (Resource resource in Resources)
            {
                buffer.WriteUnsignedBits((uint)resource.Type, 4);
                buffer.WriteString(resource.Name);
                buffer.WriteUnsignedBits(resource.Index, 12);
                buffer.WriteUnsignedBits(resource.DownloadSize, 24);
                buffer.WriteUnsignedBits((uint)resource.Flags, 3);

                if ((resource.Flags & Resource.FlagBits.Custom) == Resource.FlagBits.Custom)
                {
                    if (resource.Hash.Length != hashLength)
                    {
                        throw new ApplicationException(string.Format("Bad resource hash length. Length is \"{0}\", expected \"{1}\".", resource.Hash.Length, hashLength));
                    }

                    buffer.WriteBytes(resource.Hash);
                }

                if (resource.Reserved == null)
                {
                    buffer.WriteBoolean(false);
                }
                else
                {
                    if (resource.Reserved.Length != reservedLength)
                    {
                        throw new ApplicationException(string.Format("Bad resource reserved data length. Length is \"{0}\", expected \"{1}\".", resource.Reserved.Length, reservedLength));
                    }

                    buffer.WriteBoolean(true);
                    buffer.WriteBytes(resource.Reserved);
                }

                // No need to write the consistency list, at least for now.
                buffer.WriteBoolean(false);
            }

            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num resources: {0}", Resources.Count);
            // TODO
        }
#endif
    }
}
