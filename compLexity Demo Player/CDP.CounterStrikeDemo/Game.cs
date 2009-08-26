using System;
using System.Collections.Generic;

namespace CDP.CounterStrikeDemo
{
    public class Game : Core.SteamGame
    {
        // TODO: read checksums, built in maps, user messages etc.

        public Game()
        {
            AppId = 20;
            AppFolder = "counter-strike";
            ModFolder = "cstrike";
            Name = "Counter-Strike";
            DemoGameFolders = new string[] { "cstrike", "cs13" };
        }

        public override string GetVersionName(string checksum)
        {
            return base.GetVersionName(checksum);
        }

        public override int GetVersion(string checksum)
        {
            return base.GetVersion(checksum);
        }
    }
}
