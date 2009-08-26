using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;

namespace CDP.ViewModel
{
    public class Demos : ViewModelBase
    {
        private Core.Demo selectedItem;

        public ObservableCollection<Core.Demo> Items { get; private set; }
        public Core.Demo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != selectedItem)
                {
                    selectedItem = value;
                    mediator.Notify<Core.Demo>(Messages.SelectedDemoChanged, value);
                }
            }
        }

        private readonly IMediator mediator;
        private readonly INavigationService navigationService;
        private readonly Core.DemoManager demoManager;

        public Demos(IMediator mediator, INavigationService navigationService)
        {
            this.mediator = mediator;
            this.navigationService = navigationService;
        }

        public Demos() : this(Mediator.Instance, NavigationService.Instance)
        {
            Items = new ObservableCollection<Core.Demo>();
            demoManager = new Core.DemoManager();
        }

        public override void Initialise()
        {
            mediator.Register<string>(Messages.SelectedFolderChanged, SelectedFolderChanged, this);
            demoManager.AddPlugin(0, typeof(HalfLifeDemo.Demo), new HalfLifeDemo.Handler());
            demoManager.AddPlugin(1, typeof(CounterStrikeDemo.Demo), new CounterStrikeDemo.Handler());
        }

        public override void Initialise(object parameter)
        {
        }

        public void SelectedFolderChanged(string path)
        {
            Items.Clear();

            // TODO: check handlers for valid demo extensions
            foreach (string fileName in Directory.GetFiles(path, "*.dem", SearchOption.TopDirectoryOnly))
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
                Items.Add(demo);
            },
            (Core.Demo)sender);
        }
    }
}
