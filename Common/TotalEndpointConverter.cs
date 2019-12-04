using System;
using System.Globalization;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class TotalEndpointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var arr = value.ToString().Split(new char[] {'('});
                return arr[arr.Length-1].Replace(")","").Replace("item","Endpoint");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}