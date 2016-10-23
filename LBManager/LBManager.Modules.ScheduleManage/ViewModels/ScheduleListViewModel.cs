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
    public class ScheduleListViewModel: BindableBase
    {
        private FileSystemWatcher _fileWatcher;
        public ScheduleListViewModel()
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

        private ObservableCollection<ScheduleViewModel> _scheduleList = new ObservableCollection<ScheduleViewModel>();
        public ObservableCollection<ScheduleViewModel> ScheduleList
        {
            get { return _scheduleList; }
            set { SetProperty(ref _scheduleList, value); }
        }

        private ScheduleViewModel _currentSchedule;
        public ScheduleViewModel CurrentSchedule
        {
            get { return _currentSchedule; }
            set { SetProperty(ref _currentSchedule, value); }
        }

        public DelegateCommand NewScheduleCommand { get; private set; }


        private void FetchProgramSchedules(string directoryPath)
        {
            DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileInfo in folder.GetFiles("*.playprog"))
            {
                ScheduleList.Add(new ScheduleViewModel(fileInfo));
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
                ScheduleList.Add(new ScheduleViewModel(new FileInfo(e.FullPath)));
            }), null);

            //throw new NotImplementedException();
        }
    }
}
