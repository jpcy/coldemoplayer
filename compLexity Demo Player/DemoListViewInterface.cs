using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace compLexity_Demo_Player
{
    public interface IDemoListView
    {
        void DemoLoadingFinished(Demo demo);
    }

    public partial class DemoListView : IDemoListView
    {
        /// <summary>
        /// Callback for when a demo is finished loading. Should still be called, but with the "demo" parameter as null if loading fails. Adds the demo to the listview if demo is not null.
        /// </summary>
        /// <param name="demo"></param>
        public void DemoLoadingFinished(Demo demo)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Procedure(() =>
            {
                DecreaseThreadPoolCount();

                if (demo != null) // did the demo load successfully?
                {
                    demoCollection.Add(new DemoListViewData(demo, FindIcon(demo)));
                }

                if (threadPoolCount == 0 && threadPoolFull)
                {
                    SelectDemo(demoNameToSelect);
                    threadPool.Clear();
                }
            }));
        }
    }
}
