using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Tabidus.POC.GUI.Common
{
    class QuarantinFeatureToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if ((string) value == "OA")
                    return new BitmapImage(new Uri(@"/Images/server_small_green_online.png", UriKind.Relative));
                if((string)value == "OI")
                    return new BitmapImage(new Uri(@"/Images/eii.png", UriKind.Relative));
                if ((string)value == "OP")
                    return new BitmapImage(new Uri(@"/Images/endpoint_menu_hover.png", UriKind.Relative));
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
