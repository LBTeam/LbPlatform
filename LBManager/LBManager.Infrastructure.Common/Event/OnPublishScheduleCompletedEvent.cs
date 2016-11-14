using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnPublishScheduleCompletedEvent : PubSubEvent<OnPublishScheduleCompletedEventArg>
    {
    }

    public class OnPublishScheduleCompletedEventArg:EventArgs
    {
        //public string FileName { get; set; }
        public OnPublishScheduleCompletedEventArg()
        {
            //FileName = fileName;
        }
    }
}
