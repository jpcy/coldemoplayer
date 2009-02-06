using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32; // Registry
using System.IO;
using System.Runtime.InteropServices; // DllImport
using System.Security.Permissions;
using System.Diagnostics; // ProcessPriorityClass

namespace compLexity_Demo_Player
{
    public class ProgramSettings
    {
        public enum Playback
        {
            Playdemo,
            Viewdemo
        }

        [XmlIgnore]
        public readonly String ProgramName = "compLexity Demo Player";
        [XmlIgnore]
        public readonly Int32 ProgramVersionMajor = 1;
        [XmlIgnore]
        public readonly Int32 ProgramVersionMinor = 1;
        [XmlIgnore]
        public readonly Int32 ProgramVersionUpdate = 5;
        [XmlIgnore]
        public String ProgramVersion
        {
            get
            {
                return String.Format("{0}.{1}.{2}", ProgramVersionMajor, ProgramVersionMinor, ProgramVersionUpdate);
            }
        }
        [XmlIgnore]
        public readonly String ComplexityUrl = "http://www.complexitygaming.com/";

        [XmlIgnore]
        public readonly String LaunchConfigFileName = "coldemoplayer.cfg";
        [XmlIgnore]
        public readonly String LaunchDemoFileName = "coldemoplayer.dem";

        [XmlIgnore]
        public String ProgramExeFullPath = "";
        [XmlIgnore]
        public String ProgramPath = "";
        [XmlIgnore]
        public String ProgramDataPath = "";

        // updating, on-demand map downloading
        public String UpdateUrl = "http://coldemoplayer.gittodachoppa.com/update115/";
        public String MapsUrl = "http://coldemoplayer.gittodachoppa.com/maps/";
        public Boolean AutoUpdate = true;

        // last path/file
        public String LastPath = "";
        public String LastFileName = "";

        // window state
        public System.Windows.WindowState WindowState = System.Windows.WindowState.Normal;
        public Double WindowWidth = 800.0;
        public Double WindowHeight = 600.0;
        public Double ExplorerPaneWidth = 320.0;
        public Double DemoListPaneHeight = 150.0;

        // steam
        public String SteamExeFullPath = "";
        public String SteamAccountFolder = "";
        public String SteamAdditionalLaunchParameters = "";

        // half-life
        public String HlExeFullPath = "";
        public String HlAdditionalLaunchParameters = "";

        // hlae
        public String HlaeExeFullPath = "";

        // file/protocol association
        public Boolean AssociateWithDemFiles = true;
        public Boolean AssociateWithHlswProtocol = false;

        // playback
        public Playback PlaybackType = Playback.Playdemo;
        public Boolean PlaybackRemoveShowscores = true;
        public Boolean PlaybackRemoveFtb = true;
        public Boolean PlaybackStartListenServer = true;
        public Boolean PlaybackConvertNetworkProtocol = true;
        public Boolean PlaybackCloseWhenFinished = false;
        public Boolean PlaybackUseHlae = false;

        // analysis
        public System.Windows.WindowState AnalysisWindowState = System.Windows.WindowState.Normal;

        // logging
        public Boolean LogMessageParsingErrors = false;

        // tray
        public Boolean MinimizeToTray = false;

        // game process priority
        public ProcessPriorityClass GameProcessPriority = ProcessPriorityClass.Normal;

        // server browser
        public System.Windows.WindowState ServerBrowserWindowState = System.Windows.WindowState.Normal;
        public Boolean ServerBrowserConvertTimeZone = true;
        public Boolean ServerBrowserStartListenServer = false;
        public Boolean ServerBrowserCloseWhenFinished = false;
        public String ServerBrowserLastGotfragGame = "Counter-Strike 1.6";
        public String[] ServerBrowserFavourites = null;
    }

    public static class Config
    {
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);

        private const String fileName = "config.xml";
        public static ProgramSettings Settings = null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the config file exists, False if it doesn't.</returns>
        public static Boolean Read()
        {
            Boolean result = true;
            Settings = new ProgramSettings();
            
            // XmlIgnore doesn't seem to apply to deserialization...
            String programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Settings.ProgramName;
            String configFullPath = programDataPath + "\\" + fileName;

            if (File.Exists(configFullPath))
            {
                // deserialize
                Settings = (ProgramSettings)Common.XmlFileDeserialize(configFullPath, typeof(ProgramSettings));
            }
            else
            {
                result = false;
                ReadFromRegistry();
            }

            Settings.ProgramDataPath = programDataPath;
            Settings.ProgramExeFullPath = Common.SanitisePath(Environment.GetCommandLineArgs()[0]);
#if DEBUG
            // BLEH: this is what happens when you can't use macros in setting the debug working directory.
            Settings.ProgramPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            for (Int32 i = 0; i < 3; i++)
            {
                Int32 lastSeparatorIndex = Settings.ProgramPath.LastIndexOf("\\");
                Settings.ProgramPath = Settings.ProgramPath.Remove(lastSeparatorIndex, Settings.ProgramPath.Length - lastSeparatorIndex);
            }

            Settings.ProgramPath += "\\bin";
#else
            Settings.ProgramPath = Path.GetDirectoryName(Settings.ProgramExeFullPath);
#endif

            return result;
        }

        public static void Write()
        {
            Common.XmlFileSerialize(Settings.ProgramDataPath + "\\" + fileName, Settings, typeof(ProgramSettings));
        }

        /// <summary>
        /// Called by Read if config file doesn't exist. Tries to fill in as much information as possible such as Steam and Half-Life's install paths, by reading from the registry.
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
                    if ((String)subkey.GetValue("") != Config.Settings.ProgramExeFullPath)
                    {
                        refreshIcon = true;
                    }
                    subkey.SetValue("", Config.Settings.ProgramExeFullPath);
                }

                // set open text
                using (RegistryKey subkey = key.CreateSubKey("Shell\\open"))
                {
                    subkey.SetValue("", "Open with compLexity Demo Player");
                }

                // set open command
                using (RegistryKey subkey = key.CreateSubKey("Shell\\open\\command"))
                {
                    String openCommand = "\"" + Config.Settings.ProgramExeFullPath + "\" \"%1\"";
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
                    subkey.SetValue("", Settings.ProgramExeFullPath);
                }

                using (RegistryKey subkey = key.CreateSubKey("Shell\\open\\command"))
                {
                    String s = Settings.ProgramExeFullPath + " \"%1\"";
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
