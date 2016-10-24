using LBManager.Modules.ScheduleManage.Views;
using Prism.Commands;
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
    public class ScheduleSummaryListViewModel: BindableBase
    {
        private FileSystemWatcher _fileWatcher;
        public ScheduleSummaryListViewModel()
        {
            string mediaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media");
            if (!Directory.Exists(mediaDirectory))
            {
                Directory.CreateDirectory(mediaDirectory);
            }
            FetchProgramSchedules(mediaDirectory);
            InitializeFileWatcher(mediaDirectory);
            NewScheduleCommand = new DelegateCommand(() => { NewSchedule(); }, () => { return CanNewSchedule(); });
        }

        private bool CanNewSchedule()
        {
            return true;
        }

        private void NewSchedule()
        {
            ScheduleView scheduleView = new ScheduleView();
            scheduleView.Owner = Application.Current.MainWindow;
            scheduleView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            scheduleView.ShowDialog();
        }

        private ObservableCollection<ScheduleSummaryViewModel> _scheduleSummaryList = new ObservableCollection<ScheduleSummaryViewModel>();
        public ObservableCollection<ScheduleSummaryViewModel> ScheduleSummaryList
        {
            get { return _scheduleSummaryList; }
            set { SetProperty(ref _scheduleSummaryList, value); }
        }

        private ScheduleSummaryViewModel _currentScheduleSummary;
        public ScheduleSummaryViewModel CurrentScheduleSummary
        {
            get { return _currentScheduleSummary; }
            set { SetProperty(ref _currentScheduleSummary, value); }
        }

        public DelegateCommand NewScheduleCommand { get; private set; }


        private void FetchProgramSchedules(string directoryPath)
        {
            DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileInfo in folder.GetFiles("*.playprog"))
            {
                ScheduleSummaryList.Add(new ScheduleSummaryViewModel(fileInfo));
            }
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
            throw new NotImplementedException();
        }

        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ScheduleSummaryList.Add(new ScheduleSummaryViewModel(new FileInfo(e.FullPath)));
            }), null);

            //throw new NotImplementedException();
        }
    }
}
