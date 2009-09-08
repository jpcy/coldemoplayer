using System;

namespace CDP.ViewModel
{
    public class Header : Core.ViewModelBase
    {
        public DelegateCommand OptionsCommand { get; private set; }
        public DelegateCommand AboutCommand { get; private set; }

        public override void Initialise()
        {
            OptionsCommand = new DelegateCommand(OptionsCommandExecute);
            AboutCommand = new DelegateCommand(AboutCommandExecute);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }

        public void OptionsCommandExecute()
        {
            System.Windows.MessageBox.Show("options");
        }

        public void AboutCommandExecute()
        {
            System.Windows.MessageBox.Show("about");
        }
    }
}
