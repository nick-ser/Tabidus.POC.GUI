using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool && values[1] is string)
            {
                var hasText = !(bool) values[0];
                var valueSelected = (string) values[1];

                if (valueSelected == "is in subnet" && !hasText)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}