using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Win32;
using CDP.Core.Extensions;

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
        string ProgramUserDataPath { get; }
        string ProgramDataPath { get; }
        string FileAssociationSettingPrefix { get; }

        void Add<T>(string key, T defaultValue);
        void Add<T>(string key, T defaultValue, bool isUrl);
        void Add(Setting setting);
        void Load(IDemoManager demoManager);
        void Save();
    }

    /// <remarks>
    /// A Setting with IsUrl set to true receives special treatment. 
    /// 
    /// This is so updates can make changes to the default URL value without having to either hard-code the URL, or clear the settings file when an update is installed. 
    /// 
    /// When writing the setting, if the value is unchanged from the default value a marker value "default" is written instead. When reading the setting, if the value is set to the marker value "default", the default value for the setting is used, otherwise the value of the setting is used.
    /// </remarks>
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
                return "{0}.{1}.{2}".Args(ProgramVersionMajor, ProgramVersionMinor, ProgramVersionUpdate);
            }
        }

        public string ComplexityUrl
        {
            get { return "http://www.complexitygaming.com/"; }
        }

        private readonly string programExeFullPath;
        private readonly string programPath;
        private readonly string programUserDataPath;
        private readonly string programDataPath;

        public string ProgramExeFullPath
        {
            get { return programExeFullPath; }
        }
        public string ProgramPath
        {
            get { return programPath; }
        }
        public string ProgramUserDataPath
        {
            get { return programUserDataPath; }
        }
        public string ProgramDataPath
        {
            get { return programDataPath; }
        }

        public string FileAssociationSettingPrefix
        {
            get { return fileAssociationSettingPrefix; }
        }

        public readonly List<Setting> definitions = new List<Setting>();
        public readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();
        private readonly string fileName = "settings.xml";
        private readonly string rootElement = "Settings";
        private readonly string fileAssociationSettingPrefix = "FileAssociation_";
        private readonly string useDefaultUrlMarker = "default";
        private bool IsLoaded = false;

        public Settings()
        {
            programUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProgramName);
            programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProgramName);

            if (!Directory.Exists(programUserDataPath))
            {
                Directory.CreateDirectory(programUserDataPath);
            }

            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            programExeFullPath = Environment.GetCommandLineArgs()[0];

            // Use the working directory in debug mode, otherwise use the directory that the executable is in.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                programPath = Environment.CurrentDirectory;
            }
            else
            {
                programPath = Path.GetDirectoryName(ProgramExeFullPath);
            }

            Add("UpdateUrl", "http://coldemoplayer.gittodachoppa.com/update115/", true);
            Add("MapsUrl", "http://coldemoplayer.gittodachoppa.com/maps/", true);
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

        public void Add<T>(string key, T defaultValue, bool isUrl)
        {
            Add(new Setting(key, typeof(T), defaultValue, isUrl));
        }

        public void Add(Setting setting)
        {
            if (IsLoaded)
            {
                throw new InvalidOperationException("Cannot add a setting after the settings file has been loaded.");
            }

            definitions.Add(setting);
        }

        public void Load(IDemoManager demoManager)
        {
            // Add plugin-specific settings.
            foreach (Setting setting in demoManager.GetAllPluginSettings())
            {
                Add(setting);
            }

            // Add file association settings
            foreach (string extension in demoManager.GetAllPluginFileExtensions())
            {
                Add(fileAssociationSettingPrefix + extension, false);
            }

            IsLoaded = true;
            string path = Path.Combine(ProgramUserDataPath, fileName);
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
                        if (setting.IsUrl && element.Value == useDefaultUrlMarker)
                        {
                            value = setting.DefaultValue;
                        }
                        else if (setting.Type == typeof(bool))
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
                            throw new ApplicationException("Unsupported setting type \"{0}\".".Args(setting.Type));
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
            using (XmlTextWriter xml = new XmlTextWriter(Path.Combine(ProgramUserDataPath, fileName), Encoding.Unicode))
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

                        if (setting.IsUrl && value == setting.DefaultValue)
                        {
                            xml.WriteValue(useDefaultUrlMarker);
                        }
                        else
                        {
                            xml.WriteValue(value.ToString());
                        }

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
