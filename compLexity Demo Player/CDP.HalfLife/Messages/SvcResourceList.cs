using System;
using System.Collections.Generic;
using System.IO;

namespace CDP.HalfLife.Messages
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

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
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

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint nEntries = buffer.ReadUBits(12);

            for (int i = 0; i < nEntries; i++)
            {
                buffer.SeekBits(4);
                buffer.SeekString();
                buffer.SeekBits(36);
                Resource.FlagBits flags = (Resource.FlagBits)buffer.ReadUBits(3);

                if ((flags & Resource.FlagBits.Custom) == Resource.FlagBits.Custom)
                {
                    buffer.SeekBytes(hashLength);
                }

                if (buffer.ReadBoolean())
                {
                    buffer.SeekBytes(reservedLength);
                }
            }

            // Consistency list.
            // Indicies of resources to force consistency upon. Delta compressed from the last index, starting with 0.
            if (buffer.ReadBoolean())
            {
                while (buffer.ReadBoolean())
                {
                    int nBits = (buffer.ReadBoolean() ? 5 : 10);
                    buffer.SeekBits(nBits);
                }
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint nEntries = buffer.ReadUBits(12);
            Resources = new List<Resource>((int)nEntries);

            for (int i = 0; i < nEntries; i++)
            {
                Resource resource = new Resource();
                resource.Type = (Resource.Types)buffer.ReadUBits(4);
                resource.Name = buffer.ReadString();
                resource.Index = buffer.ReadUBits(12);
                resource.DownloadSize = buffer.ReadUBits(24);
                resource.Flags = (Resource.FlagBits)buffer.ReadUBits(3);

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
                    uint index = buffer.ReadUBits(nBits) + currentIndex;
                    ConsistencyList.Add(index);
                    currentIndex = index;
                }
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUBits((uint)Resources.Count, 12);

            foreach (Resource resource in Resources)
            {
                buffer.WriteUBits((uint)resource.Type, 4);
                buffer.WriteString(resource.Name);
                buffer.WriteUBits(resource.Index, 12);
                buffer.WriteUBits(resource.DownloadSize, 24);
                buffer.WriteUBits((uint)resource.Flags, 3);

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
            }

            // No need to write the consistency list, at least for now.
            buffer.WriteBoolean(false);

            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num resources: {0}", Resources.Count);
            // TODO
        }
    }
}
