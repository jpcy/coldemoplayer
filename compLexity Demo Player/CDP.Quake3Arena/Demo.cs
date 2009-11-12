using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.Quake3Arena
{
    public class Demo : IdTech3.Demo
    {
        public override string GameName
        {
            get
            {
                string result = "Quake III Arena";

                if (gameName != "baseq3")
                {
                    // TODO: read proper mod names from a config file.
                    result += string.Format(" ({0})", gameName);
                }

                return result;
            }
        }

        public override bool CanAnalyse
        {
            get { return true; }
        }

        public override bool CanPlay
        {
            get { return true; }
        }
    }
}
