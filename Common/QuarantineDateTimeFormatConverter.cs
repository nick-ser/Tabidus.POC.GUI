using System;
using System.Globalization;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    class QuarantineDateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
                return ((DateTime)value).ToString("dd-MM-yyyy HH:mm:ss");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
