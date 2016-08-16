using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
   public class ProgramScheduleDetailViewModel:BindableBase
    {

        public ProgramScheduleDetailViewModel()
        {
            MediaList = new ObservableCollection<MediaItem>()
            {
                new MediaItem()
                {
                    Name = "001.avi",
                    Duration ="12:10",
                    Size = "3MB"
                },
                 new MediaItem()
                {
                    Name = "002.avi",
                    Duration ="12:10",
                    Size = "3MB"
                },
                  new MediaItem()
                {
                    Name = "003.avi",
                    Duration ="12:10",
                    Size = "3MB"
                },
                   new MediaItem()
                {
                    Name = "004.avi",
                    Duration ="12:10",
                    Size = "3MB"
                },
                    new MediaItem()
                {
                    Name = "005.avi",
                    Duration ="12:10",
                    Size = "3MB"
                },

            };
        }

        private ObservableCollection<MediaItem> _mediaList;
        public ObservableCollection<MediaItem> MediaList
        {
            get { return _mediaList; }
            set { SetProperty(ref _mediaList, value); }
        }
    }

    public class MediaItem:BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        private string _size;
        public string Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }
    }
}
