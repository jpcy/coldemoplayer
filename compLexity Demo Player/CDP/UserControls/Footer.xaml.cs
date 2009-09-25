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
using System.ComponentModel;
using System.Windows.Markup;

namespace CDP.UserControls
{
    [DefaultProperty("NestedContent")]
    [ContentProperty("NestedContent")]
    public partial class Footer : UserControl
    {
        public static readonly DependencyProperty NestedContentProperty;

        public object NestedContent
        {
            get { return GetValue(NestedContentProperty); }
            set { SetValue(NestedContentProperty, value); }
        }

        public Footer()
        {
            InitializeComponent();
        }

        static Footer()
        {
            NestedContentProperty = DependencyProperty.Register("NestedContent", typeof(object), typeof(Footer));
        }
    }
}
