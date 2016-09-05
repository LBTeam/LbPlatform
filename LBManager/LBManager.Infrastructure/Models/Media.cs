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


        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public FileType FileType { get; set; }
        public string FileMD5 { get; set; }



    }
}
