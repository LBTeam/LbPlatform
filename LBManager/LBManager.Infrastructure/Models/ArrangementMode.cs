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
    public enum ArrangementMode
    {
        [Description("正常铺满")]
        StandardCovered,
        [Description("混合铺满")]
        MixedCovered,
        [Description("指定")]
        Manual
    }
}
