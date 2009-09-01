using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CDP
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Core.Settings.Instance.LoadMainConfig();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Core.Settings.Instance.SaveMainConfig();
            Core.Settings.Instance.SaveDemoConfig();
        }
    }
}
