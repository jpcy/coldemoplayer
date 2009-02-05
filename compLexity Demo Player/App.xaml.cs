using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Common.LogException(e.Exception);
            Common.Message(null, "Unhandled exception.", e.Exception, MessageWindow.Flags.Error);
        }
    }
}
