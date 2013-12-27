using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    public abstract class SteamLauncher : Launcher
    {
        protected override void VerifyProgramSettings()
        {
            // verify that steam.exe exists
            if (!File.Exists(Config.Settings.SteamExeFullPath))
            {
                throw new ApplicationException("Unable to find \"Steam.exe\" at \"" + Config.Settings.SteamExeFullPath + "\". Go to Options\\Preferences and select the correct Steam path.");
            }

            // verify there is steam app info for this server/demo
            Game game = GameManager.Find(Demo);

            if (game == null)
            {
                throw new ApplicationException(String.Format("No Steam information can be found for the game \"{0}\". The demo cannot be played.", Demo.GameFolderName));
            }

            // verify that the steam account folder exists
            // don't bother if the game uses the "common" folder instead
            String steamAccountFullPath = String.Empty;

            if (!game.UsesCommonFolder)
            {
                steamAccountFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder;

                if (!Directory.Exists(steamAccountFullPath))
                {
                    throw new ApplicationException("Steam account folder \"" + steamAccountFullPath + "\" doesn't exist. Go to Options\\Preferences and select a valid Steam account folder.");
                }
            }

            // verify that the game folder exists
            if (game.UsesCommonFolder)
            {
                if (Demo.Engine == Demo.Engines.Source)
                {
                    gameFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\common\\" + game.FolderExtended + "\\" + Demo.GameFolderName;
                }
                else
                {
                    // if the user is using a custom Steam hl.exe path, use that to calculate the game path instead.
                    if (File.Exists(Config.Settings.SteamHlExeFullPath))
                    {
                        gameFullPath = Path.GetDirectoryName(Config.Settings.SteamHlExeFullPath) + "\\" + Demo.GameFolderName;
                    }
                    else
                    {
                        gameFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\common\\Half-Life\\" + Demo.GameFolderName;
                    }
                }
            }
            else
            {
                gameFullPath = steamAccountFullPath + "\\" + game.FolderExtended + "\\" + Demo.GameFolderName;
            }

            if (!Directory.Exists(gameFullPath))
            {
                throw new ApplicationException("The game \"" + game.Name + "\" doesn't seem to be installed.\n\nGame folder \"" + gameFullPath + "\" doesn't exist.\n\nCheck that the correct Steam account is selected in Options\\Preferences.");
            }

            // hlae: verify the exe path is set
            if (UseHlae)
            {
                if (!File.Exists(Config.Settings.HlaeExeFullPath))
                {
                    throw new ApplicationException("Unable to find the HLAE executable \"hlae.exe\" at \"" + Config.Settings.HlaeExeFullPath + "\". Go to Options\\Preferences and select the correct HLAE executable path.");
                }
            }
        }

        protected override void VerifyRunningPrograms()
        {
            // verify that steam is running
            if (Common.FindProcess("Steam", Config.Settings.SteamExeFullPath) == null)
            {
                throw new ApplicationException("Steam is not running. Launch Steam, log into your account and try again.");
            }

            // verify that HLAE is not running
            if (UseHlae)
            {
                if (Common.FindProcess("hlae", Config.Settings.HlaeExeFullPath) != null)
                {
                    throw new ApplicationException("Cannot play a demo with HLAE when it's already running. Exit HLAE and try again.");
                }
            }

            // verify that the game is not running
            Game game = GameManager.Find(Demo);

            // even if HLAE is being used, still need to check that game isn't already running
            if (game.UsesCommonFolder)
            {
                if (Demo.Engine == Demo.Engines.Source)
                {
                    processExeFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + game.FolderExtended + "\\";  
                }
                else
                {
                    processExeFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\common\\Half-Life\\";
                }
            }
            else
            {
                processExeFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder + "\\" + game.FolderExtended + "\\";                
            }

            processExeFullPath += game.ExecutableName;

            if (Common.FindProcess(Path.GetFileNameWithoutExtension(processExeFullPath), processExeFullPath) != null)
            {
                throw new ApplicationException("Cannot play a demo while \"" + game.Name + "\" is already running. Exit the game and try again.");
            }

            if (UseHlae)
            {
                processExeFullPath = Config.Settings.HlaeExeFullPath;
            }
        }

        protected override void LaunchProgram()
        {
            Game game = GameManager.Find(Demo);

            // calculate launch parameters
            launchParameters = (UseHlae ? "" : "-applaunch " + game.AppId + " ");

            if ((Config.Settings.PlaybackStartListenServer || Demo.GameFolderName == "tfc") && Demo.Engine != Demo.Engines.Source)
            {
                launchParameters += "-nomaster +maxplayers 10 +sv_lan 1 +map " + Demo.MapName + " ";
            }

            if (Demo.Engine == Demo.Engines.Source)
            {
                // skip intro videos
                // CS:GO quits unexpectedly if this is omitted
                launchParameters += "-novid ";

                if (Demo.GameFolderName == "tf")
                {
                    // show the console when loading the demo
                    launchParameters += "+toggleconsole ";
                }
            }

            launchParameters += "+exec " + Config.LaunchConfigFileName + " " + Config.Settings.SteamAdditionalLaunchParameters;

            // launch the program process
            if (UseHlae)
            {
                Hlae hlae = new Hlae();
                hlae.ReadConfig();
                hlae.WriteConfig(gameFullPath.Remove(gameFullPath.LastIndexOf('\\')) + "\\hl.exe", Demo.GameFolderName);

                ProcessStartInfo startInfo = new ProcessStartInfo(Config.Settings.HlaeExeFullPath);
                startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Config.Settings.HlaeExeFullPath);
                startInfo.Arguments = "-ipcremote";

                Process.Start(startInfo);
            }
            else
            {
                Process.Start(Config.Settings.SteamExeFullPath, launchParameters);
            }
        }
    }
}
