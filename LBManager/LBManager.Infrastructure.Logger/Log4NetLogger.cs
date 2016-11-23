using log4net;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Logger
{
    public class Log4NetLogger: ILoggerFacade
    {
        private readonly ILog _Logger = LogManager.GetLogger(typeof(Log4NetLogger));
       
        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    _Logger.Debug(message);
                    break;
                case Category.Warn:
                    _Logger.Warn(message);
                    break;
                case Category.Exception:
                    _Logger.Error(message);
                    break;
                case Category.Info:
                    _Logger.Info(message);
                    break;
            }
        }
    }
}
