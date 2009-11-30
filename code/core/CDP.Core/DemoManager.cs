using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace CDP.Core
{
    public interface IDemoManager
    {
        /// <summary>
        /// Register a plugin with the demo manager.
        /// </summary>
        /// <param name="plugin">The plugin to register.</param>
        void RegisterPlugin(Plugin plugin);

        /// <summary>
        /// Returns all of the settings of all registered plugins (without duplicates).
        /// </summary>
        /// <returns>An array of unique settings.</returns>
        Setting[] GetAllPluginSettings();

        /// <summary>
        /// Return all of the demo file extensions of all registered plugins (without duplicates).
        /// </summary>
        /// <returns>An array of demo file extensions.</returns>
        string[] GetAllPluginFileExtensions();

        /// <summary>
        /// Get the names of all plugins that handle the given file extension.
        /// </summary>
        /// <param name="extension">The demo file extension.</param>
        /// <returns>An array of the full names of plugins that match the extension.</returns>
        string[] GetPluginNames(string extension);

        /// <summary>
        /// Create a demo.
        /// </summary>
        /// <param name="fileName">The file name of the demo to create.</param>
        /// <returns>A new demo.</returns>
        Demo CreateDemo(string fileName);

        /// <summary>
        /// Create a launcher for the specified demo.
        /// </summary>
        /// <param name="demo">The demo to be launched by the returned launcher object.</param>
        /// <returns>A new launcher.</returns>
        Launcher CreateLauncher(Demo demo);
    }

    [Singleton]
    public class DemoManager : IDemoManager
    {
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly List<Plugin> plugins = new List<Plugin>();

        public void RegisterPlugin(Plugin plugin)
        {
            plugins.Add(plugin);
        }

        public Setting[] GetAllPluginSettings()
        {
            List<Setting> settings = new List<Setting>();

            foreach (Plugin plugin in plugins)
            {
                if (plugin.Settings != null)
                {
                    settings.AddRange(plugin.Settings);
                }
            }

            return settings.Distinct().ToArray();
        }

        public string[] GetAllPluginFileExtensions()
        {
            List<string> extensions = new List<string>();

            foreach (Plugin plugin in plugins)
            {
                foreach (string extension in plugin.FileExtensions)
                {
                    extensions.Add(extension);
                }
            }

            return extensions.Distinct().ToArray();
        }

        public string[] GetPluginNames(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("String cannot be null or empty.", extension);
            }

            extension = extension.ToLower();

            var query = from p in plugins
                        where p.FileExtensions.Contains(extension)
                        select p.FullName;

            return query.ToArray();
        }

        public Demo CreateDemo(string fileName)
        {
            Plugin plugin = FindPlugin(fileName);

            if (plugin == null)
            {
                return null;
            }

            Demo demo = plugin.CreateDemo();
            demo.FileName = fileName;
            demo.Plugin = plugin;
            return demo;
        }

        public Launcher CreateLauncher(Demo demo)
        {
            Launcher launcher = demo.Plugin.CreateLauncher();
            launcher.Initialise(demo);
            return launcher;
        }

        /// <summary>
        /// Find the highest priority plugin that can handle the given demo file.
        /// </summary>
        /// <param name="demoFileName">The demo filename.</param>
        /// <returns>A plugin, or null if no suitable plugin is found.</returns>
        private Plugin FindPlugin(string demoFileName)
        {
            string extension = fileSystem.GetExtension(demoFileName);

            using (FastFileStreamBase stream = fileSystem.OpenRead(demoFileName))
            {
                return plugins.Where(p =>
                {
                    if (!p.FileExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    return p.IsValidDemo(stream, extension);
                }).OrderByDescending(dh => dh.Priority).FirstOrDefault();
            }
        }
    }
}
