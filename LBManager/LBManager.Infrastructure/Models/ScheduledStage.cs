using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class ScheduledStage
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LoopCount { get; set; }
        public IList<Media> MediaList { get; set; }
    }
}
