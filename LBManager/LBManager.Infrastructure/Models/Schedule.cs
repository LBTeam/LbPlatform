using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class Schedule
    {
        public string Name { get; set; }
        public ScheduleType Type { get; set; }
        public int Width { get; set; }
        public int Heigh { get; set; }
        public List<DisplayRegion> DisplayRegionList { get; set; }

        public List<Media> GetAllMedia()
        {
            List<Media> mediaList = new List<Media>();
            foreach (var regionItem in DisplayRegionList)
            {
                foreach (var stageItem in regionItem.StageList)
                {
                    mediaList.AddRange(stageItem.MediaList);
                }
            }
            return mediaList;
        }

        public bool VerifyTimeConflict()
        {
            return true;
        }

    }

    public class DisplayRegion
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Heigh { get; set; }
        public List<ScheduledStage> StageList { get; set; }

        public RepeatMode RepeatMode { get; set; }
        public List<Media> GetRegionMediaList()
        {
            List<Media> mediaList = new List<Media>();
            foreach (var stageItem in StageList)
            {
                mediaList.AddRange(stageItem.MediaList);
            }
            return mediaList;
        }
    }
}
