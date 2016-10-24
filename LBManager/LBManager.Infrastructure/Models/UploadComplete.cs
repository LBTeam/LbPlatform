using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class UploadComplete
    {
        private string _fileName;
        private MediaType _fileType;
        private ScheduleType _playType;
        private string _fileMD5;
        private List<PartComplete> _parts;
        private List<string> _screens;

        public UploadComplete()
        {

        }

        public UploadComplete(string _fileName, MediaType _fileType, ScheduleType planType, string _fileMD5, List<PartComplete> _parts, List<string> _screens)
        {
            this._fileName = _fileName;
            this._fileType = _fileType;
            this._fileMD5 = _fileMD5;
            this._parts = _parts;
            this._screens = _screens;
            this._playType = planType;
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;
            }
        }

        public MediaType FileType
        {
            get
            {
                return _fileType;
            }

            set
            {
                _fileType = value;
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

        public List<PartComplete> Parts
        {
            get
            {
                return _parts;
            }

            set
            {
                _parts = value;
            }
        }

        public List<string> Screens
        {
            get
            {
                return _screens;
            }

            set
            {
                _screens = value;
            }
        }
    }

    public class PartComplete
    {
        private int _partNumber;
        private string _mD5;

        public int PartNumber
        {
            get
            {
                return _partNumber;
            }

            set
            {
                _partNumber = value;
            }
        }

        public string MD5
        {
            get
            {
                return _mD5;
            }

            set
            {
                _mD5 = value;
            }
        }
    }
}
