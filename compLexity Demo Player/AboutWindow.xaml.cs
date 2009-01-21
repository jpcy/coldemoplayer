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
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            uiProgramDescriptionTextBlock.Text = Config.Settings.ProgramName + " - " + Config.Settings.ProgramVersion;
        }

        private void uiCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
