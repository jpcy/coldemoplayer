using System;
using System.IO;

namespace CDP.Core
{
    public class Config
    {
        public static Config Instance
        {
            get { return instance; }
        }

        public string ProgramName
        {
            get { return "compLexity Demo Player"; }
        }

        public int ProgramVersionMajor
        {
            get { return 2; }
        }

        public int ProgramVersionMinor
        {
            get { return 0; }
        }

        public int ProgramVersionUpdate
        {
            get { return 0; }
        }

        public string ProgramVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}", ProgramVersionMajor, ProgramVersionMinor, ProgramVersionUpdate);
            }
        }

        public string ComplexityUrl
        {
            get { return "http://www.complexitygaming.com/"; }
        }

        public string ProgramExeFullPath { get; private set; }
        public string ProgramPath { get; private set; }
        public string ProgramDataPath { get; private set; }

        private static readonly Config instance = new Config();

        private Config()
        {
            ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + ProgramName;
            ProgramExeFullPath = Environment.GetCommandLineArgs()[0];
            ProgramPath = Path.GetDirectoryName(ProgramExeFullPath);
#if DEBUG
            // BLEH: this is what happens when you can't use macros in setting the debug working directory.
            // Remove the last four folder names from the path, e.g. "\compLexity Demo Player\CDP\bin\Debug" to "\bin".
            ProgramPath = Path.GetFullPath("../../../../bin");
#endif
        }
    }
}
