using System;
using CDP.Core;

namespace CDP.ViewModel
{
    public class Main : ViewModelBase
    {
        public ViewModelBase Address { get; private set; }
        public ViewModelBase Demos { get; private set; }
        public ViewModelBase Demo { get; private set; }

        private readonly ISettings settings;
        private readonly IDemoManager demoManager;

        public Main()
            : this(Settings.Instance, new DemoManager(), new Address(), new Demos(), new Demo())
        {
        }

        public Main(ISettings settings, IDemoManager demoManager, ViewModelBase address, ViewModelBase demos, ViewModelBase demo)
        {
            this.settings = settings;
            this.demoManager = demoManager;
            Address = address;
            Demos = demos;
            Demo = demo;
        }

        public override void Initialise()
        {
            demoManager.AddPlugin(0, new HalfLifeDemo.Handler());
            demoManager.AddPlugin(1, new CounterStrikeDemo.Handler());

            foreach (Setting setting in demoManager.GetAllDemoHandlerSettings())
            {
                settings.Add(setting);
            }

            settings.Load();
            Address.Initialise();
            Demos.Initialise(demoManager);
            Demo.Initialise(demoManager);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
