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
    public enum ScheduleType
    {
        [Description("播放方案")]
        Common = 0,
        [Description("紧急插播")]
        Emergency = 7,
        [Description("缺省方案")]
        Default = 8
    }
}
