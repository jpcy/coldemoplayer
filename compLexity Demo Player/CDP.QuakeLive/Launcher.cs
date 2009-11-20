using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CDP.Core;

namespace CDP.QuakeLive
{
    public class Launcher : Core.Launcher
    {
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly string demoFileName = "coldemoplayer.dm_73";
        private readonly string localLow;
        private Demo demo;

        public Launcher()
        {
            // FIXME: Windows XP
            localLow = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow");
        }

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            processExecutableFileName = null;
        }

        public override string CalculateDestinationFileName()
        {
            return fileSystem.PathCombine(localLow, "id Software", "quakelive", "home", "baseq3", "demos", demoFileName);
        }

        public override bool Verify()
        {
            string demosFolderPath = fileSystem.PathCombine(localLow, "id Software", "quakelive", "home", "baseq3", "demos");

            if (!Directory.Exists(demosFolderPath))
            {
                Directory.CreateDirectory(demosFolderPath);
            }

            return true;
        }

        public override void Launch()
        {
        }
    }
}
