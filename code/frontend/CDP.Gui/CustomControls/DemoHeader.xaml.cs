using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CDP.Gui.CustomControls
{
    [DefaultProperty("NestedContent")]
    [ContentProperty("NestedContent")]
    internal partial class DemoHeader : UserControl
    {
        public static readonly DependencyProperty NestedContentProperty;

        public object NestedContent
        {
            get { return GetValue(NestedContentProperty); }
            set { SetValue(NestedContentProperty, value); }
        }

        public DemoHeader()
        {
            InitializeComponent();
        }

        static DemoHeader()
        {
            NestedContentProperty = DependencyProperty.Register("NestedContent", typeof(object), typeof(DemoHeader));
        }
    }
}
