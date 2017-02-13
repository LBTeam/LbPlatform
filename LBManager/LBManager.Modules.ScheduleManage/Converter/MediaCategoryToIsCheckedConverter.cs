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
    public class MediaCategoryToIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MediaCategory category = (MediaCategory)value;
            return category == MediaCategory.UserAd ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isChecked = (bool?)value;
            return isChecked == true ? MediaCategory.UserAd : MediaCategory.PSAs; 
        }
    }
}
