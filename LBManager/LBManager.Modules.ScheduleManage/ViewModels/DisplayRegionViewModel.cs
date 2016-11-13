using LBManager.Infrastructure.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System;
using Microsoft.Win32;
using System.IO;
using LBManager.Infrastructure.Utility;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class DisplayRegionViewModel : BindableBase
    {
        private DisplayRegion _displayRegion;
        public DisplayRegionViewModel()
        {
           // AddMediaCommand = new DelegateCommand(() => { AddMedia(); });
        }


        public DisplayRegionViewModel(DisplayRegion displayRegion)
        {
            _displayRegion = displayRegion;
            _x = displayRegion.X;
            _y = displayRegion.Y;
            _width = displayRegion.Width;
            _heigh = displayRegion.Heigh;
            _name = displayRegion.Name;
            foreach (var mediaItem in displayRegion.MediaList)
            {
                MediaList.Add(new MediaViewModel(mediaItem));
            }
            CurrentMedia = MediaList.Count == 0 ? null : MediaList[0];

           // AddMediaCommand = new DelegateCommand(() => { AddMedia(); });
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private int _x;
        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _heigh;
        public int Heigh
        {
            get { return _heigh; }
            set { SetProperty(ref _heigh, value); }
        }

        public DisplayRegion Region { get { return _displayRegion; } }

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
            set { SetProperty(ref _currentMedia, value); }
        }

        public DelegateCommand AddMediaCommand => new DelegateCommand(AddMedia);
        public DelegateCommand RemoveMediaCommand => new DelegateCommand(RemoveMedia, CanRemoveMedia);

        private bool CanRemoveMedia()
        {
            return CurrentMedia == null ? false : true;
        }

        private void RemoveMedia()
        {
            MediaList.Remove(CurrentMedia);
            if (MediaList.Count >0)
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
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "JPEG|*.jpg|PNG|*.png|MP4|*.mp4";
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
            //    string fileExtension = Path.GetExtension(openFileDialog.FileName);

            //    MediaList.Add(new Media()
            //    {
            //        URL = fileInfo.FullName,
            //        Type = fileInfo.Extension.Equals(".mp4", StringComparison.CurrentCultureIgnoreCase) ? MediaType.Video : MediaType.Image,
            //        FileSize = fileInfo.Length,
            //        MD5 = FileUtils.ComputeFileMd5(fileInfo.FullName)
            //    });
            //}

            var view = new OpenFileDialog()
            {
                Filter = "Video File|*.mp4;*.mov;*.wmv"
            };

            if (view.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(view.FileName);
                string fileExtension = Path.GetExtension(view.FileName);

                var media = new Media()
                {
                    URL = fileInfo.FullName,
                    Type = GetMediaType(fileInfo.Extension),
                    FileSize = fileInfo.Length,
                    MD5 = FileUtils.ComputeFileMd5(fileInfo.FullName),
                    LoopCount = 1
                };
                MediaList.Add(new MediaViewModel(media));
            }

            // var result = DialogHost.Show(view, "ScheduleRootDialog", AddMediaDialogClosingEventHandler);
        }

        //private void AddMediaDialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //{
        //    if ((bool)eventArgs.Parameter == false) return;
        //    eventArgs.Cancel();

        //    DialogHost dialog = sender as DialogHost;
        //    var view = dialog.DialogContent as OpenFileDialog;
        //    if (view.ShowDialog() == true)
        //    {
        //        FileInfo fileInfo = new FileInfo(view.FileName);
        //        string fileExtension = Path.GetExtension(view.FileName);

        //        var media = new Media()
        //        {
        //            URL = fileInfo.FullName,
        //            Type = GetMediaType(fileInfo.Extension),
        //            FileSize = fileInfo.Length,
        //            MD5 = FileUtils.ComputeFileMd5(fileInfo.FullName)
        //        };
        //    }

        //    Task.Delay(TimeSpan.FromSeconds(1))
        //        .ContinueWith((t, _) =>
        //            eventArgs.Session.Close(false),
        //            null,
        //            TaskScheduler.FromCurrentSynchronizationContext());
        //}

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