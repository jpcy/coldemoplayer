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
        void AddPlugin(uint priority, DemoHandler demoHandler);
        Setting[] GetAllDemoHandlerSettings();
        string[] ValidDemoExtensions();

        /// <summary>
        /// Gets the names of all demo handlers that handle the given extension.
        /// </summary>
        /// <param name="extension">The demo file extension.</param>
        /// <returns>A list of demo handler full names.</returns>
        string[] GetDemoHandlerNames(string extension);

        Demo CreateDemo(string fileName);
        Launcher CreateLauncher(Demo demo);
    }

    [Singleton]
    public class DemoManager : IDemoManager
    {
        private class Plugin
        {
            public uint Priority { get; private set; }
            public DemoHandler DemoHandler { get; private set; }

            public Plugin(uint priority, DemoHandler demoHandler)
            {
                Priority = priority;
                DemoHandler = demoHandler;
            }
        }

        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly List<Plugin> plugins = new List<Plugin>();

        public void AddPlugin(uint priority, DemoHandler demoHandler)
        {
            plugins.Add(new Plugin(priority, demoHandler));
        }

        public Setting[] GetAllDemoHandlerSettings()
        {
            List<Setting> settings = new List<Setting>();

            foreach (Plugin plugin in plugins)
            {
                settings.AddRange(plugin.DemoHandler.Settings);
            }

            return settings.Distinct().ToArray();
        }

        public string[] ValidDemoExtensions()
        {
            List<string> extensions = new List<string>();

            foreach (Plugin plugin in plugins)
            {
                foreach (string extension in plugin.DemoHandler.Extensions)
                {
                    extensions.Add(extension);
                }
            }

            return extensions.Distinct().ToArray();
        }

        public string[] GetDemoHandlerNames(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("String cannot be null or empty.", extension);
            }

            extension = extension.ToLower();

            var query = from p in plugins
                        where p.DemoHandler.Extensions.Contains(extension)
                        select p.DemoHandler.FullName;

            return query.ToArray();
        }

        public Demo CreateDemo(string fileName)
        {
            Plugin plugin = FindPlugin(fileName);

            if (plugin == null)
            {
                return null;
            }

            Demo demo = plugin.DemoHandler.CreateDemo();
            demo.FileName = fileName;
            demo.Handler = plugin.DemoHandler;
            return demo;
        }

        public Launcher CreateLauncher(Demo demo)
        {
            Launcher launcher = demo.Handler.CreateLauncher();
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
                    if (!p.DemoHandler.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    return p.DemoHandler.IsValidDemo(stream, extension);
                }).OrderByDescending(p => p.Priority).FirstOrDefault();
            }
        }
    }
}
