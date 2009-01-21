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
using System.Collections.ObjectModel; // ObservableCollection
using System.ComponentModel; // INotifyPropertyChanged
using System.IO;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    public partial class MapPoolWindow : Window
    {
        private class Game : NotifyPropertyChangedItem
        {
            public String Text
            {
                get
                {
                    return gameInfo.GameName + " (" + gameInfo.GameFolder + ")";
                }
            }

            public SteamGameInfo SteamGameInfo
            {
                get
                {
                    return gameInfo;
                }
            }

            public GameConfig GameConfig
            {
                get
                {
                    return gameConfig;
                }
            }

            private SteamGameInfo gameInfo;
            private GameConfig gameConfig;

            public Game(SteamGameInfo gameInfo, GameConfig gameConfig)
            {
                this.gameInfo = gameInfo;
                this.gameConfig = gameConfig;
            }
        }

        private class Map : NotifyPropertyChangedItem
        {
            public Boolean BuiltIn
            {
                get
                {
                    return builtIn;
                }
            }

            public String BuiltInText
            {
                get
                {
                    return (builtIn ? "Yes" : "No");
                }
            }

            public String Name { get; set; }
            public String Checksum { get; set; }
            public String WadFiles { get; set; }

            private Boolean builtIn;

            public Map(Boolean builtIn, String name, String checksum, String wadFiles)
            {
                this.builtIn = builtIn;
                Name = name;
                Checksum = checksum;
                WadFiles = wadFiles;
            }
        }

        private ObservableCollection<Game> gameCollection;
        private ObservableCollection<Map> mapCollection;

        public MapPoolWindow()
        {
            gameCollection = new ObservableCollection<Game>();
            mapCollection = new ObservableCollection<Map>();

            foreach (GameConfig gameConfig in GameConfigList.GoldSrcGameTable.Values)
            {
                // look for a matching SteamGameInfo entry
                SteamGameInfo gameInfo = Steam.GetGameInfo(false, gameConfig.GameFolder);

                if (gameInfo != null)
                {
                    gameCollection.Add(new Game(gameInfo, gameConfig));
                }
            }

            InitializeComponent();
        }

        private void RefreshMapListView()
        {
            mapCollection.Clear();

            Game game = (Game)uiGameComboBox.SelectedItem;

            if (game == null)
            {
                return;
            }

            String engine = "goldsrc";
            //String engine = (game.Engine == Demo.EngineEnum.Source ? "source" : "goldsrc");

            // add all the "built in" maps listed in the game config
            foreach (GameConfig.Map map in game.GameConfig.Maps)
            {
                mapCollection.Add(new Map(true, map.Name, String.Format("{0}", map.Checksum), ""));
            }

            // enumerate all folders in maps\\*engine*\\*game folder* and assume they are checksums
            foreach (String directory in Directory.GetDirectories(Config.Settings.ProgramPath + String.Format("\\maps\\{0}\\{1}", engine, game.SteamGameInfo.GameFolder)))
            {
                String checksum = directory.Remove(0, directory.LastIndexOf('\\') + 1);

                try
                {
                    Convert.ToUInt32(checksum);
                }
                catch
                {
                    continue;
                }

                // enumerate all *.bsp files
                foreach (String bspFile in Directory.GetFiles(directory, "*.bsp"))
                {
                    String wadFiles = "";

                    // enumerate all *.wad files
                    foreach (String wadFile in Directory.GetFiles(directory, "*.wad"))
                    {
                        wadFiles += System.IO.Path.GetFileName(wadFile) + ";";
                    }

                    mapCollection.Add(new Map(false, System.IO.Path.GetFileName(bspFile), checksum, wadFiles));
                }
            }
        }

        #region Event Handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // game combobox
            uiGameComboBox.ItemsSource = gameCollection;
            uiGameComboBox.SelectedIndex = 0;

            // map list
            uiMapListView.ItemsSource = mapCollection;
            uiMapListView.Sort();
        }

        private void uiGameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshMapListView();
        }

        private void uiAddButton_Click(object sender, RoutedEventArgs e)
        {
            // get selected game
            Game game = (Game)uiGameComboBox.SelectedItem;

            if (game == null)
            {
                Common.Message(this, "Select a game first.");
                return;
            }

            // open file dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Title = "Add Map";
            dialog.Filter = "BSP files (*.bsp)|*.bsp";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog(this) == false)
            {
                return;
            }

            String mapFileName = System.IO.Path.GetFileName(dialog.FileName);

            // calculate checksum
            UInt32 checksum = 0;

            try
            {
                checksum = HalfLifeMapChecksum.Calculate(dialog.FileName);
            }
            catch (Exception ex)
            {
                Common.Message(this, String.Format("Error calculate map checksum for map \"{0}\"", mapFileName), ex, MessageWindow.Flags.Error);
                return;
            }

            // see if map is built-in
            if (game.GameConfig.MapExists(checksum, System.IO.Path.GetFileNameWithoutExtension(mapFileName)))
            {
                Common.Message(this, String.Format("The map \"{0}\" with the checksum \"{1}\" is a built-in map. It does not need to be added to the map pool.", mapFileName, checksum));
                return;
            }

            // see if the game map folder already exists
            //String engine = (game.Engine == Demo.EngineEnum.Source ? "source" : "goldsrc");
            String engine = "goldsrc";
            String gameMapFolder = Config.Settings.ProgramPath + String.Format("\\maps\\{0}\\{1}", engine, game.SteamGameInfo.GameFolder);

            if (!Directory.Exists(gameMapFolder))
            {
                Directory.CreateDirectory(gameMapFolder);
            }

            // see if the checksum folder already exists
            String checksumFolder = gameMapFolder + "\\" + String.Format("{0}", checksum);

            if (!Directory.Exists(checksumFolder))
            {
                Directory.CreateDirectory(checksumFolder);
            }

            // see if the map already exists
            String mapFullPath = checksumFolder + "\\" + mapFileName;

            if (File.Exists(mapFullPath))
            {
                Common.Message(this, String.Format("The map \"{0}\" with the checksum \"{1}\" already exists.", mapFileName, checksum));
                return;
            }

            // copy the map over
            File.Copy(dialog.FileName, mapFullPath);

            // update UI and inform the user of success
            RefreshMapListView();
            Common.Message(this, String.Format("Successfully added the map \"{0}\" with the checksum \"{1}\" to the map pool.", mapFileName, checksum));
        }

        private void uiRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // get selected game
            Game game = (Game)uiGameComboBox.SelectedItem;

            if (game == null)
            {
                Common.Message(this, "Select a game first.");
                return;
            }

            // get selected map
            Map map = (Map)uiMapListView.SelectedItem;

            if (map == null)
            {
                Common.Message(this, "Select a map first.");
                return;
            }

            // make sure map isn't built-in
            if (map.BuiltIn)
            {
                return;
            }

            // confirmation window
            if (Common.Message(this, "Are you sure you want to remove the select map from the map pool? The corresponding BSP file and any associated WAD files will be deleted.", null, MessageWindow.Flags.YesNo) != MessageWindow.Result.Yes)
            {
                return;
            }

            // delete the checksum folder and all contents
            //String engine = (game.Engine == Demo.EngineEnum.Source ? "source" : "goldsrc");
            String engine = "goldsrc";
            String checksumFolder = Config.Settings.ProgramPath + String.Format("\\maps\\{0}\\{1}\\{2}", engine, game.SteamGameInfo.GameFolder, map.Checksum);

            Directory.Delete(checksumFolder, true);

            // update UI
            RefreshMapListView();
        }

        private void uiCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
