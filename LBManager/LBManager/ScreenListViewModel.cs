using LBManager.Infrastructure.Interfaces;
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
        public ScreenListViewModel(IScreenService screenService)
        {
            _screenService = screenService;
            FetchScreens();
        }

        private async void FetchScreens()
        {
            var screens = await _screenService.GetScreens();
            foreach (var item in screens)
            {
                ScreenList.Add(new ScreenViewModel(item));
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
