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
            get { return (bool)settings["CsRemoveFadeToBlack"]; }
            set { settings["CsRemoveFadeToBlack"] = value; }
        }

        public SettingsViewModel()
        {
        }
    }
}
