using System;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcSpawnBaseline : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_spawnbaseline; }
        }

        public override string Name
        {
            get { return "svc_spawnbaseline"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol > 43; }
        }

        public class Entity
        {
            public uint Id { get; set; }
            public uint Type { get; set; }
            public string TypeName { get; set; }
            public Delta Delta { get; set; }
        }

        public List<Entity> Entities { get; set; }
        public List<Delta> ExtraEntityDeltas { get; set; }

        public override void Skip(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            while (true)
            {
                uint id = buffer.ReadUBits(11);

                if (id == (1 << 11) - 1) // All 1's.
                {
                    break;
                }

                uint type = buffer.ReadUBits(2);
                string typeName = "custom_entity_state_t";

                if ((type & 1) != 0)
                {
                    if (id > 0 && id <= demo.MaxClients)
                    {
                        typeName = "entity_state_player_t";
                    }
                    else
                    {
                        typeName = "entity_state_t";
                    }
                }

                DeltaStructure structure = demo.FindReadDeltaStructure(typeName);
                structure.SkipDelta(buffer);
            }

            uint footer = buffer.ReadUBits(5);

            if (footer != (1 << 5) - 1) // All 1's.
            {
                throw new ApplicationException(string.Format("Bad svc_spawnbaseline footer \"{0}\".", footer));
            }

            uint nExtraEntityDeltas = buffer.ReadUBits(6);
            DeltaStructure entityStateStructure = demo.FindReadDeltaStructure("entity_state_t");

            for (int i = 0; i < nExtraEntityDeltas; i++)
            {
                entityStateStructure.SkipDelta(buffer);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            Entities = new List<Entity>();

            while (true)
            {
                uint id = buffer.ReadUBits(11);

                if (id == (1 << 11) - 1) // All 1's.
                {
                    break;
                }

                Entity entity = new Entity
                {
                    Id = id
                };

                entity.Type = buffer.ReadUBits(2);
                entity.TypeName = "custom_entity_state_t";

                if ((entity.Type & 1) != 0)
                {
                    if (entity.Id > 0 && entity.Id <= demo.MaxClients)
                    {
                        entity.TypeName = "entity_state_player_t";
                    }
                    else
                    {
                        entity.TypeName = "entity_state_t";
                    }
                }

                DeltaStructure structure = demo.FindReadDeltaStructure(entity.TypeName);
                entity.Delta = structure.CreateDelta();
                structure.ReadDelta(buffer, entity.Delta);
                Entities.Add(entity);
            }

            uint footer = buffer.ReadUBits(5);

            if (footer != (1 << 5) - 1) // All 1's.
            {
                throw new ApplicationException(string.Format("Bad svc_spawnbaseline footer \"{0}\".", footer));
            }

            uint nExtraEntityDeltas = buffer.ReadUBits(6);
            ExtraEntityDeltas = new List<Delta>((int)nExtraEntityDeltas);
            DeltaStructure entityStateStructure = demo.FindReadDeltaStructure("entity_state_t");

            for (int i = 0; i < nExtraEntityDeltas; i++)
            {
                Delta delta = entityStateStructure.CreateDelta();
                entityStateStructure.ReadDelta(buffer, delta);
                ExtraEntityDeltas.Add(delta);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

        public override void Write(BitWriter buffer)
        {
            foreach (Entity entity in Entities)
            {
                buffer.WriteUBits(entity.Id, 11);
                buffer.WriteUBits(entity.Type, 2);
                DeltaStructure structure = demo.FindWriteDeltaStructure(entity.TypeName);
                structure.WriteDelta(buffer, entity.Delta);
            }

            buffer.WriteUBits((1 << 16) - 1, 16);

            buffer.WriteUBits((uint)ExtraEntityDeltas.Count, 6);
            DeltaStructure entityStateStructure = demo.FindWriteDeltaStructure("entity_state_t");

            foreach (Delta delta in ExtraEntityDeltas)
            {
                entityStateStructure.WriteDelta(buffer, delta);
            }

            buffer.PadRemainingBitsInCurrentByte();
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Num entities: {0}", Entities.Count);
            // TODO
        }
    }
}
