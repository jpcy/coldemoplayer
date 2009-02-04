using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace compLexity_Demo_Player
{
    public static class GameManager
    {
        private static readonly String steamConfigFileName = "steam.xml";
        private static List<Game> games = new List<Game>();

        public static void Initialise(String configPath)
        {
            games.Add(new Games.CounterStrike());
            games.Add(new Games.CounterStrikeSource());

            // Read from steam.xml.
            SteamGameInfo[] steamGames = (SteamGameInfo[])Common.XmlFileDeserialize(configPath + "\\" + steamConfigFileName, typeof(SteamGameInfo[]));

            foreach (SteamGameInfo sgi in steamGames)
            {
                // Make sure the game hasn't already been added.
                if (games.Find(game => game.Engine == sgi.Engine && game.Folder == sgi.GameFolder) == null)
                {
                    games.Add(new Game(sgi));
                }
            }
        }

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

        public static Game Find(Demo demo)
        {
            return Find((demo.Engine == Demo.EngineEnum.Source ? Game.Engines.Source : Game.Engines.HalfLife), demo.GameFolderName);
        }

        public static List<Game> ListAll(Function<Game,Boolean> selector)
        {
            List<Game> result = new List<Game>();

            foreach (Game game in games)
            {
                if (selector == null || selector(game))
                {
                    result.Add(game);
                }
            }

            return result;
        }

        public static Boolean CanAnalyse(Demo demo)
        {
            if (demo == null)
            {
                return false;
            }

            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanAnalyse(demo);
        }
    }
}
