using System;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
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

        public class Entity
        {
            public uint Id { get; set; }
            public uint Type { get; set; }
            public Delta Delta { get; set; }
        }

        public List<Entity> Entities { get; set; }
        public List<Delta> ExtraEntityDeltas { get; set; }

        public override void Read(BitReader buffer)
        {
            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            Entities = new List<Entity>();

            while (true)
            {
                uint id = buffer.ReadUnsignedBits(11);

                if (id == (1 << 11) - 1) // All 1's.
                {
                    break;
                }

                Entity entity = new Entity
                {
                    Id = id
                };

                entity.Type = buffer.ReadUnsignedBits(2);
                string typeName = "custom_entity_state_t";

                if ((entity.Type & 1) != 0)
                {
                    if (entity.Id > 0 && entity.Id <= demo.MaxClients)
                    {
                        typeName = "entity_state_player_t";
                    }
                    else
                    {
                        typeName = "entity_state_t";
                    }
                }

                DeltaStructure structure = demo.FindDeltaStructure(typeName);
                entity.Delta = structure.CreateDelta();
                structure.ReadDelta(buffer, entity.Delta);
                Entities.Add(entity);
            }

            uint footer = buffer.ReadUnsignedBits(5);

            if (footer != (1 << 5) - 1) // All 1's.
            {
                throw new ApplicationException(string.Format("Bad svc_spawnbaseline footer \"{0}\".", footer));
            }

            uint nExtraEntityDeltas = buffer.ReadUnsignedBits(6);
            ExtraEntityDeltas = new List<Delta>((int)nExtraEntityDeltas);
            DeltaStructure entityStateStructure = demo.FindDeltaStructure("entity_state_t");

            for (int i = 0; i < nExtraEntityDeltas; i++)
            {
                Delta delta = entityStateStructure.CreateDelta();
                entityStateStructure.ReadDelta(buffer, delta);
                ExtraEntityDeltas.Add(delta);
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
            log.WriteLine("Num entities: {0}", Entities.Count);
            // TODO
        }
#endif
    }
}
