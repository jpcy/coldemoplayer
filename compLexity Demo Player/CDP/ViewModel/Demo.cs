using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDP.ViewModel
{
    public class Demo : ViewModelBase
    {
        public Core.Demo Data { get; private set; }
        
        private readonly IMediator mediator;

        public Demo()
            : this(Mediator.Instance)
        {
        }

        public Demo(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public override void Initialise()
        {
            mediator.Register<Core.Demo>(Messages.SelectedDemoChanged, SelectedDemoChanged, this);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }

        public void SelectedDemoChanged(Core.Demo demo)
        {
            Data = demo;
            OnPropertyChanged("Data");
        }
    }
}
