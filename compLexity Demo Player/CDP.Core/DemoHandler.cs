using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

namespace CDP.Core
{
    /// <summary>
    /// Represents a desciption for how a certain demo type should be handled by the demo player.
    /// </summary>
    public abstract class DemoHandler
    {
        public class PlayerColumn
        {
            public string Header { get; private set; }
            public string DisplayMemberBinding { get; private set; }

            public PlayerColumn(string header, string displayMemberBinding)
            {
                Header = header;
                DisplayMemberBinding = displayMemberBinding;
            }
        }

        public abstract string FullName { get; }
        public abstract string Name { get; }
        public abstract string[] Extensions { get; } // e.g. "dem".
        public abstract PlayerColumn[] PlayerColumns { get; }
        public abstract Setting[] Settings { get; }
        public abstract UserControl SettingsView { get; }

        public abstract bool IsValidDemo(FastFileStreamBase stream);
        public abstract Demo CreateDemo();
        public abstract Launcher CreateLauncher();
        public abstract UserControl CreateAnalysisView();
        public abstract ViewModelBase CreateAnalysisViewModel(Demo demo);
    }
}
