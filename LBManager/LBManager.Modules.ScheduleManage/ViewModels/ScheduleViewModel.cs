using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class ScheduleViewModel : BindableBase
    {
        private Schedule _schedule;
        public ScheduleViewModel()
        {

            PreviewScreenScheduleCommand = new DelegateCommand(() => { PreviewScreenSchedule(); });
        }

        public ScheduleViewModel(Schedule schedule)
            : this()
        {

        }

        private void PreviewScreenSchedule()
        {

        }

        public ScheduleViewModel(FileInfo fileInfo)
            : this()
        {
            ParseScheduleFile(fileInfo.FullName);
        }

        private void ParseScheduleFile(string fullName)
        {
            string readContents = string.Empty;
            using (StreamReader streamReader = new StreamReader(fullName, Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }
            _schedule = JsonConvert.DeserializeObject<Schedule>(readContents);
            foreach (var displayRegionItem in _schedule.DisplayRegionList)
            {
                DisplayRegions.Add(new DisplayRegionViewModel(displayRegionItem));
            }
            CurrentDisplayRegion = DisplayRegions[0];
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
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

        private ObservableCollection<DisplayRegionViewModel> _displayRegions = new ObservableCollection<DisplayRegionViewModel>();
        public ObservableCollection<DisplayRegionViewModel> DisplayRegions
        {
            get { return _displayRegions; }
            set { SetProperty(ref _displayRegions, value); }
        }

        private DisplayRegionViewModel _currentDisplayRegion;
        public DisplayRegionViewModel CurrentDisplayRegion
        {
            get { return _currentDisplayRegion; }
            set
            {
                if (value != _currentDisplayRegion)
                {
                    Messager.Default.EventAggregator.GetEvent<OnCurrentDisplayRegionChangedEvent>().Publish(new OnCurrentDisplayRegionChangedEventArg(_currentDisplayRegion.Region, value.Region));
                    SetProperty(ref _currentDisplayRegion, value);
                }
            }
        }

        public DelegateCommand PreviewScreenScheduleCommand { get; private set; }
    }

}
