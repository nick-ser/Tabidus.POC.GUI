using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class ImageEndpointStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if ((bool) value)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}