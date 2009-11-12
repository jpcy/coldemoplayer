using System;

namespace CDP.Quake3Arena
{
    public enum EntityTypes_Protocol43and45 : uint
    {
        ET_GENERAL,
        ET_PLAYER,
        ET_ITEM,
        ET_MISSILE,
        ET_MOVER,
        ET_BEAM,
        ET_PORTAL,
        ET_SPEAKER,
        ET_PUSH_TRIGGER,
        ET_TELEPORT_TRIGGER,
        ET_INVISIBLE,
        ET_GRAPPLE,
        ET_EVENTS
    }

    public enum EntityTypes : uint
    {
        ET_GENERAL,
        ET_PLAYER,
        ET_ITEM,
        ET_MISSILE,
        ET_MOVER,
        ET_BEAM,
        ET_PORTAL,
        ET_SPEAKER,
        ET_PUSH_TRIGGER,
        ET_TELEPORT_TRIGGER,
        ET_INVISIBLE,
        ET_GRAPPLE,
        ET_TEAM, // New in protocol 48.
        ET_EVENTS
    }

    public enum EntityEvents_Protocol43and45 : uint
    {
	    EV_NONE,
	    EV_FOOTSTEP,
	    EV_FOOTSTEP_METAL,
	    EV_FOOTSPLASH,
	    EV_FOOTWADE,
	    EV_SWIM,
	    EV_STEP_4,
	    EV_STEP_8,
	    EV_STEP_12,
	    EV_STEP_16,
	    EV_FALL_SHORT,
	    EV_FALL_MEDIUM,
	    EV_FALL_FAR,
	    EV_JUMP_PAD,
	    EV_JUMP,
	    EV_WATER_TOUCH,
	    EV_WATER_LEAVE,
	    EV_WATER_UNDER,
	    EV_WATER_CLEAR,
	    EV_ITEM_PICKUP,
	    EV_GLOBAL_ITEM_PICKUP,
	    EV_NOAMMO,
	    EV_CHANGE_WEAPON,
	    EV_FIRE_WEAPON,
	    EV_USE_ITEM0,
	    EV_USE_ITEM1,
	    EV_USE_ITEM2,
	    EV_USE_ITEM3,
	    EV_USE_ITEM4,
	    EV_USE_ITEM5,
	    EV_USE_ITEM6,
	    EV_USE_ITEM7,
	    EV_USE_ITEM8,
	    EV_USE_ITEM9,
	    EV_USE_ITEM10,
	    EV_USE_ITEM11,
	    EV_USE_ITEM12,
	    EV_USE_ITEM13,
	    EV_USE_ITEM14,
	    EV_USE_ITEM15,
	    EV_ITEM_RESPAWN,
	    EV_ITEM_POP,
	    EV_PLAYER_TELEPORT_IN,
	    EV_PLAYER_TELEPORT_OUT,
	    EV_GRENADE_BOUNCE,
	    EV_GENERAL_SOUND,
	    EV_GLOBAL_SOUND,
	    EV_BULLET_HIT_FLESH,
	    EV_BULLET_HIT_WALL,
	    EV_MISSILE_HIT,
	    EV_MISSILE_MISS,
	    EV_RAILTRAIL,
	    EV_SHOTGUN,
	    EV_BULLET,
	    EV_PAIN,
	    EV_DEATH1,
	    EV_DEATH2,
	    EV_DEATH3,
	    EV_OBITUARY,
	    EV_POWERUP_QUAD,
	    EV_POWERUP_BATTLESUIT,
	    EV_POWERUP_REGEN,
	    EV_GIB_PLAYER,
	    EV_DEBUG_LINE,
	    EV_TAUNT
    }

    public enum EntityEvents : uint
    {
        EV_NONE,
        EV_FOOTSTEP,
        EV_FOOTSTEP_METAL,
        EV_FOOTSPLASH,
        EV_FOOTWADE,
        EV_SWIM,
        EV_STEP_4,
        EV_STEP_8,
        EV_STEP_12,
        EV_STEP_16,
        EV_FALL_SHORT,
        EV_FALL_MEDIUM,
        EV_FALL_FAR,
        EV_JUMP_PAD,			// boing sound at origin, jump sound on player
        EV_JUMP,
        EV_WATER_TOUCH,	// foot touches
        EV_WATER_LEAVE,	// foot leaves
        EV_WATER_UNDER,	// head touches
        EV_WATER_CLEAR,	// head leaves
        EV_ITEM_PICKUP,			// normal item pickups are predictable
        EV_GLOBAL_ITEM_PICKUP,	// powerup / team sounds are broadcast to everyone
        EV_NOAMMO,
        EV_CHANGE_WEAPON,
        EV_FIRE_WEAPON,
        EV_USE_ITEM0,
        EV_USE_ITEM1,
        EV_USE_ITEM2,
        EV_USE_ITEM3,
        EV_USE_ITEM4,
        EV_USE_ITEM5,
        EV_USE_ITEM6,
        EV_USE_ITEM7,
        EV_USE_ITEM8,
        EV_USE_ITEM9,
        EV_USE_ITEM10,
        EV_USE_ITEM11,
        EV_USE_ITEM12,
        EV_USE_ITEM13,
        EV_USE_ITEM14,
        EV_USE_ITEM15,
        EV_ITEM_RESPAWN,
        EV_ITEM_POP,
        EV_PLAYER_TELEPORT_IN,
        EV_PLAYER_TELEPORT_OUT,
        EV_GRENADE_BOUNCE,		// eventParm will be the soundindex
        EV_GENERAL_SOUND,
        EV_GLOBAL_SOUND,		// no attenuation
        EV_GLOBAL_TEAM_SOUND,
        EV_BULLET_HIT_FLESH,
        EV_BULLET_HIT_WALL,
        EV_MISSILE_HIT,
        EV_MISSILE_MISS,
        EV_MISSILE_MISS_METAL,
        EV_RAILTRAIL,
        EV_SHOTGUN,
        EV_BULLET,				// otherEntity is the shooter
        EV_PAIN,
        EV_DEATH1,
        EV_DEATH2,
        EV_DEATH3,
        EV_OBITUARY,
        EV_POWERUP_QUAD,
        EV_POWERUP_BATTLESUIT,
        EV_POWERUP_REGEN,
        EV_GIB_PLAYER,			// gib a previously living player
        EV_SCOREPLUM,			// score plum
        EV_PROXIMITY_MINE_STICK,
        EV_PROXIMITY_MINE_TRIGGER,
        EV_KAMIKAZE,			// kamikaze explodes
        EV_OBELISKEXPLODE,		// obelisk explodes
        EV_OBELISKPAIN,			// obelisk is in pain
        EV_INVUL_IMPACT,		// invulnerability sphere impact
        EV_JUICED,				// invulnerability juiced effect
        EV_LIGHTNINGBOLT,		// lightning bolt bounced of invulnerability sphere
        EV_DEBUG_LINE,
        EV_STOPLOOPINGSOUND,
        EV_TAUNT,
        EV_TAUNT_YES,
        EV_TAUNT_NO,
        EV_TAUNT_FOLLOWME,
        EV_TAUNT_GETFLAG,
        EV_TAUNT_GUARDBASE,
        EV_TAUNT_PATROL
    }

    public enum MeansOfDeath_Protocol43and45 : uint
    {
        MOD_UNKNOWN,
        MOD_SHOTGUN,
        MOD_GAUNTLET,
        MOD_MACHINEGUN,
        MOD_GRENADE,
        MOD_GRENADE_SPLASH,
        MOD_ROCKET,
        MOD_ROCKET_SPLASH,
        MOD_PLASMA,
        MOD_PLASMA_SPLASH,
        MOD_RAILGUN,
        MOD_LIGHTNING,
        MOD_BFG,
        MOD_BFG_SPLASH,
        MOD_WATER,
        MOD_SLIME,
        MOD_LAVA,
        MOD_CRUSH,
        MOD_TELEFRAG,
        MOD_FALLING,
        MOD_SUICIDE,
        MOD_TARGET_LASER,
        MOD_TRIGGER_HURT,
        MOD_GRAPPLE
    }

    public enum MeansOfDeath : uint
    {
        MOD_UNKNOWN,
        MOD_SHOTGUN,
        MOD_GAUNTLET,
        MOD_MACHINEGUN,
        MOD_GRENADE,
        MOD_GRENADE_SPLASH,
        MOD_ROCKET,
        MOD_ROCKET_SPLASH,
        MOD_PLASMA,
        MOD_PLASMA_SPLASH,
        MOD_RAILGUN,
        MOD_LIGHTNING,
        MOD_BFG,
        MOD_BFG_SPLASH,
        MOD_WATER,
        MOD_SLIME,
        MOD_LAVA,
        MOD_CRUSH,
        MOD_TELEFRAG,
        MOD_FALLING,
        MOD_SUICIDE,
        MOD_TARGET_LASER,
        MOD_TRIGGER_HURT,
        MOD_NAIL, // New in protocol 48.
        MOD_CHAINGUN, // New in protocol 48.
        MOD_PROXIMITY_MINE, // New in protocol 48.
        MOD_KAMIKAZE, // New in protocol 48.
        MOD_JUICED, // New in protocol 48.
        MOD_GRAPPLE
    }

    public enum Genders
    {
        Male,
        Female,
        Neutral
    }
}