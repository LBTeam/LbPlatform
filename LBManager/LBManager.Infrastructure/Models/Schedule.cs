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
            foreach (var item in DisplayRegionList)
            {
                mediaList.AddRange(item.MediaList);
            }
            return mediaList;
        }
    }

    public class DisplayRegion
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Heigh { get; set; }
        public List<Media> MediaList { get; set; }
        public List<VideoMedia> VideoMediaList { get; set; }
    }
}
