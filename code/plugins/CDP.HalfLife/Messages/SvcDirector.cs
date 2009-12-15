using System;
using System.IO;

namespace CDP.HalfLife.Messages
{
    public class SvcDirector : EngineMessage
    {
        // See HL SDK, common/hltv.h.
        public enum Types : byte
        {
            DRC_CMD_NONE = 0,	// NULL director command
            DRC_CMD_START = 1,	// start director mode
            DRC_CMD_EVENT = 2,	// informs about director command
            DRC_CMD_MODE = 3,	// switches camera modes
            DRC_CMD_CAMERA = 4,	// sets camera registers
            DRC_CMD_TIMESCALE = 5,	// sets time scale
            DRC_CMD_MESSAGE = 6,	// send HUD centerprint
            DRC_CMD_SOUND = 7,	// plays a particular sound
            DRC_CMD_STATUS = 8,	// status info about broadcast
            DRC_CMD_BANNER = 9,	// banner file name for HLTV gui
            DRC_CMD_FADE = 10,	// send screen fade command
            DRC_CMD_SHAKE = 11,	// send screen shake command
            DRC_CMD_STUFFTEXT = 12	// like the normal svc_stufftext but as director command
        }

        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_director; }
        }

        public override string Name
        {
            get { return "svc_director"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return true; }
        }

        public const byte DRC_CMD_START = 1;
        public const byte DRC_CMD_MODE = 3;
        public const byte OBS_IN_EYE = 4;

        public Types Type { get; set; }
        public byte[] Data { get; set; }

        public override void Skip(BitReader buffer)
        {
            byte length = buffer.ReadByte();
            buffer.SeekBytes(length);
        }

        public override void Read(BitReader buffer)
        {
            byte length = buffer.ReadByte();

            if (length > 0)
            {
                Type = (Types)buffer.ReadByte();
                length--;

                if (length > 0)
                {
                    Data = buffer.ReadBytes(length);
                }
            }
        }

        public override void Write(BitWriter buffer)
        {
            if (Data == null)
            {
                buffer.WriteByte(0);
            }
            else
            {
                buffer.WriteByte((byte)Data.Length);
                buffer.WriteBytes(Data);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Type: {0}", Type);

            if (Data != null)
            {
                log.WriteLine("Length: {0}", Data.Length);
            }
        }
    }
}
