using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class BackupScheduleRequest
    {
        public BackupScheduleRequest() { }
        public string FileName { get; set; }
        public string FileMD5 { get; set; }
    }
}
