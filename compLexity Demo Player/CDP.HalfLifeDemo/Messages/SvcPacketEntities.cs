using System;
using System.IO;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcPacketEntities : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_packetentities; }
        }

        public override string Name
        {
            get { return "svc_packetentities"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public class Entity
        {
            public uint Id { get; set; }
            public uint? BaselineIndex { get; set; }
            public Delta Delta { get; set; }
            public uint Unknown { get; set; }
            public bool Custom { get; set; }
            public string TypeName { get; set; }
        }

        public ushort MaxEntities { get; set; }
        public List<Entity> Entities { get; set; }

        public override void Skip(BitReader buffer)
        {
            buffer.SeekBytes(2);

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

                if (buffer.ReadBoolean())
                {
                    entityId++;
                }
                else
                {
                    if (buffer.ReadBoolean())
                    {
                        entityId = buffer.ReadUBits(11);
                    }
                    else
                    {
                        entityId += buffer.ReadUBits(6);
                    }
                }

                if (demo.GameFolderName == "tfc")
                {
                    // TODO: look into this. Does TFC simply use some feature other games don't or is it a fork of the Half-Life engine. Probably the former.
                    buffer.SeekBits(1); // unknown
                }

                bool custom = buffer.ReadBoolean();

                if (buffer.ReadBoolean())
                {
                    buffer.SeekBits(6);
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

                DeltaStructure structure = demo.FindReadDeltaStructure(typeName);
                structure.SkipDelta(buffer);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            MaxEntities = buffer.ReadUShort();

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

                // Entity ID delta compression.
                // 3 cases:
                // 1) increment by 1.
                // 2) increment by value (6 bits).
                // 3) absolute (11 bits).
                if (buffer.ReadBoolean())
                {
                    entityId++;
                }
                else
                {
                    if (buffer.ReadBoolean())
                    {
                        entityId = buffer.ReadUBits(11);
                    }
                    else
                    {
                        entityId += buffer.ReadUBits(6);
                    }                    
                }

                Entity entity = new Entity
                {
                    Id = entityId
                };
                
                if (demo.GameFolderName == "tfc")
                {
                    // TODO: look into this. Does TFC simply use some feature other games don't or is it a fork of the Half-Life engine. Probably the former.
                    entity.Unknown = buffer.ReadUBits(1);
                }

                entity.Custom = buffer.ReadBoolean();

                if (buffer.ReadBoolean())
                {
                    entity.BaselineIndex = buffer.ReadUBits(6);
                }

                entity.TypeName = "entity_state_t";

                if (entityId > 0 && entityId <= demo.MaxClients)
                {
                    entity.TypeName = "entity_state_player_t";
                }
                else if (entity.Custom)
                {
                    entity.TypeName = "custom_entity_state_t";
                }

                DeltaStructure structure = demo.FindReadDeltaStructure(entity.TypeName);
                entity.Delta = structure.CreateDelta();
                structure.ReadDelta(buffer, entity.Delta);
                Entities.Add(entity);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteUShort(MaxEntities); // TODO: should this be set to Entities.Count?

            foreach (Entity entity in Entities)
            {
                buffer.WriteBoolean(false);
                buffer.WriteBoolean(true);
                buffer.WriteUBits(entity.Id, 11);

                if (demo.GameFolderName == "tfc")
                {
                    buffer.WriteUBits(entity.Unknown, 1);
                }

                buffer.WriteBoolean(entity.Custom);

                if (entity.BaselineIndex == null)
                {
                    buffer.WriteBoolean(false);
                }
                else
                {
                    buffer.WriteBoolean(true);
                    buffer.WriteUBits(entity.BaselineIndex.Value, 6);
                }

                DeltaStructure structure = demo.FindWriteDeltaStructure(entity.TypeName);
                structure.WriteDelta(buffer, entity.Delta);
            }

            buffer.WriteUShort(0);
            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Max entities: {0}", MaxEntities);
            log.WriteLine("Num entities: {0}", Entities.Count);
            // TODO
        }
    }
}
