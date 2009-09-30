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

        public override bool CanSkipWhenWriting
        {
            // Because of weapon index num. bits.
            get { return demo.NetworkProtocol >= 47; }
        }

        public class Weapon
        {
            public uint Index { get; set; }
            public Delta Delta { get; set; }
        }

        public byte? DeltaSequenceNumber { get; set; }
        public Delta Delta { get; set; }
        public List<Weapon> Weapons { get; set; }

        public override void Skip(BitReader buffer)
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
                buffer.SeekBytes(1);
            }

            DeltaStructure clientDataStructure = demo.FindDeltaStructure("clientdata_t");
            clientDataStructure.SkipDelta(buffer);
            DeltaStructure weaponDataStructure = demo.FindDeltaStructure("weapon_data_t");

            while (buffer.ReadBoolean())
            {
                if (demo.NetworkProtocol < 47) // TODO: beta steam detection
                {
                    buffer.SeekBits(5);
                }
                else
                {
                    buffer.SeekBits(6);
                }

                weaponDataStructure.SkipDelta(buffer);
            }

            buffer.SeekRemainingBitsInCurrentByte();
        }

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
                    weapon.Index = buffer.ReadUBits(5);
                }
                else
                {
                    weapon.Index = buffer.ReadUBits(6);
                }

                weapon.Delta = weaponStructure.CreateDelta();
                weaponStructure.ReadDelta(buffer, weapon.Delta);
                Weapons.Add(weapon);
            }

            buffer.SeekRemainingBitsInCurrentByte();
            buffer.Endian = BitReader.Endians.Little;
        }

        public override void Write(BitWriter buffer)
        {
            if (demo.IsHltv)
            {
                return;
            }

            if (DeltaSequenceNumber == null)
            {
                buffer.WriteBoolean(false);
            }
            else
            {
                buffer.WriteBoolean(true);
                buffer.WriteByte(DeltaSequenceNumber.Value);
            }

            DeltaStructure clientDataStructure = demo.FindDeltaStructure("clientdata_t");
            clientDataStructure.WriteDelta(buffer, Delta);
            DeltaStructure weaponStructure = demo.FindDeltaStructure("weapon_data_t");

            foreach (Weapon weapon in Weapons)
            {
                buffer.WriteBoolean(true);
                buffer.WriteUBits(weapon.Index, 6);
                weaponStructure.WriteDelta(buffer, weapon.Delta);
            }

            buffer.WriteBoolean(false);
        }

        public override void Log(StreamWriter log)
        {
            if (demo.IsHltv)
            {
                return;
            }

            log.WriteLine("Delta sequence number: {0}", DeltaSequenceNumber);
            // TODO
        }
    }
}
