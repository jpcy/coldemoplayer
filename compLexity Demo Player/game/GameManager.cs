using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media;
using JsonExSerializer;

namespace compLexity_Demo_Player
{
    public static class GameManager
    {
        private static readonly String steamConfigFileName = "steam.json";
        private static List<Game> games = new List<Game>();

        /// <summary>
        /// Initialises the game manager.
        /// </summary>
        /// <param name="configPath">The path to the folder that contains steam.xml.</param>
        public static void Initialise(String configPath)
        {
            // Add any games that have their own class (i.e. they support features beyond simple playback).
            games.Add(new Games.CounterStrike());
            games.Add(new Games.CounterStrikeSource());

            // Read from steam.json.
            SteamGameInfo[] steamGames;

            using (StreamReader stream = new StreamReader(configPath + "\\" + steamConfigFileName))
            {
                Serializer serializer = new Serializer(typeof(SteamGameInfo[]));
                steamGames = (SteamGameInfo[])serializer.Deserialize(stream);
            }

            foreach (SteamGameInfo sgi in steamGames)
            {
                // Make sure the game hasn't already been added.
                if (games.Find(game => game.Engine == sgi.Engine && game.Folder == sgi.GameFolder) == null)
                {
                    games.Add(new Game(sgi));
                }
            }
        }

        /// <summary>
        /// Finds a game.
        /// </summary>
        /// <param name="engine">The game engine.</param>
        /// <param name="folder">The game folder.</param>
        /// <returns>A Game if successful, otherwise null.</returns>
        public static Game Find(Game.Engines engine, String folder)
        {
            foreach (Game game in games)
            {
                if (game.Engine == engine && game.Folder == folder)
                {
                    return game;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a game.
        /// </summary>
        /// <param name="demo">The demo that was recorded using the game that is being searched for.</param>
        /// <returns>A Game if successful, otherwise null.</returns>
        public static Game Find(Demo demo)
        {
            return Find((demo.Engine == Demo.Engines.Source ? Game.Engines.Source : Game.Engines.HalfLife), demo.GameFolderName);
        }

        /// <summary>
        /// Lists all games that meet a condition.
        /// </summary>
        /// <param name="match">The predicate that determines whether the game should be returned in the result.</param>
        /// <returns>A list of games.</returns>
        public static List<Game> ListAll(Predicate<Game> match)
        {
            return games.FindAll(match);
        }

        /// <summary>
        /// Determines whether the game used to record a demo has support for analysis.
        /// </summary>
        public static Boolean CanAnalyse(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanAnalyse(demo);
        }

        /// <summary>
        /// Determines whether a demo can be converted to the current network protocol.
        /// </summary>
        public static bool CanConvertNetworkProtocol(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanConvertNetworkProtocol(demo);
        }

        /// <summary>
        /// Determines whether a demo supports removing fade to black.
        /// </summary>
        public static bool CanRemoveFadeToBlack(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanRemoveFadeToBlack(demo);
        }

        /// <summary>
        /// Determines whether a demo supports the removal of HLTV ads.
        /// </summary>
        /// <param name="demo"></param>
        public static bool CanRemoveHltvAds(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanRemoveHltvAds(demo);
        }

        /// <summary>
        /// Determines whether a demo supports the removal of HLTV slow motion.
        /// </summary>
        /// <param name="demo"></param>
        public static bool CanRemoveHltvSlowMotion(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanRemoveHltvSlowMotion(demo);
        }

        /// <summary>
        /// Calculate team colours.
        /// </summary>
        /// <returns>A colour, or null if no game information can be found.</returns>
        public static SolidColorBrush TeamColour(Demo demo, String team)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return null;
            }

            return game.TeamColour(team);
        }

        /// <summary>
        /// Returns the svc_event_reliable event ID that corresponds to a new round.
        /// </summary>
        public static Int32 NewRoundEventId(Demo demo)
        {
            Game game = Find(demo);

            if (game == null)
            {
                return -1;
            }

            return game.NewRoundEventId(demo);
        }
    }
}
