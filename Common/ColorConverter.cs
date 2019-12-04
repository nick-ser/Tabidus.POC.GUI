using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Tabidus.POC.GUI.Common
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (SolidColorBrush) (new BrushConverter().ConvertFrom(value.ToString()));
            }
            return (SolidColorBrush) (new BrushConverter().ConvertFrom("#D2D2D3"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}