using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

namespace CDP.Core
{
    /// <summary>
    /// The plugin entry point.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// Represents a single column in the players listview.
        /// </summary>
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

        /// <summary>
        /// The full name of the plugin.
        /// </summary>
        /// <example>Quake III Arena</example>
        public abstract string FullName { get; }

        /// <summary>
        /// The short name of the plugin.
        /// </summary>
        /// <example>Q3A</example>
        public abstract string Name { get; }

        /// <summary>
        /// The plugin priority. Plugins with a higher priority decide whether a given file is a valid demo first.
        /// </summary>
        public abstract uint Priority { get; }

        /// <summary>
        /// File extensions of demos that correspond to this plugin.
        /// </summary>
        public abstract string[] FileExtensions { get; }

        /// <summary>
        /// Players listview columns.
        /// </summary>
        public abstract PlayerColumn[] PlayerColumns { get; }

        /// <summary>
        /// Settings used by this plugin. They don't have to be unique, other registered plugins can use the same settings, although the underlying setting types must match.
        /// </summary>
        public abstract Setting[] Settings { get; }

        /// <summary>
        /// Determines whether the given file is a valid demo for this plugin.
        /// </summary>
        /// <param name="stream">The file stream, with it's position set to the beginning.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>True if the file is a valid demo for this plugin, otherwise false.</returns>
        public abstract bool IsValidDemo(FastFileStreamBase stream, string fileExtension);

        /// <summary>
        /// Create a demo.
        /// </summary>
        /// <returns>A new demo.</returns>
        public abstract Demo CreateDemo();

        /// <summary>
        /// Create a launcher.
        /// </summary>
        /// <returns>A new launcher.</returns>
        public abstract Launcher CreateLauncher();

        /// <summary>
        /// Create an analysis view.
        /// </summary>
        /// <returns>A new analysis view as a user control.</returns>
        public abstract UserControl CreateAnalysisView();

        /// <summary>
        /// Create an analysis viewmodel.
        /// </summary>
        /// <param name="demo">The demo to be analysed.</param>
        /// <returns>A new analysis viewmodel.</returns>
        public abstract ViewModelBase CreateAnalysisViewModel(Demo demo);

        /// <summary>
        /// Create a settings view.
        /// </summary>
        /// <param name="demo">The currently selected demo.</param>
        /// <returns>A new settings view as a user control.</returns>
        public abstract UserControl CreateSettingsView(Demo demo);
    }
}
