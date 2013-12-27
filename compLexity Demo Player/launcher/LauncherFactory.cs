using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    static public class LauncherFactory
    {
        public static Launcher CreateLauncher(Demo demo)
        {
            return CreateLauncher(demo, null);
        }

        public static Launcher CreateLauncher(Boolean sourceEngine)
        {
            return CreateLauncher(null, sourceEngine);
        }

        private static Launcher CreateLauncher(Demo demo, Boolean? sourceEngine)
        {
            Launcher launcher = null;

            if (demo == null)
            {
                Debug.Assert(sourceEngine != null);

                if (sourceEngine == true)
                {
                    return new SourceLauncher();
                }
                else
                {
                    return new HalfLifeSteamLauncher();
                }
            }
            else
            {
                switch (demo.Engine)
                {
                    case Demo.Engines.HalfLife:
                        if (((HalfLifeDemo)demo).ConvertNetworkProtocol())
                        {
                            if (Config.Settings.PlaybackProgramOldCs == ProgramSettings.PlaybackProgram.CounterStrike)
                            {
                                launcher = new CounterStrikeLauncher();
                            }
                            else
                            {
                                launcher = new HalfLifeSteamLauncher();
                            }
                        }
                        else
                        {
                            launcher = new HalfLifeLauncher();
                        }
                        break;

                    case Demo.Engines.HalfLifeSteam:
                        if (((HalfLifeDemo)demo).NetworkProtocol == 47 && demo.GameFolderName == "cstrike" && Config.Settings.PlaybackProgramOldCs == ProgramSettings.PlaybackProgram.CounterStrike)
                        {
                            launcher = new CounterStrikeLauncher();
                        }
                        else
                        {
                            launcher = new HalfLifeSteamLauncher();
                        }
                        break;

                    case Demo.Engines.Source:
                        launcher = new SourceLauncher();
                        break;
                }
            }

            if (launcher == null)
            {
                return null;
            }

            launcher.Demo = demo;
            return launcher;
        }
    }
}
