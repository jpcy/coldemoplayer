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
            demoManager.AddPlugin(0, typeof(HalfLifeDemo.Demo), new HalfLifeDemo.Handler(), typeof(HalfLifeDemo.Launcher));
            demoManager.AddPlugin(1, typeof(CounterStrikeDemo.Demo), new CounterStrikeDemo.Handler(), typeof(HalfLifeDemo.Launcher));

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
