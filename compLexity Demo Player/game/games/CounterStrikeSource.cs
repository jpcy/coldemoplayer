using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compLexity_Demo_Player.Games
{
    public class CounterStrikeSource : Game
    {
        public CounterStrikeSource(SteamGameInfo sgi)
            : base(sgi)
        {
        }

        public override bool CanAnalyse(Demo demo)
        {
            return (demo.DemoProtocol == 3 && demo.NetworkProtocol == 7);
        }
    }
}
