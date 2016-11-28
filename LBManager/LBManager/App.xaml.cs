using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LBManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

        }

        private static void RunInDebugMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            LBManagerBootstrapper bootstrapper = new LBManagerBootstrapper();
            bootstrapper.Run();
        }

        private static void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                LBManagerBootstrapper bootstrapper = new LBManagerBootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                // HandleException(ex);
            }
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ExceptionObject);
            //throw new NotImplementedException();
        }

        #region 待重构
        public static string SessionToken = string.Empty;
        #endregion
    }
}
