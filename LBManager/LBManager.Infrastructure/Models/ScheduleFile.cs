using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
   public class ScheduleFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ScheduleType Type { get; set; }
        public List<MediaFile> MediaList { get; set; }
    }
}
