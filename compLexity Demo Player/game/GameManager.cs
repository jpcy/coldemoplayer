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
            // Read from steam.xml.
            SteamGameInfo[] steamGames = (SteamGameInfo[])Common.XmlFileDeserialize(configPath + "\\" + steamConfigFileName, typeof(SteamGameInfo[]));

            foreach (SteamGameInfo sgi in steamGames)
            {
                games.Add(GameFactory.Create(sgi));
            }

            // Read game configs.
            Procedure<Game.Engines> enumerateConfigFolder = delegate(Game.Engines engine)
            {
                String engineName = (engine == Game.Engines.Source ? "source" : "goldsrc");
                DirectoryInfo directoryInfo = new DirectoryInfo(Config.Settings.ProgramPath + "\\config\\" + engineName);

                if (!directoryInfo.Exists)
                {
                    return;
                }

                foreach (FileInfo fi in directoryInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                {
                    String gameFolder = Path.GetFileNameWithoutExtension(fi.Name);
                    Game game = Find(engine, gameFolder);
                    game.ReadConfig((GameConfig)Common.XmlFileDeserialize(fi.FullName, typeof(GameConfig)));
                }
            };

            enumerateConfigFolder(Game.Engines.HalfLife);
            enumerateConfigFolder(Game.Engines.Source);
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
            Game game = Find(demo);

            if (game == null)
            {
                return false;
            }

            return game.CanAnalyse(demo);
        }
    }
}
