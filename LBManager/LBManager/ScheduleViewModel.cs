using LBManager.Infrastructure.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ScheduleViewModel:BindableBase
    {


        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set { SetProperty(ref _fileSize, value); }
        }

        private FileType _type;
        public FileType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private string _fileMD5;
        public string FileMD5
        {
            get { return _fileMD5; }
            set { SetProperty(ref _fileMD5, value); }
        }
    }
}
