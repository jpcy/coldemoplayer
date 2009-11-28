using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace CDP.Core
{
    public interface ISettings
    {
        object this[string key] { get; set; }
        string ProgramName { get; }
        int ProgramVersionMajor { get; }
        int ProgramVersionMinor { get; }
        int ProgramVersionUpdate { get; }
        string ProgramVersion { get; }
        string ComplexityUrl { get; }
        string ProgramExeFullPath { get; }
        string ProgramPath { get; }
        string ProgramDataPath { get; }

        void Add<T>(string key, T defaultValue);
        void Add(Setting setting);
        void Load();
        void Save();
    }

    [Singleton]
    public class Settings : ISettings
    {
        public object this[string key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                dictionary[key] = value;
            }
        }

        public string ProgramName
        {
            get { return "compLexity Demo Player"; }
        }

        public int ProgramVersionMajor
        {
            get { return 2; }
        }

        public int ProgramVersionMinor
        {
            get { return 0; }
        }

        public int ProgramVersionUpdate
        {
            get { return 0; }
        }

        public string ProgramVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}", ProgramVersionMajor, ProgramVersionMinor, ProgramVersionUpdate);
            }
        }

        public string ComplexityUrl
        {
            get { return "http://www.complexitygaming.com/"; }
        }

        private readonly string programExeFullPath;
        private readonly string programPath;
        private readonly string programDataPath;

        public string ProgramExeFullPath
        {
            get { return programExeFullPath; }
        }
        public string ProgramPath
        {
            get { return programPath; }
        }
        public string ProgramDataPath
        {
            get { return programDataPath; }
        }

        public readonly List<Setting> definitions = new List<Setting>();
        public readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();
        private readonly string fileName = "settings.xml";
        private readonly string rootElement = "Settings";
        private bool IsLoaded = false;

        public Settings()
        {
            programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProgramName);

            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            programExeFullPath = Environment.GetCommandLineArgs()[0];
            programPath = Path.GetDirectoryName(ProgramExeFullPath);

            // BLEH: this is what happens when you can't use macros in setting the debug working directory.
            // Remove the last four folder names from the path, e.g. "\compLexity Demo Player\CDP\bin\Debug" to "\bin".
            if (System.Diagnostics.Debugger.IsAttached)
            {
                programPath = Path.GetFullPath("../../../../../bin");
            }

            Add("UpdateUrl", "http://coldemoplayer.gittodachoppa.com/update115/");
            Add("MapsUrl", "http://coldemoplayer.gittodachoppa.com/maps/");
            Add("AutoUpdate", true);
            Add("LastPath", string.Empty);
            Add("LastFileName", string.Empty);
            Add("SteamExeFullPath", string.Empty);
            Add("SteamAccountName", string.Empty);
        }

        public void Add<T>(string key, T defaultValue)
        {
            Add(new Setting(key, typeof(T), defaultValue));
        }

        public void Add(Setting setting)
        {
            if (IsLoaded)
            {
                throw new InvalidOperationException("Cannot add a setting after the settings file has been loaded.");
            }

            definitions.Add(setting);
        }

        public void Load()
        {
            IsLoaded = true;
            string path = Path.Combine(ProgramDataPath, fileName);
            XDocument xml = null;

            if (File.Exists(path))
            {
                try
                {
                    xml = XDocument.Load(path);
                }
                catch (Exception ex)
                {
                    new ErrorReporter().LogWarning(null, ex);
                    xml = null;
                }
            }

            foreach (Setting setting in definitions)
            {
                object value = setting.DefaultValue;

                if (xml != null)
                {
                    XElement element = xml.Root.Elements().SingleOrDefault(x => x.Name == setting.Key);

                    if (element != null)
                    {
                        if (setting.Type == typeof(bool))
                        {
                            value = bool.Parse(element.Value);
                        }
                        else if (setting.Type.IsEnum)
                        {
                            value = Enum.Parse(setting.Type, element.Value);
                        }
                        else if (setting.Type == typeof(string))
                        {
                            value = element.Value;
                        }
                        else
                        {
                            throw new ApplicationException(string.Format("Unsupported setting type \"{0}\".", setting.Type));
                        }
                    }
                }

                dictionary.Add(setting.Key, value);
            }

            if (xml == null)
            {
                PopulateDefaults();
            }
        }

        public void Save()
        {
            using (XmlTextWriter xml = new XmlTextWriter(Path.Combine(ProgramDataPath, fileName), Encoding.Unicode))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement(rootElement);

                foreach (Setting setting in definitions)
                {
                    object value = dictionary[setting.Key];

                    if (value != null)
                    {
                        xml.WriteStartElement(setting.Key);
                        xml.WriteValue(value.ToString());
                        xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }

        /// <summary>
        /// Populate as many default settings as possible with values read from the registry.
        /// </summary>
        private void PopulateDefaults()
        {
            // Steam.
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam"))
            {
                if (key != null)
                {
                    // Read SteamExeFullPath.
                    string steamExeFullPath = (string)key.GetValue("SteamExe");

                    if (steamExeFullPath != null && File.Exists(steamExeFullPath))
                    {
                        dictionary["SteamExeFullPath"] = steamExeFullPath;
                    }

                    // Try to guess the Steam account.
                    string steamPath = (string)key.GetValue("SteamPath");

                    if (steamPath != null && Directory.Exists(steamPath))
                    {
                        string[] invalidSteamAppFolders =
                        {
                            "common",
                            "media",
                            "sourcemods"
                        };

                        DirectoryInfo steamAccount = new DirectoryInfo(Path.Combine(steamPath, "SteamApps")).GetDirectories().FirstOrDefault(di => !invalidSteamAppFolders.Contains(di.Name.ToLower()));

                        if (steamAccount != null)
                        {
                            dictionary["SteamAccountName"] = steamAccount.Name;
                        }
                    }
                }
            }
        }
    }
}
