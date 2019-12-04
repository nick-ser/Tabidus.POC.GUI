using System;
using System.Globalization;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class GroupDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var arr = value.ToString().Split(new char[] {'('});
                return arr[0];
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}