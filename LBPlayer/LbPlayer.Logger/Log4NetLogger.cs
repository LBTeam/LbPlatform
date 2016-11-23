using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbPlayer.Logger
{
    public  class Log4NetLogger
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(Log4NetLogger));

        public static void LogDebug(string message)
        {
            _Logger.Debug(message);
        }

        public static void LogInfo(string message)
        {
            _Logger.Info(message);
        }

        public static void LogError(string message)
        {
            _Logger.Error(message);
        }
    }
}
