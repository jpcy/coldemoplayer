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

        public Main()
            : this(new Address(), new Demos(), new Demo())
        {
        }

        public Main(Core.ViewModelBase address, Core.ViewModelBase demos, Core.ViewModelBase demo)
        {
            Address = address;
            Demos = demos;
            Demo = demo;
        }

        public override void Initialise()
        {
            Address.Initialise();
            Demos.Initialise();
            Demo.Initialise();
        }

        public override void Initialise(object parameter)
        {
        }
    }
}
