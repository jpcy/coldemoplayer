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
            get { return (bool)config.Demo["CsRemoveFadeToBlack"]; }
            set { config.Demo["CsRemoveFadeToBlack"] = value; }
        }

        public SettingsViewModel()
            : this(Core.Settings.Instance)
        {
        }

        public SettingsViewModel(Core.Settings config) : base(config)
        {
        }
    }
}
