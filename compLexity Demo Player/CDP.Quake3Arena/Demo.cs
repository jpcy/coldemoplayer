using System;
using System.Collections.Generic;
using System.Linq;

namespace CDP.Quake3Arena
{
    public class Demo : IdTech3.Demo
    {
        public override string GameName
        {
            get
            {
                if (mod == null)
                {
                    return string.Format("Quake III Arena ({0})", gameName);
                }
                else
                {
                    return mod.Name;
                }
            }
        }

        public override bool CanAnalyse
        {
            get { return mod == null ? false : mod.CanAnalyse; }
        }

        public override bool CanPlay
        {
            get { return true; }
        }

        private Mod mod;

        public override void Load()
        {
            base.Load();
            mod = ((Handler)handler).FindMod(ModFolder);
        }
    }
}
