using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Interfaces;
using LBManager.Modules.ScheduleManage.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ScreenListViewModel:BindableBase
    {
        private IScreenService _screenService;
        private ScheduleSummaryListViewModel _scheduleList;
        public ScreenListViewModel(IScreenService screenService, ScheduleSummaryListViewModel scheduleList)
        {
            _screenService = screenService;
            _scheduleList = scheduleList;
            Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Subscribe(state => 
            {
                if (state.Status)
                {
                    FetchScreens();
                }
                else
                {
                    ScreenList.Clear();
                }
            });
        }

        private async void FetchScreens()
        {
            var screens = await _screenService.GetScreens();
            foreach (var item in screens)
            {
                ScreenList.Add(new ScreenViewModel(item, _scheduleList));
            }
        }

        private ObservableCollection<ScreenViewModel> _screenList = new ObservableCollection<ScreenViewModel>();
        public ObservableCollection<ScreenViewModel> ScreenList
        {
            get { return _screenList; }
            set { SetProperty(ref _screenList, value); }
        }


    }
}
