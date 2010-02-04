using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CDP.Core;
using CDP.Core.Extensions;

namespace CDP.QuakeLive
{
    public class Launcher : Core.Launcher
    {
        // FOLDERID_LocalAppDataLow
        // http://msdn.microsoft.com/en-us/library/bb762584%28VS.85%29.aspx
        private readonly Guid LocalAppDataLow = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
        private readonly string demoFileName = "coldemoplayer.dm_73";
        private readonly string appDataPath;
        private Demo demo;

        public Launcher()
        {
            // There's no Environment.SpecialFolder for LocalLow. Use Vista/Windows 7 specific Win32 API function.
            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(fileSystem.PathCombine(Environment.GetEnvironmentVariable("windir"), "system32", "shell32.dll"));

            // SHGetKnownFolderPath
            // Minimum DLL Version: shell32.dll version 6.0.6000 or later.
            // http://msdn.microsoft.com/en-us/library/bb762188%28VS.85%29.aspx
            if (fv.FileMajorPart > 6 || (fv.FileMajorPart == 6 && fv.FileMinorPart > 0) || (fv.FileMajorPart == 6 && fv.FileMinorPart == 0 && fv.FileBuildPart >= 6000))
            {
                IntPtr path;
                int result = SHGetKnownFolderPath(LocalAppDataLow, 0, IntPtr.Zero, out path);

                if (result == 0)
                {
                    appDataPath = Marshal.PtrToStringUni(path);
                    Marshal.FreeCoTaskMem(path);
                }
                else
                {
                    throw new ApplicationException("SHGetKnownFolderPath failed with HRESULT \'{0}\'.".Args(result));
                }
            }
            else
            {
                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }

        public override void Initialise(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            processExecutableFileName = null;
        }

        public override string CalculateDestinationFileName()
        {
            return fileSystem.PathCombine(appDataPath, "id Software", "quakelive", "home", "baseq3", "demos", demoFileName);
        }

        public override bool Verify()
        {
            string demosFolderPath = fileSystem.PathCombine(appDataPath, "id Software", "quakelive", "home", "baseq3", "demos");

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
