using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class PlatformConverter : IValueConverter
    {
        private readonly Dictionary<string, string> _dictPlatformMapping =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"32 Bit", "86"},
                {"64 Bit", "64"}
            };
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var strVal = value.ToString();
                if (_dictPlatformMapping.Values.Contains(strVal))
                    return _dictPlatformMapping.FirstOrDefault(c => c.Value == strVal).Key;
            }
            return "32 Bit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var strVal = value.ToString();
                if (_dictPlatformMapping.ContainsKey(strVal))
                    return _dictPlatformMapping[strVal];
            }
            return "86";
        }
    }
}
