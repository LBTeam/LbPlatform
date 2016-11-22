using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Interfaces;
using LBManager.Modules.ScheduleManage.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Prism.Logging;
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
        private ILoggerFacade _logger;
        private ScheduleSummaryListViewModel _scheduleList;
        public ScreenListViewModel(ScheduleSummaryListViewModel scheduleList)
        {
            _screenService = ServiceLocator.Current.GetInstance<IScreenService>();
            _logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();

            _scheduleList = scheduleList;
            Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Subscribe(state => 
            {
                if (state.Status)
                {
                    _logger.Log("获取LED屏信息列表", Category.Debug, Priority.Medium);
                    FetchScreens();
                }
                else
                {
                    _logger.Log("清除LED屏信息列表", Category.Debug, Priority.Medium);
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
