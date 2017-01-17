using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class FetchBackedUpScheduleResponse
    {
        public FetchBackedUpScheduleResponse() { }
        public string ProgramId { get; set; }
        public string ProgramName { get; set; }
        public string ProgramUrl { get; set; }
        public List<BackedUpMediaInfo> MediaList { get; set; }
    }

    public class BackedUpMediaInfo
    {
        public BackedUpMediaInfo() { }
        public string MediaId { get; set; }
        public string MediaName { get; set; }
        public string MediaUrl { get; set; }
    }
}
