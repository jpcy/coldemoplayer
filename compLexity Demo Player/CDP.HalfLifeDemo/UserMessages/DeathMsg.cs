using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.UserMessages
{
    public class DeathMsg : UserMessage
    {
        public override string Name
        {
            get { return "DeathMsg"; }
        }

        public byte KillerSlot { get; set; }
        public byte VictimSlot { get; set; }
        public bool Headshot { get; set; }
        public string WeaponName { get; set; }

        public override void Read(BitReader buffer)
        {
            KillerSlot = buffer.ReadByte();
            VictimSlot = buffer.ReadByte();
            Headshot = buffer.ReadByte() == 1;
            WeaponName = buffer.ReadString();
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
