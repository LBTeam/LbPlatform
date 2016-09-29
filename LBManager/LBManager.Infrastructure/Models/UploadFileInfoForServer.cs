using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LBManager.Infrastructure.Models
{
    public class UploadFileInfoForServer
    {
        private string _name;
        private string _mD5;
        private string _key;
        private FileType _type;
        private List<Part> _parts;
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }
        [JsonProperty("key")]
        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }
        [JsonProperty("parts")]
        public List<Part> Parts
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
        [JsonProperty("md5")]
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
        [JsonProperty("type")]
        public FileType Type
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

        public UploadFileInfoForServer(string name, string key, List<Part> parts)
        {
            this._name = name;
            this._key = key;
            this._parts = parts;
        }
    }
    public class Part
    {
        private int _partNumber;
        private long _seekTo;
        private long _length;
        private string _url;

        [JsonProperty("partNumber")]
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
        [JsonProperty("seekTo")]
        public long SeekTo
        {
            get
            {
                return _seekTo;
            }

            set
            {
                _seekTo = value;
            }
        }
        [JsonProperty("length")]
        public long Length
        {
            get
            {
                return _length;
            }

            set
            {
                _length = value;
            }
        }
        [JsonProperty("url")]
        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
            }
        }

        public Part(int partNumber, long seekTo, long length, string url)
        {
            this._partNumber = partNumber;
            this._seekTo = seekTo;
            this._length = length;
            this._url = url;
        }
    }
}
