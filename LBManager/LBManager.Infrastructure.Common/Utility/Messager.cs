using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Utility
{
    public class Messager
    {
        public IEventAggregator EventAggregator { get; private set; }
        private Messager()
        {
            EventAggregator = new EventAggregator();
        }

        public static readonly Messager Default = new Messager();
    }
}
