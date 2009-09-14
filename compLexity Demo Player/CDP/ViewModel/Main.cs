using System;
using CDP.Core;

namespace CDP.ViewModel
{
    public class Main : ViewModelBase
    {
        public Header Header { get; private set; }
        public Address Address { get; private set; }
        public Demos Demos { get; private set; }
        public Demo Demo { get; private set; }

        private readonly ISettings settings = ObjectCreator.Get<ISettings>();
        private readonly IDemoManager demoManager = ObjectCreator.Get<IDemoManager>();

        public Main()
        {
            demoManager.AddPlugin(0, new HalfLifeDemo.Handler());
            demoManager.AddPlugin(1, new CounterStrikeDemo.Handler());

            foreach (Setting setting in demoManager.GetAllDemoHandlerSettings())
            {
                settings.Add(setting);
            }

            settings.Load();

            Header = new Header();
            Address = new Address();
            Demos = new Demos();
            Demo = new Demo();
        }
    }
}
