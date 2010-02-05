using System;

namespace CDP.QuakeLive
{
    public class Executable
    {
        public string Name { get; set; }
        public string FileName { get; set; }
    }

    public class Config
    {
        public Executable[] Executables { get; set; }
    }
}
