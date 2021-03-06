﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class UploadScheduleFileInfo
    {
        private string _filePath;
        private long _fileSize;
        private string _fileMD5;
        private FileContentType _type;
        private ScheduleType _playType;
        private List<MediaTempInfo> _mediaList;

        public string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                _filePath = value;
            }
        }

        public long FileSize
        {
            get
            {
                return _fileSize;
            }

            set
            {
                _fileSize = value;
            }
        }

        public string FileMD5
        {
            get
            {
                return _fileMD5;
            }

            set
            {
                _fileMD5 = value;
            }
        }

        public List<MediaTempInfo> MediaList
        {
            get
            {
                return _mediaList;
            }

            set
            {
                _mediaList = value;
            }
        }

        public FileContentType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public ScheduleType PlanType
        {
            get
            {
                return _playType;
            }
            set
            {
                _playType = value;
            }
        }

        public UploadScheduleFileInfo(string filePath, long fileSize, string fileMD5, FileContentType type, ScheduleType playType)
        {
            _filePath = filePath;
            _fileSize = fileSize;
            _fileMD5 = fileMD5;
            _type = type;
            _playType = playType;
        }
    }

}
