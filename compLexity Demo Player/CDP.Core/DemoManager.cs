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
            public Type DemoType { get; set; }
            public DemoHandler DemoHandler { get; set; }
        }

        private Adapters.IAssembly assemblyAdapter;
        private Adapters.IFile fileAdapter;
        private Adapters.IFolder folderAdapter;
        private List<Plugin> plugins = new List<Plugin>();

        public DemoManager(Adapters.IAssembly assemblyAdapter, Adapters.IFile fileAdapter, Adapters.IFolder folderAdapter)
        {
            this.assemblyAdapter = assemblyAdapter;
            this.fileAdapter = fileAdapter;
            this.folderAdapter = folderAdapter;
        }

        public DemoManager()
            : this(new Adapters.Assembly(), new Adapters.File(), new Adapters.Folder())
        {
        }

        public void LoadPlugins(string path)
        {
            foreach (string assemblyFileName in folderAdapter.GetFiles(path, "*.dll"))
            {
                Type[] types = assemblyAdapter.GetTypes(assemblyFileName);
                Type demoType = types.SingleOrDefault(t => t.BaseType == typeof(Demo));
                Type demoHandlerType = types.SingleOrDefault(t => t.BaseType == typeof(DemoHandler));

                if (demoType == null)
                {
                    throw new ApplicationException(string.Format("Assembly \"{0}\" doesn't contain a class that inherits from Demo.", assemblyFileName));
                }

                if (demoHandlerType == null)
                {
                    throw new ApplicationException(string.Format("Assembly \"{0}\" doesn't contain a class that inherits from DemoHandler.", assemblyFileName));
                }

                plugins.Add(new Plugin()
                {
                    DemoType = demoType,
                    DemoHandler = (DemoHandler)Activator.CreateInstance(demoHandlerType)
                });

                // TODO: calculate priority based on class heirarchy.
            }
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

            // Create a demo instance and load the demo file.
            Demo demo = (Demo)Activator.CreateInstance(plugin.DemoType);
            demo.FileName = fileName;
            demo.Handler = plugin.DemoHandler;
            return demo;
        }

        private Plugin FindPlugin(string fileName)
        {
            string extension = Path.GetExtension(fileName).Replace(".", null);

            using (Stream stream = fileAdapter.OpenRead(fileName))
            {
                // TODO: take priority into account.
                return plugins.FirstOrDefault(dp =>
                {
                    if (!dp.DemoHandler.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    return dp.DemoHandler.IsValidDemo(stream);
                });
            }
        }
    }
}
