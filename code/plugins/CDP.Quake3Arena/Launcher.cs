using System.IO;
using System.Diagnostics;
using CDP.Core.Extensions;

namespace CDP.Quake3Arena
{
    /// <remarks>
    /// Demos don't have to be in a mod's "demos" folder to play, baseq3 is fine - so demos are always copied into baseq3/demos.
    /// 
    /// Setting fs_game to a mod folder that doesn't exist is fine, it just falls back to baseq3. This removes the need to check if a mod folder exists before playback. Mod folders for demos recorded with some mods don't need to exist anyway (i.e. if the mod doesn't add any new resources).
    /// </remarks>
    public class Launcher : Core.Launcher
    {
        protected readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        protected readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private readonly string demoFileName = "coldemoplayer";
        private Demo demo;

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            processExecutableFileName = (string)settings["Quake3ExeFullPath"];
        }

        public override string CalculateDestinationFileName()
        {
            string extension;

            if (demo.ConvertTarget == IdTech3.ConvertTargets.Protocol68)
            {
                extension = "dm_68";
            }
            else if (demo.ConvertTarget == IdTech3.ConvertTargets.Protocol73)
            {
                extension = "dm_73";
            }
            else
            {
                // Keep the same extension.
                extension = fileSystem.GetExtension(demo.FileName);
            }

            return fileSystem.PathCombine(Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]), "baseq3", "demos", demoFileName + "." + extension);
        }

        public override bool Verify()
        {
            string exeFullPath = (string)settings["Quake3ExeFullPath"];

            if (string.IsNullOrEmpty(exeFullPath))
            {
                Message = Strings.ExeNotSet;
                return false;
            }

            if (!File.Exists(exeFullPath))
            {
                Message = Strings.ExeNotFound.Args(exeFullPath);
                return false;
            }

            string demosFolderPath = fileSystem.PathCombine(Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]), "baseq3", "demos");

            if (!Directory.Exists(demosFolderPath))
            {
                Directory.CreateDirectory(demosFolderPath);
            }

            // TODO: make sure the game process isn't already running.

            return true;
        }

        public override void Launch()
        {
            // Explicitly providing an extension avoid the potential problem of multiple demos existing with the different extensions. Example: coldemoplayer.dm_66 and coldemoplayer.dm_68 - the filename with a dm_66 extension takes priority and will play if the command "demo coldemoplayer" is executed.
            string extension = fileSystem.GetExtension(CalculateDestinationFileName());

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = processExecutableFileName,
                WorkingDirectory = Path.GetDirectoryName((string)settings["Quake3ExeFullPath"]),
                Arguments = "+demo " + demoFileName + "." + extension + " +set fs_game " + demo.ModFolder
            };

            Process.Start(psi);
        }
    }
}
