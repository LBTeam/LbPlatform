using JumpKick.HttpLib;
using JumpKick.HttpLib.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Com.Net
{
    public delegate void ProgressChanged(long bytesRead, long? totalBytes);
    public delegate void Completed(long totalBytes);
    public class UploadTransmit
    {
        public event ProgressChanged ProgressChanged = delegate { };
        public event Completed Completed = delegate { };
        private string _url;
        private List<UploadFileInfo> _uploadFileList = new List<UploadFileInfo>();
        private const string DefaultBaseChars = "0123456789ABCDEF";
        private HttpClient _httpClient = new HttpClient();

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">访问ECS服务器路径</param>
        /// <param name="UploadFileList">上传文件的列表</param>
        public UploadTransmit(string url,List<UploadFileInfo> UploadFileList)
        {
            _url = url;
            _uploadFileList = UploadFileList;


            UploadFileInfo u = new UploadFileInfo(@"E:\Test\test.playprog", "4", "333");
            u.MediaList = new List<media>();
            u.MediaList.Add(new media(@"E:\Test\1.png", "333"));
            u.MediaList.Add(new media(@"E:\Test\2.png", "333"));

            UploadFileInfo u2 = new UploadFileInfo(@"E:\Test\1.png", "4", "333");
            UploadFileInfo u3 = new UploadFileInfo(@"E:\Test\2.png", "4", "333");
            _uploadFileList.Add(u);
            _uploadFileList.Add(u2);
            _uploadFileList.Add(u3);

        }
        public bool StartUpload()
        {
            List<UploadFileInfoForServer> streamPartList = null;
            if(!GetStreamUrl(out streamPartList))
            {
                return false;
            }
            if(streamPartList==null|| streamPartList.Count==0)
            {
                return false;
            }
            for (int i = 0; i < streamPartList.Count; i++)
            {
                for (int j = 0; j < streamPartList[i].Parts.Count; j++)
                {
                    var fs = File.Open(streamPartList[i].Name, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Seek(long.Parse(streamPartList[i].Parts[j].SeekTo), 0);

                    Http.Put(streamPartList[i].Parts[j].Url).Upload(new[] { new NamedFileStream(streamPartList[i].Key, streamPartList[i].Name, "application/octet-stream", fs) },new { },(bytesSent, totalBytes)=>
                    {
                         //UpdateText(bytesSent.ToString());
                    },
                    (totalBytes) => { }).

                    OnFail((fail) =>
                    {
                        // UpdateText(fail.Message.ToString());
                    }
                    )
                    .OnSuccess((result) =>
                    {
                        
                        //UpdateText("Completed");
                    }).Go();
                }

            }

            return true;

        }

        private bool GetStreamUrl(out List<UploadFileInfoForServer> streamPartList)
        {
            streamPartList = null;
            try
            {
                string UploadFileJson = JsonConvert.SerializeObject(_uploadFileList);
                string replayData, errorInfo;
                //_httpClient.Post("http://192.168.1.107/LbPlatform/LBCloud/?s=api/Manager/demo", UploadFileJson, out d, out cc);
                _httpClient.Post(_url, UploadFileJson, out replayData, out errorInfo);
                if (errorInfo != null && errorInfo != "")
                {
                    Debug.WriteLine("GetStreamUrl Error:" + errorInfo);
                    return false;
                }
                streamPartList = JsonConvert.DeserializeObject<List<UploadFileInfoForServer>>(replayData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetStreamUrl Error:"+ex.Message.ToString());
                return false;
            }
            return true;
        }
        /// <summary>
        /// 对输入流计算md5值
        /// </summary>
        /// <param name="input">待计算的输入流</param>
        /// <param name="partSize">计算多长的输入流</param>
        /// <returns>流的md5值</returns>
        public string ComputeContentMd5(Stream input, long partSize)
        {
            using (var md5 = MD5.Create())
            {
                int readSize = (int)partSize;
                long pos = input.Position;
                byte[] buffer = new byte[readSize];
                readSize = input.Read(buffer, 0, readSize);

                var data = md5.ComputeHash(buffer, 0, readSize);
                var charset = DefaultBaseChars.ToCharArray();
                var sBuilder = new StringBuilder();
                foreach (var b in data)
                {
                    sBuilder.Append(charset[b >> 4]);
                    sBuilder.Append(charset[b & 0x0F]);
                }
                input.Seek(pos, SeekOrigin.Begin);
                return Convert.ToBase64String(data);
            }
        }

        public void Dispose()
        {

        }
    }

    public class UploadFileInfoForServer
    {
        private string _name;
        private string _key;
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

        public UploadFileInfoForServer(string name, string key, List<Part> parts)
        {
            this._name = name;
            this._key = key;
            this._parts = parts;
        }
    }
    public class Part
    {
        private string _partNumber;
        private string _seekTo;
        private string _length;
        private string _url;

        [JsonProperty("partNumber")]
        public string PartNumber
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
        public string SeekTo
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
        public string Length
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

        public Part(string partNumber, string seekTo, string length, string url)
        {
            this._partNumber = partNumber;
            this._seekTo = seekTo;
            this._length = length;
            this._url = url;
        }
    }

    public class UploadFileInfo
    {
        private string _filePath;
        private string _fileSize;
        private string _fileMD5;
        private List<media> _mediaList;

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

        public string FileSize
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

        public List<media> MediaList
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

        public UploadFileInfo(string filePath,string fileSize,string fileMD5)
        {
            _filePath = filePath;
            _fileSize = fileSize;
            _fileMD5 = fileMD5;
        }
    }

    public class media
    {
        private string _mediaName;
        private string _mediaMD5;

        public media(string _mediaName, string _mediaMD5)
        {
            this._mediaName = _mediaName;
            this._mediaMD5 = _mediaMD5;
        }

        public string MediaName
        {
            get
            {
                return _mediaName;
            }

            set
            {
                _mediaName = value;
            }
        }

        public string MediaMD5
        {
            get
            {
                return _mediaMD5;
            }

            set
            {
                _mediaMD5 = value;
            }
        }
    }
}
