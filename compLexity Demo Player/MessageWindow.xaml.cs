using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace compLexity_Demo_Player
{
    public partial class MessageWindow : Window
    {
        // an Ok button is implied if YesNo or ContinueAbort aren't set.
        public enum Flags
        {
            YesNo           = (1<<0),
            ContinueAbort   = (1<<1),
            Error           = (1<<2)
        }

        public enum Result
        {
            Ok,
            Yes,
            No,
            Continue,
            Abort,
            Close // window was closed
        }

        private Result result = Result.Close;
        private Procedure<Result> setResult;

        public Result CloseResult // arghh
        {
            get
            {
                return result;
            }
        }

        public MessageWindow(String message, Exception ex, Flags flags, Procedure<Result> setResult)
        {
            this.setResult = setResult;

            if (Owner != null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            InitializeComponent();

            // change control layout based on flags
            uiErrorStackPanel.Visibility = Visibility.Collapsed;
            uiOkButton.Visibility = Visibility.Collapsed;
            uiContinueButton.Visibility = Visibility.Collapsed;
            uiAbortButton.Visibility = Visibility.Collapsed;
            uiYesButton.Visibility = Visibility.Collapsed;
            uiNoButton.Visibility = Visibility.Collapsed;

            if ((flags & Flags.Error) != 0)
            {
                uiErrorStackPanel.Visibility = Visibility.Visible;
                Title = "Error";
            }

            if ((flags & Flags.ContinueAbort) != 0)
            {
                uiContinueButton.Visibility = Visibility.Visible;
                uiAbortButton.Visibility = Visibility.Visible;
            }
            else if ((flags & Flags.YesNo) != 0)
            {
                uiYesButton.Visibility = Visibility.Visible;
                uiNoButton.Visibility = Visibility.Visible;
            }
            else
            {
                uiOkButton.Visibility = Visibility.Visible;
            }
                        

            // fill in textbox
            String text = "";

            if (message != null)
            {
                text = message + "\r\n\r\n";
            }

            if (ex != null)
            {
                if ((flags & Flags.Error) != 0)
                {
                    Procedure<Exception> appendExceptionInfo = null;
                    
                    appendExceptionInfo = e =>
                    {
                        if (e.InnerException != null)
                        {
                            appendExceptionInfo(e.InnerException);
                        }

                        text += e.Message + "\r\n\r\n" + "Inner Exception: " + (e != ex).ToString() + "\r\n\r\n" + "Source: " + e.Source + "\r\n\r\n" + "Type: " + e.GetType().ToString() + "\r\n\r\n" + e.StackTrace + "\r\n\r\n";
                    };

                    appendExceptionInfo(ex);
                }
                else
                {
                    text += ex.Message;
                }
            }

            uiMessageTextBox.Text = text;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (setResult != null)
            {
                setResult(result);
            }
        }

        private void uiOkButton_Click(object sender, RoutedEventArgs e)
        {
            result = Result.Ok;
            Close();
        }

        private void uiContinueButton_Click(object sender, RoutedEventArgs e)
        {
            result = Result.Continue;
            Close();
        }

        private void uiAbortButton_Click(object sender, RoutedEventArgs e)
        {
            result = Result.Abort;
            Close();
        }

        private void uiYesButton_Click(object sender, RoutedEventArgs e)
        {
            result = Result.Yes;
            Close();
        }

        private void uiNoButton_Click(object sender, RoutedEventArgs e)
        {
            result = Result.No;
            Close();
        }
    }
}
