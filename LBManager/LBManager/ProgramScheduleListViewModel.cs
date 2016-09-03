using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace LBManager
{
    public class ProgramScheduleListViewModel : BindableBase
    {
        private FileSystemWatcher _fileWatcher;
        public ProgramScheduleListViewModel()
        {
            string mediaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media");
            if (!Directory.Exists(mediaDirectory))
            {
                Directory.CreateDirectory(mediaDirectory);
            }
            FetchProgramSchedules(mediaDirectory);
            InitializeFileWatcher(mediaDirectory);

        }


        private ObservableCollection<ScheduleFileInfo> _scheduleFileInfoList = new ObservableCollection<ScheduleFileInfo>();
        public ObservableCollection<ScheduleFileInfo> ScheduleFileInfoList
        {
            get { return _scheduleFileInfoList; }
            set { SetProperty(ref _scheduleFileInfoList, value); }
        }


        private void FetchProgramSchedules(string directoryPath)
        {
            DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileInfo in folder.GetFiles("*.playprog"))
            {
                ScheduleFileInfoList.Add(new ScheduleFileInfo(fileInfo));
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
                ScheduleFileInfoList.Add(new ScheduleFileInfo(new FileInfo(e.FullPath)));
            }), null);

            //throw new NotImplementedException();
        }
    }

    public class ScheduleFileInfo : BindableBase
    {
        public ScheduleFileInfo()
        { }
        public ScheduleFileInfo(FileInfo fileInfo)
        {
            _fileName = fileInfo.Name;
            _fileSize = fileInfo.Length / 1048576.0;
            _lastWriteTime = fileInfo.LastWriteTime;
            _filePath = fileInfo.FullName;
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        private double _fileSize;
        public double FileSize
        {
            get { return _fileSize; }
            set { SetProperty(ref _fileSize, value); }
        }

        private DateTime _lastWriteTime;
        public DateTime LastWriteTime
        {
            get { return _lastWriteTime; }
            set { SetProperty(ref _lastWriteTime, value); }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

    }
}
