using LBManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class Media
    {
        public Media()
        {
        }


        public string URL { get; set; }
        public long FileSize { get; set; }
        public MediaType Type { get; set; }
        public string MD5 { get; set; }
        public string Cron { get; set; }
        public string Content { get; set; }



    }
}
