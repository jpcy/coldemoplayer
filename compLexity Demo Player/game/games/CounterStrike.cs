using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player.Games
{
    public class CounterStrike : Game
    {
        public CounterStrike()
        {
            Engine = Engines.HalfLife;
            AppId = 10;
            Folder = "cstrike";
            FolderExtended = "counter-strike";
            Name = "Counter-Strike";
            ReadConfig();
        }

        public override bool CanAnalyse(Demo demo)
        {
            return true;
        }
    }
}
