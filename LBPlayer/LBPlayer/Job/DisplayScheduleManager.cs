using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
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

        public void StartScheduler()
        {
            _scheduler.Start();
        }

        public void ScheduleJob(IJobDetail job, ITrigger trigger)
        {
            _scheduler.ScheduleJob(job, trigger);
        }
    }
}
