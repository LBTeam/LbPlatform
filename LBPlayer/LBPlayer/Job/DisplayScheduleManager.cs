using LBManager.Infrastructure.Models;
using LBManager.Infrastructure.Utility;
using LbPlayer.Logger;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBPlayer.Job
{
    public class DisplayScheduleManager
    {
        #region ------- 单例实现 -------

        private static DisplayScheduleManager scheduleManagerInstance;

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        // 定义私有构造函数，使外界不能创建该类实例
        private DisplayScheduleManager()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static DisplayScheduleManager GetInstance()
        {
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            // 双重锁定只需要一句判断就可以了
            if (scheduleManagerInstance == null)
            {
                lock (locker)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (scheduleManagerInstance == null)
                    {
                        scheduleManagerInstance = new DisplayScheduleManager();
                    }
                }
            }
            return scheduleManagerInstance;
        }

        #endregion

        private IScheduler _scheduler;
        private List<IJobDetail> _mainScheduleJobList = new List<IJobDetail>();
        //private IJobDetail _mainJobDetail;
        //private ITrigger _mainJobTrigger;
        //private IJobDetail _defaultJobDetail;
        //private ITrigger _defaultJobTrigger;

        public void StartScheduler()
        {
            _scheduler.Start();
        }

        public void ScheduleJob(IJobDetail job, ITrigger trigger, JobType jobType)
        {
            if (JobType.Main == jobType)
            {
                _mainScheduleJobList.Add(job);
            }
            else if (JobType.Default == jobType)
            {
                //_defaultJobDetail = job;
                //_defaultJobTrigger = trigger;
            }
            _scheduler.ScheduleJob(job, trigger);
        }

        public void ApplyMainSchedule(Schedule schedule)
        {
            try
            {
                var jobKeys = _mainScheduleJobList.Select(j => j.Key);
                _scheduler.DeleteJobs(jobKeys.ToList());

                if (!schedule.VerifyTimeConflict())
                {
                    Log4NetLogger.LogDebug(string.Format("{0}播放方案存在播放时间冲突", schedule.Name));
                }
                foreach (var regionItem in schedule.DisplayRegionList)
                {

                    if (regionItem.ScheduleMode == ScheduleMode.CPP)
                    {
                        Log4NetLogger.LogDebug(string.Format("当前排期为CPP"));
                        foreach (var stageItem in regionItem.StageList)
                        {
                            IList<string> mediaPathList = new List<string>();
                            foreach (var mediaItem in stageItem.MediaList)
                            {
                                string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                                if (File.Exists(mediaPath) && mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath))
                                {
                                    mediaPathList.Add(mediaPath);
                                }
                                else
                                {
                                    Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                                    return;
                                }
                            }

                            JobDataMap jobDataMap = new JobDataMap();
                            jobDataMap.Add("ScheduleName", schedule.Name);
                            jobDataMap.Add("ScheduledStageInfo", string.Format("{0}~{1}", stageItem.StartTime, stageItem.EndTime));
                            jobDataMap.Add("MediaPathList", mediaPathList);
                            jobDataMap.Add("LoopCount", stageItem.LoopCount);

                            IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                                .WithIdentity(Guid.NewGuid().ToString(), regionItem.Name)
                                .UsingJobData(jobDataMap)
                                .Build();

                            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                .WithIdentity(Guid.NewGuid().ToString(), regionItem.Name)
                                .StartAt(stageItem.StartTime)
                                .EndAt(stageItem.EndTime)
                                .Build();

                            ScheduleJob(job, trigger, JobType.Main);
                        }
                    }
                    else if (regionItem.ScheduleMode == ScheduleMode.CPM)
                    {
                        Log4NetLogger.LogDebug(string.Format("当前排期为CPM"));
                        foreach (var stageItem in regionItem.StageList)
                        {
                            if (regionItem.RepeatMode == RepeatMode.Manual)
                            {
                                ApplayManualSchedule(schedule, regionItem, stageItem);
                            }
                            else
                            {
                                ApplayDailySchedule(schedule, regionItem, stageItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("错误{0}", ex.Message));
                return;
            }

            Log4NetLogger.LogInfo(string.Format("应用播放方案{0}", schedule.Name));
        }

        private bool ApplayManualSchedule(Schedule schedule, DisplayRegion region, ScheduledStage stage)
        {
            if (stage.ArrangementMode == ArrangementMode.Manual)
            {
                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in stage.MediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/* && mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        for (int i = 0; i < mediaItem.LoopCount; i++)
                        {
                            mediaPathList.Add(mediaPath);
                        }
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                string dailyPart = stage.StartTime.Day == stage.EndTime.Day ? stage.StartTime.Day.ToString() : stage.StartTime.Day + "-" + stage.EndTime.Day;
                string monthPart = stage.StartTime.Month == stage.EndTime.Month ? stage.StartTime.Month.ToString() : stage.StartTime.Month + "-" + stage.EndTime.Month;
                string cron = string.Format("{0} {1} {2} {3} {4} ? *", stage.StartTime.Second, stage.StartTime.Minute, stage.StartTime.Hour, dailyPart, monthPart);

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", stage.LoopCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

                var trigger = TriggerBuilder.Create()
                     .WithCronSchedule(cron)
                     .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else if (stage.ArrangementMode == ArrangementMode.StandardCovered)
            {
                int repeatCount;
                var realTotalTime = stage.MediaList.Sum(m => m.Duration.TotalSeconds * m.LoopCount);
                var planTotalTime = (stage.EndTime.TimeOfDay - stage.StartTime.TimeOfDay).TotalSeconds;
                if (realTotalTime > 1)
                    repeatCount = (int)(planTotalTime / realTotalTime) + 1;
                else
                    repeatCount = (int)planTotalTime;
                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in stage.MediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        for (int i = 0; i < mediaItem.LoopCount; i++)
                        {
                            mediaPathList.Add(mediaPath);
                        }
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                string dailyPart = stage.StartTime.Day == stage.EndTime.Day ? stage.StartTime.Day.ToString() : stage.StartTime.Day + "-" + stage.EndTime.Day;
                string monthPart = stage.StartTime.Month == stage.EndTime.Month ? stage.StartTime.Month.ToString() : stage.StartTime.Month + "-" + stage.EndTime.Month;
                string cron = string.Format("{0} {1} {2} {3} {4} ? *", stage.StartTime.Second, stage.StartTime.Minute, stage.StartTime.Hour, dailyPart, monthPart);

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", repeatCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

                
                var trigger = TriggerBuilder.Create()
                     .WithCronSchedule(cron)
                     .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else if (stage.ArrangementMode == ArrangementMode.MixedCovered)
            {
                List<LBManager.Infrastructure.Models.Media> unitMediaList = new List<LBManager.Infrastructure.Models.Media>();
                int findCount = stage.MediaList.Max(m => m.LoopCount);
                for (int i = 0; i < findCount; i++)
                {
                    foreach (var media in stage.MediaList)
                    {
                        if (media.LoopCount > 0)
                        {
                            unitMediaList.Add(media);
                            media.LoopCount--;
                        }
                    }
                }

                int repeatCount;
                var realTotalTime = unitMediaList.Sum(m => m.Duration.TotalSeconds);
                var planTotalTime = (stage.EndTime.TimeOfDay - stage.StartTime.TimeOfDay).TotalSeconds;
                if (realTotalTime > 1)
                    repeatCount = (int)(planTotalTime / realTotalTime) + 1;
                else
                    repeatCount = (int)planTotalTime;

                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in unitMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        mediaPathList.Add(mediaPath);
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                string dailyPart = stage.StartTime.Day == stage.EndTime.Day ? stage.StartTime.Day.ToString() : stage.StartTime.Day + "-" + stage.EndTime.Day;
                string monthPart = stage.StartTime.Month == stage.EndTime.Month ? stage.StartTime.Month.ToString() : stage.StartTime.Month + "-" + stage.EndTime.Month;
                string cron = string.Format("{0} {1} {2} {3} {4} ? *", stage.StartTime.Second, stage.StartTime.Minute, stage.StartTime.Hour, dailyPart, monthPart);

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", repeatCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

               
                var trigger = TriggerBuilder.Create()
                     .WithCronSchedule(cron)
                     .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool ApplayDailySchedule(Schedule schedule, DisplayRegion region, ScheduledStage stage)
        {
            if (stage.ArrangementMode == ArrangementMode.Manual)
            {
                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in stage.MediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/* && mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        for (int i = 0; i < mediaItem.LoopCount; i++)
                        {
                            mediaPathList.Add(mediaPath);
                        }
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", stage.LoopCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithCronSchedule(stage.Cron)
                    .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else if (stage.ArrangementMode == ArrangementMode.StandardCovered)
            {
                int repeatCount;
                var realTotalTime = stage.MediaList.Sum(m => m.Duration.TotalSeconds * m.LoopCount);
                var planTotalTime = (stage.EndTime - stage.StartTime).TotalSeconds;
                if (realTotalTime > 1)
                    repeatCount = (int)(planTotalTime / realTotalTime) + 1;
                else
                    repeatCount = (int)planTotalTime;
                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in stage.MediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        for (int i = 0; i < mediaItem.LoopCount; i++)
                        {
                            mediaPathList.Add(mediaPath);
                        }
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", repeatCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithCronSchedule(stage.Cron)
                    .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else if (stage.ArrangementMode == ArrangementMode.MixedCovered)
            {
                List<LBManager.Infrastructure.Models.Media> unitMediaList = new List<LBManager.Infrastructure.Models.Media>();
                int findCount = stage.MediaList.Max(m => m.LoopCount);
                for (int i = 0; i < findCount; i++)
                {
                    foreach (var media in stage.MediaList)
                    {
                        if (media.LoopCount > 0)
                        {
                            unitMediaList.Add(media);
                            media.LoopCount--;
                        }
                    }
                }

                int repeatCount;
                var realTotalTime = unitMediaList.Sum(m => m.Duration.TotalSeconds);
                var planTotalTime = (stage.EndTime - stage.StartTime).TotalSeconds;
                if (realTotalTime > 1)
                    repeatCount = (int)(planTotalTime / realTotalTime) + 1;
                else
                    repeatCount = (int)planTotalTime;

                IList<string> mediaPathList = new List<string>();
                foreach (var mediaItem in unitMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        mediaPathList.Add(mediaPath);
                    }
                    else
                    {
                        Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                        return false;
                    }
                }

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.Add("ScheduleName", schedule.Name);
                jobDataMap.Add("ScheduledStageInfo", string.Format("{0}", stage.Cron));
                jobDataMap.Add("MediaPathList", mediaPathList);
                jobDataMap.Add("LoopCount", repeatCount);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithCronSchedule(stage.Cron)
                    .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else
            {
                return false;
            }
            return true;
        }
    }


    public enum JobType
    {
        Main,
        Default
    }
}
