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

        public AnalysisProgress(Core.Demo demo)
        {
            ProgressViewModel = new Progress();
            ProgressViewModel.CancelEvent += new EventHandler(ProgressViewModel_CancelEvent);

            this.demo = demo;
            demo.ProgressChangedEvent += new EventHandler<Core.Demo.ProgressChangedEventArgs>(demo_ProgressChangedEvent);
            demo.OperationErrorEvent += new EventHandler<Core.Demo.OperationErrorEventArgs>(demo_OperationErrorEvent);
            demo.OperationCompleteEvent += new EventHandler(demo_OperationCompleteEvent);
            demo.OperationCancelledEvent += new EventHandler(demo_OperationCancelledEvent);

            analysisViewModel = new Analysis(demo);
        }

         public override void OnNavigateComplete()
        {
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
            navigationService.Invoke(new Action(() => navigationService.Navigate(new View.Analysis(), analysisViewModel)));
        }

        void demo_OperationCancelledEvent(object sender, EventArgs e)
        {
            navigationService.Invoke(new Action(() => navigationService.Home()));
        }

        void demo_OperationErrorEvent(object sender, Core.Demo.OperationErrorEventArgs e)
        {
            navigationService.Invoke(new Action<string, Exception>((msg, ex) =>
            {
                navigationService.Navigate(new View.AnalysisError(), new AnalysisError(msg, ex));
            }), e.ErrorMessage, e.Exception);
        }
    }
}
