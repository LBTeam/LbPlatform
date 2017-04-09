using LBManager.Modules.ScheduleManage.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduleFileViewModel : TreeViewItemViewModel
    {
        private static double Megabytes = 1024.0 * 1024.0;

       // public ScheduleFileViewModel() { }
        public ScheduleFileViewModel(FileInfo fileInfo,TreeViewItemViewModel parent)
            :base(parent,true)
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





        private DelegateCommand _editScheduleFileCommand;

        public DelegateCommand EditScheduleFileCommand
        {
            get
            {
                if (_editScheduleFileCommand == null)
                {
                    _editScheduleFileCommand = new DelegateCommand(EditScheduleFile, CanEditScheduleFile);
                }
                return _editScheduleFileCommand;
            }
        }
        private bool CanEditScheduleFile()
        {
            return true;
        }

        private void EditScheduleFile()
        {
            ScheduleView scheduleView = new ScheduleView();
            scheduleView.Owner = Application.Current.MainWindow;
            scheduleView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            scheduleView.DataContext = new ScheduleViewModel(new FileInfo(FilePath));
            scheduleView.ShowDialog();

        }

        private DelegateCommand _deleteScheduleFileCommand;

        public DelegateCommand DeleteScheduleFileCommand
        {
            get
            {
                if (_deleteScheduleFileCommand == null)
                {
                    _deleteScheduleFileCommand = new DelegateCommand(DeleteScheduleFile, CanDeleteScheduleFile);
                }
                return _deleteScheduleFileCommand;
            }
        }

        private bool CanDeleteScheduleFile()
        {
            return true;
        }

        private void DeleteScheduleFile()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
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


        public DelegateCommand PreviewScreenScheduleCommand { get; private set; }
    }
}
