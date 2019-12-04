using System;
using System.Globalization;
using System.Windows.Data;
using Tabidus.POC.Common.Constants;

namespace Tabidus.POC.GUI.Common
{
    public class ComputerImageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var imageName = (string) value;
                return string.IsNullOrEmpty(imageName)
                    ? CommonConstants.POC_GUI_PATH + "Images/icon_noagent.png" : imageName == "server"
                    ? CommonConstants.POC_GUI_PATH + "Images/icon_server_group.png"
                    : imageName == "desktop"
                        ? CommonConstants.POC_GUI_PATH + "Images/icon_desktop_group.png"
                        : CommonConstants.POC_GUI_PATH + "Images/icon_notebook_group.png";
            }
            return CommonConstants.POC_GUI_PATH + "Images/icon_noagent.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}