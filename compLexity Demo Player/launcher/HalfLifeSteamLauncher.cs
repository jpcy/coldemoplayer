using System;
using System.Collections.Generic;
using System.Text;
using System.IO; // File

namespace compLexity_Demo_Player
{
    public class HalfLifeSteamLauncher : SteamLauncher
    {
        protected override void PreLaunch()
        {
            CheckMapVersion();
        }
    }
}
