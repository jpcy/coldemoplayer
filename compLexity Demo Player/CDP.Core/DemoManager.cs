using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace CDP.Core
{
    public class DemoManager
    {
        private class Plugin
        {
            public uint Priority { get; private set; }
            public Type DemoType { get; private set; }
            public DemoHandler DemoHandler { get; private set; }

            public Plugin(uint priority, Type demoType, DemoHandler demoHandler)
            {
                Priority = priority;
                DemoType = demoType;
                DemoHandler = demoHandler;
            }
        }

        private readonly Adapters.IFile fileAdapter;
        private readonly Adapters.IPath pathAdapter;
        private readonly List<Plugin> plugins = new List<Plugin>();

        public DemoManager(Adapters.IFile fileAdapter, Adapters.IPath pathAdapter)
        {
            this.fileAdapter = fileAdapter;
            this.pathAdapter = pathAdapter;
        }

        public DemoManager()
            : this(new Adapters.File(), new Adapters.Path())
        {
        }

        public void AddPlugin(uint priority, Type demoType, DemoHandler demoHandler)
        {
            if (!demoType.IsSubclassOf(typeof(Demo)))
            {
                throw new ArgumentException("Type must inherit from CDP.Core.Demo.", "demoType");
            }

            plugins.Add(new Plugin(priority, demoType, demoHandler));
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

        public Demo CreateDemo(string fileName)
        {
            Plugin plugin = FindPlugin(fileName);

            if (plugin == null)
            {
                return null;
            }

            Demo demo = (Demo)Activator.CreateInstance(plugin.DemoType);
            demo.FileName = fileName;
            demo.Handler = plugin.DemoHandler;
            return demo;
        }

        /// <summary>
        /// Find the highest priority plugin that can handle the given demo file.
        /// </summary>
        /// <param name="demoFileName">The demo filename.</param>
        /// <returns>A plugin, or null if no suitable plugin is found.</returns>
        private Plugin FindPlugin(string demoFileName)
        {
            string extension = pathAdapter.GetExtension(demoFileName);

            using (Stream stream = fileAdapter.OpenRead(demoFileName))
            {
                return plugins.Where(p =>
                {
                    if (!p.DemoHandler.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    return p.DemoHandler.IsValidDemo(stream);
                }).OrderByDescending(p => p.Priority).FirstOrDefault();
            }
        }
    }
}
