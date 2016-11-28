using System;

namespace LBManager.Infrastructure.Models
{
    public class Media
    {
        public Media()
        {
        }


        public string URL { get; set; }
        public long FileSize { get; set; }
        public TimeSpan Duration { get; set; }
        public MediaType Type { get; set; }
        public string MD5 { get; set; }
        public string Cron { get; set; }
        public string Content { get; set; }
        public int LoopCount { get; set; }


    }
}
