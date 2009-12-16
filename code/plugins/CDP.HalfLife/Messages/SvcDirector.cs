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

        // Spectator Movement modes
        // See HL SDK: pm_shared.h
        public enum ObserverModes : byte
        {
            OBS_NONE,
            OBS_CHASE_LOCKED,
            OBS_CHASE_FREE,
            OBS_ROAMING,		
            OBS_IN_EYE,
            OBS_MAP_FREE,
            OBS_MAP_CHASE
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

        public Types? Type { get; set; }
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
            byte length = 0;

            if (Type != null)
            {
                length++;
            }

            if (Data != null)
            {
                if (Data.Length > byte.MaxValue)
                {
                    throw new InvalidOperationException("Data length must fit in a byte.");
                }

                length += (byte)Data.Length;
            }

            buffer.WriteByte(length);

            if (Type != null)
            {
                buffer.WriteByte((byte)Type.Value);
            }

            if (Data != null)
            {
                buffer.WriteBytes(Data);
            }
        }

        public override void Log(StreamWriter log)
        {
            if (Type != null)
            {
                log.WriteLine("Type: {0}", Type);
            }

            if (Data != null)
            {
                log.WriteLine("Length: {0}", Data.Length);
            }
        }
    }
}
