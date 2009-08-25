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
    public class FlipBoolToVisibilityConverter : IValueConverter
    {
        public static FlipBoolToVisibilityConverter Instance = new FlipBoolToVisibilityConverter();

        #region IValueConverter Members        

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return Visibility.Hidden;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
                return true;
            return false;
        }

        #endregion
    }
}
