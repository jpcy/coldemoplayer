using System.Linq;
using System.IO;
using System.Windows.Controls;
using JsonExSerializer;

namespace CDP.Quake3Arena
{
    public class Plugin : IdTech3.Plugin
    {
        public override string FullName
        {
            get { return "Quake III Arena"; }
        }

        public override string Name
        {
            get { return "Q3A"; }
        }

        public override string[] FileExtensions
        {
            get { return new string[] { "dm3", "dm_48", "dm_66", "dm_67", "dm_68" }; }
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
        private Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private Config config;

        public Plugin()
        {
            ReadConfig();
        }

        protected virtual void ReadConfig()
        {
            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "idtech3", "quake3arena.json")))
            {
                Serializer serializer = new Serializer(typeof(Config));
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

        public override UserControl CreateSettingsView(Core.Demo demo)
        {
            if (settingsView == null)
            {
                settingsView = new SettingsView
                {
                    DataContext = new SettingsViewModel(config.Executables)
                };
            }

            return settingsView;
        }

        public virtual Mod FindMod(string modFolder)
        {
            return config.Mods.FirstOrDefault(m => m.Folder == modFolder);
        }
    }
}
