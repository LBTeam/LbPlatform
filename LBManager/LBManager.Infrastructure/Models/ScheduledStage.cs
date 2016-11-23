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
        public List<VideoMedia> VideoMediaList { get; set; }
        public List<ImageMedia> ImageMediaList { get; set; }
        public List<TextMedia> TextMediaList { get; set; }
    }
}
