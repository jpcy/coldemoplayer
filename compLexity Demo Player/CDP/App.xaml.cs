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
            Core.NinjectFactory.LoadModule(new NinjectModule());
            Core.NinjectFactory.LoadModule(new Core.NinjectModule());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Core.Settings.Instance.Save();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }
    }
}
