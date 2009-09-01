using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CDP.Core
{
    public class Settings
    {
        public class MainConfig
        {
            // Updating, on-demand map downloading.
            public string UpdateUrl { get; set; }
            public string MapsUrl { get; set; }
            public bool AutoUpdate { get; set; }

            // Last path/file.
            public string LastPath { get; set; }
            public string LastFileName { get; set; }

            // Steam.
            public string SteamExeFullPath { get; set; }
            public string SteamAccountFolder { get; set; }
            public string SteamAdditionalLaunchParameters { get; set; }

            public MainConfig()
            {
                UpdateUrl = "http://coldemoplayer.gittodachoppa.com/update115/";
                MapsUrl = "http://coldemoplayer.gittodachoppa.com/maps/";
                AutoUpdate = true;
            }
        }

        public static Settings Instance
        {
            get { return instance; }
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

        public string ProgramExeFullPath { get; private set; }
        public string ProgramPath { get; private set; }
        public string ProgramDataPath { get; private set; }
        public MainConfig Main { get; private set; }
        public Dictionary<string, object> Demo { get; private set; }

        private static readonly Settings instance = new Settings();
        private readonly string mainConfigFileName = "mainconfig.xml";
        private readonly string demoConfigFileName = "democonfig.xml";
        private readonly string demoConfigRootElement = "DemoConfig";

        private Settings()
        {
            ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProgramName);
            ProgramExeFullPath = Environment.GetCommandLineArgs()[0];
            ProgramPath = Path.GetDirectoryName(ProgramExeFullPath);
#if DEBUG
            // BLEH: this is what happens when you can't use macros in setting the debug working directory.
            // Remove the last four folder names from the path, e.g. "\compLexity Demo Player\CDP\bin\Debug" to "\bin".
            ProgramPath = Path.GetFullPath("../../../../bin");
#endif
            Demo = new Dictionary<string, object>();
        }

        public void LoadMainConfig()
        {
            string fileName = Path.Combine(ProgramDataPath, mainConfigFileName);

            if (!File.Exists(fileName))
            {
                Main = new MainConfig();
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MainConfig));

                using (StreamReader stream = new StreamReader(fileName))
                {
                    Main = (MainConfig)serializer.Deserialize(stream);
                }
            }
        }

        public void SaveMainConfig()
        {
            string fileName = Path.Combine(ProgramDataPath, mainConfigFileName);
            XmlSerializer serializer = new XmlSerializer(typeof(MainConfig));

            using (StreamWriter stream = new StreamWriter(fileName))
            {
                serializer.Serialize(stream, Main);
            }
        }

        public void LoadDemoConfig(DemoHandler.Setting[] settings)
        {
            string fileName = Path.Combine(ProgramDataPath, demoConfigFileName);
            XDocument xml = null;

            if (File.Exists(fileName))
            {
                xml = XDocument.Load(fileName);
            }

            foreach (DemoHandler.Setting setting in settings)
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
                        else
                        {
                            throw new ApplicationException(string.Format("Unsupported demo setting type \"{0}\".", setting.Type));
                        }
                    }
                }

                Demo.Add(setting.Key, value);
            }
        }

        public void SaveDemoConfig()
        {
            using (XmlTextWriter xml = new XmlTextWriter(Path.Combine(ProgramDataPath, demoConfigFileName), Encoding.ASCII))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement(demoConfigRootElement);

                foreach (KeyValuePair<string, object> setting in Demo)
                {
                    xml.WriteStartElement(setting.Key);
                    xml.WriteValue(setting.Value.ToString());
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }
    }
}
