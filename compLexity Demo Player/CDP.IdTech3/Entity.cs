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

        public NetField[] NetFields
        {
            get { return netFields; }
        }

        public uint Number { get; set; } // not read internally because svc_snapshot needs to check for a sentinel value.
        public bool Remove { get; set; }
        public bool Delta { get; set; }
        public byte Lc { get; set; }

        private class FieldState
        {
            public NetField NetField { get; set; }
            public Object Value { get; set; }
        }

        protected int protocol;
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

        protected Entity()
        {
            Initialise();
        }

        public Entity(int protocol)
        {
            this.protocol = protocol;
            Initialise();
        }

        // This should be overridden in derived classes so they can provide their own netfields.
        protected virtual void Initialise()
        {
            if (protocol <= 68)
            {
                InitialiseState(Protocol68NetFields);
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

            Lc = buffer.ReadByte();

            for (int i = 0; i < Lc; i++)
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

            log.WriteLine("Lc: {0}", Lc);

            for (int i = 0; i < netFields.Length; i++)
            {
                if (this[i] != null)
                {
                    log.WriteLine("Field: {0}, Value: {1}", netFields[i].Name, this[i]);
                }
            }
        }

        private static readonly NetField[] Protocol68NetFields =
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
