using LBManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LBManager.Modules.ScheduleManage.Converter
{
    public class ScheduleModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ScheduleMode mode = (ScheduleMode)value;
            return mode == ScheduleMode.CPM ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var targetValue = (bool)value;
            return targetValue ? ScheduleMode.CPM : ScheduleMode.CPP;
        }
    }
}
