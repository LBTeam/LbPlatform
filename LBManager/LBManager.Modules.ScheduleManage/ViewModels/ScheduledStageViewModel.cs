using LBManager.Infrastructure.Models;
using LBManager.Infrastructure.Utility;
using LBManager.Modules.ScheduleManage.Utility;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduledStageViewModel : BindableBase
    {
        public ScheduledStageViewModel() { }

        public ScheduledStageViewModel(ScheduledStage stage)
        {
            _startTime = stage.StartTime;
            _endTime = stage.EndTime;
            _loopCount = stage.LoopCount;
            foreach (var mediaItem in stage.MediaList)
            {
                _mediaList.Add(new MediaViewModel(mediaItem));
            }
            _currentMedia = _mediaList.Count == 0 ? null : _mediaList[0];
        }
        private ObservableCollection<MediaViewModel> _mediaList = new ObservableCollection<MediaViewModel>();
        public ObservableCollection<MediaViewModel> MediaList
        {
            get { return _mediaList; }
            set { SetProperty(ref _mediaList, value); }
        }

        private MediaViewModel _currentMedia;
        public MediaViewModel CurrentMedia
        {
            get { return _currentMedia; }
            set
            {
                SetProperty(ref _currentMedia, value);
                RemoveMediaCommand.RaiseCanExecuteChanged();
            }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        private int _loopCount = 1;
        public int LoopCount
        {
            get { return _loopCount; }
            set { SetProperty(ref _loopCount, value); }
        }


        private DelegateCommand _removeMediaCommand;
        public DelegateCommand RemoveMediaCommand
        {
            get
            {
                if (_removeMediaCommand == null)
                {
                    _removeMediaCommand = new DelegateCommand(RemoveMedia, CanRemoveMedia);
                }
                return _removeMediaCommand;
            }
        }


        private DelegateCommand _addMediaCommand;
        public DelegateCommand AddMediaCommand
        {
            get
            {
                if (_addMediaCommand == null)
                {
                    _addMediaCommand = new DelegateCommand(AddMedia);
                }
                return _addMediaCommand;
            }
        }


        private bool CanRemoveMedia()
        {
            return CurrentMedia == null ? false : true;
        }

        private void RemoveMedia()
        {
            MediaList.Remove(CurrentMedia);
            if (MediaList.Count > 0)
            {
                CurrentMedia = MediaList[0];
            }
            else
            {
                CurrentMedia = null;
            }
        }


        private void AddMedia()
        {

            var view = new OpenFileDialog()
            {
                Filter = "Video File|*.mp4;*.mov;*.wmv"
            };

            if (view.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(view.FileName);
                string fileExtension = Path.GetExtension(view.FileName);
                FFmpegMediaInfo info = new FFmpegMediaInfo(view.FileName);
                var media = new Media()
                {
                    URL = fileInfo.FullName,
                    Type = GetMediaType(fileInfo.Extension),
                    FileSize = fileInfo.Length,
                    MD5 = FileUtils.ComputeFileMd5(fileInfo.FullName),
                    LoopCount = 1
                };
                CurrentMedia = new MediaViewModel(media);
                CurrentMedia.Duration = Math.Round(info.Duration.TotalSeconds, 1);
                MediaList.Add(CurrentMedia);
            }
        }

        private static MediaType GetMediaType(string fileExtension)
        {
            MediaType mediaType;
            switch (fileExtension.ToLower())
            {
                case ".mp4":
                case ".mov":
                    mediaType = MediaType.Video;
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                    mediaType = MediaType.Image;
                    break;
                default:
                    mediaType = MediaType.Text;
                    break;
            }
            return mediaType;
        }
    }
}
