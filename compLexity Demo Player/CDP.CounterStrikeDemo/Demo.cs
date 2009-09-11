using System;

namespace CDP.CounterStrikeDemo
{
    public class Demo : HalfLifeDemo.Demo
    {
        public override string MapImagesRelativePath
        {
            get
            {
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
    }
}
