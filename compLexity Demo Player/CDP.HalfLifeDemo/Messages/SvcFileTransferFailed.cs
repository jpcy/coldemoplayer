using System;
using System.IO;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcFileTransferFailed : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_filetxferfailed; }
        }

        public override string Name
        {
            get { return "svc_filetxferfailed"; }
        }

        public string FileName { get; set; }

        public override void Read(BitReader buffer)
        {
            FileName = buffer.ReadString();
        }

        public override byte[] Write()
        {
            BitWriter buffer = new BitWriter();
            buffer.WriteString(FileName);
            return buffer.Data;
        }

#if DEBUG
        public override void Log(StreamWriter log)
        {
            log.WriteLine("FileName: {0}", FileName);
        }
#endif
    }
}
