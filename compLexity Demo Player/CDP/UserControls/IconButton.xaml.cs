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

namespace CDP.UserControls
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
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(IconButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCommandChanged)));
            IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(IconButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIconSourceChanged)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(IconButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));
        }

        public IconButton()
        {
            InitializeComponent();
        }

        private static void OnCommandChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.buttonControl.Command = iconButton.Command;

        }
        private static void OnIconSourceChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.iconControl.Source = iconButton.IconSource;
        }

        private static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.textControl.Text = iconButton.Text;
        }
    }
}
