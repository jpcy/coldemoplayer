using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace CDP.ViewModel
{
    public class Demos : Core.ViewModelBase
    {
        public class Item
        {
            public Core.Demo Demo { get; private set; }
            public BitmapImage Icon { get; private set; }

            public Item(Core.Demo demo, BitmapImage icon)
            {
                Demo = demo;
                Icon = icon;
            }
        }

        private Item selectedItem;

        public ObservableCollection<Item> Items { get; private set; }
        public Item SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != selectedItem)
                {
                    selectedItem = value;
                    mediator.Notify<Core.Demo>(Messages.SelectedDemoChanged, selectedItem.Demo);
                }
            }
        }

        private readonly IMediator mediator;
        private readonly INavigationService navigationService;
        private readonly Core.IFileSystem fileSystem;
        private readonly IconCache iconCache = new IconCache();
        private Core.DemoManager demoManager;

        public Demos(IMediator mediator, INavigationService navigationService, Core.IFileSystem fileSystem)
        {
            this.mediator = mediator;
            this.navigationService = navigationService;
            this.fileSystem = fileSystem;
        }

        public Demos() : this(Mediator.Instance, NavigationService.Instance, new Core.FileSystem())
        {
            Items = new ObservableCollection<Item>();
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void Initialise(object parameter)
        {
            demoManager = (Core.DemoManager)parameter;
            mediator.Register<string>(Messages.SelectedFolderChanged, SelectedFolderChanged, this);
        }

        public void SelectedFolderChanged(string path)
        {
            Items.Clear();

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
                Items.Add(new Item(demo, iconCache.FindIcon(demo.IconFileNames)));
            },
            (Core.Demo)sender);
        }
    }
}
