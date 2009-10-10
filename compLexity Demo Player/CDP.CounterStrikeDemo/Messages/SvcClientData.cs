using System;
using System.Collections.Generic;
using BitReader = CDP.Core.BitReader;

namespace CDP.CounterStrike.Messages
{
    class SvcClientData : CDP.HalfLife.Messages.SvcClientData
    {
        public override bool CanSkipWhenWriting
        {
            get { return demo.NetworkProtocol >= 47 && !IsBeta16(); }
        }

        private bool IsBeta16()
        {
            return demo.NetworkProtocol == 46 && ((CDP.CounterStrike.Demo)demo).Version == CDP.CounterStrike.Demo.Versions.CounterStrike16;
        }

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

            CDP.HalfLife.DeltaStructure clientDataStructure = demo.FindReadDeltaStructure("clientdata_t");
            clientDataStructure.SkipDelta(buffer);
            CDP.HalfLife.DeltaStructure weaponDataStructure = demo.FindReadDeltaStructure("weapon_data_t");

            while (buffer.ReadBoolean())
            {
                if (demo.NetworkProtocol < 47 && !IsBeta16())
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

            CDP.HalfLife.DeltaStructure clientDataStructure = demo.FindReadDeltaStructure("clientdata_t");
            Delta = clientDataStructure.CreateDelta();
            clientDataStructure.ReadDelta(buffer, Delta);
            CDP.HalfLife.DeltaStructure weaponStructure = demo.FindReadDeltaStructure("weapon_data_t");
            Weapons = new List<Weapon>();

            while (buffer.ReadBoolean())
            {
                Weapon weapon = new Weapon();

                if (demo.NetworkProtocol < 47 && !IsBeta16())
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
        }
    }
}
