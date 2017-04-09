using LBManager.Infrastructure.Interfaces;
using LBManager.Infrastructure.Models;
using LBManager.Infrastructure.Utility;
using LBManager.Modules.ScheduleManage.Views;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduleSummaryListViewModel : BindableBase
    {
        private static string mediaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media");
        private FileSystemWatcher _fileWatcher;
        private ILoggerFacade _logger;
        private IScheduleService _scheduleService;
        public ScheduleSummaryListViewModel()
        {
            _logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
            _scheduleService = ServiceLocator.Current.GetInstance<IScheduleService>();
            if (!Directory.Exists(mediaDirectory))
            {
                Directory.CreateDirectory(mediaDirectory);
            }
            // FetchProgramSchedules(mediaDirectory);
            ScheduleWorkDirectory = new ScheduleDirectoryViewModel(new DirectoryInfo(mediaDirectory), null);
            GetAllScheduleFile(ScheduleWorkDirectory);
            InitializeFileWatcher(mediaDirectory);
        }

        private void GetAllScheduleFile(ScheduleDirectoryViewModel directoryViewModel)
        {
            foreach (var item in directoryViewModel.Children)
            {
                if (item.GetType() == typeof(ScheduleDirectoryViewModel))
                {
                    GetAllScheduleFile(item as ScheduleDirectoryViewModel);
                }
                else if (item.GetType() == typeof(ScheduleFileViewModel))
                {
                    ScheduleSummaryList.Add(item as ScheduleFileViewModel);
                }
            }
        }

        private ScheduleDirectoryViewModel _scheduleWorkDirectory;
        public ScheduleDirectoryViewModel ScheduleWorkDirectory
        {
            get { return _scheduleWorkDirectory; }
            set { SetProperty(ref _scheduleWorkDirectory, value); }
        }

        private ObservableCollection<ScheduleFileViewModel> _scheduleSummaryList = new ObservableCollection<ScheduleFileViewModel>();
        public ObservableCollection<ScheduleFileViewModel> ScheduleSummaryList
        {
            get { return _scheduleSummaryList; }
            set { SetProperty(ref _scheduleSummaryList, value); }
        }

        public DelegateCommand NewScheduleFileCommand => new DelegateCommand(NewScheduleFile, CanNewScheduleFile);

        private bool CanNewScheduleFile()
        {
            return true;
        }

        private void NewScheduleFile()
        {
            ScheduleView scheduleView = new ScheduleView();
            scheduleView.Owner = Application.Current.MainWindow;
            scheduleView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            scheduleView.DataContext = new ScheduleViewModel(mediaDirectory);
            scheduleView.ShowDialog();
        }

        private DelegateCommand _addScheduleDirectoryCommand;
        public DelegateCommand AddScheduleDirectoryCommand =>
            _addScheduleDirectoryCommand ?? (_addScheduleDirectoryCommand = new DelegateCommand(AddScheduleDirectory, CanAddScheduleDirectory));

        private bool CanAddScheduleDirectory()
        {
            return true;
        }

        private void AddScheduleDirectory()
        {

        }


        private void InitializeFileWatcher(string targetDirectory)
        {
            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = targetDirectory;
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _fileWatcher.Filter = "";
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.EnableRaisingEvents = true;

            _fileWatcher.Created += _fileWatcher_Created;
            _fileWatcher.Changed += _fileWatcher_Changed;
            _fileWatcher.Renamed += _fileWatcher_Renamed;
            _fileWatcher.Deleted += _fileWatcher_Deleted;
        }

        private void _fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _logger.Log(string.Format("删除{0}文件", e.FullPath), Category.Debug, Priority.Medium);
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RereshScheduleWorkDirecotry();
            }), null);
        }


        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            _logger.Log(string.Format("重命名{0}文件为{1}", e.OldFullPath, e.FullPath), Category.Debug, Priority.Medium);
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RereshScheduleWorkDirecotry();
            }), null);
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RereshScheduleWorkDirecotry();
            }), null);
            //throw new NotImplementedException();
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _logger.Log(string.Format("新增{0}文件", e.FullPath), Category.Debug, Priority.Medium);
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RereshScheduleWorkDirecotry();
            }), null);

            //throw new NotImplementedException();
        }


        private void RereshScheduleWorkDirecotry()
        {
            ScheduleWorkDirectory.Children.Clear();
            ScheduleWorkDirectory.LoadChildren();
            ScheduleSummaryList.Clear();
            GetAllScheduleFile(ScheduleWorkDirectory);
        }
    }
}
