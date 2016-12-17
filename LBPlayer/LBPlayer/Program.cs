using LbPlayer.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LBPlayer
{
    static class Program
    {
        // private static Log4NetLogger _logger;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new LBPlayerMain());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log4NetLogger.LogError(e.Exception.Message);
            //throw new NotImplementedException();
        }
    }
}
