using System;
using System.Windows;
using System.Windows.Navigation;

namespace CDP
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CDP.NavigationService service = CDP.NavigationService.Instance;
            service.Window = this;
            service.Navigate("Main");
        }
    }
}
