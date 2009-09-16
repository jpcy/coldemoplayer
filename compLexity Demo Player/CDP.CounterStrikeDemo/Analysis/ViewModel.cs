using System;
using System.Linq;

namespace CDP.CounterStrikeDemo.Analysis
{
    public class ViewModel : HalfLifeDemo.Analysis.ViewModel
    {
        public ViewModel(Demo demo) : base(demo)
        {
            demo.AddMessageCallback<HalfLifeDemo.Messages.SvcEventReliable>(MessageEventReliable);
        }

        #region Message handlers
        private void MessageEventReliable(HalfLifeDemo.Messages.SvcEventReliable message)
        {
            // TODO: should be 29 for 1.6, 26 for everything else.
            if (message.Index == 29)
            {
                NewRound();
            }
        }
        #endregion
    }
}
