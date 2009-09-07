using System;

namespace CDP.CounterStrikeDemo
{
    public class Demo : HalfLifeDemo.Demo
    {
        public override string MapImagesRelativePath
        {
            get
            {
                Core.IFileSystem fileSystem = new Core.FileSystem();
                return fileSystem.PathCombine("goldsrc", "cstrike", MapName + ".jpg");
            }
        }

        public override bool CanPlay
        {
            get { return true; }
        }

        public override bool CanAnalyse
        {
            get { return true; }
        }

        public Demo()
            : this(Core.Settings.Instance, new Core.FileSystem())
        {
        }

        public Demo(Core.ISettings settings, Core.IFileSystem fileSystem)
            : base(settings, fileSystem)
        {
        }

    }
}
