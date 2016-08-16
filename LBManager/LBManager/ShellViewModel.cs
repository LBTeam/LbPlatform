using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ShellViewModel : BindableBase
    {
        public ShellViewModel()
        {
            ScheduleDetailViewModel = new ProgramScheduleDetailViewModel();
        }

        private ProgramScheduleDetailViewModel _scheduleDetailViewModel;
        public ProgramScheduleDetailViewModel ScheduleDetailViewModel
        {
            get { return _scheduleDetailViewModel; }
            set { SetProperty(ref _scheduleDetailViewModel, value); }
        }


    }
}
