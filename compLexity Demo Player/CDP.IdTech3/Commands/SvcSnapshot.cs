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

            Player = new Player(demo.Protocol);
            Player.Read(buffer);

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

        public override void Write(BitWriter buffer)
        {
            buffer.WriteInt(ServerTime);
            buffer.WriteByte(DeltaNum);
            buffer.WriteByte(SnapFlags);

            if (AreaMask == null || AreaMask.Length == 0)
            {
                buffer.WriteByte(0);
            }
            else
            {
                buffer.WriteByte((byte)AreaMask.Length);
                buffer.WriteBytes(AreaMask);
            }

            Player player = null;

            if (demo.Protocol == demo.ConvertTargetProtocol)
            {
                player = Player;
            }
            else
            {
                player = new Player(demo.ConvertTargetProtocol, Player);
            }

            player.Write(buffer);            

            // Write packet entities.
            foreach (Entity entity in Entities)
            {
                buffer.WriteUBits(entity.Number, Entity.GENTITYNUM_BITS);

                if (demo.Protocol == demo.ConvertTargetProtocol)
                {
                    entity.Write(buffer);
                }
                else
                {
                    Entity newEntity = new Entity(demo.ConvertTargetProtocol, entity);
                    newEntity.Write(buffer);
                }
            }

            buffer.WriteUBits(Entity.GENTITYSENTINEL, Entity.GENTITYNUM_BITS);
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
                Player.Log(log);
            }

            foreach (Entity entity in Entities)
            {
                entity.Log(log);
            }
        }
    }
}
