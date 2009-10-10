using System;
using System.IO;

namespace CDP.CounterStrike
{
    class Launcher : HalfLife.Launcher
    {
        public override bool Verify()
        {
            bool baseResult = base.Verify();

            if (!baseResult)
            {
                return false;
            }

            Game game = (Game)((Demo)demo).Game;

            // See if the map is built-in (i.e. this demo was recorded using the same map that exists in the current version of the game).
            if (game.BuiltInMapExists(demo.MapChecksum, demo.MapName))
            {
                return true;
            }

            // See if the map already exists in the game's "maps" folder.
            string mapDestinationPath = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"], appFolder, gameFolder, "maps");
            string mapDestinationFileName = fileSystem.PathCombine(mapDestinationPath, demo.MapName + ".bsp");

            if (File.Exists(mapDestinationFileName))
            {
                HalfLife.MapChecksum mapChecksum = new HalfLife.MapChecksum();
                mapChecksum.Calculate(mapDestinationFileName);

                if (mapChecksum.Equals(demo.MapChecksum))
                {
                    // Suitable map found.
                    return true;
                }
            }

            // See if the map exists in the map pool.
            string mapSourcePath = fileSystem.PathCombine(settings.ProgramDataPath, "maps", "goldsrc", "cstrike", demo.MapChecksum.ToString());
            string mapSourceFileName = fileSystem.PathCombine(mapSourcePath, demo.MapName + ".bsp");

            if (!File.Exists(mapSourceFileName))
            {
                // TODO: block and attempt to download the required map here.

                Message = string.Format("No suitable map found in the map pool: \'{0}\' with the checksum \'{1}\'.", demo.MapName, demo.MapChecksum);
                return false;
            }

            return true;
        }
    }
}
