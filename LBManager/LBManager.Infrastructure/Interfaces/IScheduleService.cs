using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Interfaces
{
    public interface IScheduleService
    {
        Task<bool> GetBackedUpSchedules();

        Task<bool> BackupSchedules(string backupRequest);
    }
}
