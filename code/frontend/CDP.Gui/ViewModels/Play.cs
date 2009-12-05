using System;
using System.Threading;

namespace CDP.Gui.ViewModels
{
    internal class Play : Core.ViewModelBase
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
            demo.OperationWarningEvent += demo_OperationWarningEvent;
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
                navigationService.Navigate(new Views.DemoInformation(), new DemoInformation(demo.FileName, launcher.Message));
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

        void demo_OperationWarningEvent(object sender, Core.Demo.OperationWarningEventArgs e)
        {
            navigationService.Invoke(new Action<Core.Demo, string, Exception>((demo, msg, ex) =>
            {
                Action onContinue = delegate
                {
                    // Go back to Play view.
                    navigationService.Back();
                    demo.SetOperationWarningResult(Core.Demo.OperationWarningResults.Continue);
                };

                Action onCancel = delegate
                {
                    // Set result to cancel and wait for the demo to raise OperationCancelledEvent.
                    demo.SetOperationWarningResult(Core.Demo.OperationWarningResults.Cancel);
                };

                navigationService.Navigate(new Views.DemoWarning(), new DemoWarning(demo.FileName, msg, ex, onContinue, onCancel));
            }), (Core.Demo)sender, e.Message, e.Exception);
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            RemoveDemoWriteEventHandlers();
            navigationService.Invoke(new Action<Core.Demo, string, Exception>((demo, msg, ex) =>
            {
                navigationService.Navigate(new Views.DemoError(), new DemoError(demo.FileName, msg, ex));
            }), (Core.Demo)sender, e.ErrorMessage, e.Exception);
        }

        private void RemoveDemoWriteEventHandlers()
        {
            demo.ProgressChangedEvent -= demo_ProgressChangedEvent;
            demo.OperationWarningEvent -= demo_OperationWarningEvent;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
            demo.OperationCancelledEvent -= demo_OperationCancelledEvent;
        }
    }
}
