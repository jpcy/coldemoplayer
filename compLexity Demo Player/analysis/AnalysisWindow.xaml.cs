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
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Collections;

namespace compLexity_Demo_Player
{
    public partial class AnalysisWindow : Window
    {
        protected Demo demo = null;
        protected IProgressWindow progressWindowInterface;
        private XmlDocument gameLog = new XmlDocument() { PreserveWhitespace = true };
        private XmlElement gameLogParagraph;

        public AnalysisWindow()
        {
            InitializeComponent();

            // create game log document
            XmlElement root = GameLogCreateXmlElement("Section");
            gameLog.AppendChild(root);

            Procedure<String, String> addRootXmlAttribute = delegate(String name, String value)
            {
                XmlAttribute attribute = gameLog.CreateAttribute(name);
                attribute.InnerXml = value;
                root.Attributes.Append(attribute);
            };

            addRootXmlAttribute("xml:space", "preserve");

            gameLogParagraph = GameLogCreateXmlElement("Paragraph");
            root.AppendChild(gameLogParagraph);
        }

        protected virtual void Parse() { }

        private XmlElement GameLogCreateXmlElement(String name)
        {
            return gameLog.CreateElement(name, @"http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        }

        #region Event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = Config.Settings.AnalysisWindowState;

            // show progress window/start parsing thread
            ProgressWindow progressWindow = new ProgressWindow() { Owner = this };
            progressWindow.Thread = new Thread(new ThreadStart(Parse)) { Name = "Demo Parsing Thread" };
            progressWindowInterface = (IProgressWindow)progressWindow;
            if (progressWindow.ShowDialog() == false)
            {
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Config.Settings.AnalysisWindowState = WindowState;

            uiNetworkGraphGrid.Children.Clear();
            uiNetworkGraph.Dispose();
            uiNetworkGraphWindowsFormHost.Dispose();

            GC.Collect();
        }

        private void uiAnalysisTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // the listview doesn't correctly size columns when it's not visible

            TabItem tab = (TabItem)uiAnalysisTabControl.SelectedItem;

            if (tab == null)
            {
                return;
            }
            else if (tab == uiScoreboardTabItem)
            {
                uiScoreboardRoundsListView.ResizeColumns();
                uiScoreboardTeamScoresListView.ResizeColumns();
                uiScoreboardPlayerScoresListView.ResizeColumns();
            }
            else if (tab == uiPlayersTabItem)
            {
                uiPlayersListView.ResizeColumns();
                uiPlayerInfoKeysListView.ResizeColumns();
                uiPlayerInfoKeyTimestampsListView.ResizeColumns();
            }
            else if (tab == uiNetworkGraphTabItem)
            {
                uiNetworkGraphPlayersListView.ResizeColumns();
            }
        }

        private void uiCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
