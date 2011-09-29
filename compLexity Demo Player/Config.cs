using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32; // Registry
using System.IO;
using System.Runtime.InteropServices; // DllImport
using System.Security.Permissions;
using System.Diagnostics; // ProcessPriorityClass
using JsonExSerializer;

namespace compLexity_Demo_Player
{
    public class ProgramSettings
    {
        public enum Playback
        {
            Playdemo,
            Viewdemo
        }

        // updating, on-demand map downloading
        public String UpdateUrl { get; set; }
        public String MapsUrl { get; set; }
        public Boolean AutoUpdate { get; set; }

        // last path/file
        public String LastPath { get; set; }
        public String LastFileName { get; set; }

        // window state
        public System.Windows.WindowState WindowState { get; set; }
        public Double WindowWidth { get; set; }
        public Double WindowHeight { get; set; }
        public Double ExplorerPaneWidth { get; set; }
        public Double DemoListPaneHeight { get; set; }

        // steam
        public String SteamExeFullPath { get; set; }
        public String SteamAccountFolder { get; set; }
        public String SteamAdditionalLaunchParameters { get; set; }

        // half-life
        public String HlExeFullPath { get; set; }
        public String HlAdditionalLaunchParameters { get; set; }

        // hlae
        public String HlaeExeFullPath { get; set; }

        // file/protocol association
        public Boolean AssociateWithDemFiles { get; set; }
        public Boolean AssociateWithHlswProtocol { get; set; }

        // playback
        public Playback PlaybackType { get; set; }
        public Boolean PlaybackRemoveShowscores { get; set; }
        public Boolean PlaybackRemoveFtb { get; set; }
        public Boolean PlaybackRemoveHltvAds { get; set; }
        public Boolean PlaybackRemoveHltvSlowMotion { get; set; }
        public Boolean PlaybackStartListenServer { get; set; }
        public Boolean PlaybackConvertNetworkProtocol { get; set; }
        public Boolean PlaybackCloseWhenFinished { get; set; }
        public Boolean PlaybackUseHlae { get; set; }
        public Boolean PlaybackRemoveWeaponAnimations { get; set; }

        // analysis
        public System.Windows.WindowState AnalysisWindowState { get; set; }

        // logging
        public Boolean LogMessageParsingErrors { get; set; }

        // tray
        public Boolean MinimizeToTray { get; set; }

        // game process priority
        public ProcessPriorityClass GameProcessPriority { get; set; }

        // server browser
        public System.Windows.WindowState ServerBrowserWindowState { get; set; }
        public Boolean ServerBrowserStartListenServer { get; set; }
        public Boolean ServerBrowserCloseWhenFinished { get; set; }
        public String[] ServerBrowserFavourites { get; set; }

        public ProgramSettings()
        {
            UpdateUrl = String.Empty;
            MapsUrl = String.Empty;
            AutoUpdate = true;
            WindowState = System.Windows.WindowState.Normal;
            WindowWidth = 800.0;
            WindowHeight = 600.0;
            ExplorerPaneWidth = 320.0;
            DemoListPaneHeight = 150.0;
            AssociateWithDemFiles = true;
            AssociateWithHlswProtocol = false;
            PlaybackType = Playback.Playdemo;
            PlaybackRemoveShowscores = true;
            PlaybackRemoveFtb = true;
            PlaybackRemoveHltvAds = false;
            PlaybackRemoveHltvSlowMotion = false;
            PlaybackStartListenServer = true;
            PlaybackConvertNetworkProtocol = true;
            PlaybackCloseWhenFinished = false;
            PlaybackUseHlae = false;
            PlaybackRemoveWeaponAnimations = false;
            AnalysisWindowState = System.Windows.WindowState.Normal;
            LogMessageParsingErrors = false;
            MinimizeToTray = false;
            GameProcessPriority = ProcessPriorityClass.Normal;
            ServerBrowserWindowState = System.Windows.WindowState.Normal;
            ServerBrowserStartListenServer = false;
            ServerBrowserCloseWhenFinished = false;
        }
    }

    public static class Config
    {
        public static String ProgramName
        {
            get
            {
                return "compLexity Demo Player";
            }
        }

        public static Int32 ProgramVersionMajor
        {
            get
            {
                return 1;
            }
        }

        public static Int32 ProgramVersionMinor
        {
            get
            {
                return 1;
            }
        }

        public static Int32 ProgramVersionUpdate
        {
            get
            {
                return 13;
            }
        }

        public static String ProgramVersion
        {
            get
            {
                return String.Format("{0}.{1}.{2}", ProgramVersionMajor, ProgramVersionMinor, ProgramVersionUpdate);
            }
        }

        public static String ComplexityUrl
        {
            get
            {
                return "http://www.complexitygaming.com/";
            }
        }

        public static String LaunchConfigFileName
        {
            get
            {
                return "coldemoplayer.cfg";
            }
        }

        public static String LaunchDemoFileName
        {
            get
            {
                return "coldemoplayer.dem";
            }
        }

        public static String UpdateUrl
        {
            get
            {
                if (String.IsNullOrEmpty(Settings.UpdateUrl))
                {
                    return "http://coldemoplayer.googlecode.com/svn/trunk/hosted/update/";
                }
                else
                {
                    return Settings.UpdateUrl;
                }
            }
        }

        public static String MapsUrl
        {
            get
            {
                if (String.IsNullOrEmpty(Settings.MapsUrl))
                {
                    return "http://coldemoplayer.googlecode.com/svn/trunk/hosted/maps/";
                }
                else
                {
                    return Settings.MapsUrl;
                }
            }
        }

        public static String ProgramExeFullPath { get; private set; }
        public static String ProgramPath { get; private set; }
        public static String ProgramDataPath { get; private set; }

        public static ProgramSettings Settings { get; private set; }

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);
        private const String fileName = "config.json";

        /// <summary>
        /// Reads the program configuration from the configuration file, if it exists. Otherwise, default values and information from the registry are used.
        /// </summary>
        /// <returns>True if the config file exists, False if it doesn't.</returns>
        public static Boolean Read()
        {
            ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + ProgramName;
            ProgramExeFullPath = Common.SanitisePath(Environment.GetCommandLineArgs()[0]);
#if DEBUG
            // Use the working directory when debugging. This is due to the Express edition of VS2008's limited debugging options.
            // The working directory should be set to the bin sub-directory (e.g. trunk\bin).
            ProgramPath = Environment.CurrentDirectory;
#else
            ProgramPath = Path.GetDirectoryName(ProgramExeFullPath);
#endif

            Settings = null;
            Boolean result = true;
            String configFullPath = ProgramDataPath + "\\" + fileName;

            if (File.Exists(configFullPath))
            {
                // deserialize
                try
                {
                    using (StreamReader stream = new StreamReader(configFullPath))
                    {
                        Serializer serializer = new Serializer(typeof(ProgramSettings));
                        Settings = (ProgramSettings)serializer.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    // Assume the file has been corrupted by the user or some other external influence. Log the exception as a warning, delete the file and use default config values.
                    Common.LogException(ex, true);
                    File.Delete(configFullPath);
                    Settings = null;
                }
            }

            if (Settings == null)
            {
                Settings = new ProgramSettings();
                result = false;
                ReadFromRegistry();
            }

            return result;
        }

        /// <summary>
        /// Writes the current program configuration to disk.
        /// </summary>
        public static void Write()
        {
            // The data path should have been created by the installer, but it's possible the installer wasn't used (e.g. portable copy on a removable drive).
            if (!Directory.Exists(ProgramDataPath))
            {
                Directory.CreateDirectory(ProgramDataPath);
            }

            using (StreamWriter stream = new StreamWriter(ProgramDataPath + "\\" + fileName))
            {
                Serializer serializer = new Serializer(typeof(ProgramSettings));
                serializer.Serialize(Settings, stream);
            }
        }

        /// <summary>
        /// Reads as much useful information as possible from the registry, such as Steam and Half-Life's install paths.
        /// </summary>
        private static void ReadFromRegistry()
        {
            // read SteamExe
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
            {
                if (key != null)
                {
                    Settings.SteamExeFullPath = (string)key.GetValue("SteamExe", "");

                    if (Settings.SteamExeFullPath == null)
                    {
                        Settings.SteamExeFullPath = "";
                    }
                    else
                    {
                        Settings.SteamExeFullPath = Common.SanitisePath(Settings.SteamExeFullPath);

                        // this registry value always seems to be lower case. replace steam.exe with Steam.exe
                        Settings.SteamExeFullPath = Settings.SteamExeFullPath.Replace("steam.exe", "Steam.exe");
                    }
                }
            }

            // check if SteamExe is valid (contains Steam.exe)
            if (File.Exists(Settings.SteamExeFullPath))
            {
                // Find an account name (first folder in "SteamApps" that isn't "common" or "SourceMods")
                // "common" created by peggle extreme, left 4 dead etc.
                DirectoryInfo steamAppsDirInfo = new DirectoryInfo(Path.GetDirectoryName(Settings.SteamExeFullPath) + "\\SteamApps");

                foreach (DirectoryInfo dirInfo in steamAppsDirInfo.GetDirectories())
                {
                    if (dirInfo.Name.ToLower() != "common" && dirInfo.Name.ToLower() != "sourcemods")
                    {
                        Settings.SteamAccountFolder = dirInfo.Name;
                        break;
                    }
                }
            }
            else
            {
                // bad steam exe path, make the user enter it manually
                Settings.SteamExeFullPath = "";
            }

            // read half-life folder path, add hl.exe to it
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Half-Life"))
            {
                if (key != null)
                {
                    Settings.HlExeFullPath = (String)key.GetValue("InstallPath");

                    if (Settings.HlExeFullPath == null)
                    {
                        Settings.HlExeFullPath = "";
                    }
                    else
                    {
                        Common.SanitisePath(Settings.HlExeFullPath);
                        Settings.HlExeFullPath += "\\hl.exe";

                        if (!File.Exists(Settings.HlExeFullPath))
                        {
                            // bad hl.exe path, make the user enter it manually
                            Settings.HlExeFullPath = "";
                        }
                    }
                }
            }
        }

        static public void AssociateWithDemFiles()
        {
#if !DEBUG
            Boolean refreshIcon = false;

            // create ".dem" entry
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(".dem"))
            {
                key.SetValue("", "compLexity Demo Player");
            }

            // create demo player entry
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("compLexity Demo Player"))
            {
                // set icon
                using (RegistryKey subkey = key.CreateSubKey("DefaultIcon"))
                {
                    if ((String)subkey.GetValue("") != Config.ProgramExeFullPath)
                    {
                        refreshIcon = true;
                    }
                    subkey.SetValue("", Config.ProgramExeFullPath);
                }

                // set open text
                using (RegistryKey subkey = key.CreateSubKey("Shell\\open"))
                {
                    subkey.SetValue("", "Open with compLexity Demo Player");
                }

                // set open command
                using (RegistryKey subkey = key.CreateSubKey("Shell\\open\\command"))
                {
                    String openCommand = "\"" + Config.ProgramExeFullPath + "\" \"%1\"";
                    subkey.SetValue("", openCommand);
                }
            }

            if (refreshIcon)
            {
                // SHCNE_ASSOCCHANGED = 0x08000000
                // SHCNF_IDLIST	= 0

                IntPtr p = new IntPtr();
                SHChangeNotify(0x08000000, 0, p, p);
            }
#endif
        }

        public static void AddHlswProtocolAssociation()
        {
            RemoveHlswProtocolAssociation();

            // create hlsw entry
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("hlsw"))
            {
                key.SetValue("", "URL:HLSW Protocol");
                key.SetValue("URL Protocol", "");

                using (RegistryKey subkey = key.CreateSubKey("DefaultIcon"))
                {
                    subkey.SetValue("", ProgramExeFullPath);
                }

                using (RegistryKey subkey = key.CreateSubKey("Shell\\open\\command"))
                {
                    String s = ProgramExeFullPath + " \"%1\"";
                    subkey.SetValue("", s);
                }
            }
        }

        public static void RemoveHlswProtocolAssociation()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("hlsw", true))
            {
                if (key != null)
                {
                    Registry.ClassesRoot.DeleteSubKeyTree("hlsw");
                }
            }
        }
    }
}
