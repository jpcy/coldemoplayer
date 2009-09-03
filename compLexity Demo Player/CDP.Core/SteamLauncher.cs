using System;
using System.IO;

namespace CDP.Core
{
    public abstract class SteamLauncher : Launcher
    {
        protected readonly ISettings settings;
        protected readonly IFileSystem fileSystem;

        protected string gameName;
        protected int appId;
        protected string appFolder;
        protected string gameFolder;

        public SteamLauncher(ISettings settings, IFileSystem fileSystem)
        {
            this.settings = settings;
            this.fileSystem = fileSystem;
        }

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
                Message = string.Format("Unable to find \"Steam.exe\" at \"{0}\". Go to Options and select the correct Steam path.", settings["SteamExeFullPath"]);
                return false;
            }

            // Verify that the Steam account folder exists.
            string steamAccountPath = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"]);

            if (!Directory.Exists(steamAccountPath))
            {
                Message = string.Format("Steam account folder \"{0}\" doesn't exist. Go to Options and select a valid Steam account folder.", settings["SteamAccountName"]);
                return false;
            }

            // Verify that the game folder exists.
            string gamePath = fileSystem.PathCombine(steamAccountPath, appFolder, gameFolder);

            if (!Directory.Exists(gamePath))
            {
                Message = string.Format("The game \"{0}\" doesn't seem to be installed on the selected Steam account. The folder \"{1}\" does not exist. Check that the correct Steam account is selected in Options.", gameName, gamePath);
                return false;
            }

            // Verify that Steam is running.
            if (FindProcess("Steam", (string)settings["SteamExeFullPath"]) == null)
            {
                Message = "Steam is not running. Launch Steam, log into your account and try again.";
                return false;
            }

            if (processExecutableFileName == null)
            {
                throw new InvalidOperationException("processExecutableFileName cannot be null.");
            }

            // Verify that the game is not already running.
            if (FindProcess(Path.GetFileNameWithoutExtension(processExecutableFileName), processExecutableFileName) != null)
            {
                Message = string.Format("Cannot play a demo while \"{0}\" is already running. Exit the game and try again.", gameName);
                return false;
            }

            return true;
        }
    }
}
