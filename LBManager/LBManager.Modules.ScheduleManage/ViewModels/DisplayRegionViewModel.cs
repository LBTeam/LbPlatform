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
            ScheduledStageList.CollectionChanged += ScheduledStageList_CollectionChanged;
            SelectedScheduledStage = new ScheduledStageViewModel();
            ScheduledStageList.Add(_selectedScheduledStage);
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
            ScheduledStageList.CollectionChanged += ScheduledStageList_CollectionChanged;
            foreach (var stageItem in displayRegion.StageList)
            {
                ScheduledStageList.Add(new ScheduledStageViewModel(stageItem));
            }
            SelectedScheduledStage = _scheduledStageList.Count == 0 ? null : _scheduledStageList[0];
        }

        private void ScheduledStageList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RemoveScheduledStageCommand.RaiseCanExecuteChanged();
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

        private DelegateCommand _addScheduledStageCommand;
        public DelegateCommand AddScheduledStageCommand
        {
            get
            {
                if (_addScheduledStageCommand == null)
                {
                    _addScheduledStageCommand = new DelegateCommand(AddScheduledStage,CanAddScheduledStage); 
                }
                return _addScheduledStageCommand;
            }
        }

        private DelegateCommand _removeScheduledStageCommand;
        public DelegateCommand RemoveScheduledStageCommand
        {
            get
            {
                if (_removeScheduledStageCommand == null)
                {
                    _removeScheduledStageCommand = new DelegateCommand(RemoveScheduledStage, CanRemoveScheduledStage);
                }
                return _removeScheduledStageCommand;
            }
        }

        private bool CanRemoveScheduledStage()
        {
            return ScheduledStageList.Count > 1 ? true : false;
        }

        private void RemoveScheduledStage()
        {
            ScheduledStageList.Remove(SelectedScheduledStage);
            if (ScheduledStageList.Count > 0)
            {
                SelectedScheduledStage = ScheduledStageList[0];
            }
            else
            {
                SelectedScheduledStage = null;
            }
           
        }

        private bool CanAddScheduledStage()
        {
            return true;
        }

        private void AddScheduledStage()
        {
            ScheduledStageViewModel stageViewModel = new ScheduledStageViewModel();
            ScheduledStageList.Add(stageViewModel);
            SelectedScheduledStage = stageViewModel;
        }
    }
}