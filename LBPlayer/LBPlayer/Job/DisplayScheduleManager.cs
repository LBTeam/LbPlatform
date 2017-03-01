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
using Models = LBManager.Infrastructure.Models;

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
        private List<AppliedScheduleInfo> _appliedScheduleInfoTable = new List<AppliedScheduleInfo>();
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
            _scheduler.ScheduleJob(job, trigger);
        }

        public void ApplyMainSchedule(Schedule schedule)
        {
            try
            {
                var jobKeys = _mainScheduleJobList.Select(j => j.Key);
                _scheduler.DeleteJobs(jobKeys.ToList());
                _appliedScheduleInfoTable.Clear();

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
                            List<Models.Media> mediaPathList = new List<Models.Media>();
                            foreach (var mediaItem in stageItem.MediaList)
                            {
                                string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                                if (File.Exists(mediaPath))/* && mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                                {
                                    mediaPathList.Add(mediaItem);
                                }
                                else
                                {
                                    Log4NetLogger.LogDebug(string.Format("获取当前排期{0}中媒体{1}失败。", schedule, mediaPath));
                                    return;
                                }
                            }

                            GenerateJobSchedule(schedule, stageItem, mediaPathList, regionItem);
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

        private void GenerateJobSchedule(Schedule schedule, ScheduledStage stageItem, List<Models.Media> mediaPathList,
            DisplayRegion regionItem)
        {
            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Add("ScheduleName", schedule.Name);
            jobDataMap.Add("ScheduledStageInfo", string.Format("{0}~{1}", stageItem.StartTime, stageItem.EndTime));
            jobDataMap.Add("MediaPathList", mediaPathList);
            jobDataMap.Add("LoopCount", stageItem.LoopCount);

            IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                .WithIdentity(Guid.NewGuid().ToString(), regionItem.Name)
                .UsingJobData(jobDataMap)
                .Build();

            ITrigger trigger = (ITrigger)TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString(), regionItem.Name)
                .StartAt(stageItem.StartTime)
                .EndAt(stageItem.EndTime)
                .WithPriority(7)
                .Build();

            ScheduleJob(job, trigger, JobType.Main);

            SetAppliedScheduleInfoTable(stageItem.StartTime, stageItem.EndTime, mediaPathList);
        }

        private void SetAppliedScheduleInfoTable(DateTime startTime, DateTime endTime, List<Models.Media> mediaPathList)
        {
            _appliedScheduleInfoTable.Add(new AppliedScheduleInfo()
            {
                StartTime = startTime,
                EndTime = endTime,
                MediaList = mediaPathList
            });
        }

        private bool ApplayManualSchedule(Schedule schedule, DisplayRegion region, ScheduledStage stage)
        {
            List<Models.Media> mediaPathList = new List<Models.Media>();
            DateTime startTime = stage.StartTime;
            DateTime endTime = stage.EndTime;
            if (stage.ArrangementMode == ArrangementMode.StandardCovered || stage.ArrangementMode == ArrangementMode.Manual)
            {

                List<Models.Media> userMediaList = new List<Models.Media>();

                foreach (var media in stage.MediaList)
                {
                    if (media.Category == MediaCategory.UserAd && media.LoopCount > 0)
                    {
                        userMediaList.Add(media);
                        media.LoopCount--;
                    }
                }

                var publicMedias = stage.MediaList.Where(m => m.Category == MediaCategory.PSAs)?.ToList();

                var playMediaList = MakePlayMediaList(stage, userMediaList, publicMedias);


                foreach (var mediaItem in playMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        for (int i = 0; i < mediaItem.LoopCount; i++)
                        {
                            mediaPathList.Add(mediaItem);
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
                jobDataMap.Add("LoopCount", 1);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();


                var trigger = TriggerBuilder.Create()
                     .WithCronSchedule(cron)
                     .WithPriority(7)
                     .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else if (stage.ArrangementMode == ArrangementMode.MixedCovered)
            {
                List<LBManager.Infrastructure.Models.Media> userMediaList = new List<LBManager.Infrastructure.Models.Media>();

                int findCount = stage.MediaList.Max(m => m.LoopCount);
                var publicMedias = stage.MediaList.Where(m => m.Category == MediaCategory.PSAs).ToList();
                for (int i = 0; i < findCount; i++)
                {
                    foreach (var media in stage.MediaList)
                    {
                        if (media.Category == MediaCategory.UserAd && media.LoopCount > 0)
                        {
                            userMediaList.Add(media);
                            media.LoopCount--;
                        }
                    }
                }

                var playMediaList = MakePlayMediaList(stage, userMediaList, publicMedias);

                foreach (var mediaItem in playMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        mediaPathList.Add(mediaItem);
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
                jobDataMap.Add("LoopCount", 1);

                IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), region.Name)
                    .UsingJobData(jobDataMap)
                    .Build();


                var trigger = TriggerBuilder.Create()
                     .WithCronSchedule(cron)
                     .WithPriority(7)
                     .Build();

                ScheduleJob(job, trigger, JobType.Main);
            }
            else
            {
                return false;
            }

            SetAppliedScheduleInfoTable(startTime, endTime, mediaPathList);
            return true;
        }

        private List<Models.Media> MakePlayMediaList(ScheduledStage stage, List<Models.Media> userMediaList, List<Models.Media> publicMedias)
        {
          
            var realTotalTime = userMediaList.Sum(m => m.Duration.TotalSeconds);
            var planTotalTime = (stage.EndTime - stage.StartTime).TotalSeconds;
            var remainingTime = (int)(planTotalTime - realTotalTime);
            List<LBManager.Infrastructure.Models.Media> playMediaList =
                new List<LBManager.Infrastructure.Models.Media>(userMediaList);

            if (publicMedias == null || publicMedias.Count == 0)
            {
                return playMediaList;
            }

            int minPublicMediaSeconds = (int)publicMedias.Min(m => m.Duration.TotalSeconds);
            int maxPublicMediaSeconds = (int)publicMedias.Min(m => m.Duration.TotalSeconds);
            var minPublicMedia = publicMedias.FirstOrDefault(m => (int)m.Duration.TotalSeconds == minPublicMediaSeconds);
            int insertIndex = 0;
            int publishMediaIndex = 0;
            while (remainingTime > 0)
            {
                int minDiff = remainingTime - maxPublicMediaSeconds;
                int maxDiff = remainingTime - minPublicMediaSeconds;
                int index = (2 * insertIndex + 1) >= (playMediaList.Count - 1) ? (playMediaList.Count - 1) : (2 * insertIndex + 1);
                LBManager.Infrastructure.Models.Media insertItem = publishMediaIndex >= (publicMedias.Count - 1)
                    ? publicMedias[publicMedias.Count - 1]
                    : publicMedias[publishMediaIndex];
                if (minDiff >= 0)
                {
                    if (index == (playMediaList.Count - 1))
                        playMediaList.Add(insertItem);
                    else
                        playMediaList.Insert(index, insertItem);
                    remainingTime = minDiff;
                }
                else
                {
                    if (maxDiff >= 0)
                    {
                        if (minPublicMedia != null)
                            playMediaList.Insert(index, minPublicMedia);
                        remainingTime = maxDiff;
                    }
                    else
                    {
                        playMediaList.Add(insertItem);
                        break;
                    }
                }
                insertIndex++;
                publishMediaIndex++;
            }
            return playMediaList;
        }

        private List<Models.Media> MakePlayMediaList(DateTime stageStartTime, DateTime stageEndTime, List<Models.Media> userMediaList, List<Models.Media> publicMedias)
        {
            var realTotalTime = userMediaList.Sum(m => m.Duration.TotalSeconds);
            var planTotalTime = (stageEndTime - stageStartTime).TotalSeconds;
            var remainingTime = (int)(planTotalTime - realTotalTime);
            List<LBManager.Infrastructure.Models.Media> playMediaList =
                new List<LBManager.Infrastructure.Models.Media>(userMediaList);
            if (publicMedias == null || publicMedias.Count == 0)
            {
                return playMediaList;
            }
            int minPublicMediaSeconds = (int)publicMedias.Min(m => m.Duration.TotalSeconds);
            int maxPublicMediaSeconds = (int)publicMedias.Min(m => m.Duration.TotalSeconds);
            var minPublicMedia = publicMedias.FirstOrDefault(m => (int)m.Duration.TotalSeconds == minPublicMediaSeconds);
            int insertIndex = 0;
            int publishMediaIndex = 0;
            while (remainingTime > 0)
            {
                int minDiff = remainingTime - maxPublicMediaSeconds;
                int maxDiff = remainingTime - minPublicMediaSeconds;
                int index = (2 * insertIndex + 1) >= (playMediaList.Count - 1) ? (playMediaList.Count - 1) : (2 * insertIndex + 1);
                Models.Media insertItem = publishMediaIndex >= (publicMedias.Count - 1)
                    ? publicMedias[publicMedias.Count - 1]
                    : publicMedias[publishMediaIndex];
                if (minDiff >= 0)
                {
                    if (index == (playMediaList.Count - 1))
                        playMediaList.Add(insertItem);
                    else
                        playMediaList.Insert(index, insertItem);
                    remainingTime = minDiff;
                }
                else
                {
                    if (maxDiff >= 0)
                    {
                        if (minPublicMedia != null)
                            playMediaList.Insert(index, minPublicMedia);
                        remainingTime = maxDiff;
                    }
                    else
                    {
                        playMediaList.Add(insertItem);
                        break;
                    }
                }
                insertIndex++;
                publishMediaIndex++;
            }
            return playMediaList;
        }
        public void ApplyEmergencySchedule(Schedule emergencySchedule)
        {
            try
            {
                if (!emergencySchedule.VerifyTimeConflict())
                {
                    Log4NetLogger.LogDebug(string.Format("{0}播放方案存在播放时间冲突", emergencySchedule.Name));
                }
                foreach (var regionItem in emergencySchedule.DisplayRegionList)
                {
                    foreach (var scheduledStage in regionItem.StageList)
                    {
                        GenerateEmergencyJobSchedule(emergencySchedule.Name, regionItem.Name, scheduledStage);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("错误{0}", ex.Message));
                return;
            }

            Log4NetLogger.LogInfo(string.Format("应用播放方案{0}", emergencySchedule.Name));
        }

        private void GenerateEmergencyJobSchedule(string scheduleName, string regionName, ScheduledStage emergencyScheduledStage)
        {
            List<Models.Media> userMediaList = new List<Models.Media>();

            var timeSpan = (emergencyScheduledStage.EndTime - emergencyScheduledStage.StartTime).TotalSeconds;

            int findCount = emergencyScheduledStage.MediaList.Max(m => m.LoopCount);
            for (int i = 0; i < findCount; i++)
            {
                foreach (var media in emergencyScheduledStage.MediaList)
                {
                    if (media.Category == MediaCategory.UserAd && media.LoopCount > 0)
                    {
                        userMediaList.Add(media);
                        media.LoopCount--;
                    }
                }
            }

            var singleTimeSpan = userMediaList.Sum(m => m.Duration.TotalSeconds);
            int addCounter = 0;
            while (timeSpan > singleTimeSpan)
            {
                userMediaList.Add(userMediaList[addCounter]);
                singleTimeSpan += userMediaList[addCounter].Duration.TotalSeconds;
            }

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Add("ScheduleName", scheduleName);
            jobDataMap.Add("ScheduledStageInfo", string.Format("{0}~{1}", emergencyScheduledStage.StartTime, emergencyScheduledStage.EndTime));
            jobDataMap.Add("MediaPathList", userMediaList);
            jobDataMap.Add("LoopCount", emergencyScheduledStage.LoopCount);

            IJobDetail job = JobBuilder.Create<LEDDisplayJob>()
                .WithIdentity(Guid.NewGuid().ToString(), regionName)
                .UsingJobData(jobDataMap)
                .Build();

            ITrigger trigger = (ITrigger)TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString(), regionName)
                .StartAt(emergencyScheduledStage.StartTime)
                .EndAt(emergencyScheduledStage.EndTime)
                .WithPriority(1)
                .Build();

            ScheduleJob(job, trigger, JobType.Emergency);
            RearrangeStageSchedule(emergencyScheduledStage);

        }

        private void RearrangeStageSchedule(ScheduledStage emergencyScheduledStage)
        {
            foreach (var appliedScheduleInfo in _appliedScheduleInfoTable)
            {
                List<Models.Media> stageMedias = new List<Models.Media>();
                List<Models.Media> playMediaList = new List<Models.Media>();
                if (emergencyScheduledStage.StartTime.TimeOfDay >= appliedScheduleInfo.EndTime.TimeOfDay)
                {
                    continue;
                }
                else
                {
                    var mediaEndTime = appliedScheduleInfo.StartTime.TimeOfDay;
                    foreach (var media in appliedScheduleInfo.MediaList)
                    {
                        if (media.Category == MediaCategory.UserAd)
                            mediaEndTime += media.Duration;
                        if (emergencyScheduledStage.StartTime.TimeOfDay >= mediaEndTime)
                        {
                            continue;
                        }
                        else
                        {
                            if (media.Category == MediaCategory.UserAd)
                                stageMedias.Add(media);
                        }
                    }
                }
                var listToBeArrangedTotalSeconds = stageMedias.Sum(m => m.Duration.TotalSeconds);
                var currentStageRemainingTimeTotalSeconds = (appliedScheduleInfo.EndTime.TimeOfDay - emergencyScheduledStage.EndTime.TimeOfDay).TotalSeconds;
                if (listToBeArrangedTotalSeconds > currentStageRemainingTimeTotalSeconds)
                {
                    playMediaList = stageMedias;
                }
                else
                {
                    playMediaList = MakePlayMediaList(emergencyScheduledStage.EndTime, appliedScheduleInfo.EndTime, stageMedias,
                        appliedScheduleInfo.MediaList.Where(m => m.Category == MediaCategory.PSAs).ToList());
                }
                JobDataMap redoJobDatamap = new JobDataMap();
                redoJobDatamap.Add("ScheduleName", appliedScheduleInfo.ScheduleName);
                redoJobDatamap.Add("ScheduledStageInfo", string.Format("{0}~{1}", emergencyScheduledStage.EndTime, appliedScheduleInfo.EndTime));
                redoJobDatamap.Add("MediaPathList", playMediaList);
                redoJobDatamap.Add("LoopCount", 1);

                IJobDetail redoJob = JobBuilder.Create<LEDDisplayJob>()
                    .WithIdentity(Guid.NewGuid().ToString(), appliedScheduleInfo.RegionName)
                    .UsingJobData(redoJobDatamap)
                    .Build();

                var redoTrigger = TriggerBuilder.Create()
                    .WithIdentity(Guid.NewGuid().ToString(), appliedScheduleInfo.RegionName)
                    .StartAt(emergencyScheduledStage.EndTime)
                    .EndAt(appliedScheduleInfo.EndTime)
                    .WithPriority(4)
                    .Build();

                ScheduleJob(redoJob, redoTrigger, JobType.Main);
            }
        }

        private bool ApplayDailySchedule(Schedule schedule, DisplayRegion region, ScheduledStage stage)
        {
            List<Models.Media> mediaPathList = new List<Models.Media>();
            DateTime startTime = stage.StartTime;
            DateTime endTime = stage.EndTime;
            if (stage.ArrangementMode == ArrangementMode.StandardCovered || stage.ArrangementMode == ArrangementMode.Manual)
            {
                List<Models.Media> userMediaList = new List<Models.Media>();

                foreach (var media in stage.MediaList)
                {
                    if (media.Category == MediaCategory.UserAd && media.LoopCount > 0)
                    {
                        userMediaList.Add(media);
                        media.LoopCount--;
                    }
                }

                var publicMedias = stage.MediaList.Where(m => m.Category == MediaCategory.PSAs)?.ToList();

                var playMediaList = MakePlayMediaList(stage, userMediaList, publicMedias);

                foreach (var mediaItem in playMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        //for (int i = 0; i < mediaItem.LoopCount; i++)
                        //{
                        mediaPathList.Add(mediaItem);
                        //}
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
                jobDataMap.Add("LoopCount", 1);

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
                List<Models.Media> userMediaList = new List<Models.Media>();

                int findCount = stage.MediaList.Max(m => m.LoopCount);
                var publicMedias = stage.MediaList.Where(m => m.Category == MediaCategory.PSAs).ToList();
                for (int i = 0; i < findCount; i++)
                {
                    foreach (var media in stage.MediaList)
                    {
                        if (media.Category == MediaCategory.UserAd && media.LoopCount > 0)
                        {
                            userMediaList.Add(media);
                            media.LoopCount--;
                        }
                    }
                }

                var playMediaList = MakePlayMediaList(stage, userMediaList, publicMedias);

                foreach (var mediaItem in playMediaList)
                {
                    string mediaPath = Path.Combine(ApplicationConfig.GetMediaFilePath(), Path.GetFileNameWithoutExtension(mediaItem.URL) + "_" + mediaItem.MD5 + Path.GetExtension(mediaItem.URL));
                    if (File.Exists(mediaPath))/*&& mediaItem.MD5 == FileUtils.ComputeFileMd5(mediaPath)*/
                    {
                        mediaPathList.Add(mediaItem);
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
                jobDataMap.Add("LoopCount", 1);

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

            SetAppliedScheduleInfoTable(startTime, endTime, mediaPathList);
            return true;
        }

    }

    public class AppliedScheduleInfo
    {
        public string ScheduleName { get; set; }
        public string RegionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Models.Media> MediaList { get; set; }
    }

    public enum JobType
    {
        Main,
        Emergency,
        Default
    }
}
