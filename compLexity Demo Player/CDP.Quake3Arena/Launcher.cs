using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CDP.Quake3Arena
{
    /// <remarks>
    /// Demos don't have to be in a mod's "demos" folder to play, baseq3 is fine - so demos are always copied into baseq3/demos.
    /// </remarks>
    public class Launcher : Core.Launcher
    {
        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        protected readonly string demoFileName = "coldemoplayer";
        protected Demo demo;

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            processExecutableFileName = (string)settings["Quake3ExeFullPath"];
        }

        public override string CalculateDestinationFileName()
        {
            return fileSystem.PathCombine(Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]), "baseq3", "demos", fileSystem.ChangeExtension(demoFileName, fileSystem.GetExtension(demo.FileName)));
        }

        public override bool Verify()
        {
            if (!File.Exists((string)settings["Quake3ExeFullPath"]))
            {
                Message = string.Format("quake3.exe path not set.");
                return false;
            }

            string demosFolderPath = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]), "baseq3", "demos");

            if (!Directory.Exists(demosFolderPath))
            {
                Directory.CreateDirectory(demosFolderPath);
            }

            return true;
        }

        public override void Launch()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = processExecutableFileName,
                WorkingDirectory = Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]),
                Arguments = "+demo " + fileSystem.ChangeExtension(demoFileName, fileSystem.GetExtension(demo.FileName)) + " +set fs_game " + demo.ModFolder
            };

            Process.Start(psi);
        }
    }
}
