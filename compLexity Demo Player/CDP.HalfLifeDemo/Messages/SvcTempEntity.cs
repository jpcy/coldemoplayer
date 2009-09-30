using System;
using BitReader = CDP.Core.BitReader;
using BitWriter = CDP.Core.BitWriter;
using System.IO;

namespace CDP.HalfLifeDemo.Messages
{
    public class SvcTempEntity : EngineMessage
    {
        public override byte Id
        {
            get { return (byte)EngineMessageIds.svc_tempentity; }
        }

        public override string Name
        {
            get { return "svc_tempentity"; }
        }

        public override bool CanSkipWhenWriting
        {
            get { return false; }
        }

        public byte Type { get; set; }
        public byte[] Data { get; set; }

        public override void Skip(BitReader buffer)
        {
            Read(buffer);
        }

        public override void Read(BitReader buffer)
        {
            Type = buffer.ReadByte();
            int startingByteIndex = buffer.CurrentByte;
            int length = 0;

            // My eyes, it burns!
            switch (Type)
            {
                // obsolete
                case 16: // TE_BEAM
                case 26: // TE_BEAMHOSE
                    break;

                // simple coord format messages
                case 2: // TE_GUNSHOT
                case 4: // TE_TAREXPLOSION 
                case 9: // TE_SPARKS
                case 10: // TE_LAVASPLASH
                case 11: // TE_TELEPORT
                    length = 6;
                    break;

                case 0: // TE_BEAMPOINTS
                    length = 24;
                    break;

                case 1: // TE_BEAMENTPOINT
                    length = 20;
                    break;

                case 3: // TE_EXPLOSION
                    length = 11;
                    break;

                case 5: // TE_SMOKE
                    length = 10;
                    break;

                case 6: // TE_TRACER
                    length = 12;
                    break;

                case 7: // TE_LIGHTNING 
                    length = 17;
                    break;

                case 8: // TE_BEAMENTS
                    length = 16;
                    break;

                case 12: // TE_EXPLOSION2
                    length = 8;
                    break;

                case 13: // TE_BSPDECAL
                    buffer.SeekBytes(8);
                    ushort entityIndex = buffer.ReadUShort();
                    length = 10;

                    if (entityIndex != 0)
                    {
                        length += 2;
                    }
                    break;

                case 14: // TE_IMPLOSION
                    length = 9;
                    break;

                case 15: // TE_SPRITETRAIL
                    length = 19;
                    break;

                case 17: // TE_SPRITE
                    length = 10;
                    break;

                case 18: // TE_BEAMSPRITE
                    length = 16;
                    break;

                case 19: // TE_BEAMTORUS
                case 20: // TE_BEAMDISK
                case 21: // TE_BEAMCYLINDER
                    length = 24;
                    break;

                case 22: // TE_BEAMFOLLOW
                    length = 10;
                    break;

                case 23: // TE_GLOWSPRITE
                    // SDK is wrong
                    /* 
                        write_coord()	 position
                        write_coord()
                        write_coord()
                        write_short()	 model index
                        write_byte()	 life in 0.1's
                        write_byte()	scale in 0.1's
                        write_byte()	brightness
                    */
                    length = 11;
                    break;

                case 24: // TE_BEAMRING
                    length = 16;
                    break;

                case 25: // TE_STREAK_SPLASH
                    length = 19;
                    break;

                case 27: // TE_DLIGHT
                    length = 13;
                    break;

                case 28: // TE_ELIGHT
                    length = 16;
                    break;

                case 29: // TE_TEXTMESSAGE
                    buffer.SeekBytes(5);
                    byte textParmsEffect = buffer.ReadByte();
                    buffer.SeekBytes(14);
                    length = 20;

                    if (textParmsEffect == 2)
                    {
                        buffer.SeekBytes(2);
                        length += 2;
                    }

                    // capped to 512 bytes (including null terminator)
                    string message = buffer.ReadString();
                    length += message.Length + 1;
                    break;

                case 30: // TE_LINE
                case 31: // TE_BOX
                    length = 17;
                    break;

                case 99: // TE_KILLBEAM
                    length = 2;
                    break;

                case 100: // TE_LARGEFUNNEL
                    length = 10;
                    break;

                case 101: // TE_BLOODSTREAM
                    length = 14;
                    break;

                case 102: // TE_SHOWLINE
                    length = 12;
                    break;

                case 103: // TE_BLOOD
                    length = 14;
                    break;

                case 104: // TE_DECAL
                    length = 9;
                    break;

                case 105: // TE_FIZZ
                    length = 5;
                    break;

                case 106: // TE_MODEL
                    // WRITE_ANGLE could be a short..
                    length = 17;
                    break;

                case 107: // TE_EXPLODEMODEL
                    length = 13;
                    break;

                case 108: // TE_BREAKMODEL
                    length = 24;
                    break;

                case 109: // TE_GUNSHOTDECAL
                    length = 9;
                    break;

                case 110: // TE_SPRITE_SPRAY
                    length = 17;
                    break;

                case 111: // TE_ARMOR_RICOCHET
                    length = 7;
                    break;

                case 112: // TE_PLAYERDECAL (could be a trailing short after this, apparently...)
                    length = 10;
                    break;

                case 113: // TE_BUBBLES
                case 114: // TE_BUBBLETRAIL
                    length = 19;
                    break;

                case 115: // TE_BLOODSPRITE
                    length = 12;
                    break;

                case 116: // TE_WORLDDECAL
                case 117: // TE_WORLDDECALHIGH
                    length = 7;
                    break;

                case 118: // TE_DECALHIGH
                    length = 9;
                    break;

                case 119: // TE_PROJECTILE
                    length = 16;
                    break;

                case 120: // TE_SPRAY
                    length = 18;
                    break;

                case 121: // TE_PLAYERSPRITES
                    length = 5;
                    break;

                case 122: // TE_PARTICLEBURST
                    length = 6;
                    break;

                case 123: // TE_FIREFIELD
                    length = 9;
                    break;

                case 124: // TE_PLAYERATTACHMENT
                    length = 7;
                    break;

                case 125: // TE_KILLPLAYERATTACHMENTS
                    length = 1;
                    break;

                case 126: // TE_MULTIGUNSHOT
                    length = 18;
                    break;

                case 127: // TE_USERTRACER
                    length = 15;
                    break;

                default:
                    throw new ApplicationException(String.Format("Unknown temp entity type \"{0}\".", Type));
            }

            if (length > 0)
            {
                buffer.SeekBytes(startingByteIndex, SeekOrigin.Begin);
                Data = buffer.ReadBytes(length);
            }
        }

        public override void Write(BitWriter buffer)
        {
            buffer.WriteByte(Type);

            if (Data != null)
            {
                buffer.WriteBytes(Data);
            }
        }

        public override void Log(StreamWriter log)
        {
            log.WriteLine("Type: {0}", Type);
            log.WriteLine("Length: {0}", Data == null ? 0 : Data.Length);
        }
    }
}
