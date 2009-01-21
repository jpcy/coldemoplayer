using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ZedGraph;
using System.Collections.ObjectModel; // ObservableCollection

namespace compLexity_Demo_Player
{
    public class HalfLifeAnalysisWindow : AnalysisWindow
    {
        private HalfLifeDemoAnalyser analyser;

        public HalfLifeAnalysisWindow(Demo demo)
        {
            this.demo = demo;

            uiPlayersListView.SelectionChanged += new SelectionChangedEventHandler(uiPlayersListView_SelectionChanged);
            uiPlayerInfoKeysListView.SelectionChanged += new SelectionChangedEventHandler(uiPlayerInfoKeysListView_SelectionChanged);
            uiScoreboardRoundsListView.SelectionChanged += new SelectionChangedEventHandler(uiScoreboardRoundsListView_SelectionChanged);
            uiNetworkGraphPlayersListView.SelectionChanged += new SelectionChangedEventHandler(uiNetworkGraphPlayersListView_SelectionChanged);

            // create the zedgraph control
            uiNetworkGraph.GraphPane.Title.Text = "";
            uiNetworkGraph.GraphPane.XAxis.Title.Text = "Time (%)";
            uiNetworkGraph.GraphPane.YAxis.Title.Text = "";
        }

        protected override void Parse()
        {
            analyser = new HalfLifeDemoAnalyser((HalfLifeDemo)demo, (IAnalysisWindow)this, progressWindowInterface);

            try
            {
                analyser.Parse();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                GameLogWrite("*** Error ***", Brushes.Red);
                String errorMessage = "Error analysing demo.";

                if (demo.Status == Demo.StatusEnum.CorruptDirEntries)
                {
                    errorMessage += " Since the demo is marked as having corrupt directory entries, this is to be expected. As a result, the analysis may be incomplete.";
                }

                progressWindowInterface.Error(errorMessage, ex, false, null);
            }

            ParsingFinished(analyser.Players, analyser.Rounds, analyser.Players);
        }

        #region Event Handlers
        void HalfLifeAnalysisWindow_Initialized(object sender, EventArgs e)
        {
            
        }

        private void uiPlayersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HalfLifeDemoAnalyser.Player player = (HalfLifeDemoAnalyser.Player)uiPlayersListView.SelectedItem;

            if (player != null)
            {
                uiPlayerInfoKeysListView.ItemsSource = player.InfoKeys;
                uiPlayerInfoKeysListView.SelectedIndex = 0;
            }
            else
            {
                uiPlayerInfoKeysListView.ItemsSource = null;
            }
        }

        private void uiPlayerInfoKeysListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HalfLifeDemoAnalyser.InfoKey infoKey = (HalfLifeDemoAnalyser.InfoKey)uiPlayerInfoKeysListView.SelectedItem;

            if (infoKey != null)
            {
                uiPlayerInfoKeyTimestampsListView.ItemsSource = infoKey.Values;
                uiPlayerInfoKeyTimestampsListView.SelectedIndex = 0;
            }
            else
            {
                uiPlayerInfoKeyTimestampsListView.ItemsSource = null;
            }
        }

        void uiScoreboardRoundsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HalfLifeGameState.Round round = (HalfLifeGameState.Round)uiScoreboardRoundsListView.SelectedItem;

            if (round != null)
            {
                uiScoreboardTeamScoresListView.ItemsSource = round.Teams;
                uiScoreboardPlayerScoresListView.ItemsSource = round.Players;
            }
            else
            {
                uiScoreboardTeamScoresListView.ItemsSource = null;
                uiScoreboardPlayerScoresListView.ItemsSource = null;
            }
        }

        void uiNetworkGraphPlayersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // FIXME: hack to work around memory leak
            if (uiNetworkGraph.GraphPane == null)
            {
                return;
            }

            HalfLifeDemoAnalyser.Player player = (HalfLifeDemoAnalyser.Player)uiNetworkGraphPlayersListView.SelectedItem;

            if (player == null)
            {
                uiNetworkGraph.GraphPane.CurveList.Clear();
                uiNetworkGraph.Refresh();
                return;
            }

            PointPairList pingPointList = new PointPairList();
            PointPairList lossPointList = new PointPairList();

            Int32 maxSampleSize = player.NetworkStates.Count / 1000;
            Int32 nSamplesTaken = 0;
            UInt32 ping = 0;
            UInt32 loss = 0;

            if (maxSampleSize == 0)
            {
                maxSampleSize = 1;
            }

            Int32 i = 0;

            foreach (HalfLifeDemoAnalyser.Player.NetworkState networkState in player.NetworkStates)
            {
                ping += networkState.Ping;
                loss += networkState.Loss;
                nSamplesTaken++;

                if (nSamplesTaken == maxSampleSize)
                {
                    // add to point pair lists
                    Single time = i / (Single)player.NetworkStates.Count * 100.0f;
                    pingPointList.Add(new PointPair(time, ping / (Single)nSamplesTaken));
                    lossPointList.Add(new PointPair(time, loss / (Single)nSamplesTaken));

                    ping = 0;
                    loss = 0;
                    nSamplesTaken = 0;
                }

                i++;
            }

            uiNetworkGraph.GraphPane.CurveList.Clear();
            uiNetworkGraph.GraphPane.AddCurve("Ping", pingPointList, System.Drawing.Color.Red, SymbolType.None);
            uiNetworkGraph.GraphPane.AddCurve("Loss", lossPointList, System.Drawing.Color.Blue, SymbolType.None);
            uiNetworkGraph.AxisChange();
            uiNetworkGraph.RestoreScale(uiNetworkGraph.GraphPane);
            uiNetworkGraph.Refresh();
        }

        #endregion
    }
}
