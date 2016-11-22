using log4net;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Utility
{
    public class FileLogger : ILoggerFacade
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(FileLogger));

        public void Log(string message, Category category, Priority priority)
        {
            log4net.Config.XmlConfigurator.Configure();
            switch (category)
            {
                case Category.Debug:
                    log.Debug(message);
                    break;
                case Category.Warn:
                    log.Warn(message);
                    break;
                case Category.Exception:
                    log.Error(message);
                    break;
                case Category.Info:
                    log.Info(message);
                    break;
            }
        }
    }
}
