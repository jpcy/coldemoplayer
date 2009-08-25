using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcUpdateUserInfo : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_updateuserinfo; }
        }

        public override string Name
        {
            get { return "svc_updateuserinfo"; }
        }

        const int checksumLength = 16;

        public byte Slot { get; set; }
        public int EntityId { get; set; }
        public string Info { get; set; }
        public byte[] Checksum { get; set; }

        public override void Read(BitReader buffer)
        {
            Slot = buffer.ReadByte();
            EntityId = buffer.ReadInt();
            Info = buffer.ReadString();
            
            if (demo.NetworkProtocol > 43)
            {
                Checksum = buffer.ReadBytes(checksumLength);
            }
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteByte(Slot);
            buffer.WriteInt(EntityId);
            buffer.WriteString(Info);

            if (Checksum == null)
            {
                buffer.WriteBytes(new byte[checksumLength]);
            }
            else
            {
                buffer.WriteBytes(Checksum);
            }

            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Slot: {0}", Slot);
            log.WriteLine("Entity ID: {0}", EntityId);
            log.WriteLine("Info: {0}", Info);
        }
#endif
    }
}
