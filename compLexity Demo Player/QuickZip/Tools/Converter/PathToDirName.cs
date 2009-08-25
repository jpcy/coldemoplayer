using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;

namespace QuickZip.Tools
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PathToDirNameConverter : IValueConverter
    {
        #region IValueConverter Members
        public static PathToDirNameConverter Instance = new PathToDirNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            if (path == null)
                return null;
            if (path.StartsWith("*")) //Bypass
                return path.Substring(1);


            string ppath = Path.GetFileName(path);

            if (path.EndsWith(":\\")) //Root
            {
                DriveInfo di = new DriveInfo(path);

                if (di.DriveType == DriveType.Removable || di.DriveType == DriveType.Network)
                {
                    return path;
                }

                if (di.IsReady)
                    return path + " (" + di.VolumeLabel + ")";
            }

            return ppath == "" ? path : ppath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();                
        }

        #endregion
    }
}
