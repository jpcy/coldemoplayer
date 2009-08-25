using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcDirector : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_director; }
        }

        public override string Name
        {
            get { return "svc_director"; }
        }

        public byte Length { get; set; }
        public byte[] Data { get; set; }

        public override void Read(BitReader buffer)
        {
            Length = buffer.ReadByte();

            if (Length > 0)
            {
                Data = buffer.ReadBytes(Length);
            }
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("Length: {0}", Length);
        }
#endif
    }
}
