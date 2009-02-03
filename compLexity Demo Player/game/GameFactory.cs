using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compLexity_Demo_Player
{
    public static class GameFactory
    {
        public static Game Create(SteamGameInfo sgi)
        {
            if (sgi.Engine == Game.Engines.HalfLife)
            {
                if (sgi.GameFolder == "cstrike")
                {
                    return new Games.CounterStrike(sgi);
                }
            }
            else if (sgi.Engine == Game.Engines.Source)
            {
                if (sgi.GameFolder == "cstrike")
                {
                    return new Games.CounterStrikeSource(sgi);
                }
            }

            return new Game(sgi);
        }
    }
}
