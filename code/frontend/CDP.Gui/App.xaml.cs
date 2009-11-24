using System;
using System.Windows;
using CDP.Core;

namespace CDP.Gui
{
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ObjectCreator.Get<ISettings>().Save();
        }
    }
}
