using System;
using System.IO;
using System.Diagnostics;

namespace CDP.HalfLifeDemo
{
    public class Launcher : Core.SteamLauncher
    {
        private readonly string configFileName = "coldemoplayer.cfg";
        private Demo demo;

        public Launcher(Demo demo)
            : this(demo, new Core.FileSystem())
        {
        }

        public Launcher(Demo demo, Core.IFileSystem fileSystem) : base(fileSystem)
        {
            this.demo = demo;

            if (demo.Game == null)
            {
                throw new InvalidOperationException("Cannot launch a Half-Life demo recorded with an unknown game.");
            }

            gameName = demo.Game.Name;
            appId = demo.Game.AppId;
            appFolder = demo.Game.AppFolder;
            gameFolder = demo.Game.ModFolder;

            Core.Settings settings = Core.Settings.Instance;

            if (!string.IsNullOrEmpty(settings.Main.SteamExeFullPath) && !string.IsNullOrEmpty(settings.Main.SteamAccountName))
            {
                processExecutableFileName = fileSystem.PathCombine(Path.GetDirectoryName(settings.Main.SteamExeFullPath), "SteamApps", settings.Main.SteamAccountName, appFolder, "hl.exe");
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
            if ((bool)Core.Settings.Instance.Demo["HlStartListenServer"] == true)
            {
                launchParameters += string.Format(" -nomaster +maxplayers 10 +sv_lan 1 +map {0}", demo.MapName);
            }

            launchParameters += " +exec " + configFileName;

            if (!string.IsNullOrEmpty(Core.Settings.Instance.Main.SteamAdditionalLaunchParameters))
            {
                launchParameters += " " + Core.Settings.Instance.Main.SteamAdditionalLaunchParameters;
            }

            Process.Start(Core.Settings.Instance.Main.SteamExeFullPath, launchParameters);
        }
    }
}
