using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.Collections.Generic;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcClientData : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_clientdata; }
        }

        public override string Name
        {
            get { return "svc_clientdata"; }
        }

        public class Weapon
        {
            public uint Index { get; set; }
            public Delta Delta { get; set; }
        }

        public byte? DeltaSequenceNumber { get; set; }
        public Delta Delta { get; set; }
        public List<Weapon> Weapons { get; set; }

        public override void Read(BitReader buffer)
        {
            if (demo.IsHltv)
            {
                return;
            }

            if (demo.NetworkProtocol <= 43)
            {
                buffer.Endian = BitReader.Endians.Big;
            }

            if (buffer.ReadBoolean())
            {
                DeltaSequenceNumber = buffer.ReadByte();
            }

            DeltaStructure clientDataStructure = demo.FindDeltaStructure("clientdata_t");
            Delta = clientDataStructure.CreateDelta();
            clientDataStructure.ReadDelta(buffer, Delta);
            DeltaStructure weaponStructure = demo.FindDeltaStructure("weapon_data_t");
            Weapons = new List<Weapon>();

            while (buffer.ReadBoolean())
            {
                Weapon weapon = new Weapon();

                if (demo.NetworkProtocol < 47) // TODO: beta steam detection
                {
                    weapon.Index = buffer.ReadUnsignedBits(5);
                }
                else
                {
                    weapon.Index = buffer.ReadUnsignedBits(6);
                }

                weapon.Delta = weaponStructure.CreateDelta();
                weaponStructure.ReadDelta(buffer, weapon.Delta);
                Weapons.Add(weapon);
            }

            buffer.SkipRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override byte[] Write()
        {
            if (demo.IsHltv)
            {
                return null;
            }

            BitWriter buffer = new BitWriter();

            if (DeltaSequenceNumber == null)
            {
                buffer.WriteBoolean(false);
            }
            else
            {
                buffer.WriteBoolean(true);
                buffer.WriteByte(DeltaSequenceNumber.Value);
            }

            // TODO

            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            if (demo.IsHltv)
            {
                return;
            }

            log.WriteLine("Delta sequence number: {0}", DeltaSequenceNumber);
            // TODO
        }
#endif
    }
}
