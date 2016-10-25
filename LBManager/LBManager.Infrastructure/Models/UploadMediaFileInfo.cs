using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class UploadMediaFileInfo
    {
        private string _filePath;
        private long _fileSize;
        private string _fileMD5;
        private MediaType _type;

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

              public MediaType Type
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


        public UploadMediaFileInfo(string filePath, long fileSize, string fileMD5, MediaType type)
        {
            _filePath = filePath;
            _fileSize = fileSize;
            _fileMD5 = fileMD5;
            _type = type;
        }
    }
}
