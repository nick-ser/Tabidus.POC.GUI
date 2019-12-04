using System;
using System.Globalization;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class ExpandedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                // Changes expander icon image as per new layout
                if ((bool) value)
                {
                    return "../../Images/arr.png";
                }
                return "../../Images/arr.png";
            }
            return "../../Images/arr.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}