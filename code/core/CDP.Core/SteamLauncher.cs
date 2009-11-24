using System;
using System.IO;

namespace CDP.Core
{
    public abstract class SteamLauncher : Launcher
    {
        protected readonly ISettings settings = ObjectCreator.Get<ISettings>();
        protected readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();

        protected string gameName;
        protected int appId;
        protected string appFolder;
        protected string gameFolder;

        public override bool Verify()
        {
            if (gameName == null)
            {
                throw new InvalidOperationException("gameName cannot be null.");
            }
            if (appId == 0)
            {
                throw new InvalidOperationException("appId cannot be 0.");
            }
            if (appFolder == null)
            {
                throw new InvalidOperationException("appFolder cannot be null.");
            }
            if (gameFolder == null)
            {
                throw new InvalidOperationException("gameFolder cannot be null.");
            }

            // Verify that steam.exe exists.
            if (!File.Exists((string)settings["SteamExeFullPath"]))
            {
                Message = string.Format(Strings.SteamExePathNotSet, settings["SteamExeFullPath"]);
                return false;
            }

            // Verify that the Steam account folder exists.
            string steamAccountPath = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"]);

            if (!Directory.Exists(steamAccountPath))
            {
                Message = string.Format(Strings.SteamAccountFolderDoesNotExist, settings["SteamAccountName"]);
                return false;
            }

            // Verify that the game folder exists.
            string gamePath = fileSystem.PathCombine(steamAccountPath, appFolder, gameFolder);

            if (!Directory.Exists(gamePath))
            {
                Message = string.Format(Strings.SteamGameFolderDoesNotExist, gameName, gamePath);
                return false;
            }

            // Verify that Steam is running.
            if (FindProcess("Steam", (string)settings["SteamExeFullPath"]) == null)
            {
                Message = Strings.SteamNotRunning;
                return false;
            }

            if (processExecutableFileName == null)
            {
                throw new InvalidOperationException("processExecutableFileName cannot be null.");
            }

            // Verify that the game is not already running.
            if (FindProcess(Path.GetFileNameWithoutExtension(processExecutableFileName), processExecutableFileName) != null)
            {
                Message = string.Format(Strings.SteamGameAlreadyRunning, gameName);
                return false;
            }

            return true;
        }
    }
}
