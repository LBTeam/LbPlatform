using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class ScheduledStage
    {
        public DateTime StartTime { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 7, 30, 0);
        public DateTime EndTime { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 21, 30, 0);
        public string Cron { get; set; }
        public int LoopCount { get; set; }
        public ArrangementMode ArrangementMode { get; set; }
        public IList<Media> MediaList { get; set; }
    }
}
