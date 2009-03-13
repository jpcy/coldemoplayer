using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics; // FileVersionInfo, Process
using System.IO; // FileInfo
using System.Threading;

namespace compLexity_Demo_Player
{
    public class HalfLifeLauncher : Launcher
    {
        protected override void VerifyProgramSettings()
        {
            // verify hl.exe exists
            if (!File.Exists(Config.Settings.HlExeFullPath))
            {
                throw new ApplicationException("Unable to find \"hl.exe\" at \"" + Config.Settings.HlExeFullPath + "\". Go to Options\\Preferences and select the correct Half-Life path.");
            }

            // verify that the game folder exists
            gameFullPath = Path.GetDirectoryName(Config.Settings.HlExeFullPath) + "\\" + Demo.GameFolderName;

            if (!Directory.Exists(gameFullPath))
            {
                throw new ApplicationException("Game folder \"" + gameFullPath + "\" doesn't exist. The game \"" + Demo.GameName + "\" doesn't seem to be installed.");
            }
        }

        protected override void VerifyRunningPrograms()
        {
            Debug.Assert(gameFullPath != null);

            // verify that hl.exe is not running
            if (Common.FindProcess("hl", Config.Settings.HlExeFullPath) != null)
            {
                throw new ApplicationException("Cannot play a demo while a Half-Life engine game is already running. Exit the game and try again.");
            }

            processExeFullPath = Config.Settings.HlExeFullPath;
        }

        protected override void LaunchProgram()
        {
            String launchParameters = "-console -toconsole -game " + Demo.GameFolderName + " +exec " + Config.LaunchConfigFileName;

            // start hl.exe
            Process.Start(Config.Settings.HlExeFullPath, launchParameters + " " + Config.Settings.HlAdditionalLaunchParameters);
        }

        protected override void PreLaunch()
        {
            //CheckMapVersion();
        }
    }
}
