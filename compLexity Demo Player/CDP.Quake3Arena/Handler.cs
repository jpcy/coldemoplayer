using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using CDP.IdTech3;
using CDP.IdTech3.Commands;
using System.Xml.Serialization;

namespace CDP.Quake3Arena
{
    public class Handler : IdTech3.Handler
    {
        public override string FullName
        {
            get { return "Quake III Arena"; }
        }

        public override string Name
        {
            get { return "Q3A"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dm3", "dm_48", "dm_66", "dm_67", "dm_68", "dm_73" }; }
        }

        public override Core.Setting[] Settings
        {
            get
            {
                return new Core.Setting[]
                {
                    new Core.Setting("Quake3ExeFullPath", typeof(string), string.Empty),
                    new Core.Setting("Quake3ConvertProtocol", typeof(bool), true)
                };
            }
        }

        private UserControl settingsView;
        public override UserControl SettingsView
        {
            get
            {
                if (settingsView == null)
                {
                    settingsView = new SettingsView
                    {
                        DataContext = new SettingsViewModel(config.ExecutableFileNames)
                    };
                }

                return settingsView;
            }
        }

        private Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private Config config;

        public Handler()
        {
            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "idtech3", "quake3arena.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Config));
                config = (Config)serializer.Deserialize(stream);
            }
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            return new Launcher();
        }

        public override UserControl CreateAnalysisView()
        {
            return new Analysis.View();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new Analysis.ViewModel((Demo)demo);
        }

        public Mod FindMod(string modFolder)
        {
            return config.Mods.FirstOrDefault(m => m.Folder == modFolder);
        }
    }
}
