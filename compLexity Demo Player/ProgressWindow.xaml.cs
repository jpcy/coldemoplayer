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
using System.Threading; // Thread
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public partial class ProgressWindow : Window
    {
        public Thread Thread { get; set; }
        public Object ThreadParameter { get; set; }
        private Boolean hasActivated = false;

        public ProgressWindow(String title)
        {
            InitializeComponent();
            this.Title = title;
        }

        public ProgressWindow() : this("Working...")
        {
        }

        private void uiCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closingWindowExternal)
            {
                Common.AbortThread(Thread);
                DialogResult = false;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (!hasActivated)
            {
                hasActivated = true;

                if (ThreadParameter == null)
                {
                    Thread.Start();
                }
                else
                {
                    Thread.Start(ThreadParameter);
                }
            }
        }
    }
}
