using System;
using System.Windows;
using System.Windows.Navigation;

namespace CDP.Gui
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CDP.Gui.INavigationService service = Core.ObjectCreator.Get<CDP.Gui.INavigationService>();
            service.Window = this;
            service.Navigate(new View.Main(), new ViewModel.Main());
        }
    }
}