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
        public IList<string> MediaPathList { get; set; }
        public int LoopCount { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.MergedJobDataMap;

            LoopCount = dataMap.GetInt("LoopCount");
            MediaPathList = dataMap["MediaPathList"] as IList<string>;

            LEDScreenDisplayer.GetInstance().DisplayMedias(MediaPathList, LoopCount);

            Log4NetLogger.LogInfo("Instance " + key + ":");
        }
    }
}
