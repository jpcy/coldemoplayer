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
        public bool CanConvert { get; set; }
        public bool CanAnalyse { get; set; }
    }

    public class Config
    {
        public Mod[] Mods { get; set; }
        public string[] ExecutableFileNames { get; set; }
    }
}
