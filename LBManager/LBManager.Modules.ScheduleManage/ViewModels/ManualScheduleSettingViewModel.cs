using Prism.Mvvm;
using System;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ManualScheduleSettingViewModel : BindableBase
    {
        private DateTime _startDate = DateTime.Now;
        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }
        private DateTime _endDate = DateTime.Now;
        public DateTime EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value); }
        }
    }
}