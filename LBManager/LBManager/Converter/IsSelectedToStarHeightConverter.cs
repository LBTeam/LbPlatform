using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LBManager.Converter
{
    public class IsSelectedToStarHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return new DataGridLength(0, DataGridLengthUnitType.Star);
            }
            else
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException("parameter");
                }

                return new DataGridLength(double.Parse(parameter.ToString(), culture), DataGridLengthUnitType.Star);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
