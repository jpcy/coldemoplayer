using System;
using System.Windows.Documents;

namespace CDP.HalfLifeDemo.Analysis
{
    class ViewModel : Core.ViewModelBase
    {
        public FlowDocument GameLogDocument { get; private set; }

        private readonly Demo demo;
        private readonly Core.IFlowDocumentWriter gameLog = Core.ObjectCreator.Get<Core.IFlowDocumentWriter>();

        public ViewModel(Core.Demo demo)
        {
            this.demo = (Demo)demo;
            this.demo.AddMessageCallback<Messages.SvcPrint>(MessagePrint);
            GameLogDocument = new FlowDocument();
        }

        public override void OnNavigateComplete()
        {
            gameLog.Save(GameLogDocument);
            OnPropertyChanged("GameLogDocument");
        }

        private void MessagePrint(Messages.SvcPrint message)
        {
            gameLog.Write(message.Text + "\n");
        }
    }
}
