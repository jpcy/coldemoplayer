using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.CounterStrike
{
    public class SettingsViewModel : HalfLife.SettingsViewModel
    {
        public bool RemoveFadeToBlack
        {
            get { return (bool)settings["CsRemoveFadeToBlack"]; }
            set { settings["CsRemoveFadeToBlack"] = value; }
        }

        public SettingsViewModel()
        {
        }
    }
}
