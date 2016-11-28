using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Models;
using LBManager.Modules.ScheduleManage.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            CurrentDisplayRegion = new DisplayRegionViewModel() { Name = "Default DisplayRegion" };
            DisplayRegions.Add(CurrentDisplayRegion);
            PreviewScreenScheduleCommand = new DelegateCommand(() => { PreviewScreenSchedule(); });
            SaveScheduleCommand = new DelegateCommand<ScheduleView>((v) => { SaveSchedule(v); });
        }


        private void PreviewScreenSchedule()
        {

        }

        public ScheduleViewModel(FileInfo fileInfo)
        {
            ParseScheduleFile(fileInfo.FullName);
            PreviewScreenScheduleCommand = new DelegateCommand(() => { PreviewScreenSchedule(); });
            SaveScheduleCommand = new DelegateCommand<ScheduleView>((v) => { SaveSchedule(v); });
        }

        private void ParseScheduleFile(string fullName)
        {
            string readContents = string.Empty;
            using (StreamReader streamReader = new StreamReader(fullName, Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }
            _schedule = JsonConvert.DeserializeObject<Schedule>(readContents);
            this.Name = _schedule.Name;
            this.Width = _schedule.Width;
            this.Heigh = _schedule.Heigh;
            this.Type = _schedule.Type;
            foreach (var displayRegionItem in _schedule.DisplayRegionList)
            {
                DisplayRegions.Add(new DisplayRegionViewModel(displayRegionItem));
            }
            CurrentDisplayRegion = DisplayRegions.Count == 0 ? null : DisplayRegions[0];
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

        private ScheduleType _type;
        public ScheduleType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
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
                    Messager.Default.EventAggregator
                                    .GetEvent<OnCurrentDisplayRegionChangedEvent>()
                                    .Publish(new OnCurrentDisplayRegionChangedEventArg(_currentDisplayRegion?.Region, value.Region));
                    SetProperty(ref _currentDisplayRegion, value);
                }
            }
        }


        public DelegateCommand PreviewScreenScheduleCommand { get; private set; }
        public DelegateCommand AddDisplayRegionCommand { get; private set; }
        public DelegateCommand RemoveDisplayRegionCommand { get; private set; }
        public DelegateCommand PreviewDisplayRegionCommand { get; private set; }
        public DelegateCommand<ScheduleView> SaveScheduleCommand { get; private set; }
       

        private void SaveSchedule(ScheduleView view)
        {

            string scheduleFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media", this.Name + ".playprog");
            using (StreamWriter outputFile = new StreamWriter(scheduleFilePath, false))
            {
                _schedule = new Schedule();
                _schedule.Name = this.Name;
                _schedule.Type = this.Type;
                _schedule.Width = this.Width;
                _schedule.Heigh = this.Heigh;
                _schedule.DisplayRegionList = new List<DisplayRegion>();
                foreach (var displayRegionitem in this.DisplayRegions)
                {
                    var displayRegion = new DisplayRegion();
                    displayRegion.Name = displayRegionitem.Name;
                    displayRegion.X = displayRegionitem.X;
                    displayRegion.Y = displayRegionitem.Y;
                    displayRegion.Width = displayRegionitem.Width;
                    displayRegion.Heigh = displayRegionitem.Heigh;
                    displayRegion.StageList = new List<ScheduledStage>();
                    foreach (var stageItem in displayRegionitem.ScheduledStageList)
                    {
                        var stage = new ScheduledStage();
                        stage.StartTime = stageItem.StartTime;
                        stage.EndTime = stageItem.EndTime;
                        stage.LoopCount = stageItem.LoopCount;
                        stage.MediaList = new List<Media>();
                        foreach (var mediaItem in stageItem.MediaList)
                        {
                            var media = new Media();
                            media.URL = mediaItem.FilePath;
                            media.FileSize = mediaItem.FileSize;
                            media.Type = mediaItem.FileType;
                            media.MD5 = mediaItem.MD5;
                            media.LoopCount = mediaItem.LoopCount;
                            stage.MediaList.Add(media);
                        }
                        displayRegion.StageList.Add(stage);
                    }
                    _schedule.DisplayRegionList.Add(displayRegion);
                }

                outputFile.WriteLine(JsonConvert.SerializeObject(_schedule, new IsoDateTimeConverter() { DateTimeFormat = "HH:mm:ss" }));
            }
            
            view.Close();
        }
    }

}
