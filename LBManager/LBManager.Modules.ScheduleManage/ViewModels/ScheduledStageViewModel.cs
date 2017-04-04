using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Models;
using LBManager.Infrastructure.Utility;
using LBManager.Modules.ScheduleManage.Event;
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
using System.Windows.Controls;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduledStageViewModel : BindableBase
    {
        public ScheduledStageViewModel(DisplayRegionViewModel parentViewModel)
        {
            MediaList.CollectionChanged += MediaList_CollectionChanged;
            StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 7, 30, 0);
            StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 7, 30, 0);

            EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 21, 30, 0);
            EndTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 21, 30, 0);

            ArrangementMode = ArrangementMode.StandardCovered;
            LoopCount = 1;

            IsManualMode = parentViewModel.RepeatMode == RepeatMode.Manual ? true : false;
            Messager.Default.EventAggregator.GetEvent<OnManualModelChangedEvent>().Subscribe(arg =>
            {
                IsManualMode = arg;
            });
        }

        public ScheduledStageViewModel(DisplayRegionViewModel parentViewModel, ScheduledStage stage)
        {
            MediaList.CollectionChanged += MediaList_CollectionChanged;
            StartDate = stage.StartTime;
            StartTime = stage.StartTime;

            EndDate = stage.EndTime;
            EndTime = stage.EndTime;
            LoopCount = stage.LoopCount;
            ArrangementMode = stage.ArrangementMode;

            IsManualMode = parentViewModel.RepeatMode == RepeatMode.Manual ? true : false;

            foreach (var mediaItem in stage.MediaList)
            {
                _mediaList.Add(new MediaViewModel(mediaItem));
            }
            CurrentMedia = _mediaList.Count == 0 ? null : _mediaList[0];

            Messager.Default.EventAggregator.GetEvent<OnManualModelChangedEvent>().Subscribe(arg =>
            {
                IsManualMode = arg;
            });
        }

        private void MediaList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MediaCount = MediaList.Count;
            RealTimeSpan = TimeSpan.FromSeconds(MediaList.Sum(m => m.Duration.TotalSeconds) * LoopCount);
        }

        private bool _isManualMode = false;
        public bool IsManualMode
        {
            get { return _isManualMode; }
            set
            {
                SetProperty(ref _isManualMode, value);
            }
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

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                SetProperty(ref _startDate, value);
                Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Publish();
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                SetProperty(ref _endDate, value);
                Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Publish();
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
                Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Publish();
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
                Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Publish();
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

        private DelegateCommand<object> _removeMediaCommand;
        public DelegateCommand<object> RemoveMediaCommand
        {
            get
            {
                if (_removeMediaCommand == null)
                {
                    _removeMediaCommand = new DelegateCommand<object>(RemoveMedia, CanRemoveMedia);
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


        private bool CanRemoveMedia(object obj)
        {
            //var listBox = obj as ListBox;
            //if (listBox != null)
            //{
            //    return listBox.SelectedItems.Count == 0 ? false : true;
            //}
            //else
            //{
            //    return CurrentMedia == null ? false : true;
            //}

            return MediaList.Any(m => m.IsSelected);

        }

        private void RemoveMedia(object obj)
        {
            var listBox = obj as ListBox;
            if (listBox != null)
            {
                List<MediaViewModel> removeList = new List<MediaViewModel>(); 
                for (int index = 0; index < listBox.SelectedItems.Count; index++)
                {
                    removeList.Add(listBox.SelectedItems[index] as MediaViewModel);
                }
                foreach (var removeItem in removeList)
                {
                    MediaList.Remove(removeItem);
                }

                if (MediaList.Count > 0)
                {
                    CurrentMedia = MediaList[0];
                }
                else
                {
                    CurrentMedia = null;
                }
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
                    LoopCount = 1,
                    Category = MediaCategory.UserAd
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

        public void Cleanup()
        {
            Messager.Default.EventAggregator.GetEvent<OnManualModelChangedEvent>().Unsubscribe(arg => { });
        }
    }
}
