using System;
using CDP.Core;

namespace CDP.Gui.ViewModel
{
    internal class Main : ViewModelBase
    {
        public Header Header { get; private set; }
        public Address Address { get; private set; }
        public Demos Demos { get; private set; }
        public Demo Demo { get; private set; }

        public Main()
        {
            Header = new Header();
            Address = new Address();
            Demos = new Demos();
            Demo = new Demo();
            Header.OnNavigateComplete();
            Address.OnNavigateComplete();
            Demos.OnNavigateComplete();
            Demo.OnNavigateComplete();
        }
    }
}
