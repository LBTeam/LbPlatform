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
            ScheduleWorkDirectorys.Add(new ScheduleDirectoryViewModel(new DirectoryInfo(mediaDirectory), null));
            //InitializeFileWatcher(mediaDirectory);
        }

        private ObservableCollection<ScheduleDirectoryViewModel> _scheduleWorkDirectorys = new ObservableCollection<ScheduleDirectoryViewModel>();
        public ObservableCollection<ScheduleDirectoryViewModel> ScheduleWorkDirectorys
        {
            get { return _scheduleWorkDirectorys; }
            set { SetProperty(ref _scheduleWorkDirectorys, value); }
        }


        private ObservableCollection<ScheduleFileViewModel> _selectedSchedules = new ObservableCollection<ScheduleFileViewModel>();
        public ObservableCollection<ScheduleFileViewModel> SelectedSchedules
        {
            get { return _selectedSchedules; }
            set { SetProperty(ref _selectedSchedules, value); }
        }

        //private void InitializeFileWatcher(string targetDirectory)
        //{
        //    _fileWatcher = new FileSystemWatcher();
        //    _fileWatcher.Path = targetDirectory;
        //    _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        //    _fileWatcher.Filter = "";
        //    _fileWatcher.IncludeSubdirectories = true;
        //    _fileWatcher.EnableRaisingEvents = true;

        //    _fileWatcher.Created += _fileWatcher_Created;
        //    _fileWatcher.Changed += _fileWatcher_Changed;
        //    _fileWatcher.Renamed += _fileWatcher_Renamed;
        //    _fileWatcher.Deleted += _fileWatcher_Deleted;
        //}

        //private void _fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        //{
        //    _logger.Log(string.Format("删除{0}文件", e.FullPath), Category.Debug, Priority.Medium);
        //    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        ScheduleSummaryList.ToList().RemoveAll(s => s.FilePath == e.FullPath);
        //    }), null);
        //}

        //private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        //{
        //    _logger.Log(string.Format("重命名{0}文件为{1}", e.OldFullPath, e.FullPath), Category.Debug, Priority.Medium);
        //    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        ScheduleSummaryList.ToList().RemoveAll(s => s.FilePath == e.OldFullPath);
        //        ScheduleSummaryList.Add(new ScheduleFileViewModel(new FileInfo(e.FullPath)));
        //    }), null);
        //}

        //private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        ScheduleSummaryList.Clear();
        //        FetchProgramSchedules(mediaDirectory);
        //    }), null);
        //    //throw new NotImplementedException();
        //}

        //private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        //{
        //    _logger.Log(string.Format("新增{0}文件", e.FullPath), Category.Debug, Priority.Medium);
        //    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        ScheduleSummaryList.Add(new ScheduleFileViewModel(new FileInfo(e.FullPath)));
        //    }), null);

        //    //throw new NotImplementedException();
        //}
    }
}
