using LbPlayer.Logger;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBPlayer.Job
{
    public class LEDDisplayJob : IJob
    {
        public IList<LBManager.Infrastructure.Models.Media> MediaList { get; set; }
        public int LoopCount { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.MergedJobDataMap;

            LoopCount = dataMap.GetInt("LoopCount");
            MediaList = dataMap["MediaList"] as IList<LBManager.Infrastructure.Models.Media>;

            Log4NetLogger.LogInfo("Instance " + key + ":");
        }
    }
}
