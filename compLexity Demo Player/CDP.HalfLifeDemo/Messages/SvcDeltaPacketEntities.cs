using System;
using System.IO;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcDeltaPacketEntities : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_deltapacketentities; }
        }

        public override string Name
        {
            get { return "svc_deltapacketentities"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public class Entity
        {
            public uint Id { get; set; }
            public bool Remove { get; set; }
            public Delta Delta { get; set; }
            public bool Custom { get; set; }
            public bool Unknown1 { get; set; }
            public bool Unknown2 { get; set; }
        }

        public ushort MaxEntities { get; set; }
        public byte DeltaSequenceNumber { get; set; }
        public List<Entity> Entities { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(3);

            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            uint entityId = 0;

            while (true)
            {
                ushort footer = buffer.ReadUShort();

                if (footer == 0)
                {
                    break;
                }
                else
                {
                    buffer.SeekBits(-16);
                }

                bool remove = buffer.ReadBoolean();

                if (buffer.ReadBoolean())
                {
                    entityId = buffer.ReadUnsignedBits(11);
                }
                else
                {
                    entityId += buffer.ReadUnsignedBits(6);
                }

                if (!remove)
                {
                    if (demo.GameFolderName == "tfc")
                    {
                        // TODO: look into this. Does TFC simply use some feature other games don't or is it a fork of the Half-Life engine. Probably the former.
                        buffer.SeekBits(1); // unknown
                    }

                    bool custom = buffer.ReadBoolean();

                    if (demo.NetworkProtocol <= 43)
                    {
                        buffer.ReadBoolean(); // Unknown, always 0.
                    }

                    string typeName = "entity_state_t";

                    if (entityId > 0 && entityId <= demo.MaxClients)
                    {
                        typeName = "entity_state_player_t";
                    }
                    else if (custom)
                    {
                        typeName = "custom_entity_state_t";
                    }

                    DeltaStructure structure = demo.FindDeltaStructure(typeName);
                    structure.SkipDelta(buffer);
                }
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            MaxEntities = buffer.ReadUShort();
            DeltaSequenceNumber = buffer.ReadByte();

            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            Entities = new List<Entity>();
            uint entityId = 0;

            while (true)
            {
                ushort footer = buffer.ReadUShort();

                if (footer == 0)
                {
                    break;
                }
                else
                {
                    buffer.SeekBits(-16);
                }

                Entity entity = new Entity();
                entity.Remove = buffer.ReadBoolean();

                // Entity ID delta compression.
                // 3 cases:
                // 1) increment by 1.
                // 2) increment by value (6 bits).
                // 3) absolute (11 bits).
                if (buffer.ReadBoolean())
                {
                    entityId = buffer.ReadUnsignedBits(11);
                }
                else
                {
                    entityId += buffer.ReadUnsignedBits(6);
                }

                if (!entity.Remove)
                {
                    if (demo.GameFolderName == "tfc")
                    {
                        // TODO: look into this. Does TFC simply use some feature other games don't or is it a fork of the Half-Life engine. Probably the former.
                        entity.Unknown1 = buffer.ReadBoolean();
                    }

                    entity.Custom = buffer.ReadBoolean();

                    if (demo.NetworkProtocol <= 43)
                    {
                        entity.Unknown2 = buffer.ReadBoolean(); // Unknown, always 0.
                    }

                    string typeName = "entity_state_t";

                    if (entityId > 0 && entityId <= demo.MaxClients)
                    {
                        typeName = "entity_state_player_t";
                    }
                    else if (entity.Custom)
                    {
                        typeName = "custom_entity_state_t";
                    }

                    DeltaStructure structure = demo.FindDeltaStructure(typeName);
                    entity.Delta = structure.CreateDelta();
                    structure.ReadDelta(buffer, entity.Delta);
                }

                entity.Id = entityId;
                Entities.Add(entity);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteUShort(MaxEntities);
            buffer.WriteByte(DeltaSequenceNumber);

            foreach (Entity entity in Entities)
            {
                buffer.WriteBoolean(entity.Remove);
                buffer.WriteBoolean(true);
                buffer.WriteUnsignedBits(entity.Id, 11);

                if (!entity.Remove)
                {
                    if (demo.GameFolderName == "tfc")
                    {
                        buffer.WriteBoolean(entity.Unknown1);
                    }

                    buffer.WriteBoolean(entity.Custom);

                    if (demo.NetworkProtocol <= 43)
                    {
                        buffer.WriteBoolean(entity.Unknown2);
                    }

                    string typeName = "entity_state_t";

                    if (entity.Id > 0 && entity.Id <= demo.MaxClients)
                    {
                        typeName = "entity_state_player_t";
                    }
                    else if (entity.Custom)
                    {
                        typeName = "custom_entity_state_t";
                    }

                    DeltaStructure structure = demo.FindDeltaStructure(typeName);
                    structure.WriteDelta(buffer, entity.Delta);
                }
            }

            buffer.WriteUShort(0);
            return buffer.ToArray();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Max entities: {0}", MaxEntities);
            log.WriteLine("Delta sequence: {0}", DeltaSequenceNumber);
            log.WriteLine("Num entities: {0}", Entities.Count);
            // TODO
        }
    }
}
