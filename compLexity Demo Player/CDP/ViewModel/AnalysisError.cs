using System;

namespace CDP.ViewModel
{
    class AnalysisError : Core.ViewModelBase
    {
        public Error ErrorViewModel { get; private set; }

        public AnalysisError(string errorMessage, Exception exception)
        {
            ErrorViewModel = new Error(errorMessage, exception);
        }
    }
}
