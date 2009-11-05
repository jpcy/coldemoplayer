using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3
{
    public class Entity
    {
        public const int GENTITYNUM_BITS = 10;
        public const int MAX_GENTITIES = (1 << GENTITYNUM_BITS);
        public const int GENTITYSENTINEL = MAX_GENTITIES - 1;

        public const int BITMASK_BITS = 5;
        public const int MAX_BITMASK = (1 << BITMASK_BITS);
        public const int BITMASKSENTINEL = MAX_BITMASK - 1;

        public NetField[] NetFields
        {
            get { return netFields; }
        }

        public uint Number { get; set; } // Not read internally because svc_snapshot needs to check for a sentinel value.
        public bool Remove { get; set; }
        public bool Delta { get; set; }

        private class FieldState
        {
            public NetField NetField { get; set; }
            public Object Value { get; set; }
        }

        protected Protocols protocol;
        private FieldState[] state;
        private NetField[] netFields;

        public object this[string field]
        {
            get
            {
                FieldState fieldState = state.FirstOrDefault(s => s.NetField.Name == field);
                return fieldState.Value;
            }

            set
            {
                state.First(s => s.NetField.Name == field).Value = value;
            }
        }

        public object this[int index]
        {
            get { return state[index].Value; }
            set { state[index].Value = value; }
        }

        public Entity(Protocols protocol)
        {
            this.protocol = protocol;
            Initialise();
        }

        // This should be overridden in derived classes so they can provide their own netfields.
        protected virtual void Initialise()
        {
            if (protocol == Protocols.Protocol43 || protocol == Protocols.Protocol45)
            {
                InitialiseState(Protocol43NetFields);
            }
            else if (protocol == Protocols.Protocol48)
            {
                InitialiseState(Protocol48NetFields);
            }
            else if (protocol >= Protocols.Protocol66 && protocol <= Protocols.Protocol68)
            {
                InitialiseState(Protocol66NetFields);
            }
            else
            {
                InitialiseState(Protocol73NetFields);
            }
        }

        protected void InitialiseState(NetField[] netFields)
        {
            this.netFields = netFields;
            state = new FieldState[netFields.Length];

            for (int i = 0; i < state.Length; i++)
            {
                state[i] = new FieldState();
                state[i].NetField = netFields[i];
            }
        }

        public void Read(BitReader buffer)
        {
            Remove = buffer.ReadBoolean();

            if (Remove)
            {
                return;
            }

            Delta = buffer.ReadBoolean();

            if (!Delta)
            {
                return;
            }

            if (protocol >= Protocols.Protocol43 && protocol <= Protocols.Protocol48)
            {
                byte[] bitmask = null;
                uint bitmaskIndex = buffer.ReadUBits(5);

                if (bitmaskIndex == BITMASKSENTINEL)
                {
                    bitmask = new byte[7];

                    // 50 bits for protocols 43 and 45, 51 bits for protocol 48.
                    for (int i = 0; i < 6; i++)
                    {
                        bitmask[i] = buffer.ReadByte();
                    }

                    if (protocol == Protocols.Protocol48)
                    {
                        bitmask[6] = (byte)buffer.ReadUBits(3);
                    }
                    else
                    {
                        bitmask[6] = (byte)buffer.ReadUBits(2);
                    }
                }
                else
                {
                    bitmask = KnownBitmasks[bitmaskIndex];
                }

                for (int i = 0; i < netFields.Length; i++)
                {
                    if ((bitmask[i / 8] & (1<<(i % 8))) == 0)
                    {
                        continue;
                    }

                    if (netFields[i].Bits == 0)
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
                        this[i] = buffer.ReadUBits(netFields[i].Bits);
                    }
                }
            }
            else
            {
                byte lc = buffer.ReadByte();

                for (int i = 0; i < lc; i++)
                {
                    if (!buffer.ReadBoolean()) // No change.
                    {
                        continue;
                    }

                    if (netFields[i].Bits == 0)
                    {
                        // float
                        if (buffer.ReadBoolean())
                        {
                            if (buffer.ReadBoolean())
                            {
                                // full floating point value
                                this[i] = buffer.ReadFloat();
                            }
                            else
                            {
                                // integral float
                                this[i] = buffer.ReadIntegralFloat();
                            }
                        }
                        else
                        {
                            this[i] = 0.0f;
                        }
                    }
                    else
                    {
                        // int
                        if (buffer.ReadBoolean())
                        {
                            if (netFields[i].Signed)
                            {
                                this[i] = buffer.ReadBits(netFields[i].Bits);
                            }
                            else
                            {
                                this[i] = buffer.ReadUBits(netFields[i].Bits);
                            }
                        }
                        else
                        {
                            if (netFields[i].Signed)
                            {
                                this[i] = 0;
                            }
                            else
                            {
                                this[i] = 0u;
                            }
                        }
                    }
                }
            }
        }

        public void Log(StreamWriter log)
        {
            log.WriteLine("Entity number: {0}", Number);

            if (Remove)
            {
                log.WriteLine("Remove");
                return;
            }

            if (!Delta)
            {
                log.WriteLine("No delta");
            }

            for (int i = 0; i < netFields.Length; i++)
            {
                if (this[i] != null)
                {
                    log.WriteLine("Field: {0}, Value: {1}", netFields[i].Name, this[i]);
                }
            }
        }

        private static readonly byte[][] KnownBitmasks =
        {
		    new byte[] {0x60,0x80,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x60,0x00,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x60,0xC0,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0xE1,0x00,0x00,0x00,0x00,0x20,0x00},
		    new byte[] {0x60,0x80,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0xE0,0x80,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0xE0,0xC0,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x00,0x00,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0x40,0x80,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x20,0x80,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x60,0x80,0x00,0x00,0x01,0x00,0x00},
		    new byte[] {0xED,0x07,0x00,0x00,0x00,0x80,0x00},
		    new byte[] {0xE0,0x00,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0xED,0x07,0x00,0x00,0x00,0x30,0x00},
		    new byte[] {0x80,0x00,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x40,0x00,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0xE0,0xC0,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0x60,0x00,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0x20,0x00,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0xE1,0x00,0x00,0x00,0x04,0x20,0x00},
		    new byte[] {0xE1,0x00,0xC0,0x01,0x20,0x20,0x00},
		    new byte[] {0xE0,0xC0,0x00,0x00,0x01,0x00,0x00},
		    new byte[] {0x60,0x40,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x40,0xC0,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x60,0xC0,0x00,0x00,0x01,0x00,0x00},
		    new byte[] {0x60,0xC0,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0x60,0x80,0x00,0x00,0x01,0x00,0x01},
		    new byte[] {0x60,0x80,0x00,0x00,0x00,0x30,0x00},
		    new byte[] {0xE0,0x80,0x00,0x00,0x00,0x10,0x00},
		    new byte[] {0x20,0xC0,0x00,0x00,0x00,0x00,0x00},
		    new byte[] {0x60,0x80,0x00,0x00,0x00,0x00,0x02},
		    new byte[] {0xE0,0x40,0x00,0x00,0x00,0x00,0x00}
	    };

        /// <summary>
        /// Protocol 43 and 45 net fields.
        /// </summary>
        private static readonly NetField[] Protocol43NetFields =
        {
            new NetField("eType", 8),
            new NetField("eFlags", 16),
            new NetField("pos.trType", 8),
            new NetField("pos.trTime", 32),
            new NetField("pos.trDuration", 32),
            new NetField("pos.trBase[0]", 0),
            new NetField("pos.trBase[1]", 0),
            new NetField("pos.trBase[2]", 0),
            new NetField("pos.trDelta[0]", 0),
            new NetField("pos.trDelta[1]", 0),
            new NetField("pos.trDelta[2]", 0),
            new NetField("apos.trType", 8),
            new NetField("apos.trTime", 32),
            new NetField("apos.trDuration", 32),
            new NetField("apos.trBase[0]", 0),
            new NetField("apos.trBase[1]", 0),
            new NetField("apos.trBase[2]", 0),
            new NetField("apos.trDelta[0]", 0),
            new NetField("apos.trDelta[1]", 0),
            new NetField("apos.trDelta[2]", 0),
            new NetField("time", 32),
            new NetField("time2", 32),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("origin2[0]", 0),
            new NetField("origin2[1]", 0),
            new NetField("origin2[2]", 0),
            new NetField("angles[0]", 0),
            new NetField("angles[1]", 0),
            new NetField("angles[2]", 0),
            new NetField("angles2[0]", 0),
            new NetField("angles2[1]", 0),
            new NetField("angles2[2]", 0),
            new NetField("otherEntityNum", GENTITYNUM_BITS),
            new NetField("otherEntityNum2", GENTITYNUM_BITS),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("loopSound", 8),
            new NetField("constantLight", 32),
            new NetField("modelindex", 8),
            new NetField("modelindex2", 8),
            new NetField("frame", 16),
            new NetField("clientNum", 8),
            new NetField("solid", 24),
            new NetField("event", 10),
            new NetField("eventParm", 8),
            new NetField("powerups", 16),
            new NetField("weapon", 8),
            new NetField("legsAnim", 8),
            new NetField("torsoAnim", 8)
        };

        private static readonly NetField[] Protocol48NetFields =
        {
            new NetField("eType", 8),
            new NetField("eFlags", 19), // Changed from 16 bits in 45.
            new NetField("pos.trType", 8),
            new NetField("pos.trTime", 32),
            new NetField("pos.trDuration", 32),
            new NetField("pos.trBase[0]", 0),
            new NetField("pos.trBase[1]", 0),
            new NetField("pos.trBase[2]", 0),
            new NetField("pos.trDelta[0]", 0),
            new NetField("pos.trDelta[1]", 0),
            new NetField("pos.trDelta[2]", 0),
            new NetField("apos.trType", 8),
            new NetField("apos.trTime", 32),
            new NetField("apos.trDuration", 32),
            new NetField("apos.trBase[0]", 0),
            new NetField("apos.trBase[1]", 0),
            new NetField("apos.trBase[2]", 0),
            new NetField("apos.trDelta[0]", 0),
            new NetField("apos.trDelta[1]", 0),
            new NetField("apos.trDelta[2]", 0),
            new NetField("time", 32),
            new NetField("time2", 32),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("origin2[0]", 0),
            new NetField("origin2[1]", 0),
            new NetField("origin2[2]", 0),
            new NetField("angles[0]", 0),
            new NetField("angles[1]", 0),
            new NetField("angles[2]", 0),
            new NetField("angles2[0]", 0),
            new NetField("angles2[1]", 0),
            new NetField("angles2[2]", 0),
            new NetField("otherEntityNum", GENTITYNUM_BITS),
            new NetField("otherEntityNum2", GENTITYNUM_BITS),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("loopSound", 8),
            new NetField("constantLight", 32),
            new NetField("modelindex", 8),
            new NetField("modelindex2", 8),
            new NetField("frame", 16),
            new NetField("clientNum", 8),
            new NetField("solid", 24),
            new NetField("event", 10),
            new NetField("eventParm", 8),
            new NetField("powerups", 16),
            new NetField("weapon", 8),
            new NetField("legsAnim", 8),
            new NetField("torsoAnim", 8),
            new NetField("generic1", 8) // New in 48.
        };

        /// <summary>
        /// Protocol 66, 67 and 68 net fields.
        /// </summary>
        private static readonly NetField[] Protocol66NetFields =
        {
            new NetField("pos.trTime", 32),
            new NetField("pos.trBase[0]", 0),
            new NetField("pos.trBase[1]", 0),
            new NetField("pos.trDelta[0]", 0),
            new NetField("pos.trDelta[1]", 0),
            new NetField("pos.trBase[2]", 0),
            new NetField("apos.trBase[1]", 0),
            new NetField("pos.trDelta[2]", 0),
            new NetField("apos.trBase[0]", 0),
            new NetField("event", 10),
            new NetField("angles2[1]", 0),
            new NetField("eType", 8),
            new NetField("torsoAnim", 8),
            new NetField("eventParm", 8),
            new NetField("legsAnim", 8),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("pos.trType", 8),
            new NetField("eFlags", 19),
            new NetField("otherEntityNum", GENTITYNUM_BITS),
            new NetField("weapon", 8),
            new NetField("clientNum", 8),
            new NetField("angles[1]", 0),
            new NetField("pos.trDuration", 32),
            new NetField("apos.trType", 8),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("solid", 24),
            new NetField("powerups", 16),
            new NetField("modelindex", 8),
            new NetField("otherEntityNum2", GENTITYNUM_BITS),
            new NetField("loopSound", 8),
            new NetField("generic1", 8),
            new NetField("origin2[2]", 0),
            new NetField("origin2[0]", 0),
            new NetField("origin2[1]", 0),
            new NetField("modelindex2", 8),
            new NetField("angles[0]", 0),
            new NetField("time", 32),
            new NetField("apos.trTime", 32),
            new NetField("apos.trDuration", 32),
            new NetField("apos.trBase[2]", 0),
            new NetField("apos.trDelta[0]", 0),
            new NetField("apos.trDelta[1]", 0),
            new NetField("apos.trDelta[2]", 0),
            new NetField("time2", 32),
            new NetField("angles[2]", 0),
            new NetField("angles2[0]", 0),
            new NetField("angles2[2]", 0),
            new NetField("constantLight", 32),
            new NetField("frame", 16)
        };

        private static readonly NetField[] Protocol73NetFields =
        {
            new NetField("pos.trTime", 32),
            new NetField("pos.trBase[0]", 0),
            new NetField("pos.trBase[1]", 0),
            new NetField("pos.trDelta[0]", 0),
            new NetField("pos.trDelta[1]", 0),
            new NetField("pos.trBase[2]", 0),
            new NetField("apos.trBase[1]", 0),
            new NetField("pos.trDelta[2]", 0),
            new NetField("apos.trBase[0]", 0),
            new NetField("pos.gravity", 32), // New in 73.
            new NetField("event", 10),
            new NetField("angles2[1]", 0),
            new NetField("eType", 8),
            new NetField("torsoAnim", 8),
            new NetField("eventParm", 8),
            new NetField("legsAnim", 8),
            new NetField("groundEntityNum", GENTITYNUM_BITS),
            new NetField("pos.trType", 8),
            new NetField("eFlags", 19),
            new NetField("otherEntityNum", GENTITYNUM_BITS),
            new NetField("weapon", 8),
            new NetField("clientNum", 8),
            new NetField("angles[1]", 0),
            new NetField("pos.trDuration", 32),
            new NetField("apos.trType", 8),
            new NetField("origin[0]", 0),
            new NetField("origin[1]", 0),
            new NetField("origin[2]", 0),
            new NetField("solid", 24),
            new NetField("powerups", 16),
            new NetField("modelindex", 8),
            new NetField("otherEntityNum2", GENTITYNUM_BITS),
            new NetField("loopSound", 8),
            new NetField("generic1", 8),
            new NetField("origin2[2]", 0),
            new NetField("origin2[0]", 0),
            new NetField("origin2[1]", 0),
            new NetField("modelindex2", 8),
            new NetField("angles[0]", 0),
            new NetField("time", 32),
            new NetField("apos.trTime", 32),
            new NetField("apos.trDuration", 32),
            new NetField("apos.trBase[2]", 0),
            new NetField("apos.trDelta[0]", 0),
            new NetField("apos.trDelta[1]", 0),
            new NetField("apos.trDelta[2]", 0),
            new NetField("apos.gravity", 32), // New in 73.
            new NetField("time2", 32),
            new NetField("angles[2]", 0),
            new NetField("angles2[0]", 0),
            new NetField("angles2[2]", 0),
            new NetField("constantLight", 32),
            new NetField("frame", 16)
        };
    }
}
