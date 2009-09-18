using System;
using System.Windows;
using System.Windows.Navigation;

namespace CDP
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
            CDP.INavigationService service = Core.ObjectCreator.Get<CDP.INavigationService>();
            service.Window = this;
            service.Navigate(new View.Main(), new ViewModel.Main());
        }
    }
}
