using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core.Extensions;

namespace CDP.IdTech3.Commands
{
    public class SvcSnapshot : Command
    {
        public override CommandIds Id
        {
            get { return CommandIds.svc_snapshot; }
        }

        public override string Name
        {
            get { return "svc_snapshot"; }
        }

        public override bool IsSubCommand
        {
            get { return false; }
        }

        public override bool ContainsSubCommands
        {
            get { return false; }
        }

        public int ClientCommandSequence { get; set; }
        public int ServerTime { get; set; }
        public byte DeltaNum { get; set; }
        public byte SnapFlags { get; set; }
        public byte[] AreaMask { get; set; }
        public Player Player { get; set; }
        public List<Entity> Entities { get; set; }

        public SvcSnapshot()
        {
            Entities = new List<Entity>();
        }

        public override void Read(BitReader buffer)
        {
            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                ClientCommandSequence = buffer.ReadInt();
            }

            ServerTime = buffer.ReadInt();
            DeltaNum = buffer.ReadByte();
            SnapFlags = buffer.ReadByte();
            byte areaMaskLength = buffer.ReadByte();

            if (areaMaskLength > 0)
            {
                AreaMask = buffer.ReadBytes(areaMaskLength);
            }

            // Read player info.
            Player = new Player(demo.Protocol);
            byte lc;

            if (demo.Protocol >= Protocols.Protocol43 && demo.Protocol <= Protocols.Protocol48)
            {
                lc = (byte)Player.NetFields.Length;
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

                NetField field = Player.NetFields[i];

                if (field.Bits == 0)
                {
                    if (buffer.ReadBoolean())
                    {
                        Player[i] = buffer.ReadFloat();
                    }
                    else
                    {
                        Player[i] = buffer.ReadIntegralFloat();
                    }
                }
                else
                {
                    if (field.Signed)
                    {
                        Player[i] = buffer.ReadBits(field.Bits);
                    }
                    else
                    {
                        Player[i] = buffer.ReadUBits(field.Bits);
                    }
                }
            }

            Action<int, Action<int>> readArray = (size, callback) =>
            {
                if (buffer.ReadBoolean())
                {
                    short bits = buffer.ReadShort();

                    for (int i = 0; i < size; i++)
                    {
                        if ((bits & (1 << i)) != 0)
                        {
                            callback(i);
                        }
                    }
                }
            };

            if ((demo.Protocol >= Protocols.Protocol43 && demo.Protocol <= Protocols.Protocol48) || buffer.ReadBoolean())
            {
                readArray(Player.MAX_STATS, i => Player.Stats[i] = buffer.ReadShort());
                readArray(Player.MAX_PERSISTANT, i => Player.Persistant[i] = buffer.ReadShort());
                readArray(Player.MAX_WEAPONS, i => Player.Ammo[i] = buffer.ReadShort());
                readArray(Player.MAX_POWERUPS, i => Player.Powerups[i] = buffer.ReadInt());
            }

            // Read packet entities.
            while (true)
            {
                uint entityNumber = buffer.ReadUBits(Entity.GENTITYNUM_BITS);

                if (entityNumber == Entity.GENTITYSENTINEL)
                {
                    break;
                }

                Entity entity = new Entity(demo.Protocol);
                entity.Number = entityNumber;
                Entities.Add(entity);
                entity.Read(buffer);
            }
        }

        public override void Write(Core.BitWriter buffer)
        {
            throw new NotImplementedException();
        }

        public override void Log(StreamWriter log)
        {
            if (demo.Protocol == Protocols.Protocol43 || demo.Protocol == Protocols.Protocol45)
            {
                log.WriteLine("Client command sequence: {0}", ClientCommandSequence);
            }

            log.WriteLine("Server time: {0}", ServerTime);
            log.WriteLine("Delta num: {0}", DeltaNum);
            log.WriteLine("Snap flags: {0}", SnapFlags);

            if (AreaMask != null)
            {
                log.Write("Area mask: ");
                log.WriteBytes(AreaMask);
            }

            if (Player != null)
            {
                for (int i = 0; i < Player.NetFields.Length; i++)
                {
                    if (Player[i] != null)
                    {
                        log.WriteLine("Field: {0}, Value: {1}", Player.NetFields[i].Name, Player[i]);
                    }
                }

                log.Write("Stats: ");
                
                for (int i = 0; i < Player.MAX_STATS; i++)
                {
                    log.Write("{0} ", Player.Stats[i]);
                }

                log.Write("\nPersistant: ");

                for (int i = 0; i < Player.MAX_PERSISTANT; i++)
                {
                    log.Write("{0} ", Player.Persistant[i]);
                }

                log.Write("\nAmmo: ");

                for (int i = 0; i < Player.MAX_WEAPONS; i++)
                {
                    log.Write("{0} ", Player.Ammo[i]);
                }

                log.Write("\nPowerups: ");

                for (int i = 0; i < Player.MAX_POWERUPS; i++)
                {
                    log.Write("{0} ", Player.Powerups[i]);
                }

                log.WriteLine();
            }

            foreach (Entity entity in Entities)
            {
                entity.Log(log);
            }
        }
    }
}
