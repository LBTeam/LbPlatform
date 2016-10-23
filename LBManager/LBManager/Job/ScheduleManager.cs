using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Job
{
    public class ScheduleManager
    {
        private static ISchedulerFactory sf = new StdSchedulerFactory();
        private static String JOB_GROUP_NAME = "group";
        private static String TRIGGER_GROUP_NAME = "trigger";
        private static IScheduler sched;

        public static void StartJob(String jobName, Type jobType, JobDataMap dataMap, String time)
        {
            sched = sf.GetScheduler();

            JobDetailImpl jobDetail = new JobDetailImpl();
            jobDetail.Name = jobName;
            jobDetail.Group = JOB_GROUP_NAME;
            jobDetail.JobType = jobType;

            jobDetail.JobDataMap = dataMap;

            CronTriggerImpl trigger = new CronTriggerImpl(jobName, TRIGGER_GROUP_NAME);
            trigger.CronExpressionString = time;
            sched.ScheduleJob(jobDetail, trigger);

            if (!sched.IsShutdown)
            {
                sched.Start();
            }
        }

        public static void UpdateJobDataMap(string jobName, JobDataMap dataMap)
        {
            sched = sf.GetScheduler();
            IJobDetail jobDetail = sched.GetJobDetail(new JobKey(jobName, JOB_GROUP_NAME));
           
        }

        /**  
         * 从Scheduler 移除当前的Job,修改Trigger  
         *   
         * @param jobDetail  
         * @param time  
         * @throws SchedulerException  
         * @throws ParseException  
         */
        public static void ModifyJobTime(ITrigger trigger, IJobDetail jobDetail, String time)
        {
            sched = sf.GetScheduler();
            if (trigger != null)
            {
                CronTriggerImpl ct = (CronTriggerImpl)trigger;
                // 移除当前进程的Job   
                sched.DeleteJob(jobDetail.Key);
                // 修改Trigger   
                ct.CronExpressionString = time;
                Console.WriteLine("CronTrigger getName " + ct.JobName);
                // 重新调度jobDetail   
                sched.ScheduleJob(jobDetail, ct);
            }
        }

        public static void ShutDownJob()
        {
            if (sched != null && !sched.IsShutdown)
            {
                sched.Shutdown();
            }
        }
    }
}
