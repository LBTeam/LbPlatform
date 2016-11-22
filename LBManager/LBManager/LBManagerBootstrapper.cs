using Prism.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Prism.Logging;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Logger;
using LBManager.Infrastructure.Interfaces;

namespace LBManager
{
    public class LBManagerBootstrapper : AutofacBootstrapper
    {
        protected override ILoggerFacade CreateLogger()
        {
            return new Log4NetLogger();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            ShellViewModel viewModel = new ShellViewModel();
            Application.Current.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Application.Current.MainWindow.DataContext = viewModel;
            Application.Current.MainWindow.Show();
        }
        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            builder.RegisterType<Shell>().SingleInstance();
            builder.RegisterType<ScreenService>().As<IScreenService>();
            base.ConfigureContainerBuilder(builder);
        }

        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();

        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }
    }
}
