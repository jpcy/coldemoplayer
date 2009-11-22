using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.QuakeLive
{
    public class Demo : Quake3Arena.Demo
    {
        public override string GameName
        {
            get { return "Quake Live"; }
        }

        public override bool CanAnalyse
        {
            get { return true; }
        }
    }
}
