using System;
using System.Collections.Generic;
using System.Linq;
using CDP.Core;

namespace CDP.Quake3Arena
{
    public static class GameConstants
    {
        /// <summary>
        /// Config string indices for protocols 43 and 45.
        /// </summary>
        public static LookupTable_short ConfigStrings_Protocol43 = new LookupTable_short
        (
            new LookupElement<short>("CS_MUSIC", 2),
            new LookupElement<short>("CS_MESSAGE", 3),
            new LookupElement<short>("CS_MOTD", 4),
            new LookupElement<short>("CS_WARMUP", 5),
            new LookupElement<short>("CS_SCORES1", 6),
            new LookupElement<short>("CS_SCORES2", 7),
            new LookupElement<short>("CS_VOTE_TIME", 8),
            new LookupElement<short>("CS_VOTE_STRING", 9),
            new LookupElement<short>("CS_VOTE_YES", 10),
            new LookupElement<short>("CS_VOTE_NO", 11),
            new LookupElement<short>("CS_GAME_VERSION", 12),
            new LookupElement<short>("CS_LEVEL_START_TIME", 13),
            new LookupElement<short>("CS_INTERMISSION", 14),
            new LookupElement<short>("CS_FLAGSTATUS", 15),
            new LookupElement<short>("CS_ITEMS", 27),
            new LookupElement<short>("CS_MODELS", 32, true),
            new LookupElement<short>("CS_SOUNDS", 288, true), // (CS_MODELS+MAX_MODELS)
            new LookupElement<short>("CS_PLAYERS", 544, true), // (CS_SOUNDS+MAX_SOUNDS)
            new LookupElement<short>("CS_LOCATIONS", 672, true) // (CS_PLAYERS+MAX_CLIENTS)
        );

        /// <summary>
        /// Config string indices for protocol 48.
        /// </summary>
        public static LookupTable_short ConfigStrings_Protocol48 = new LookupTable_short
        (
            new LookupElement<short>("CS_MUSIC", 2),
            new LookupElement<short>("CS_MESSAGE", 3),
            new LookupElement<short>("CS_MOTD", 4),
            new LookupElement<short>("CS_WARMUP", 5),
            new LookupElement<short>("CS_SCORES1", 6),
            new LookupElement<short>("CS_SCORES2", 7),
            new LookupElement<short>("CS_VOTE_TIME", 8),
            new LookupElement<short>("CS_VOTE_STRING", 9),
            new LookupElement<short>("CS_VOTE_YES", 10),
            new LookupElement<short>("CS_VOTE_NO", 11),
            new LookupElement<short>("CS_TEAMVOTE_TIME", 12),
            new LookupElement<short>("CS_TEAMVOTE_STRING", 14),
            new LookupElement<short>("CS_TEAMVOTE_YES", 16),
            new LookupElement<short>("CS_TEAMVOTE_NO", 18),
            new LookupElement<short>("CS_GAME_VERSION", 20),
            new LookupElement<short>("CS_LEVEL_START_TIME", 21),
            new LookupElement<short>("CS_INTERMISSION", 22),
            new LookupElement<short>("CS_FLAGSTATUS", 23),
            new LookupElement<short>("CS_SHADERSTATE", 24),
            new LookupElement<short>("CS_BOTINFO", 25),
            new LookupElement<short>("CS_ITEMS", 27),
            new LookupElement<short>("CS_MODELS", 32, true),
            new LookupElement<short>("CS_SOUNDS", 288, true), // (CS_MODELS+MAX_MODELS)
            new LookupElement<short>("CS_PLAYERS", 544, true), // (CS_SOUNDS+MAX_SOUNDS)
            new LookupElement<short>("CS_LOCATIONS", 608, true) // (CS_PLAYERS+MAX_CLIENTS)
        );

        /// <summary>
        /// Config string indices for protocols 66, 67 and 68.
        /// </summary>
        public static LookupTable_short ConfigStrings_Protocol66 = new LookupTable_short
        (
            new LookupElement<short>("CS_MUSIC", 2),
            new LookupElement<short>("CS_MESSAGE", 3),
            new LookupElement<short>("CS_MOTD", 4),
            new LookupElement<short>("CS_WARMUP", 5),
            new LookupElement<short>("CS_SCORES1", 6),
            new LookupElement<short>("CS_SCORES2", 7),
            new LookupElement<short>("CS_VOTE_TIME", 8),
            new LookupElement<short>("CS_VOTE_STRING", 9),
            new LookupElement<short>("CS_VOTE_YES", 10),
            new LookupElement<short>("CS_VOTE_NO", 11),
            new LookupElement<short>("CS_TEAMVOTE_TIME", 12),
            new LookupElement<short>("CS_TEAMVOTE_STRING", 14),
            new LookupElement<short>("CS_TEAMVOTE_YES", 16),
            new LookupElement<short>("CS_TEAMVOTE_NO", 18),
            new LookupElement<short>("CS_GAME_VERSION", 20),
            new LookupElement<short>("CS_LEVEL_START_TIME", 21),
            new LookupElement<short>("CS_INTERMISSION", 22),
            new LookupElement<short>("CS_FLAGSTATUS", 23),
            new LookupElement<short>("CS_SHADERSTATE", 24),
            new LookupElement<short>("CS_BOTINFO", 25),
            new LookupElement<short>("CS_ITEMS", 27),
            new LookupElement<short>("CS_MODELS", 32, true), 
            new LookupElement<short>("CS_SOUNDS", 288, true), // (CS_MODELS+MAX_MODELS)
            new LookupElement<short>("CS_PLAYERS", 544, true), // (CS_SOUNDS+MAX_SOUNDS)
            new LookupElement<short>("CS_LOCATIONS", 608, true), // (CS_PLAYERS+MAX_CLIENTS)
            new LookupElement<short>("CS_PARTICLES", 672, true) // (CS_LOCATIONS+MAX_LOCATIONS) 
        );

        /// <summary>
        /// Entity types for protocols 43 and 45.
        /// </summary>
        public static LookupTable_uint EntityTypes_Protocol43 = new LookupTable_uint
        (
            new LookupElement<uint>("ET_GENERAL", 0),
            new LookupElement<uint>("ET_PLAYER", 1),
            new LookupElement<uint>("ET_ITEM", 2),
            new LookupElement<uint>("ET_MISSILE", 3),
            new LookupElement<uint>("ET_MOVER", 4),
            new LookupElement<uint>("ET_BEAM", 5),
            new LookupElement<uint>("ET_PORTAL", 6),
            new LookupElement<uint>("ET_SPEAKER", 7),
            new LookupElement<uint>("ET_PUSH_TRIGGER", 8),
            new LookupElement<uint>("ET_TELEPORT_TRIGGER", 9),
            new LookupElement<uint>("ET_INVISIBLE", 10),
            new LookupElement<uint>("ET_GRAPPLE", 11),
            new LookupElement<uint>("ET_EVENTS", 12, true)
        );

        /// <summary>
        /// Entity types for protocols 48, 66, 67 and 68.
        /// </summary>
        public static LookupTable_uint EntityTypes_Protocol48 = new LookupTable_uint
        (
            new LookupElement<uint>("ET_GENERAL", 0),
            new LookupElement<uint>("ET_PLAYER", 1),
            new LookupElement<uint>("ET_ITEM", 2),
            new LookupElement<uint>("ET_MISSILE", 3),
            new LookupElement<uint>("ET_MOVER", 4),
            new LookupElement<uint>("ET_BEAM", 5),
            new LookupElement<uint>("ET_PORTAL", 6),
            new LookupElement<uint>("ET_SPEAKER", 7),
            new LookupElement<uint>("ET_PUSH_TRIGGER", 8),
            new LookupElement<uint>("ET_TELEPORT_TRIGGER", 9),
            new LookupElement<uint>("ET_INVISIBLE", 10),
            new LookupElement<uint>("ET_GRAPPLE", 11),
            new LookupElement<uint>("ET_TEAM", 12), // New in protocol 48.
            new LookupElement<uint>("ET_EVENTS", 13, true)
        );

        /// <summary>
        /// Entity events for protocols 43 and 45.
        /// </summary>
        public static LookupTable_uint EntityEvents_Protocol43 = new LookupTable_uint
        (
            new LookupElement<uint>("EV_NONE", 0),
            new LookupElement<uint>("EV_FOOTSTEP", 1),
            new LookupElement<uint>("EV_FOOTSTEP_METAL", 2),
            new LookupElement<uint>("EV_FOOTSPLASH", 3),
            new LookupElement<uint>("EV_FOOTWADE", 4),
            new LookupElement<uint>("EV_SWIM", 5),
            new LookupElement<uint>("EV_STEP_4", 6),
            new LookupElement<uint>("EV_STEP_8", 7),
            new LookupElement<uint>("EV_STEP_12", 8),
            new LookupElement<uint>("EV_STEP_16", 9),
            new LookupElement<uint>("EV_FALL_SHORT", 10),
            new LookupElement<uint>("EV_FALL_MEDIUM", 11),
            new LookupElement<uint>("EV_FALL_FAR", 12),
            new LookupElement<uint>("EV_JUMP_PAD", 13),
            new LookupElement<uint>("EV_JUMP", 14),
            new LookupElement<uint>("EV_WATER_TOUCH", 15),
            new LookupElement<uint>("EV_WATER_LEAVE", 16),
            new LookupElement<uint>("EV_WATER_UNDER", 17),
            new LookupElement<uint>("EV_WATER_CLEAR", 18),
            new LookupElement<uint>("EV_ITEM_PICKUP", 19),
            new LookupElement<uint>("EV_GLOBAL_ITEM_PICKUP", 20),
            new LookupElement<uint>("EV_NOAMMO", 21),
            new LookupElement<uint>("EV_CHANGE_WEAPON", 22),
            new LookupElement<uint>("EV_FIRE_WEAPON", 23),
            new LookupElement<uint>("EV_USE_ITEM0", 24),
            new LookupElement<uint>("EV_USE_ITEM1", 25),
            new LookupElement<uint>("EV_USE_ITEM2", 26),
            new LookupElement<uint>("EV_USE_ITEM3", 27),
            new LookupElement<uint>("EV_USE_ITEM4", 28),
            new LookupElement<uint>("EV_USE_ITEM5", 29),
            new LookupElement<uint>("EV_USE_ITEM6", 30),
            new LookupElement<uint>("EV_USE_ITEM7", 31),
            new LookupElement<uint>("EV_USE_ITEM8", 32),
            new LookupElement<uint>("EV_USE_ITEM9", 33),
            new LookupElement<uint>("EV_USE_ITEM10", 34),
            new LookupElement<uint>("EV_USE_ITEM11", 35),
            new LookupElement<uint>("EV_USE_ITEM12", 36),
            new LookupElement<uint>("EV_USE_ITEM13", 37),
            new LookupElement<uint>("EV_USE_ITEM14", 38),
            new LookupElement<uint>("EV_USE_ITEM15", 39),
            new LookupElement<uint>("EV_ITEM_RESPAWN", 40),
            new LookupElement<uint>("EV_ITEM_POP", 41),
            new LookupElement<uint>("EV_PLAYER_TELEPORT_IN", 42),
            new LookupElement<uint>("EV_PLAYER_TELEPORT_OUT", 43),
            new LookupElement<uint>("EV_GRENADE_BOUNCE", 44),
            new LookupElement<uint>("EV_GENERAL_SOUND", 45),
            new LookupElement<uint>("EV_GLOBAL_SOUND", 46),
            new LookupElement<uint>("EV_BULLET_HIT_FLESH", 47),
            new LookupElement<uint>("EV_BULLET_HIT_WALL", 48),
            new LookupElement<uint>("EV_MISSILE_HIT", 49),
            new LookupElement<uint>("EV_MISSILE_MISS", 50),
            new LookupElement<uint>("EV_RAILTRAIL", 51),
            new LookupElement<uint>("EV_SHOTGUN", 52),
            new LookupElement<uint>("EV_BULLET", 53),
            new LookupElement<uint>("EV_PAIN", 54),
            new LookupElement<uint>("EV_DEATH1", 55),
            new LookupElement<uint>("EV_DEATH2", 56),
            new LookupElement<uint>("EV_DEATH3", 57),
            new LookupElement<uint>("EV_OBITUARY", 58),
            new LookupElement<uint>("EV_POWERUP_QUAD", 59),
            new LookupElement<uint>("EV_POWERUP_BATTLESUIT", 60),
            new LookupElement<uint>("EV_POWERUP_REGEN", 61),
            new LookupElement<uint>("EV_GIB_PLAYER", 62),
            new LookupElement<uint>("EV_DEBUG_LINE", 63),
            new LookupElement<uint>("EV_TAUNT", 64)
        );

        /// <summary>
        /// Entity events for protocols 48, 66, 67 and 68.
        /// </summary>
        public static LookupTable_uint EntityEvents_Protocol48 = new LookupTable_uint
        (
            new LookupElement<uint>("EV_NONE", 0),
            new LookupElement<uint>("EV_FOOTSTEP", 1),
            new LookupElement<uint>("EV_FOOTSTEP_METAL", 2),
            new LookupElement<uint>("EV_FOOTSPLASH", 3),
            new LookupElement<uint>("EV_FOOTWADE", 4),
            new LookupElement<uint>("EV_SWIM", 5),
            new LookupElement<uint>("EV_STEP_4", 6),
            new LookupElement<uint>("EV_STEP_8", 7),
            new LookupElement<uint>("EV_STEP_12", 8),
            new LookupElement<uint>("EV_STEP_16", 9),
            new LookupElement<uint>("EV_FALL_SHORT", 10),
            new LookupElement<uint>("EV_FALL_MEDIUM", 11),
            new LookupElement<uint>("EV_FALL_FAR", 12),
            new LookupElement<uint>("EV_JUMP_PAD", 13),
            new LookupElement<uint>("EV_JUMP", 14),
            new LookupElement<uint>("EV_WATER_TOUCH", 15),
            new LookupElement<uint>("EV_WATER_LEAVE", 16),
            new LookupElement<uint>("EV_WATER_UNDER", 17),
            new LookupElement<uint>("EV_WATER_CLEAR", 18),
            new LookupElement<uint>("EV_ITEM_PICKUP", 19),
            new LookupElement<uint>("EV_GLOBAL_ITEM_PICKUP", 20),
            new LookupElement<uint>("EV_NOAMMO", 21),
            new LookupElement<uint>("EV_CHANGE_WEAPON", 22),
            new LookupElement<uint>("EV_FIRE_WEAPON", 23),
            new LookupElement<uint>("EV_USE_ITEM0", 24),
            new LookupElement<uint>("EV_USE_ITEM1", 25),
            new LookupElement<uint>("EV_USE_ITEM2", 26),
            new LookupElement<uint>("EV_USE_ITEM3", 27),
            new LookupElement<uint>("EV_USE_ITEM4", 28),
            new LookupElement<uint>("EV_USE_ITEM5", 29),
            new LookupElement<uint>("EV_USE_ITEM6", 30),
            new LookupElement<uint>("EV_USE_ITEM7", 31),
            new LookupElement<uint>("EV_USE_ITEM8", 32),
            new LookupElement<uint>("EV_USE_ITEM9", 33),
            new LookupElement<uint>("EV_USE_ITEM10", 34),
            new LookupElement<uint>("EV_USE_ITEM11", 35),
            new LookupElement<uint>("EV_USE_ITEM12", 36),
            new LookupElement<uint>("EV_USE_ITEM13", 37),
            new LookupElement<uint>("EV_USE_ITEM14", 38),
            new LookupElement<uint>("EV_USE_ITEM15", 39),
            new LookupElement<uint>("EV_ITEM_RESPAWN", 40),
            new LookupElement<uint>("EV_ITEM_POP", 41),
            new LookupElement<uint>("EV_PLAYER_TELEPORT_IN", 42),
            new LookupElement<uint>("EV_PLAYER_TELEPORT_OUT", 43),
            new LookupElement<uint>("EV_GRENADE_BOUNCE", 44),
            new LookupElement<uint>("EV_GENERAL_SOUND", 45),
            new LookupElement<uint>("EV_GLOBAL_SOUND", 46),
            new LookupElement<uint>("EV_GLOBAL_TEAM_SOUND", 47),
            new LookupElement<uint>("EV_BULLET_HIT_FLESH", 48),
            new LookupElement<uint>("EV_BULLET_HIT_WALL", 49),
            new LookupElement<uint>("EV_MISSILE_HIT", 50),
            new LookupElement<uint>("EV_MISSILE_MISS", 51),
            new LookupElement<uint>("EV_MISSILE_MISS_METAL", 52),
            new LookupElement<uint>("EV_RAILTRAIL", 53),
            new LookupElement<uint>("EV_SHOTGUN", 54),
            new LookupElement<uint>("EV_BULLET", 55),
            new LookupElement<uint>("EV_PAIN", 56),
            new LookupElement<uint>("EV_DEATH1", 57),
            new LookupElement<uint>("EV_DEATH2", 58),
            new LookupElement<uint>("EV_DEATH3", 59),
            new LookupElement<uint>("EV_OBITUARY", 60),
            new LookupElement<uint>("EV_POWERUP_QUAD", 61),
            new LookupElement<uint>("EV_POWERUP_BATTLESUIT", 62),
            new LookupElement<uint>("EV_POWERUP_REGEN", 63),
            new LookupElement<uint>("EV_GIB_PLAYER", 64),
            new LookupElement<uint>("EV_SCOREPLUM", 65),
            new LookupElement<uint>("EV_PROXIMITY_MINE_STICK", 66),
            new LookupElement<uint>("EV_PROXIMITY_MINE_TRIGGER", 67),
            new LookupElement<uint>("EV_KAMIKAZE", 68),
            new LookupElement<uint>("EV_OBELISKEXPLODE", 69),
            new LookupElement<uint>("EV_OBELISKPAIN", 70),
            new LookupElement<uint>("EV_INVUL_IMPACT", 71),
            new LookupElement<uint>("EV_JUICED", 72),
            new LookupElement<uint>("EV_LIGHTNINGBOLT", 73),
            new LookupElement<uint>("EV_DEBUG_LINE", 74),
            new LookupElement<uint>("EV_STOPLOOPINGSOUND", 75),
            new LookupElement<uint>("EV_TAUNT", 76),
            new LookupElement<uint>("EV_TAUNT_YES", 77),
            new LookupElement<uint>("EV_TAUNT_NO", 78),
            new LookupElement<uint>("EV_TAUNT_FOLLOWME", 79),
            new LookupElement<uint>("EV_TAUNT_GETFLAG", 80),
            new LookupElement<uint>("EV_TAUNT_GUARDBASE", 81),
            new LookupElement<uint>("EV_TAUNT_PATROL", 82)
        );

        /// <summary>
        /// Means of deaths for protocols 43 and 45.
        /// </summary>
        public static LookupTable_uint MeansOfDeath_Protocol43 = new LookupTable_uint
        (
            new LookupElement<uint>("MOD_UNKNOWN", 0),
            new LookupElement<uint>("MOD_SHOTGUN", 1),
            new LookupElement<uint>("MOD_GAUNTLET", 2),
            new LookupElement<uint>("MOD_MACHINEGUN", 3),
            new LookupElement<uint>("MOD_GRENADE", 4),
            new LookupElement<uint>("MOD_GRENADE_SPLASH", 5),
            new LookupElement<uint>("MOD_ROCKET", 6),
            new LookupElement<uint>("MOD_ROCKET_SPLASH", 7),
            new LookupElement<uint>("MOD_PLASMA", 8),
            new LookupElement<uint>("MOD_PLASMA_SPLASH", 9),
            new LookupElement<uint>("MOD_RAILGUN", 10),
            new LookupElement<uint>("MOD_LIGHTNING", 11),
            new LookupElement<uint>("MOD_BFG", 12),
            new LookupElement<uint>("MOD_BFG_SPLASH", 13),
            new LookupElement<uint>("MOD_WATER", 14),
            new LookupElement<uint>("MOD_SLIME", 15),
            new LookupElement<uint>("MOD_LAVA", 16),
            new LookupElement<uint>("MOD_CRUSH", 17),
            new LookupElement<uint>("MOD_TELEFRAG", 18),
            new LookupElement<uint>("MOD_FALLING", 19),
            new LookupElement<uint>("MOD_SUICIDE", 20),
            new LookupElement<uint>("MOD_TARGET_LASER", 21),
            new LookupElement<uint>("MOD_TRIGGER_HURT", 22),
            new LookupElement<uint>("MOD_GRAPPLE", 23)
        );

        /// <summary>
        /// Means of deaths for protocols 48, 66, 67 and 68.
        /// </summary>
        public static LookupTable_uint MeansOfDeath_Protocol48 = new LookupTable_uint
        (
            new LookupElement<uint>("MOD_UNKNOWN", 0),
            new LookupElement<uint>("MOD_SHOTGUN", 1),
            new LookupElement<uint>("MOD_GAUNTLET", 2),
            new LookupElement<uint>("MOD_MACHINEGUN", 3),
            new LookupElement<uint>("MOD_GRENADE", 4),
            new LookupElement<uint>("MOD_GRENADE_SPLASH", 5),
            new LookupElement<uint>("MOD_ROCKET", 6),
            new LookupElement<uint>("MOD_ROCKET_SPLASH", 7),
            new LookupElement<uint>("MOD_PLASMA", 8),
            new LookupElement<uint>("MOD_PLASMA_SPLASH", 9),
            new LookupElement<uint>("MOD_RAILGUN", 10),
            new LookupElement<uint>("MOD_LIGHTNING", 11),
            new LookupElement<uint>("MOD_BFG", 12),
            new LookupElement<uint>("MOD_BFG_SPLASH", 13),
            new LookupElement<uint>("MOD_WATER", 14),
            new LookupElement<uint>("MOD_SLIME", 15),
            new LookupElement<uint>("MOD_LAVA", 16),
            new LookupElement<uint>("MOD_CRUSH", 17),
            new LookupElement<uint>("MOD_TELEFRAG", 18),
            new LookupElement<uint>("MOD_FALLING", 19),
            new LookupElement<uint>("MOD_SUICIDE", 20),
            new LookupElement<uint>("MOD_TARGET_LASER", 21),
            new LookupElement<uint>("MOD_TRIGGER_HURT", 22),
            new LookupElement<uint>("MOD_NAIL", 23), // New in protocol 48.
            new LookupElement<uint>("MOD_CHAINGUN", 24), // New in protocol 48.
            new LookupElement<uint>("MOD_PROXIMITY_MINE", 25), // New in protocol 48.
            new LookupElement<uint>("MOD_KAMIKAZE", 26), // New in protocol 48.
            new LookupElement<uint>("MOD_JUICED", 27), // New in protocol 48.
            new LookupElement<uint>("MOD_GRAPPLE", 28)
        );

        /// <summary>
        /// Player persistant array indices for protocols 43 and 45.
        /// </summary>
        public static LookupTable_int PlayerPersistant_Protocol43 = new LookupTable_int
        (
            new LookupElement<int>("PERS_SCORE", 0),
            new LookupElement<int>("PERS_HITS", 1),
            new LookupElement<int>("PERS_RANK", 2),
            new LookupElement<int>("PERS_TEAM", 3),
            new LookupElement<int>("PERS_SPAWN_COUNT", 4),
            new LookupElement<int>("PERS_REWARD_COUNT", 5),
            new LookupElement<int>("PERS_REWARD", 6),
            new LookupElement<int>("PERS_ATTACKER", 7),
            new LookupElement<int>("PERS_KILLED", 8),
            new LookupElement<int>("PERS_IMPRESSIVE_COUNT", 9),
            new LookupElement<int>("PERS_EXCELLENT_COUNT", 10),
            new LookupElement<int>("PERS_GAUNTLET_FRAG_COUNT", 11),
            new LookupElement<int>("PERS_ACCURACY_SHOTS", 12),
            new LookupElement<int>("PERS_ACCURACY_HITS", 13)
        );

        /// <summary>
        /// Player persistant array indices for protocols 48, 66, 67 and 68.
        /// </summary>
        public static LookupTable_int PlayerPersistant_Protocol48 = new LookupTable_int
        (
            new LookupElement<int>("PERS_SCORE", 0),
            new LookupElement<int>("PERS_HITS", 1),
            new LookupElement<int>("PERS_RANK", 2),
            new LookupElement<int>("PERS_TEAM", 3),
            new LookupElement<int>("PERS_SPAWN_COUNT", 4),
            new LookupElement<int>("PERS_PLAYEREVENTS", 5), // New.
            new LookupElement<int>("PERS_ATTACKER", 6),
            new LookupElement<int>("PERS_ATTACKEE_ARMOR", 7), // New.
            new LookupElement<int>("PERS_KILLED", 8),
            new LookupElement<int>("PERS_IMPRESSIVE_COUNT", 9),
            new LookupElement<int>("PERS_EXCELLENT_COUNT", 10),
            new LookupElement<int>("PERS_DEFEND_COUNT", 11), // New.
            new LookupElement<int>("PERS_ASSIST_COUNT", 12), // New.
            new LookupElement<int>("PERS_GAUNTLET_FRAG_COUNT", 13),
            new LookupElement<int>("PERS_CAPTURES", 14) // New.
        );
    }

    public enum Genders
    {
        Male,
        Female,
        Neutral
    }
}