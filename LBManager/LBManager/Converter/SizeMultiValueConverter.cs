using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LBManager.Converter
{
    public class SizeMultiValueConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string width = values[0].ToString();
            string height = values[1].ToString();
            if (string.IsNullOrEmpty(width) || string.IsNullOrEmpty(height))
            {
                return string.Empty;
            }
            return $"{width}*{height}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
