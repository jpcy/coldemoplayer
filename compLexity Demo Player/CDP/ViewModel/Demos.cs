using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace CDP.ViewModel
{
    public class Demos : Core.ViewModelBase
    {
        public class Item : Core.NotifyPropertyChanged
        {
            public Core.Demo Demo { get; private set; }

            public BitmapImage Icon { get; private set; }
            public string Name
            {
                get { return Demo.Name; }
            }
            public string Game
            {
                get { return Demo.GameName; }
            }
            public string Map
            {
                get { return Demo.MapName; }
            }
            public string Perspective
            {
                get { return Demo.Perspective; }
            }
            public TimeSpan Duration
            {
                get { return Demo.Duration; }
            }

            public Item(Core.Demo demo, BitmapImage icon)
            {
                Demo = demo;
                Icon = icon;
            }
        }

        private Item selectedItem;
        private object dirtyLock = new object();
        private bool isDirty = false;

        public ObservableCollection<Item> Items { get; private set; }
        public Item SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetDirty();

                if (value != selectedItem)
                {
                    settings["LastFileName"] = value.Demo.FileName;
                    selectedItem = value;
                    mediator.Notify<Core.Demo>(Messages.SelectedDemoChanged, selectedItem == null ? null : selectedItem.Demo, true);
                }
            }
        }

        private readonly IMediator mediator = Core.ObjectCreator.Get<IMediator>();
        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();
        private readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private readonly IconCache iconCache = new IconCache();
        private readonly Core.IDemoManager demoManager = Core.ObjectCreator.Get<Core.IDemoManager>();
        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();

        public Demos()
        {
            Items = new ObservableCollection<Item>();
            mediator.Register<string>(Messages.SelectedFolderChanged, SelectedFolderChanged, this);
        }

        public void SelectedFolderChanged(string path)
        {
            Items.Clear();

            if (!Directory.Exists(path))
            {
                return;
            }

            string[] validExtensions = demoManager.ValidDemoExtensions();

            foreach (string fileName in Directory.GetFiles(path).Where(f => validExtensions.Contains(fileSystem.GetExtension(f))))
            {
                Core.Demo demo = demoManager.CreateDemo(fileName);

                if (demo == null)
                {
                    continue;
                }

                demo.OperationErrorEvent += demo_OperationErrorEvent;
                demo.OperationCompleteEvent += demo_OperationCompleteEvent;

                ThreadPool.QueueUserWorkItem(new WaitCallback(o => 
                {
                    demo.Load();
                }));
            }
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            navigationService.Invoke((demo, window, errorMessage) => 
            {
                demo.OperationErrorEvent -= demo_OperationErrorEvent;
                demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
                System.Windows.MessageBox.Show(window, errorMessage);
            },
            (Core.Demo)sender, navigationService.Window, e.ErrorMessage);
        }

        void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            navigationService.Invoke(demo =>
            {
                demo.OperationErrorEvent -= demo_OperationErrorEvent;
                demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
                Item item = new Item(demo, iconCache.FindIcon(demo.IconFileNames));
                Items.Add(item);

                if (!IsDirty() && demo.FileName == (string)settings["LastFileName"])
                {
                    SelectedItem = item;
                    OnPropertyChanged("SelectedItem");
                }
            },
            (Core.Demo)sender);
        }

        private bool IsDirty()
        {
            lock (dirtyLock)
            {
                return isDirty;
            }
        }

        private void SetDirty()
        {
            lock (dirtyLock)
            {
                isDirty = true;
            }
        }
    }
}
