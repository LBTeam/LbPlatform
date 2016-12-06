using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBPlayer
{
    public class LEDScreenDisplayer
    {

        #region ------- 单例实现 -------

        private static LEDScreenDisplayer screenDisplayerInstance;

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        // 定义私有构造函数，使外界不能创建该类实例
        private LEDScreenDisplayer()
        {
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static LEDScreenDisplayer GetInstance()
        {
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            // 双重锁定只需要一句判断就可以了
            if (screenDisplayerInstance == null)
            {
                lock (locker)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (screenDisplayerInstance == null)
                    {
                        screenDisplayerInstance = new LEDScreenDisplayer();
                    }
                }
            }
            return screenDisplayerInstance;
        }

        #endregion


        private ScreenPlayer _screenPlayer;


        public void Initialize(int x, int y, int width, int height)
        {
            Action displayAction = new Action(() =>
            {
                _screenPlayer = new ScreenPlayer(x, y, width, height);
                _screenPlayer.OnCompletedPlayHandler = MediaDisplayCompleted;
                _screenPlayer.Initialize();
            });
            displayAction.BeginInvoke(null, null);
        }

        private void MediaDisplayCompleted(string message)
        {
            //throw new NotImplementedException();
        }

        public void DisplayMedias(IList<string> mediaPathList, int loopCount)
        {
            List<PlayInfoWrapper> playInfos = new List<PlayInfoWrapper>();
            foreach (var item in mediaPathList)
            {
                playInfos.Add(new PlayInfoWrapper(item, 1, 0, 0, 0, 0));
            }
            ScheduleInfoWrapper scheduleInfo = new ScheduleInfoWrapper(playInfos, loopCount);
            _screenPlayer.Play(scheduleInfo);
        }

        public void UpdateScreenWindow(int x, int y, int width, int height)
        {
            _screenPlayer.UpdateScreenSize(x, y, width, height);
        }
    }
}
