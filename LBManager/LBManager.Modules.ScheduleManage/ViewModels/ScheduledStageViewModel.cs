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
        public ScheduledStageViewModel()
        {
            MediaList.CollectionChanged += MediaList_CollectionChanged;
            StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 7, 30, 0);
            EndTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 21, 30, 0);
            LoopCount = 1;
        }

        public ScheduledStageViewModel(ScheduledStage stage)
        {
            MediaList.CollectionChanged += MediaList_CollectionChanged;
            StartTime = stage.StartTime;
            EndTime = stage.EndTime;
            LoopCount = stage.LoopCount;
            foreach (var mediaItem in stage.MediaList)
            {
                _mediaList.Add(new MediaViewModel(mediaItem));
            }
            CurrentMedia = _mediaList.Count == 0 ? null : _mediaList[0];
        }

        private void MediaList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MediaCount = MediaList.Count;
            RealTimeSpan = TimeSpan.FromSeconds(MediaList.Sum(m => m.Duration.TotalSeconds)* LoopCount);
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

        private DateTime _startTime;//= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 7, 30, 0);
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                SetProperty(ref _startTime, value);
                StageTimeSpan = _endTime - _startTime;
            }
        }

        private DateTime _endTime;//= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 21, 30, 0);
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                SetProperty(ref _endTime, value);
                StageTimeSpan = _endTime - _startTime;
            }
        }

        private int _loopCount;// = 1;
        public int LoopCount
        {
            get { return _loopCount; }
            set
            {
                SetProperty(ref _loopCount, value);
                RealTimeSpan = TimeSpan.FromSeconds(_mediaList.Sum(m => m.Duration.TotalSeconds) * _loopCount);
            }
        }

        private TimeSpan _stageTimeSpan;
        public TimeSpan StageTimeSpan
        {
            get { return _stageTimeSpan; }
            set { SetProperty(ref _stageTimeSpan, value); }
        }

        private TimeSpan _realTimeSpan;
        public TimeSpan RealTimeSpan
        {
            get { return _realTimeSpan; }
            set { SetProperty(ref _realTimeSpan, value); }
        }

        private int _mediaCount;
        public int MediaCount
        {
            get { return _mediaCount; }
            set { SetProperty(ref _mediaCount, value); }
        }


        private ArrangementMode _arrangementMode;
        public ArrangementMode ArrangementMode
        {
            get { return _arrangementMode; }
            set
            {
                SetProperty(ref _arrangementMode, value);
            }
        }

        //private void GenerateWillPlayMediaList(ArrangementMode mode)
        //{
        //    switch (mode)
        //    {
        //        case ArrangementMode.StandardCovered:
        //            foreach (var mediaItem in MediaList)
        //            {
        //                var currentMediaTotalTime = mediaItem.Duration.TotalSeconds * mediaItem.LoopCount;
                        
        //            }
        //            break;
        //        case ArrangementMode.MixedCovered:
        //            break;
        //        case ArrangementMode.Manual:
        //            break;
        //        default:
        //            break;
        //    }
        //}

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
                CurrentMedia.Duration = TimeSpan.FromSeconds(Math.Round(info.Duration.TotalSeconds, 0));
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
                case ".wmv":
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
