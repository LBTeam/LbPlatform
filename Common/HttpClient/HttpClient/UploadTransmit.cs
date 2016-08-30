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
    public delegate void Completed(object obj);
    public class UploadTransmit
    {
        public event ProgressChanged ProgressChanged = delegate { };
        public event Completed Completed = delegate { };
        private string _url;
        private List<UploadFileInfo> _uploadFileList = new List<UploadFileInfo>();
        private const string DefaultBaseChars = "0123456789ABCDEF";
        private HttpClient _httpClient = new HttpClient();
        private List<int> _screenList;

        public List<UploadFileInfo> UploadFileList
        {
            get
            {
                return _uploadFileList;
            }
       
            set
            {
                _uploadFileList = value;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">访问ECS服务器路径</param>
        /// <param name="UploadFileList">上传文件的列表</param>
        public UploadTransmit(string url,List<UploadFileInfo> UploadFileList,List<int> screenList)
        {
            _url = url;
            _uploadFileList = UploadFileList;
            _screenList = screenList;

        }
        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        private int GetUploadCount(List<UploadFileInfoForServer> streamPartList)
        {
            int count = 0;
            for (int i = 0; i < streamPartList.Count; i++)
            {
                count += streamPartList[i].Parts.Count;
            }
            return count;
        }
        private int Count;
        public bool StartUpload()
        {
            int successCount = 0;
            int failCount = 0;
            List<UploadFileInfoForServer> streamPartList = null;
            if(!GetStreamUrl(out streamPartList))
            {
                return false;
            }
            if(streamPartList==null|| streamPartList.Count==0)
            {
                return false;
            }
            Count=GetUploadCount(streamPartList);
            List<UploadComplete> _listUploadComplete = new List<UploadComplete>();
            for (int i = 0; i < streamPartList.Count; i++)
            {
                UploadComplete uploadComplete = new UploadComplete();
                uploadComplete.FileName = streamPartList[i].Name;
                uploadComplete.FileMD5= streamPartList[i].MD5;
                uploadComplete.FileType = streamPartList[i].Type;
                if(streamPartList[i].Type== FileType.Plan)
                {
                    uploadComplete.Screens = _screenList;
                }
                for (int j = 0; j < streamPartList[i].Parts.Count; j++)
                {
                    var fs = File.Open(streamPartList[i].Name, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Seek(long.Parse(streamPartList[i].Parts[j].SeekTo), 0);
                    PartComplete pc = new PartComplete();
                    pc.PartNumber = int.Parse(streamPartList[i].Parts[j].PartNumber);
                    pc.MD5= ComputeContentMd5(fs, long.Parse(streamPartList[i].Parts[j].Length));
                    if(uploadComplete.Parts==null)
                    {
                        uploadComplete.Parts = new List<PartComplete>();
                    }
                    uploadComplete.Parts.Add(pc);
                    Http.Put(streamPartList[i].Parts[j].Url)
                        .Upload(new[] { new NamedFileStream(streamPartList[i].Key, streamPartList[i].Name, "application/octet-stream", fs) },
                                new { },
                                (bytesSent, totalBytes)=>
                                {
                                    //UpdateText(bytesSent.ToString());
                                },
                                (totalBytes) => { }).OnFail((fail) =>
                                                            {
                                                                failCount++;
                                                                 // UpdateText(fail.Message.ToString());
                                                            })
                                                     .OnSuccess((result) =>
                                                               {
                                                                   //UpdateText("Completed");
                                                                   successCount++;
                                                                   if(Count==successCount)
                                                                   {
                                                                       UploadComplete(_listUploadComplete);
                                                                      
                                                                   }
                                                               })
                                                     .Go();
                }
                _listUploadComplete.Add(uploadComplete);

            }

            return true;

        }

        private void UploadComplete(List<UploadComplete> _listUploadComplete)
        {
            HttpClient _httpClient = new HttpClient();
            for (int i = 0; i < _listUploadComplete.Count; i++)
            {

                string UploadFileJson = JsonConvert.SerializeObject(_listUploadComplete[i]);
                string replayData, errorInfo;
                _httpClient.Post("http://lbcloud.ddt123.cn/?s=api/Manager/complete_upload", UploadFileJson, out replayData, out errorInfo);
                if (errorInfo != null && errorInfo != "")
                {
                    Debug.WriteLine("GetStreamUrl Error:" + errorInfo);
                }
            }
            if (Completed != null)
            {
                Completed(0);
            }

        }

        private bool GetStreamUrl(out List<UploadFileInfoForServer> streamPartList)
        {
            streamPartList = null;
            try
            {
                string UploadFileJson = JsonConvert.SerializeObject(_uploadFileList);
                string replayData, errorInfo;
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
        private FileType _type;
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

        public UploadFileInfo(string filePath,string fileSize,string fileMD5,FileType type)
        {
            _filePath = filePath;
            _fileSize = fileSize;
            _fileMD5 = fileMD5;
            _type = type;
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

    public class UploadComplete
    {
        private string _fileName;
        private FileType _fileType;
        private string _fileMD5;
        private List<PartComplete> _parts;
        private List<int> _screens;

        public UploadComplete()
        {

        }

        public UploadComplete(string _fileName, FileType _fileType, string _fileMD5, List<PartComplete> _parts, List<int> _screens)
        {
            this._fileName = _fileName;
            this._fileType = _fileType;
            this._fileMD5 = _fileMD5;
            this._parts = _parts;
            this._screens = _screens;
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

        public FileType FileType
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

        public List<int> Screens
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

namespace Com.Net
{
    public enum FileType
    {
        Plan,
        Image,
        Video,
        Text
    }
}