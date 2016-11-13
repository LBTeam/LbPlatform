using LBManager.Infrastructure.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnCurrentDisplayRegionChangedEvent: PubSubEvent<OnCurrentDisplayRegionChangedEventArg>
    {
    }

    public class OnCurrentDisplayRegionChangedEventArg:EventArgs
    {
        public DisplayRegion OldDisplayRegion { get; set; }
        public DisplayRegion NewDisplayRegion { get; set; }
        public OnCurrentDisplayRegionChangedEventArg(DisplayRegion oldRegion,DisplayRegion newRegion)
        {
            OldDisplayRegion = oldRegion;
            NewDisplayRegion = newRegion;
        }
    }
}
