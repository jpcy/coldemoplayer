using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics; // FileVersionInfo, Process
using System.IO; // FileInfo
using System.Threading;

namespace compLexity_Demo_Player
{
    public class CounterStrikeLauncher : Launcher
    {
        protected override void VerifyProgramSettings()
        {
            // verify cstrike.exe exists
            if (!File.Exists(Config.Settings.CstrikeExeFullPath))
            {
                throw new ApplicationException("Unable to find \"cstrike.exe\" at \"" + Config.Settings.CstrikeExeFullPath + "\". Go to Options\\Preferences and select the correct Counter-Strike path.");
            }

            // verify that the game folder exists
            gameFullPath = Path.GetDirectoryName(Config.Settings.CstrikeExeFullPath) + "\\" + Demo.GameFolderName;

            if (!Directory.Exists(gameFullPath))
            {
                throw new ApplicationException("Game folder \"" + gameFullPath + "\" doesn't exist. The game \"" + Demo.GameName + "\" doesn't seem to be installed.");
            }
        }

        protected override void VerifyRunningPrograms()
        {
            Debug.Assert(gameFullPath != null);

            // cstrike.exe is just a launcher, hl.exe is the real process
            processExeFullPath = Path.Combine(Path.GetDirectoryName(Config.Settings.CstrikeExeFullPath), "hl.exe");

            // verify that hl.exe is not running
            if (Common.FindProcess("hl", processExeFullPath) != null)
            {
                throw new ApplicationException("Cannot play a demo while a Counter-Strike game is already running. Exit the game and try again.");
            }
        }

        protected override void LaunchProgram()
        {
            // actually start hl.exe with -game, not cstrike.exe, because cstrike.exe won't pass along our launch parameters.
            String launchParameters = "-game " + Demo.GameFolderName + " +exec " + Config.LaunchConfigFileName;

            // the working directory must be set to the directory cstrike.exe resides in
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = processExeFullPath;
            psi.WorkingDirectory = Path.GetDirectoryName(Config.Settings.CstrikeExeFullPath);
            psi.Arguments = launchParameters + " " + Config.Settings.CstrikeAdditionalLaunchParameters;

            Process.Start(psi);
        }

        protected override void PreLaunch()
        {
            //CheckMapVersion();
        }
    }
}
