using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Player(Protocols protocol) : base(protocol)
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
