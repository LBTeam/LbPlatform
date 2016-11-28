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
using LBManager.Modules.ScheduleManage.Utility;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class DisplayRegionViewModel : BindableBase
    {
        private DisplayRegion _displayRegion;
        public DisplayRegionViewModel()
        {
            _selectedScheduledStage = new ScheduledStageViewModel();
            _scheduledStageList.Add(_selectedScheduledStage);
        }


        public DisplayRegionViewModel(DisplayRegion displayRegion)
        {
            _displayRegion = displayRegion;
            _x = displayRegion.X;
            _y = displayRegion.Y;
            _width = displayRegion.Width;
            _heigh = displayRegion.Heigh;
            _name = displayRegion.Name;
            //var mediaList = displayRegion.GetRegionMediaList();
            foreach (var stageItem in displayRegion.StageList)
            {
                _scheduledStageList.Add(new ScheduledStageViewModel(stageItem));
            }
            _selectedScheduledStage = _scheduledStageList.Count == 0 ? null : _scheduledStageList[0];
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

        private ScheduledStageViewModel _selectedScheduledStage;
        public ScheduledStageViewModel SelectedScheduledStage
        {
            get { return _selectedScheduledStage; }
            set { SetProperty(ref _selectedScheduledStage, value); }
        }

        private ObservableCollection<ScheduledStageViewModel> _scheduledStageList = new ObservableCollection<ScheduledStageViewModel>();
        public ObservableCollection<ScheduledStageViewModel> ScheduledStageList
        {
            get { return _scheduledStageList; }
            set { SetProperty(ref _scheduledStageList, value); }
        }
      
    }
}