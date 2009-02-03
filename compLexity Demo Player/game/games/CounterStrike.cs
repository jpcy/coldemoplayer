using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compLexity_Demo_Player.Games
{
    public class CounterStrike : Game
    {
        public CounterStrike(SteamGameInfo sgi)
            : base(sgi)
        {
        }

        public override bool CanAnalyse(Demo demo)
        {
            return true;
        }
    }
}
