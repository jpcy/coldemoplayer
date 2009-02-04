using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player.Games
{
    public class CounterStrikeSource : Game
    {
        public CounterStrikeSource()
        {
            Engine = Engines.Source;
            AppId = 240;
            Folder = "cstrike";
            FolderExtended = "counter-strike source";
            Name = "Counter-Strike: Source";
        }

        public override bool CanAnalyse(Demo demo)
        {
            return (demo.DemoProtocol == 3 && demo.NetworkProtocol == 7);
        }
    }
}
