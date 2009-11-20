using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CDP.Core;
using CDP.Quake3Arena;

namespace CDP.QuakeLive
{
    public class Handler : Quake3Arena.Handler
    {
        public override string FullName
        {
            get { return "Quake Live"; }
        }

        public override string Name
        {
            get { return "QL"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dm_73" }; }
        }

        public override Setting[] Settings
        {
            get { return new Setting[] { }; }
        }

        public override UserControl SettingsView
        {
            get { return null; }
        }

        protected override void ReadConfig()
        {
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            return new Launcher();
        }

        public override Mod FindMod(string modFolder)
        {
            return null;
        }
    }
}
