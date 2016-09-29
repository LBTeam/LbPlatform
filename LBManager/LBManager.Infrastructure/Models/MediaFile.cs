﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class MediaFile
    {
        public string FilePath { get; set; }
        public FileType Type { get; set; }
        public string MD5 { get; set; }
    }
}
