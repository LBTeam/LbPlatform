using LBManager.Infrastructure.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class DisplayRegionViewModel:BindableBase
    {
        private DisplayRegion _displayRegion;
        public DisplayRegionViewModel()
        {

        }
        public DisplayRegionViewModel(DisplayRegion displayRegion)
        {
            _displayRegion = displayRegion;
            _x = displayRegion.X;
            _y = displayRegion.Y;
            _width = displayRegion.Width;
            _heigh = displayRegion.Heigh;
            _name = displayRegion.Name;
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
    }
}