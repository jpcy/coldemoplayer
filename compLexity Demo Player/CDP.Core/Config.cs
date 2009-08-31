using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace CDP.Core
{
    public class Config
    {
        public static Config Instance
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
        public Dictionary<string, object> DemoSettings { get; private set; }

        private static readonly Config instance = new Config();

        private readonly string demoSettingsFileName = "demosettings.xml";
        private readonly string demoSettingsRootElement = "DemoSettings";

        private Config()
        {
            ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProgramName);
            ProgramExeFullPath = Environment.GetCommandLineArgs()[0];
            ProgramPath = Path.GetDirectoryName(ProgramExeFullPath);
#if DEBUG
            // BLEH: this is what happens when you can't use macros in setting the debug working directory.
            // Remove the last four folder names from the path, e.g. "\compLexity Demo Player\CDP\bin\Debug" to "\bin".
            ProgramPath = Path.GetFullPath("../../../../bin");
#endif
            DemoSettings = new Dictionary<string, object>();
        }

        public void LoadDemoSettings(DemoHandler.Setting[] settings)
        {
            string fileName = Path.Combine(ProgramDataPath, demoSettingsFileName);
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

                DemoSettings.Add(setting.Key, value);
            }
        }

        public void SaveDemoSettings()
        {
            using (XmlTextWriter xml = new XmlTextWriter(Path.Combine(ProgramDataPath, demoSettingsFileName), Encoding.ASCII))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement(demoSettingsRootElement);

                foreach (KeyValuePair<string, object> setting in DemoSettings)
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
