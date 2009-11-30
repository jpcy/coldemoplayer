using System;
using System.IO;
using System.Diagnostics;

namespace CDP.HalfLife
{
    public class Launcher : Core.SteamLauncher
    {
        private readonly string configFileName = "coldemoplayer.cfg";
        private readonly string demoFileName = "coldemoplayer.dem";
        protected Demo demo;

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

        public override string CalculateDestinationFileName()
        {
            return fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"], appFolder, gameFolder, demoFileName);
        }

        public override bool Verify()
        {
            return base.Verify();
        }

        public override void Launch()
        {
            // Write config file.
            using (StreamWriter stream = File.CreateText(fileSystem.PathCombine(Path.GetDirectoryName((string)settings["SteamExeFullPath"]), "SteamApps", (string)settings["SteamAccountName"], appFolder, gameFolder, configFileName)))
            {
                stream.WriteLine("alias +col_ff_slow \"host_framerate 0.01; alias col_pause col_pause1\"");
                stream.WriteLine("alias -col_ff_slow \"host_framerate 0\"");
                stream.WriteLine("alias +col_ff_fast \"host_framerate 0.1; alias col_pause col_pause1\"");
                stream.WriteLine("alias -col_ff_fast \"host_framerate 0\"");
                stream.WriteLine("alias +col_slowmo \"host_framerate 0.001; alias col_pause col_pause1\"");
                stream.WriteLine("alias -col_slowmo \"host_framerate 0\"");
                stream.WriteLine("alias col_pause1 \"host_framerate 0.000000001; alias col_pause col_pause2\"");
                stream.WriteLine("alias col_pause2 \"host_framerate 0; alias col_pause col_pause1\"");
                stream.WriteLine("alias col_pause col_pause1\n");

                stream.WriteLine("alias wait10 \"wait; wait; wait; wait; wait; wait; wait; wait; wait; wait\"\n");
                stream.WriteLine("brightness 2");
                stream.WriteLine("gamma 3");
                stream.WriteLine("wait10");
                stream.WriteLine("sv_voicecodec voice_speex");
                stream.WriteLine("sv_voicequality 5");
                stream.WriteLine("wait10");
                stream.WriteLine("wait10");
                stream.WriteLine("wait10");
                stream.WriteLine("wait10");
                stream.WriteLine("wait10");

                string playbackType = ((Plugin.PlaybackMethods)settings["HlPlaybackMethod"] == Plugin.PlaybackMethods.Playdemo ? "playdemo" : "viewdemo");

                stream.WriteLine("{0} {1}", playbackType, demoFileName);
                stream.WriteLine("wait10");
                stream.WriteLine("slot1");

                // TODO: remove this, figure out why the spec menu it's initalised correctly with old converted HLTV demos.
                if (demo.Perspective != "POV" && demo.NetworkProtocol < 47)
                {
                    stream.WriteLine("wait10");
                    stream.WriteLine("spec_menu 0");
                    stream.WriteLine("wait10");
                    stream.WriteLine("spec_mode 4");
                    stream.WriteLine("wait10");
                    stream.WriteLine("wait10");
                    stream.WriteLine("wait10");
                    stream.WriteLine("spec_menu 1");
                    stream.WriteLine("wait10");
                    stream.WriteLine("+attack");
                }

                stream.WriteLine("echo \"\"");
                stream.WriteLine("echo \"==========================\"");
                stream.WriteLine("echo \"{0}\"", settings.ProgramName);
                stream.WriteLine("echo \"==========================\"");
                stream.WriteLine("echo \"{0}\"", Strings.GameConfigAliases);
                stream.WriteLine("echo \"  +col_ff_slow ({0})\"", Strings.GameConfigFastForward);
                stream.WriteLine("echo \"  +col_ff_fast ({0})\"", Strings.GameConfigFasterFastForward);
                stream.WriteLine("echo \"  +col_slowmo ({0})\"", Strings.GameConfigSlowMotion);
                stream.WriteLine("echo \"  col_pause ({0})\"", Strings.GameConfigTogglePause);
                stream.WriteLine("echo \"\"");
                stream.WriteLine("echo \"{0}\"", string.Format(Strings.GameConfigPlayingMessage, demo.Name));
                // TODO: duration, recorded by.
                stream.WriteLine("echo \"\"");
            }

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
