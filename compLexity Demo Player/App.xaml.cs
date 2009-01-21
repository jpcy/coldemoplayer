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
            String logsFullFolderPath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) +"\\logs";

            if (!Directory.Exists(logsFullFolderPath))
            {
                Directory.CreateDirectory(logsFullFolderPath);
            }
            
            using (TextWriter writer = new StreamWriter(logsFullFolderPath + "\\" + Path.GetRandomFileName() + ".log"))
            {
                Procedure<Exception> logException = null;
                
                logException = ex =>
                {
                    if (ex.InnerException != null)
                    {
                        logException(ex.InnerException);
                    }

                    writer.WriteLine(ex.Message);
                    writer.WriteLine(ex.Source);
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine();
                };

                logException(e.Exception);
            }

            Common.Message(null, "Unhandled exception.", e.Exception, MessageWindow.Flags.Error);
        }
    }
}
