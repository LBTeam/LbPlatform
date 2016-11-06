using LBManager.Infrastructure.Models;
using LBManager.Infrastructure.Utility;
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

namespace LBManager
{
    public class EditScheduleViewModel : BindableBase
    {
        public EditScheduleViewModel()
        {
            AddMediaCommand = new DelegateCommand(AddMedia);
        }

        private void AddMedia()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPEG|*.jpg|PNG|*.png|MP4|*.mp4";
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                string fileExtension = Path.GetExtension(openFileDialog.FileName);

                MediaList.Add(new Media()
                {
                    URL = fileInfo.FullName,
                    Type = fileInfo.Extension.Equals(".mp4",StringComparison.CurrentCultureIgnoreCase)?MediaType.Video:MediaType.Image,
                    FileSize = fileInfo.Length,
                    MD5 = FileUtils.ComputeFileMd5(fileInfo.FullName)
                });
            }
        }

        private ObservableCollection<Media> _mediaList = new ObservableCollection<Media>();
        public ObservableCollection<Media> MediaList
        {
            get { return _mediaList; }
            set { SetProperty(ref _mediaList, value); }
        }

        private string _scheduleName;
        public string ScheduleName
        {
            get { return _scheduleName; }
            set { SetProperty(ref _scheduleName, value); }
        }

        private ScheduleType _type = ScheduleType.Common;
        public ScheduleType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }


        public DelegateCommand AddMediaCommand { get; private set; }

    }
}
