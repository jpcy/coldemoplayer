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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel; // ObservableCollection
using System.Collections; // ArrayList
using System.Threading; // Thread
using System.Windows.Threading;
using System.IO;

namespace compLexity_Demo_Player
{
    public partial class DemoListView : ExtendedListView
    {
        private class DemoListViewData : NotifyPropertyChangedItem
        {
            public BitmapImage Icon { get; set; }
            public String Name { get; set; }
            public String Game { get; set; }
            public String Perspective { get; set; }
            public String Duration { get; set; }
            public Demo Demo { get; set; }

            public DemoListViewData(Demo demo, BitmapImage icon)
            {
                Icon = icon;
                Name = demo.Name;
                Game = demo.GameName;
                Perspective = demo.PerspectiveString;

                if (demo.Status == Demo.StatusEnum.Ok)
                {
                    Duration = Common.DurationString(demo.DurationInSeconds);
                }
                else
                {
                    Duration = "Unknown";
                }

                Demo = demo;
            }
        }

        private ObservableCollection<DemoListViewData> demoCollection;
        private String currentPath = "";
        private String demoNameToSelect;

        private List<Thread> threadPool;
        private Boolean threadPoolFull;
        private Int32 threadPoolCount; // so the Demo class doesn't have to keep track of its own loading thread
        private readonly Object threadPoolCountLock;

        private List<BitmapImage> iconList;
        private BitmapImage unknownIcon;

        public DemoListView()
        {
            demoCollection = new ObservableCollection<DemoListViewData>();
            ItemsSource = demoCollection;

            iconList = new List<BitmapImage>();
            unknownIcon = new BitmapImage(new Uri(Config.Settings.ProgramPath + "\\icons\\unknown.ico"));

            threadPool = new List<Thread>();
            threadPoolCountLock = new Object();

            Initialized += new EventHandler(DemoListView_Initialized);
        }

        void DemoListView_Initialized(object sender, EventArgs e)
        {
        }

        public String GetCurrentPath()
        {
            return currentPath;
        }

        public void SetCurrentPath(String path)
        {
            SetCurrentPath(path, null);
        }

        public void SetCurrentPath(String path, String demoNameToSelect)
        {
            // really should refresh the demo list
            /*if (path == currentPath)
            {
                SelectDemo(demoNameToSelect);
                return;
            }*/

            this.demoNameToSelect = demoNameToSelect;

            // clear control
            demoCollection.Clear();

            // abort all running threads from thread pool
            foreach (Thread t in threadPool)
            {
                Common.AbortThread(t);
            }

            // check that directory exists, that path isn't blank
            if (path == "" || !Directory.Exists(path))
            {
                return;
            }

            currentPath = path;

            // search folder for *.dem files
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            threadPoolFull = false;
            threadPoolCount = 0;

            foreach (FileInfo fi in directoryInfo.GetFiles("*.dem", SearchOption.TopDirectoryOnly))
            {
                Thread thread;

                try
                {
                    Demo demo = DemoFactory.CreateDemo(fi.FullName);
                    thread = demo.Read((IMainWindow)((Grid)Parent).Parent, (IDemoListView)this);
                }
                catch (Exception ex)
                {
                    /*
                    * Only show an error message here if the user explicitly opened this demo.
                    * The only errors that should be happening are if the demo isn't a valid HL/Source engine demo or if there's some problem opening the file.
                    * 
                    * Any errors that occur in the demo reading thread are always shown (the thread handles this).
                    */
                    if (String.Equals(System.IO.Path.GetFileNameWithoutExtension(fi.Name), demoNameToSelect))
                    {
                        Common.Message((Window)((Grid)Parent).Parent, "Error loading demo file \"" + fi.FullName + "\"", ex, MessageWindow.Flags.Error);
                    }

                    // don't add it to the thread pool
                    continue;
                }

                threadPool.Add(thread);
                threadPoolCount++;
            }

            threadPoolFull = true;
        }

        public void SelectDemo(String name)
        {
            if (name == null)
            {
                if (demoCollection.Count > 0)
                {
                    SelectedIndex = 0;
                }
            }
            else
            {
                for (Int32 i = 0; i < demoCollection.Count; i++)
                {
                    DemoListViewData lvd = (DemoListViewData)demoCollection[i];

                    if (lvd.Name == name)
                    {
                        SelectedItem = lvd;
                        ScrollIntoView(lvd);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the currently selected demo.
        /// </summary>
        /// <returns>The selected demo, or null if one is not selected.</returns>
        public Demo GetSelectedDemo()
        {
            DemoListViewData selectedItemData = (DemoListViewData)SelectedItem;

            if (selectedItemData == null)
            {
                return null;
            }

            return selectedItemData.Demo;
        }

        private void DecreaseThreadPoolCount()
        {
            lock (threadPoolCountLock)
            {
                threadPoolCount--;
            }
        }

        // FIXME: this method looks like it was written by a fucking retard
        private BitmapImage FindIcon(Demo demo)
        {
            String engineTypeName = "";

            if (demo.Engine == Demo.EngineEnum.Source)
            {
                engineTypeName = "source";
            }
            else
            {
                engineTypeName = "goldsrc";
            }

            BitmapImage icon = null;

            // try icons\engine\game folder.ico
            icon = LoadIcon(Config.Settings.ProgramPath + "\\icons\\" + engineTypeName + "\\" + demo.GameFolderName + ".ico");

            if (icon != null)
            {
                return icon;
            }

            if (demo.Engine == Demo.EngineEnum.HalfLife)
            {
                // check that hl.exe path is set
                if (!File.Exists(Config.Settings.HlExeFullPath))
                {
                    return unknownIcon;
                }

                icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.HlExeFullPath) + "\\" + demo.GameFolderName + "\\" + demo.GameFolderName + ".ico");

                if (icon != null)
                {
                    return icon;
                }

                // special cases
                if (demo.GameFolderName == "tfc")
                {
                    icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.HlExeFullPath) + "\\TeamFortressClassic.ico");

                    if (icon != null)
                    {
                        return icon;
                    }
                }
                else if (demo.GameFolderName == "valve")
                {
                    icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.HlExeFullPath) + "\\valve.ico");

                    if (icon != null)
                    {
                        return icon;
                    }
                }
            }
            else
            {
                // check that steam.exe path is set and that the demo has corresponding steam app information
                Game game = GameManager.Find(demo);

                if (!File.Exists(Config.Settings.SteamExeFullPath) || game == null)
                {
                    return unknownIcon;
                }

                // try Steam\steam\games\x.ico, where x is the game name. e.g. counter-strike
                icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\steam\\games\\" + game.FolderExtended + ".ico");

                if (icon != null)
                {
                    return icon;
                }

                // try game folder, game.ico. e.g. counter-strike\cstrike\game.ico
                icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder + "\\" + game.FolderExtended + "\\" + demo.GameFolderName + "\\game.ico");

                if (icon != null)
                {
                    return icon;
                }

                // try game folder, resource\game.ico.
                icon = LoadIcon(System.IO.Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder + "\\" + game.FolderExtended + "\\" + demo.GameFolderName + "\\resource\\game.ico");

                if (icon != null)
                {
                    return icon;
                }
            }

            return unknownIcon;
        }

        private BitmapImage LoadIcon(String fileName)
        {
            // search cache for matching icon
            foreach (BitmapImage bi in iconList)
            {
                if (bi.UriSource.Equals(new Uri(fileName)))
                {
                    return bi;
                }
            }

            // load new icon
            BitmapImage icon = null;

            try
            {
                icon = new BitmapImage(new Uri(fileName));
            }
            catch (Exception)
            {
                return null;
            }

            // add icon to cache
            icon.Freeze();
            iconList.Add(icon);

            return icon;
        }
    }
}
