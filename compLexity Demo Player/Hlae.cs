using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Runtime.InteropServices;

namespace hlae 
{
    namespace remoting 
    {
        //	HlaeRemote_1
        //
        //	This is the main remoting interface class, which
        //  shall be exposed for people wanting to use it.
        //
        //  make sure this interface is defined in the hlae::remoting namespace
        public interface IHlaeRemote_1
        {
	        //	IsDeprecated
	        //
	        //  Retruns true in case the interface is deprecated and will go away
	        //  in futur updates.
	        bool IsDeprecated();

	        //	GetCustomArgs
	        //  
	        //	Returns:
	        //    null reference on error,
	        //    otherwise the cusomargs the user set in the launcher as String
	        String GetCustomArgs();

	        //  Launch
	        //    Tells HLAE to launch the engine
	        //  Returns:
	        //    false on error, otherwise true
	        bool Launch();

	        //	LaunchEx
	        //    Extends Launch
	        //  Params:
	        //    OverrideCustomArgs: use these instead of the user's default CustomArgs
	        //    (Also see GetCustomArgs).
	        bool LaunchEx(String OverrideCustomArgs );
        }
    } // namespace remoting
} // namespace hlae


namespace compLexity_Demo_Player
{
    public class Hlae
    {
        private HlaeConfig config;
        private const String configFileName = "hlaeconfig.xml";

        public void ReadConfig()
        {
            try
            {
                config = (HlaeConfig)Common.XmlFileDeserialize(System.IO.Path.GetDirectoryName(Config.Settings.HlaeExeFullPath) + "\\" + configFileName, typeof(HlaeConfig));
            }
            catch (Exception)
            {
                config = new HlaeConfig();
            }
        }

        public void WriteConfig(String hlExeFullPath, String gameFolder)
        {
            config.Settings.Launcher.GamePath = hlExeFullPath;
            config.Settings.Launcher.Modification = gameFolder;
            Common.XmlFileSerialize(System.IO.Path.GetDirectoryName(Config.Settings.HlaeExeFullPath) + "\\" + configFileName, config, typeof(HlaeConfig));
        }
    }

    [XmlRoot("HlaeConfig")]
    public class HlaeConfig
    {
        public class HlaeLauncher
        {
            public String GamePath = "please select";
            public String Modification = "cstrike";
            public String CustomCmdLine = "+toggleconsole";
            public Boolean GfxForce = true;
            public UInt16 GfxWidth = 800;
            public UInt16 GfxHeight = 600;
            public Byte GfxBpp = 32;
            public Boolean RememberChanges = true;
            public Boolean ForceAlpha = false;
            public Boolean OptimizeDesktopRes = true;
            public Boolean OptimizeVisibilty = true;
            public Boolean FullScreen = false;
            public Byte RenderMode = 0;
        }

        public class HlaeCustomLoader
        {
            public String HookDllPath;
            public String ProgramPath;
            public String CmdLine = "-steam -window -console -game cstrike";
        }

        public class HlaeDemoTools
        {
            public String OutputFolder;
        }

        public class HlaeSettings
        {
            public HlaeLauncher Launcher = new HlaeLauncher();
            public HlaeCustomLoader CustomLoader = new HlaeCustomLoader();
            public HlaeDemoTools DemoTools = new HlaeDemoTools();
            public Int32 UpdateCheck = 1;
        }

        public String Version = "unknown";
        public HlaeSettings Settings = new HlaeSettings();
    }
}
