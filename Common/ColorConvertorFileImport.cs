using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Tabidus.POC.GUI.Common
{
    public class ColorConvertorFileImport : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (SolidColorBrush) (new BrushConverter().ConvertFrom(value.ToString()));
            }
            return (SolidColorBrush) (new BrushConverter().ConvertFrom("#5E5F66"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}