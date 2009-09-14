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

        public class Entity
        {
            public uint Id { get; set; }
            public bool Remove { get; set; }
            public Delta Delta { get; set; }
        }

        public ushort MaxEntities { get; set; }
        public byte DeltaSequenceNumber { get; set; }
        public List<Entity> Entities { get; set; }

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
                    entity.Delta = structure.CreateDelta();
                    structure.ReadDelta(buffer, entity.Delta);
                }

                entity.Id = entityId;
                Entities.Add(entity);
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
            log.WriteLine("Max entities: {0}", MaxEntities);
            log.WriteLine("Delta sequence: {0}", DeltaSequenceNumber);
            log.WriteLine("Num entities: {0}", Entities.Count);
            // TODO
        }
#endif
    }
}
