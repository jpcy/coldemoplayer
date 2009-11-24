using System;
using System.Windows;
using CDP.Core;

namespace CDP.Gui
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ObjectCreator.Get<ISettings>().Save();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            new ErrorReporter().LogUnhandledException(e.Exception);
        }
    }
}
