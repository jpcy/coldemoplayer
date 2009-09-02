using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Main : Core.ViewModelBase
    {
        public Core.ViewModelBase Address { get; private set; }
        public Core.ViewModelBase Demos { get; private set; }
        public Core.ViewModelBase Demo { get; private set; }

        private readonly Core.DemoManager demoManager;

        public Main()
            : this(new Address(), new Demos(), new Demo())
        {
        }

        public Main(Core.ViewModelBase address, Core.ViewModelBase demos, Core.ViewModelBase demo)
        {
            Address = address;
            Demos = demos;
            Demo = demo;
            demoManager = new Core.DemoManager();
        }

        public override void Initialise()
        {
            demoManager.AddPlugin(0, typeof(HalfLifeDemo.Demo), new HalfLifeDemo.Handler(), typeof(HalfLifeDemo.Launcher));
            demoManager.AddPlugin(1, typeof(CounterStrikeDemo.Demo), new CounterStrikeDemo.Handler(), typeof(HalfLifeDemo.Launcher));
            Core.Settings.Instance.LoadDemoConfig(demoManager.GetAllDemoHandlerSettings());

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
