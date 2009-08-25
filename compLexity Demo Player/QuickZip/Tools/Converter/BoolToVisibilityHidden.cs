using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace QuickZip.Tools
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class BoolToVisibilityHiddenConverter : IValueConverter
    {
        public static BoolToVisibilityHiddenConverter Instance = new BoolToVisibilityHiddenConverter();

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return Visibility.Visible;
            else return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;
            return false;
        }

        #endregion
    }
}
