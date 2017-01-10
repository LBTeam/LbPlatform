using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class DailyScheduleSettingViewModel: BindableBase
    {
        private bool _isWorkingDay = false;
        public bool IsWorkingDay
        {
            get { return _isWorkingDay; }
            set { SetProperty(ref _isWorkingDay, value); }
        }

        private bool _isPeriod = true;
        public bool IsPeriod
        {
            get { return _isPeriod; }
            set { SetProperty(ref _isPeriod, value); }
        }

        private int _period = 1;
        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); }
        }


    }
}
