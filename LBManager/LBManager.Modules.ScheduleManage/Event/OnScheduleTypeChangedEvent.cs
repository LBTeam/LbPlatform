using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBManager.Infrastructure.Models;
using Prism.Events;

namespace LBManager.Modules.ScheduleManage.Event
{
    public class OnScheduleTypeChangedEvent : PubSubEvent<ScheduleType>
    {
    }
}
