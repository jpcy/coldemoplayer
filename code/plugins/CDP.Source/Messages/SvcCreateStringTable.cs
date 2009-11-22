using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.Source.Messages
{
    public class SvcCreateStringTable : Message
    {
        public class Entry
        {
            public bool Unknown1 { get; set; }
            public string Name { get; set; }
            public uint UserDataNumBits { get; set; }
            public byte[] UserData { get; set; }
        }

        public override MessageIds Id
        {
            get { return MessageIds.SVC_CreateStringTable; }
        }

        public override MessageIds_Protocol36 Id_Protocol36
        {
            get { return MessageIds_Protocol36.SVC_CreateStringTable; }
        }

        public override string Name
        {
            get { return "SVC_CreateStringTable"; }
        }

        public string TableName { get; set; }
        public ushort MaxEntries { get; set; }
        public uint NumEntries { get; set; }
        public uint NumBits { get; set; }
        public bool IsUserDataFixedSize { get; set; }
        public uint UserDataSize { get; set; }
        public uint UserDataSizeBits { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsUsingDictionary { get; set; }
        public bool Unknown1 { get; set; }

        // For compressed tables (in bytes).
        public int OriginalLength { get; set; }
        public int CompressedLength { get; set; }

        public List<Entry> Entries { get; set; }

        public SvcCreateStringTable()
        {
            Entries = new List<Entry>();
        }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekString();
            buffer.SeekBits(Core.Math.LogBase2(buffer.ReadUShort()) + 1);
            uint nBits = buffer.ReadUBits(20);

            if (buffer.ReadBoolean())
            {
                buffer.SeekBits(16);
            }

            if (Demo.NetworkProtocol >= 15)
            {
                buffer.SeekBits(1);
            }

            if (Demo.NetworkProtocol >= 36)
            {
                buffer.SeekBits(1);
            }

            buffer.SeekBits(nBits);
        }

        public override void Read(BitReader buffer)
        {
            TableName = buffer.ReadString();
            MaxEntries = buffer.ReadUShort();
            uint numEntriesBits = Core.Math.LogBase2(MaxEntries) + 1;
            NumEntries = buffer.ReadUBits(numEntriesBits);
            NumBits = buffer.ReadUBits(20);
            IsUserDataFixedSize = buffer.ReadBoolean();

            if (IsUserDataFixedSize)
            {
                UserDataSize = buffer.ReadUBits(12);
                UserDataSizeBits = buffer.ReadUBits(4);
            }

            if (Demo.NetworkProtocol >= 15)
            {
                IsCompressed = buffer.ReadBoolean();
            }

            if (Demo.NetworkProtocol >= 36)
            {
                IsUsingDictionary = buffer.ReadBoolean();
            }

            int endBit = buffer.CurrentBit + (int)NumBits;

            if (IsCompressed)
            {
                OriginalLength = buffer.ReadInt();
                CompressedLength = buffer.ReadInt();
                // TODO: decompress here.
            }

            if (Demo.NetworkProtocol >= 36 && !IsUserDataFixedSize)
            {
                Unknown1 = buffer.ReadBoolean();
            }

            if (IsCompressed || IsUsingDictionary)
            {
                buffer.SeekBits(endBit, SeekOrigin.Begin);
            }
            else
            {
                CyclicQueue<string> history = new CyclicQueue<string>(32);

                for (int i = 0; i < NumEntries; i++)
                {
                    Entry entry = new Entry();
                    Entries.Add(entry);
                    entry.Unknown1 = buffer.ReadBoolean();

                    if (!entry.Unknown1)
                    {
                        // delta index?
                        //log.Debug_WriteBits(buffer, 2);
                        throw new ApplicationException("uk1");
                    }

                    if (buffer.ReadBoolean())
                    {
                        if (buffer.ReadBoolean())
                        {
                            uint historyIndex = buffer.ReadUBits(5);
                            uint historyLength = buffer.ReadUBits(5);
                            string deltaEntry = buffer.ReadString();
                            entry.Name = history[(int)historyIndex].Substring(0, (int)historyLength) + deltaEntry;
                        }
                        else
                        {
                            entry.Name = buffer.ReadString();
                        }
                    }

                    if (buffer.ReadBoolean())
                    {
                        entry.UserDataNumBits = UserDataSizeBits;

                        if (!IsUserDataFixedSize)
                        {
                            int bits = 12;

                            if (Demo.NetworkProtocol >= 8)
                            {
                                bits = 14;
                            }

                            entry.UserDataNumBits = buffer.ReadUBits(bits) * 8;
                        }

                        // Read user data.
                        uint userDataSizeBytes = entry.UserDataNumBits / 8;
                        uint remainingBits = entry.UserDataNumBits % 8;
                        BitWriter writer = new BitWriter((int)userDataSizeBytes + (remainingBits > 0 ? 1 : 0));

                        if (userDataSizeBytes > 0)
                        {
                            writer.WriteBytes(buffer.ReadBytes(userDataSizeBytes));
                        }

                        if (remainingBits > 0)
                        {
                            writer.WriteUBits(buffer.ReadUBits(remainingBits), (int)remainingBits);
                        }

                        entry.UserData = writer.ToArray();
                    }

                    history.Enqueue(entry.Name);
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Table: {0}", TableName);
            log.WriteLine("Max entries: {0}", MaxEntries);
            log.WriteLine("Num entries: {0}", NumEntries);
            log.WriteLine("Num bits: {0}", NumBits);

            if (IsUserDataFixedSize)
            {
                log.WriteLine("User data size: {0}", UserDataSize);
                log.WriteLine("User data size bits: {0}", UserDataSizeBits);
            }
            else
            {
                log.WriteLine("No user data.");
            }

            if (Demo.NetworkProtocol >= 15)
            {
                log.WriteLine("Compressed: {0}", IsCompressed);
            }

            if (Demo.NetworkProtocol >= 36)
            {
                log.WriteLine("Using dictionary: {0}", IsUsingDictionary);
            }

            if (IsCompressed)
            {
                log.WriteLine("Original length: {0}", OriginalLength);
                log.WriteLine("Compressed length: {0}", CompressedLength);
            }

            if (Demo.NetworkProtocol >= 36 && !IsUserDataFixedSize)
            {
                log.WriteLine("Unknown1: {0}", Unknown1);
            }

            foreach (Entry entry in Entries)
            {
                log.WriteLine("\tName: {0}", entry.Name);
                log.WriteLine("\tuk1: {0}", entry.Unknown1);
                log.WriteLine("\tUser data num. bits: {0}", entry.UserDataNumBits);
            }
        }
    }
}
