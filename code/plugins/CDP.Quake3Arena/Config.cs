using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.Quake3Arena
{
    public class Mod
    {
        public string Folder { get; set; }
        public string Name { get; set; }
        public bool CanAnalyse { get; set; }
    }

    public class Executable
    {
        public string Name { get; set; }
        public string FileName { get; set; }
    }

    public class Config
    {
        public Mod[] Mods { get; set; }
        public Executable[] Executables { get; set; }
    }
}
