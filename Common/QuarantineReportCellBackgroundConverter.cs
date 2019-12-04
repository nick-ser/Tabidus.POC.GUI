using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.Windows.DataPresenter;
using Tabidus.POC.Common.Model.Endpoint;

namespace Tabidus.POC.GUI.Common
{
    class QuarantineReportCellBackgroundConverter : IValueConverter
    {
        virtual protected string getSoftId()
        {
            return string.Empty;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cellValuePresenter = value as CellValuePresenter;
            var dataRecord = cellValuePresenter?.Record;
            var vm = dataRecord?.DataItem as QuarantineExtended;
            if (vm != null)
            {
                int? stateId = 0;
                switch (getSoftId())
                { 
                    case "Bitdefender":
                        stateId = vm.BitdefenderStatus;
                        break;
                    case "Avira":
                        stateId = vm.AviraStatus;
                        break;
                    case "Cyren":
                        stateId = vm.CyrenStatus;
                        break;
                    case "Blacklist":
                        stateId = vm.BlacklistStatus;
                        break;
                    default:
                        stateId = 0;
                        break;
                }

                if (stateId == 1)
                {
                    return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#3A584C"));
                }
            }
            return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#217624"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AviraReportCellBackgroundConverter : QuarantineReportCellBackgroundConverter {
        override protected string getSoftId()
        {
            return "Avira";
        }
    }

    class BitdefenderReportCellBackgroundConverter : QuarantineReportCellBackgroundConverter
    {
        override protected string getSoftId()
        {
            return "Bitdefender";
        }
    }

    class CyrenReportCellBackgroundConverter : QuarantineReportCellBackgroundConverter
    {
        override protected string getSoftId()
        {
            return "Cyren";
        }
    }

    class BlacklistReportCellBackgroundConverter : QuarantineReportCellBackgroundConverter
    {
        override protected string getSoftId()
        {
            return "Blacklist";
        }
    }
}
