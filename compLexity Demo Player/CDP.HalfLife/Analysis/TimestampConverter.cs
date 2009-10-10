using System;
using System.Windows.Data;
using System.Globalization;

namespace CDP.HalfLife.Analysis
{
    [ValueConversion(typeof(float), typeof(string))]
    class TimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            TimeSpan ts = new TimeSpan(0, 0, (int)(float)value);
            return string.Format("{0}{1}:{2}", (ts.Hours > 0 ? (ts.Hours.ToString() + ":") : ""), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
