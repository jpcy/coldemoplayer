using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.CounterStrikeDemo
{
    public class SettingsViewModel : HalfLifeDemo.SettingsViewModel
    {
        public bool RemoveFadeToBlack
        {
            get { return (bool)config.DemoSettings["CsRemoveFadeToBlack"]; }
            set { config.DemoSettings["CsRemoveFadeToBlack"] = value; }
        }

        public SettingsViewModel()
            : this(Core.Config.Instance)
        {
        }

        public SettingsViewModel(Core.Config config) : base(config)
        {
        }
    }
}
