using System;
using System.Threading;

namespace CDP.Gui.ViewModel
{
    class Play : Core.ViewModelBase
    {
        public string Caption
        {
            get { return string.Format(Strings.Play_Processing, demo.Name); }
        }

        private int progress = 0;
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public Core.DelegateCommand CancelCommand { get; private set; }

        private readonly Core.Demo demo;
        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();
        private readonly Core.IDemoManager demoManager = Core.ObjectCreator.Get<Core.IDemoManager>();
        private Core.Launcher launcher;

        public Play(Core.Demo demo)
        {
            this.demo = demo;
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;
            demo.OperationCancelledEvent += demo_OperationCancelledEvent;
            CancelCommand = new Core.DelegateCommand(CancelCommandExecute);
        }

        public override void OnNavigateComplete()
        {
            launcher = demoManager.CreateLauncher(demo);

            if (!launcher.Verify())
            {
                // TODO: proper message page
                System.Windows.MessageBox.Show(launcher.Message);
                navigationService.Home();
                return;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
            {
                demo.Write(launcher.CalculateDestinationFileName());
            }));
        }

        public void CancelCommandExecute()
        {
            demo.CancelOperation();
        }

        void demo_ProgressChangedEvent(object sender, Core.Demo.ProgressChangedEventArgs e)
        {
            navigationService.Invoke(new Action<int>(p => Progress = p), e.Progress);
        }

        void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            RemoveDemoWriteEventHandlers();
            navigationService.Invoke(new Action(() =>
            {
                if (launcher.CanMonitorProcess)
                {
                    navigationService.HideWindow();
                    launcher.Launch();
                    launcher.ProcessFound += launcher_ProcessFound;
                    launcher.ProcessClosed += launcher_ProcessClosed;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                    {
                        launcher.MonitorProcessWorker();
                    }));
                }
                else
                {
                    launcher.Launch();
                    navigationService.Home();
                }
            }));
        }

        void launcher_ProcessClosed(object sender, EventArgs e)
        {
            launcher.ProcessFound -= launcher_ProcessFound;
            launcher.ProcessClosed -= launcher_ProcessClosed;

            navigationService.Invoke(new Action(() =>
            {
                navigationService.ShowWindow();
                navigationService.Home();
            }));
        }

        void launcher_ProcessFound(object sender, CDP.Core.Launcher.ProcessFoundEventArgs e)
        {
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            RemoveDemoWriteEventHandlers();
            navigationService.Invoke(new Action(() => navigationService.Home()));
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            RemoveDemoWriteEventHandlers();
            navigationService.Invoke(new Action<string, Exception>((msg, ex) =>
            {
                navigationService.Navigate(new View.Error(), new Error(msg, ex));
            }), e.ErrorMessage, e.Exception);
        }

        private void RemoveDemoWriteEventHandlers()
        {
            demo.ProgressChangedEvent -= demo_ProgressChangedEvent;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
            demo.OperationCancelledEvent -= demo_OperationCancelledEvent;
        }
    }
}
