using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnHeartbeatEvent : PubSubEvent<OnHeartbeatEventArg>
    {
    }

    public class OnHeartbeatEventArg : EventArgs
    {
        public HeartbeatStatus Status { get; set; }

        public OnHeartbeatEventArg(HeartbeatStatus status)
        {
            this.Status = status;
        }
    }

    public enum HeartbeatStatus
    {
        STOP,
        OK,
        TokenInvalid,
        TokenExpired
    }
}
