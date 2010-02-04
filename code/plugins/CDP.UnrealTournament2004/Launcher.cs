using System.IO;
using System.Diagnostics;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.UnrealTournament2004
{
    public class Launcher : Core.Launcher
    {
        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly string demoFileName = "coldemoplayer";
        private readonly string demoExtension = "demo4";
        private readonly string execFileName = "coldemoplayer.exec";
        private Demo demo;
        private string systemFolder;
        private string demosFolder;
        private string rootFolder;

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            processExecutableFileName = (string)settings["Ut2004ExeFullPath"];

            if (!string.IsNullOrEmpty(processExecutableFileName))
            {
                systemFolder = Path.GetDirectoryName(processExecutableFileName);
                rootFolder = systemFolder.Replace("System", string.Empty);
                demosFolder = systemFolder.Replace("System", "Demos");
            }
        }

        public override string CalculateDestinationFileName()
        {
            return fileSystem.PathCombine(demosFolder, demoFileName + "." + demoExtension);
        }

        public override bool Verify()
        {
            if (string.IsNullOrEmpty(processExecutableFileName))
            {
                Message = Strings.ExeNotSet;
                return false;
            }

            if (!File.Exists(processExecutableFileName) )
            {
                Message = Strings.ExeNotFound.Args(processExecutableFileName);
                return false;
            }

            if (!Directory.Exists(demosFolder))
            {
                Directory.CreateDirectory(demosFolder);
            }

            return true;
        }

        public override void Launch()
        {
            using (TextWriter writer = new StreamWriter(fileSystem.PathCombine(rootFolder, execFileName)))
            {
                writer.Write("say ");
                writer.WriteLine(Strings.PlayingDemoMessage, Path.GetFileNameWithoutExtension(demo.FileName));
                writer.WriteLine("demoplay {0}", demoFileName);
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = processExecutableFileName,
                Arguments = "EXEC=../" + execFileName
            };

            System.Diagnostics.Process.Start(psi);
        }
    }
}
