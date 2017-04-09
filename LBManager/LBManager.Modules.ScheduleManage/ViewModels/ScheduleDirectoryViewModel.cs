using LBManager.Modules.ScheduleManage.Views;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduleDirectoryViewModel : TreeViewItemViewModel
    {
        private ILoggerFacade _logger;
        public ScheduleDirectoryViewModel(DirectoryInfo directoryInfo, TreeViewItemViewModel parent)
            : base(parent, true)
        {
            FetchSchedules(directoryInfo);
            DirectoryName = Path.GetFileName( directoryInfo.FullName);
        }
        private string _directoryName = string.Empty;

        public string DirectoryName
        {
            get { return _directoryName; }
            set { SetProperty(ref _directoryName, value); }
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
            scheduleView.DataContext = new ScheduleViewModel();
            scheduleView.ShowDialog();
        }

        private DelegateCommand _deleteScheduleDirectoryCommand;

        public DelegateCommand DeleteScheduleCommand
        {
            get
            {
                if (_deleteScheduleDirectoryCommand == null)
                {
                    _deleteScheduleDirectoryCommand = new DelegateCommand(DeleteScheduleDirectory, CanDeleteScheduleDirectory);
                }
                return _deleteScheduleDirectoryCommand;
            }
        }

        private bool CanDeleteScheduleDirectory()
        {
            return true;
        }

        private void DeleteScheduleDirectory()
        {
            //if (File.Exists(CurrentScheduleSummary.FilePath))
            //{
            //    File.Delete(CurrentScheduleSummary.FilePath);
            //}
            //ScheduleSummaryList.Remove(CurrentScheduleSummary);
            //if (ScheduleSummaryList.Count > 0)
            //{
            //    CurrentScheduleSummary = ScheduleSummaryList[0];
            //}
            //else
            //{
            //    CurrentScheduleSummary = null;
            //}
        }


        //private DelegateCommand _fetchBackedUpScheduleCommand;
        //public DelegateCommand FetchBackedUpScheduleCommand
        //{
        //    get
        //    {
        //        if (_fetchBackedUpScheduleCommand == null)
        //        {
        //            _fetchBackedUpScheduleCommand = new DelegateCommand(FetchBackedUpSchedule, CanFetchBackedUpSchedule);
        //        }
        //        return _fetchBackedUpScheduleCommand;
        //    }
        //}

        //private DelegateCommand _backupScheduleCommand;

        //public DelegateCommand BackupScheduleCommand
        //{
        //    get
        //    {
        //        if (_backupScheduleCommand == null)
        //        {
        //            _backupScheduleCommand = new DelegateCommand(BackupSchedule, CanBackupSchedule);
        //        }
        //        return _backupScheduleCommand;
        //    }
        //}

        //private bool CanBackupSchedule()
        //{
        //    return CurrentScheduleSummary == null ? false : true;
        //}

        //private async void BackupSchedule()
        //{
        //    // BackupScheduleRequest backupRequest = new BackupScheduleRequest();
        //    var request = new BackupScheduleRequest();
        //    request.FileName = CurrentScheduleSummary.FilePath;
        //    request.FileMD5 = FileUtils.ComputeFileMd5(CurrentScheduleSummary.FilePath);

        //    //string jsonRequest = JsonConvert.SerializeObject(request);
        //    bool bacupResult = await _scheduleService.BackupSchedules(request);
        //}

        //private bool CanFetchBackedUpSchedule()
        //{
        //    return true;
        //}

        //private async void FetchBackedUpSchedule()
        //{
        //    bool bacupResult = await _scheduleService.GetBackedUpSchedules();
        //}

        private void FetchSchedules(DirectoryInfo directoryInfo)
        {
            //_logger.Log("获取本机播放方案...", Category.Debug, Priority.Medium);
            //DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (var directory in directoryInfo.GetDirectories())
            {
                Children.Add(new ScheduleDirectoryViewModel(directory, this));
            }
            foreach (FileInfo file in directoryInfo.GetFiles("*.playprog"))
            {
                Children.Add(new ScheduleFileViewModel(file, this));
            }
        }
    }
}