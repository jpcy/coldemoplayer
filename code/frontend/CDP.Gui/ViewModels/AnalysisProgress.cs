using System;
using System.Threading;

namespace CDP.Gui.ViewModels
{
    internal class AnalysisProgress : Core.ViewModelBase
    {
        public string Caption
        {
            get { return string.Format(Strings.Analysis_Progress, demo.Name); }
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
                // TODO: should be a warning, then show incomplete analysis.
                navigationService.Navigate(new Views.DemoError(), new DemoError(demo.FileName, msg, ex));
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
