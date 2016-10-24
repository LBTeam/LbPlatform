using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduleSummaryViewModel:BindableBase
    {
        private static double Megabytes = 1024.0 * 1024.0;
        public ScheduleSummaryViewModel(FileInfo fileInfo)
        {
            _fileName = fileInfo.Name;
            _fileSize = fileInfo.Length / Megabytes;
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



        public DelegateCommand PreviewScreenScheduleCommand { get; private set; }
    }
}
