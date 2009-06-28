using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace compLexity_Demo_Player
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Common.LogException(e.Exception);
            Common.Message(null, "Unhandled exception.", e.Exception, MessageWindow.Flags.Error);
        }

        private void ListViewCopyStyle_Loaded(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (lv.CommandBindings.Count == 0)
            {
                lv.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, ListViewCopyExecute, ListViewCopyCanExecute));
            }
        }

        private void ListViewCopyExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ListView lv = (ListView)e.Source;
            Common.ListViewCopySelectedRowsToClipboard(lv);
        }

        private void ListViewCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListView lv = e.Source as ListView;
            e.CanExecute = (lv != null && lv.SelectedItems.Count > 0);
        }
    }
}
