using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;

namespace CDP.Gui
{
    [ContentProperty("Text")]
    internal class IconButton : Button
    {
        public static readonly DependencyProperty IconSourceProperty;
        public static readonly DependencyProperty TextProperty;

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
            IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(IconButton));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(IconButton));
        }
    }
}
