using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Main : ViewModelBase
    {
        public ViewModelBase Address { get; private set; }
        public ViewModelBase Demos { get; private set; }
        public ViewModelBase Demo { get; private set; }

        public Main()
            : this(new Address(), new Demos(), new Demo())
        {
        }

        public Main(ViewModelBase address, ViewModelBase demos, ViewModelBase demo)
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
