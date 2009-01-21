using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player
{
    public static class AnalysisWindowFactory
    {
        public static AnalysisWindow CreateAnalysisWindow(Demo demo)
        {
            AnalysisWindow window = null;

            switch (demo.Engine)
            {
                case Demo.EngineEnum.HalfLife:
                case Demo.EngineEnum.HalfLifeSteam:
                    window = new HalfLifeAnalysisWindow(demo);
                    break;

                case Demo.EngineEnum.Source:
                    window = new SourceAnalysisWindow(demo);
                    break;
            }

            if (window == null)
            {
                return null;
            }

            //window.SetDemo(demo);
            return window;
        }
    }
}
