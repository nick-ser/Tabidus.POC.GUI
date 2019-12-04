using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class InvertBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return booleanValue?Visibility.Collapsed:Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
