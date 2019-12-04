using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tabidus.POC.Common.Constants;

namespace Tabidus.POC.GUI.Common
{
    public class SubnetAndVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var valueSelected = (string) value;

                if (valueSelected != ConstantHelper.IsInSubnetOperator)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}