using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace CDP.Gui.UserControls
{
    [ContentProperty("Text")]
    public partial class IconButton : UserControl
    {
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty IconSourceProperty;
        public static readonly DependencyProperty TextProperty;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        static IconButton()
        {
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(IconButton));
            IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(IconButton));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(IconButton));
        }

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
