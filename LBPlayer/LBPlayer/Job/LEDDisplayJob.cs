using LbPlayer.Logger;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models = LBManager.Infrastructure.Models;

namespace LBPlayer.Job
{
    public class LEDDisplayJob : IJob
    {
        public string SchedulePath { get; set; }
        public IList<Models.Media> MediaPathList { get; set; }
        public int LoopCount { get; set; }
        public string ScheduledStageInfo { get; set; }


        public void Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.MergedJobDataMap;

            LoopCount = dataMap.GetInt("LoopCount");
            SchedulePath = dataMap.GetString("ScheduleName");
            MediaPathList = dataMap["MediaPathList"] as IList<Models.Media>;
            ScheduledStageInfo = dataMap.GetString("ScheduledStageInfo");

            LEDScreenDisplayer.GetInstance().DisplayMedias(MediaPathList, LoopCount);

            Log4NetLogger.LogInfo(string.Format("开始执行排期[{0}]中{1}阶段任务", SchedulePath, ScheduledStageInfo));
        }
    }
}
