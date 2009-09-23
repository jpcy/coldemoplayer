using System;
using System.Threading;

namespace CDP.ViewModel
{
    public class AnalysisProgress : Core.ViewModelBase
    {
        public Progress ProgressViewModel { get; private set; }

        private readonly INavigationService navigationService = Core.ObjectCreator.Get<INavigationService>();
        private readonly Core.Demo demo;
        private readonly Analysis analysisViewModel;
        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public AnalysisProgress(Core.Demo demo)
        {
            ProgressViewModel = new Progress();
            ProgressViewModel.CancelEvent += ProgressViewModel_CancelEvent;

            this.demo = demo;
            demo.ProgressChangedEvent += demo_ProgressChangedEvent;
            demo.OperationErrorEvent += demo_OperationErrorEvent;
            demo.OperationCompleteEvent += demo_OperationCompleteEvent;
            demo.OperationCancelledEvent += demo_OperationCancelledEvent;

            analysisViewModel = new Analysis(demo);
        }

        public override void OnNavigateComplete()
        {
            sw.Start();

            ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
            {
                demo.Read();
            }));
        }

        void ProgressViewModel_CancelEvent(object sender, EventArgs e)
        {
            demo.CancelOperation();
        }

        void demo_ProgressChangedEvent(object sender, Core.Demo.ProgressChangedEventArgs e)
        {
            navigationService.Invoke(new Action<int>(progress => ProgressViewModel.Value = progress), e.Progress);
        }

        void demo_OperationCompleteEvent(object sender, EventArgs e)
        {
            sw.Stop();
            System.Windows.MessageBox.Show(sw.Elapsed.ToString());
            RemoveEventHandlers();
            navigationService.Invoke(new Action(() => navigationService.Navigate(new View.Analysis(), analysisViewModel)));
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            RemoveEventHandlers();
            navigationService.Invoke(new Action(() => navigationService.Home()));
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            RemoveEventHandlers();
            navigationService.Invoke(new Action<string, Exception>((msg, ex) =>
            {
                navigationService.Navigate(new View.AnalysisError(), new AnalysisError(msg, ex));
            }), e.ErrorMessage, e.Exception);
        }

        private void RemoveEventHandlers()
        {
            ProgressViewModel.CancelEvent -= ProgressViewModel_CancelEvent;
            demo.ProgressChangedEvent -= demo_ProgressChangedEvent;
            demo.OperationErrorEvent -= demo_OperationErrorEvent;
            demo.OperationCompleteEvent -= demo_OperationCompleteEvent;
            demo.OperationCancelledEvent -= demo_OperationCancelledEvent;
        }
    }
}
