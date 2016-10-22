using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnLoginEvent: PubSubEvent<OnLoginEventArgs>
    {
    }

    public class OnLoginEventArgs: EventArgs
    {
        public bool Status { get; private set; }
        public string Account { get; private set; }

        public OnLoginEventArgs(bool status,string account)
        {
            this.Status = status;
            this.Account = account;
        }
    }
}
