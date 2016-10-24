using Prism.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autofac;

namespace LBManager
{
    public class LBManagerBootstrapper: AutofacBootstrapper
    {
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
            base.ConfigureContainerBuilder(builder);
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }
    }
}
