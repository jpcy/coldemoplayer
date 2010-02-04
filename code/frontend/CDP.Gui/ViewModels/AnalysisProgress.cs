using System;
using System.Threading;
using CDP.Core.Extensions;

namespace CDP.Gui.ViewModels
{
    internal class AnalysisProgress : Core.ViewModelBase
    {
        public string Caption
        {
            get { return Strings.Analysis_Progress.Args(demo.Name); }
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

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();
        private readonly Core.Demo demo;
        private readonly Analysis analysisViewModel;

        public AnalysisProgress(Core.Demo demo)
        {
            this.demo = demo;
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;
            demo.OperationCancelledEvent += demo_OperationCancelledEvent;
            CancelCommand = new Core.DelegateCommand(CancelCommandExecute);
            analysisViewModel = new Analysis(demo);
        }

        public override void OnNavigateComplete()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
            {
                demo.Read();
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
            RemoveEventHandlers();
            navigationService.Invoke(new Action(() => navigationService.Navigate(new Views.Analysis(), analysisViewModel)));
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            RemoveEventHandlers();
            navigationService.Invoke(new Action(() => navigationService.Home()));
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            RemoveEventHandlers();
            navigationService.Invoke(new Action<Core.Demo, string, Exception>((demo, msg, ex) =>
            {
                Action onContinue = delegate
                {
                    navigationService.Navigate(new Views.Analysis(), analysisViewModel);
                };

                Action onCancel = delegate
                {
                    navigationService.Home();
                };

                navigationService.Navigate(new Views.DemoWarning(), new DemoWarning(demo.FileName, Strings.AnalysisError, ex, onContinue, onCancel));
            }), (Core.Demo)sender, e.ErrorMessage, e.Exception);
        }

        private void RemoveEventHandlers()
        {
            demo.ProgressChangedEvent -= demo_ProgressChangedEvent;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
            demo.OperationCancelledEvent -= demo_OperationCancelledEvent;
        }
    }
}
