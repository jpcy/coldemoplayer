using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CDP.CounterStrike
{
    public class Plugin : HalfLife.Plugin
    {
        public override string FullName
        {
            get { return "Counter-Strike"; }
        }

        public override string Name
        {
            get { return "cstrike"; }
        }

        public override uint Priority
        {
            get { return 1; }
        }

        public override Core.Setting[] Settings
        {
            get
            {
                return new Core.Setting[]
                {
                    new Core.Setting("CsRemoveFadeToBlack", typeof(bool), true)
                };
            }
        }

        private UserControl settingsView;
        private HalfLife.Game game;

        public Plugin()
        {
            game = games.Single(g => g.ModFolder == "cstrike");
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (!base.IsValidDemo(stream, fileExtension))
            {
                return false;
            }

            // magic (8 bytes) + demo protocol (4 bytes) + network protocol (4 bytes) + map name (260 bytes) = 276
            // game folder = 260
            if (stream.BytesLeft < 276 + 260)
            {
                return false;
            }

            stream.Seek(276, SeekOrigin.Begin);
            return game.DemoGameFolders.Contains(stream.ReadString(), StringComparer.InvariantCultureIgnoreCase);
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            return new HalfLife.Launcher();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new Analysis.ViewModel((Demo)demo);
        }

        public override UserControl CreateSettingsView(Core.Demo demo)
        {
            if (settingsView == null)
            {
                settingsView = new SettingsView { DataContext = new SettingsViewModel() };
            }

            return settingsView;
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            RegisterEngineMessage<Messages.SvcClientData>();
            RegisterUserMessage<UserMessages.ClCorpse>();
            RegisterUserMessage<UserMessages.SendAudio>();
        }
    }
}
