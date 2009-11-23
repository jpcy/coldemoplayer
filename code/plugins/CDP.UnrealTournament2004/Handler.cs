using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using CDP.Core;

namespace CDP.UnrealTournament2004
{
    public class Handler : Core.DemoHandler
    {
        public override string FullName
        {
            get { return "Unreal Tournament 2004"; }
        }

        public override string Name
        {
            get { return "UT2004"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "demo4" }; }
        }

        public override DemoHandler.PlayerColumn[] PlayerColumns
        {
            get { return null; }
        }

        public override Setting[] Settings
        {
            get
            {
                return new Setting[]
                {
                    new Setting("Ut2004ExeFullPath", typeof(string), string.Empty)
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
                        DataContext = new SettingsViewModel()
                    };
                }

                return settingsView;
            }
        }

        private readonly string magic = "UT2004 DEMO FILE";

        public override bool IsValidDemo(FastFileStreamBase stream, string fileExtension)
        {
            if (stream.Length < magic.Length)
            {
                return false;
            }

            for (int i = 0; i < magic.Length; i++)
            {
                if (stream.ReadChar() != magic[i])
                {
                    return false;
                }
            }

            return true;
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
            throw new NotImplementedException();
        }

        public override ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            throw new NotImplementedException();
        }
    }
}
