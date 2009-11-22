using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3
{
    public class Player : Entity
    {
        public const int MAX_STATS = 16;
        public const int MAX_PERSISTANT = 16;
        public const int MAX_POWERUPS = 16;
        public const int MAX_WEAPONS = 16;

        public short[] Stats { get; set; }
        public short[] Persistant { get; set; }
        public int[] Powerups { get; set; }
        public short[] Ammo { get; set; }

        public Player(Protocols protocol)
            : base(protocol)
        {
            InitialiseArrays();
        }

        public Player(Protocols protocol, Player player)
            : base(protocol, player)
        {
            InitialiseArrays();
            Array.Copy(player.Stats, Stats, MAX_STATS);
            Array.Copy(player.Persistant, Persistant, MAX_PERSISTANT);
            Array.Copy(player.Powerups, Powerups, MAX_POWERUPS);
            Array.Copy(player.Ammo, Ammo, MAX_WEAPONS);
        }

        private void InitialiseArrays()
        {
            Stats = new short[MAX_STATS];
            Persistant = new short[MAX_PERSISTANT];
            Powerups = new int[MAX_POWERUPS];
            Ammo = new short[MAX_WEAPONS];
        }

        protected override void Initialise()
        {
            if (protocol == Protocols.Protocol43 || protocol == Protocols.Protocol45)
            {
                InitialiseState(Protocol43PlayerNetFields);
            }
            else if (protocol == Protocols.Protocol48)
            {
                InitialiseState(Protocol48PlayerNetFields);
            }
            else
            {
                InitialiseState(Protocol66PlayerNetFields);
            }
        }

        public override void Read(BitReader buffer)
        {
            byte lc;

            if (protocol >= Protocols.Protocol43 && protocol <= Protocols.Protocol48)
            {
                lc = (byte)fields.Length;
            }
            else
            {
                lc = buffer.ReadByte();
            }

            for (int i = 0; i < lc; i++)
            {
                if (!buffer.ReadBoolean())
                {
                    continue;
                }

                if (fields[i].Bits == 0)
                {
                    if (buffer.ReadBoolean())
                    {
                        this[i] = buffer.ReadFloat();
                    }
                    else
                    {
                        this[i] = buffer.ReadIntegralFloat();
                    }
                }
                else
                {
                    if (fields[i].Signed)
                    {
                        this[i] = buffer.ReadBits(fields[i].Bits);
                    }
                    else
                    {
                        this[i] = buffer.ReadUBits(fields[i].Bits);
                    }
                }
            }

            Action<int, Action<int>> readArray = (size, readElement) =>
            {
                if (buffer.ReadBoolean())
                {
                    short bits = buffer.ReadShort();

                    for (int i = 0; i < size; i++)
                    {
                        if ((bits & (1 << i)) != 0)
                        {
                            readElement(i);
                        }
                    }
                }
            };

            if ((protocol >= Protocols.Protocol43 && protocol <= Protocols.Protocol48) || buffer.ReadBoolean())
            {
                readArray(MAX_STATS, i => Stats[i] = buffer.ReadShort());
                readArray(MAX_PERSISTANT, i => Persistant[i] = buffer.ReadShort());
                readArray(MAX_WEAPONS, i => Ammo[i] = buffer.ReadShort());
                readArray(MAX_POWERUPS, i => Powerups[i] = buffer.ReadInt());
            }
        }

        public override void Write(BitWriter buffer)
        {
            int lc = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                if (this[i] != null)
                {
                    lc = i + 1;
                }
            }

            buffer.WriteByte((byte)lc);

            for (int i = 0; i < lc; i++)
            {
                if (this[i] == null)
                {
                    buffer.WriteBoolean(false);
                    continue;
                }

                buffer.WriteBoolean(true);

                if (fields[i].Bits == 0)
                {
                    buffer.WriteIntegralFloatMaybe((float)this[i]);
                }
                else
                {
                    if (fields[i].Signed)
                    {
                        buffer.WriteBits((int)this[i], fields[i].Bits);
                    }
                    else
                    {
                        buffer.WriteUBits((uint)this[i], fields[i].Bits);
                    }
                }
            }

            if (Stats.All(i => i == 0) && Persistant.All(i => i == 0) && Ammo.All(i => i == 0) && Powerups.All(i => i == 0))
            {
                buffer.WriteBoolean(false);
            }
            else
            {
                buffer.WriteBoolean(true);
                WriteArray(buffer, Stats, i => i == 0, i => buffer.WriteShort(i));
                WriteArray(buffer, Persistant, i => i == 0, i => buffer.WriteShort(i));
                WriteArray(buffer, Ammo, i => i == 0, i => buffer.WriteShort(i));
                WriteArray(buffer, Powerups, i => i == 0, i => buffer.WriteInt(i));
            }
        }

        private void WriteArray<T>(BitWriter buffer, T[] array, Func<T, bool> elementIsZero, Action<T> writeElement)
        {
            int bitMask = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (!elementIsZero(array[i]))
                {
                    bitMask |= 1 << i;
                }
            }

            if (bitMask == 0)
            {
                buffer.WriteBoolean(false);
                return;
            }

            buffer.WriteBoolean(true);
            buffer.WriteShort((short)bitMask);

            for (int i = 0; i < array.Length; i++)
            {
                if ((bitMask & 1 << i) != 0)
                {
                    writeElement(array[i]);
                }
            }
        }

        public override void Log(StreamWriter log)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (this[i] != null)
                {
                    log.WriteLine("Field: {0}, Value: {1}", fields[i].Name, this[i]);
                }
            }

            log.Write("Stats: ");

            for (int i = 0; i < MAX_STATS; i++)
            {
                log.Write("{0} ", Stats[i]);
            }

            log.Write("\nPersistant: ");

            for (int i = 0; i < MAX_PERSISTANT; i++)
            {
                log.Write("{0} ", Persistant[i]);
            }

            log.Write("\nAmmo: ");

            for (int i = 0; i < MAX_WEAPONS; i++)
            {
                log.Write("{0} ", Ammo[i]);
            }

            log.Write("\nPowerups: ");

            for (int i = 0; i < MAX_POWERUPS; i++)
            {
                log.Write("{0} ", Powerups[i]);
            }

            log.WriteLine();
        }

        /// <summary>
        /// Protocol 43 and 45 net fields.
        /// </summary>
        private static readonly NetField[] Protocol43PlayerNetFields = 
        {
            new NetField("commandTime", 32),
            new NetField("pm_type", 8),
            new NetField("bobCycle", 8),
            new NetField("pm_flags", 16),
            new NetField("pm_time", 16),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("velocity[0]", 0),
            new NetField("velocity[1]", 0),
            new NetField("velocity[2]", 0),
            new NetField("weaponTime", 16),
            new NetField("gravity", 16),
            new NetField("speed", 16),
            new NetField("delta_angles[0]", 16),
            new NetField("delta_angles[1]", 16),
            new NetField("delta_angles[2]", 16),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("legsTimer", 8),
            new NetField("torsoTimer", 12),
            new NetField("legsAnim", 8),
            new NetField("torsoAnim", 8),
            new NetField("movementDir", 4),
            new NetField("eFlags", 16),
            new NetField("eventSequence", 16),
            new NetField("events[0]", 8),
            new NetField("events[1]", 8),
            new NetField("eventParms[0]", 8),
            new NetField("eventParms[1]", 8),
            new NetField("externalEvent", 8),
            new NetField("externalEventParm", 8),
            new NetField("clientNum", 8),
            new NetField("weapon", 5),
            new NetField("weaponstate", 4),
            new NetField("viewangles[0]", 0),
            new NetField("viewangles[1]", 0),
            new NetField("viewangles[2]", 0),
            new NetField("viewheight", 8),
            new NetField("damageEvent", 8),
            new NetField("damageYaw", 8),
            new NetField("damagePitch", 8),
            new NetField("damageCount", 8),
            new NetField("grapplePoint[0]", 0),
            new NetField("grapplePoint[1]", 0),
            new NetField("grapplePoint[2]", 0),
        };

        private static readonly NetField[] Protocol48PlayerNetFields = 
        {
            new NetField("commandTime", 32),
            new NetField("pm_type", 8),
            new NetField("bobCycle", 8),
            new NetField("pm_flags", 16),
            new NetField("pm_time", 16),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("velocity[0]", 0),
            new NetField("velocity[1]", 0),
            new NetField("velocity[2]", 0),
            new NetField("weaponTime", 16),
            new NetField("gravity", 16),
            new NetField("speed", 16),
            new NetField("delta_angles[0]", 16),
            new NetField("delta_angles[1]", 16),
            new NetField("delta_angles[2]", 16),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("legsTimer", 8),
            new NetField("torsoTimer", 12),
            new NetField("legsAnim", 8),
            new NetField("torsoAnim", 8),
            new NetField("movementDir", 4),
            new NetField("eFlags", 16),
            new NetField("eventSequence", 16),
            new NetField("events[0]", 8),
            new NetField("events[1]", 8),
            new NetField("eventParms[0]", 8),
            new NetField("eventParms[1]", 8),
            new NetField("externalEvent", 10), // Changed from 8 bits in protocol 45.
            new NetField("externalEventParm", 8),
            new NetField("clientNum", 8),
            new NetField("weapon", 5),
            new NetField("weaponstate", 4),
            new NetField("viewangles[0]", 0),
            new NetField("viewangles[1]", 0),
            new NetField("viewangles[2]", 0),
            new NetField("viewheight", 8),
            new NetField("damageEvent", 8),
            new NetField("damageYaw", 8),
            new NetField("damagePitch", 8),
            new NetField("damageCount", 8),
            new NetField("grapplePoint[0]", 0),
            new NetField("grapplePoint[1]", 0),
            new NetField("grapplePoint[2]", 0),
            new NetField("jumppad_ent", GENTITYNUM_BITS), // New in 48.
            new NetField("loopSound", 16), // New in 48.
            new NetField("generic1", 8) // New in 48.
        };

        /// <summary>
        /// Protocol 66, 67, 68 and 73 net fields.
        /// </summary>
        private static readonly NetField[] Protocol66PlayerNetFields = 
        {
            new NetField("commandTime", 32),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("bobCycle", 8),
            new NetField("velocity[0]", 0),
            new NetField("velocity[1]", 0),
            new NetField("viewangles[1]", 0),
            new NetField("viewangles[0]", 0),
            new NetField("weaponTime", 16, true),
            new NetField("origin[2]", 0),
            new NetField("velocity[2]", 0),
            new NetField("legsTimer", 8),
            new NetField("pm_time", 16, true),
            new NetField("eventSequence", 16),
            new NetField("torsoAnim", 8),
            new NetField("movementDir", 4),
            new NetField("events[0]", 8),
            new NetField("legsAnim", 8),
            new NetField("events[1]", 8),
            new NetField("pm_flags", 16),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("weaponstate", 4),
            new NetField("eFlags", 16),
            new NetField("externalEvent", 10),
            new NetField("gravity", 16),
            new NetField("speed", 16),
            new NetField("delta_angles[1]", 16),
            new NetField("externalEventParm", 8),
            new NetField("viewheight", 8, true),
            new NetField("damageEvent", 8),
            new NetField("damageYaw", 8),
            new NetField("damagePitch", 8),
            new NetField("damageCount", 8),
            new NetField("generic1", 8),
            new NetField("pm_type", 8),
            new NetField("delta_angles[0]", 16),
            new NetField("delta_angles[2]", 16),
            new NetField("torsoTimer", 12),
            new NetField("eventParms[0]", 8),
            new NetField("eventParms[1]", 8),
            new NetField("clientNum", 8),
            new NetField("weapon", 5),
            new NetField("viewangles[2]", 0),
            new NetField("grapplePoint[0]", 0),
            new NetField("grapplePoint[1]", 0),
            new NetField("grapplePoint[2]", 0),
            new NetField("jumppad_ent", 10),
            new NetField("loopSound", 16)
        };
    }
}
