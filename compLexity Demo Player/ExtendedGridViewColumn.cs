using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace compLexity_Demo_Player
{
    public class ExtendedGridViewColumn : GridViewColumn
    {
        public string SortPropertyName
        {
            get { return (String)GetValue(SortPropertyNameProperty); }
            set { SetValue(SortPropertyNameProperty, value); }
        }

        public static readonly DependencyProperty SortPropertyNameProperty = DependencyProperty.Register("SortPropertyName", typeof(String), typeof(ExtendedGridViewColumn), new UIPropertyMetadata(""));

        public Boolean IsDefaultSortColumn
        {
            get { return (Boolean)GetValue(IsDefaultSortColumnProperty); }
            set { SetValue(IsDefaultSortColumnProperty, value); }
        }

        public static readonly DependencyProperty IsDefaultSortColumnProperty = DependencyProperty.Register("IsDefaultSortColumn", typeof(Boolean), typeof(ExtendedGridViewColumn), new UIPropertyMetadata(false));

        public Boolean IsLowPriority
        {
            get { return (Boolean)GetValue(IsLowPriorityProperty); }
            set { SetValue(IsLowPriorityProperty, value); }
        }

        public static readonly DependencyProperty IsLowPriorityProperty = DependencyProperty.Register("IsLowPriority", typeof(Boolean), typeof(ExtendedGridViewColumn), new UIPropertyMetadata(false));

        public Boolean IsFillColumn
        {
            get { return (Boolean)GetValue(IsFillColumnProperty); }
            set { SetValue(IsFillColumnProperty, value); }
        }

        public static readonly DependencyProperty IsFillColumnProperty = DependencyProperty.Register("IsFillColumn", typeof(Boolean), typeof(ExtendedGridViewColumn), new UIPropertyMetadata(false));
    }
}
