using System.IO;
using System.Windows.Controls;
using JsonExSerializer;
using CDP.Core;
using CDP.Quake3Arena;

namespace CDP.QuakeLive
{
    public class Plugin : Quake3Arena.Plugin
    {
        public override string FullName
        {
            get { return "Quake Live"; }
        }

        public override string Name
        {
            get { return "QL"; }
        }

        public override string[] FileExtensions
        {
            get { return new string[] { "dm_73" }; }
        }

        public override Setting[] Settings
        {
            get
            {
                return new Setting[]
                {
                    new Setting("QuakeLiveExeFullPath", typeof(string), string.Empty)
                };
            }
        }

        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private UserControl settingsView;
        private Config config;

        protected override void ReadConfig()
        {
            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "idtech3", "quakelive.json")))
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

        public override Mod FindMod(string modFolder)
        {
            return null;
        }
    }
}
