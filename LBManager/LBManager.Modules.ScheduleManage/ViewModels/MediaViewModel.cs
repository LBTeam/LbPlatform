using LBManager.Infrastructure.Models;
using Prism.Mvvm;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class MediaViewModel:BindableBase
    {
        public MediaViewModel() { }

        public MediaViewModel(Media media)
        {
            _name = System.IO.Path.GetFileName(media.URL);
            _filePath = media.URL;
            _fileType = media.Type;
            _fileSize = media.FileSize;
            _md5 = media.MD5;
            _cron = media.Cron;
            _content = media.Content;
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        private MediaType _fileType;
        public MediaType FileType
        {
            get { return _fileType; }
            set { SetProperty(ref _fileType, value); }
        }

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set { SetProperty(ref _fileSize, value); }
        }

        private string _md5;
        public string MD5
        {
            get { return _md5; }
            set { SetProperty(ref _md5, value); }
        }

        private string _cron;
        public string Cron
        {
            get { return _cron; }
            set { SetProperty(ref _cron, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
    }
}