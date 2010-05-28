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
using System.Text.RegularExpressions;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public partial class AddServerWindow : Window
    {
        public Boolean AddServer
        {
            get
            {
                return addServer;
            }
        }

        public String ServerAddress
        {
            get
            {
                return serverAddress;
            }
        }

        private Boolean addServer = false;
        private String serverAddress = null;

        public AddServerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            uiServerAddressTextBox.Focus();
        }

        private void uiAddButton_Click(object sender, RoutedEventArgs e)
        {
            // check validity of address
            if (!Regex.IsMatch(uiServerAddressTextBox.Text, @"[-a-zA-Z0-9.]+(:\d+)"))
            {
                Common.Message(this, "Invalid address. The address must be in the format ip:port or name:port.\r\n\r\nExamples: 66.228.122.147:27025 or hltv4.verygames.net:27098.");
                return;
            }

            addServer = true;
            serverAddress = uiServerAddressTextBox.Text;
            Close();
        }

        private void uiCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
