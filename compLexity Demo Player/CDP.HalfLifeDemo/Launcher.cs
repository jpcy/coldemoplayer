using System;
using System.IO;
using System.Diagnostics;

namespace CDP.HalfLifeDemo
{
    public class Launcher : Core.SteamLauncher
    {
        private readonly string configFileName = "coldemoplayer.cfg";
        private Demo demo;

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;

            if (this.demo.Game == null)
            {
                throw new InvalidOperationException("Cannot launch a Half-Life demo recorded with an unknown game.");
            }

            gameName = this.demo.Game.Name;
            appId = this.demo.Game.AppId;
            appFolder = this.demo.Game.AppFolder;
            gameFolder = this.demo.Game.ModFolder;

            if (!string.IsNullOrEmpty((string)settings["SteamExeFullPath"]) && !string.IsNullOrEmpty((string)settings["SteamAccountName"]))
            {
                processExecutableFileName = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"], appFolder, "hl.exe");
            }
        }

        public override bool Verify()
        {
            return base.Verify();
        }

        public override void Launch()
        {
            string launchParameters = string.Format("-applaunch {0}", appId);

            // TODO: check demo capabilities to see if starting a listen server is possible
            if ((bool)settings["HlStartListenServer"] == true)
            {
                launchParameters += string.Format(" -nomaster +maxplayers 10 +sv_lan 1 +map {0}", demo.MapName);
            }

            launchParameters += " +exec " + configFileName;

            if (!string.IsNullOrEmpty((string)settings["SteamAdditionalLaunchParameters"]))
            {
                launchParameters += " " + settings["SteamAdditionalLaunchParameters"];
            }

            Process.Start((string)settings["SteamExeFullPath"], launchParameters);
        }
    }
}
