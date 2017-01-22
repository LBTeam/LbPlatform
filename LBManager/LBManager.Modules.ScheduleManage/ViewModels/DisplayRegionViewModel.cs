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
using LBManager.Infrastructure.Common.Utility;
using LBManager.Modules.ScheduleManage.Event;
using Itenso.TimePeriod;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class DisplayRegionViewModel : BindableBase
    {
        private DisplayRegion _displayRegion;
        public DisplayRegionViewModel()
        {
            ScheduledStageList.CollectionChanged += ScheduledStageList_CollectionChanged;
            SelectedScheduledStage = new ScheduledStageViewModel(this);
            ScheduledStageList.Add(SelectedScheduledStage);
            Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Subscribe(() => { VerifyTime(); });
        }



        public DisplayRegionViewModel(DisplayRegion displayRegion)
        {
            _displayRegion = displayRegion;
            _x = displayRegion.X;
            _y = displayRegion.Y;
            _width = displayRegion.Width;
            _heigh = displayRegion.Heigh;
            _name = displayRegion.Name;
            _scheduleMode = displayRegion.ScheduleMode;
            _repeatMode = displayRegion.RepeatMode;


            ScheduledStageList.CollectionChanged += ScheduledStageList_CollectionChanged;
            foreach (var stageItem in displayRegion.StageList)
            {
                ScheduledStageList.Add(new ScheduledStageViewModel(this, stageItem));
            }
            SelectedScheduledStage = _scheduledStageList.Count == 0 ? null : _scheduledStageList[0];

            if (_repeatMode == RepeatMode.Manual)
            {
                ManualScheduleSetting.StartDate = SelectedScheduledStage.StartTime;
                ManualScheduleSetting.EndDate = SelectedScheduledStage.EndTime;
            }

            Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Subscribe(() => { VerifyTime(); });
        }


        private void VerifyTime()
        {
            TimePeriodCollection periods = new TimePeriodCollection();

            TimeConflictError = string.Empty;

            foreach (var stageItem in ScheduledStageList)
            {
                int periodDays = (stageItem.EndDate.Date - stageItem.StartDate.Date).Days;
                for (int i = 0; i < periodDays; i++)
                {
                    DateTime start = new DateTime(stageItem.StartDate.Year, stageItem.StartDate.Month, stageItem.StartDate.Day, stageItem.StartTime.Hour, stageItem.StartTime.Minute, stageItem.StartTime.Second).AddDays(i);
                    DateTime end = new DateTime(stageItem.StartDate.Year, stageItem.StartDate.Month, stageItem.StartDate.Day, stageItem.EndTime.Hour, stageItem.EndTime.Minute, stageItem.EndTime.Second).AddDays(i);
                    periods.Add(new TimeRange(start, end));
                }
            }

            TimePeriodIntersector<TimeRange> periodIntersector =
                              new TimePeriodIntersector<TimeRange>();
            ITimePeriodCollection intersectedPeriods = periodIntersector.IntersectPeriods(periods);

            foreach (ITimePeriod intersectedPeriod in intersectedPeriods)
            {
                TimeConflictError += "\n在" + intersectedPeriod + "时段,时间冲突！";
                // Console.WriteLine("Intersected Period: " + intersectedPeriod);
            }
        }
        private void ScheduledStageList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RemoveScheduledStageCommand.RaiseCanExecuteChanged();
        }

        private string _timeConflictError = string.Empty;
        public string TimeConflictError
        {
            get { return _timeConflictError; }
            set { SetProperty(ref _timeConflictError, value); }
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

        private ScheduleMode _scheduleMode = ScheduleMode.CPM;
        public ScheduleMode ScheduleMode
        {
            get { return _scheduleMode; }
            set { SetProperty(ref _scheduleMode, value); }
        }

        private RepeatMode _repeatMode = RepeatMode.Daily;
        public RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                SetProperty(ref _repeatMode, value);
                Messager.Default.EventAggregator.GetEvent<OnManualModelChangedEvent>().Publish(_repeatMode == RepeatMode.Manual ? true : false);
            }
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


        private DailyScheduleSettingViewModel _dailyScheduleSetting = new DailyScheduleSettingViewModel();
        public DailyScheduleSettingViewModel DailyScheduleSetting
        {
            get { return _dailyScheduleSetting; }
            set { SetProperty(ref _dailyScheduleSetting, value); }
        }

        private ManualScheduleSettingViewModel _manualScheduleSetting = new ManualScheduleSettingViewModel();
        public ManualScheduleSettingViewModel ManualScheduleSetting
        {
            get { return _manualScheduleSetting; }
            set { SetProperty(ref _manualScheduleSetting, value); }
        }

        private DelegateCommand _addScheduledStageCommand;
        public DelegateCommand AddScheduledStageCommand
        {
            get
            {
                if (_addScheduledStageCommand == null)
                {
                    _addScheduledStageCommand = new DelegateCommand(AddScheduledStage, CanAddScheduledStage);
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
            ScheduledStageViewModel stageViewModel = new ScheduledStageViewModel(this);
            ScheduledStageList.Add(stageViewModel);
            SelectedScheduledStage = stageViewModel;
        }

        public string GetCron(int second, int minute, int hour)
        {
            string result = string.Empty;
            switch (RepeatMode)
            {
                case RepeatMode.Daily:
                    if (DailyScheduleSetting.IsPeriod)
                    {
                        result = string.Format("{0} {1} {2} 1/{3} * ? *", second, minute, hour, DailyScheduleSetting.Period);
                    }
                    else if (DailyScheduleSetting.IsWorkingDay)
                    {
                        result = string.Format("{0} {1} {2} ? * MON-FRI *", second, minute, hour);
                    }
                    break;
                case RepeatMode.Weekly:
                    break;
                case RepeatMode.Monthly:
                    break;
                case RepeatMode.Manual:
                    break;
                default:
                    break;
            }

            return result;
        }

        public void Cleanup()
        {
            Messager.Default.EventAggregator.GetEvent<OnTimeValidationTriggeredEvent>().Unsubscribe(() => { VerifyTime(); });
            foreach (var item in ScheduledStageList)
            {
                item.Cleanup();
            }
        }
    }
}