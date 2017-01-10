using LBManager.Infrastructure.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RepeatMode
    {
        [Description("日")]
        Daily,
        [Description("周")]
        Weekly,
        [Description("月")]
        Monthly,
        [Description("指定")]
        Manual
    }

}
