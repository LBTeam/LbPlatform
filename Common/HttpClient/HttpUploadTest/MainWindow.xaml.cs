using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HttpUploadTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient = new HttpClient();
        private string _uploadFileName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            
        }


        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            List<UploadFileInfoForServer> uploadPartsInfo = null;
            using (FileStream fileStream = new FileStream(_uploadFileName, FileMode.Open))
            {
                string md5 = ComputeContentMd5(fileStream, fileStream.Length);
                UploadFileInfo fileInfo = new UploadFileInfo(_uploadFileName, fileStream.Length.ToString(), md5, FileType.Image);
                uploadPartsInfo = await GenerateUploadPartInfo("http://lbcloud.ddt123.cn/?s=api/Manager/upload", new List<UploadFileInfo>() { fileInfo});
                foreach (var part in uploadPartsInfo[0].Parts)
                {
                    long partSize = long.Parse(part.Length);
                    fileStream.Seek(long.Parse(part.SeekTo), SeekOrigin.Begin);
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(part.Url);
                    wr.ContentType = "application/octet-stream";
                    wr.Method = "PUT";

                    using (Stream rs = wr.GetRequestStream())
                    {
                        long writeTotalBytes = 0;
                        byte[] inData = new byte[4096];
                        int bytesRead = fileStream.Read(inData, 0, inData.Length);

                        while (writeTotalBytes < partSize)
                        {
                            rs.Write(inData, 0, bytesRead);
                            writeTotalBytes += bytesRead;
                            bytesRead = fileStream.Read(inData, 0, inData.Length);
                        }
                    }
                    fileStream.Seek(0, SeekOrigin.Begin);

                    using (var response = (HttpWebResponse)wr.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {

                        }
                        //using (Stream responseStream = response.GetResponseStream())
                        //{ 
                        //    StreamReader reader = new StreamReader(responseStream);
                        //    Debug.WriteLine(reader.ReadToEnd());
                        //}
                    }
                }
            }


        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Button openFileButton = sender as Button;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                uploadFileTextBox.Text = openFileDialog.FileName;
                _uploadFileName = openFileDialog.FileName;
            }
        }

        //public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        //{
        //    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
        //    wr.ContentType = "application/octet-stream";
        //    wr.Method = "PUT";

        //    Stream rs = wr.GetRequestStream();

        //    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
        //    foreach (string key in nvc.Keys)
        //    {
        //        rs.Write(boundarybytes, 0, boundarybytes.Length);
        //        string formitem = string.Format(formdataTemplate, key, nvc[key]);
        //        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
        //        rs.Write(formitembytes, 0, formitembytes.Length);
        //    }
        //    rs.Write(boundarybytes, 0, boundarybytes.Length);

        //    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
        //    string header = string.Format(headerTemplate, paramName, file, contentType);
        //    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
        //    rs.Write(headerbytes, 0, headerbytes.Length);

        //    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
        //    byte[] buffer = new byte[4096];
        //    int bytesRead = 0;
        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
        //    {
        //        rs.Write(buffer, 0, bytesRead);
        //    }
        //    fileStream.Close();

        //    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
        //    rs.Write(trailer, 0, trailer.Length);
        //    rs.Close();

        //    WebResponse wresp = null;
        //    try
        //    {
        //        wresp = wr.GetResponse();
        //        Stream stream2 = wresp.GetResponseStream();
        //        StreamReader reader2 = new StreamReader(stream2);

        //    }
        //    catch (Exception ex)
        //    {

        //        if (wresp != null)
        //        {
        //            wresp.Close();
        //            wresp = null;
        //        }
        //    }
        //    finally
        //    {
        //        wr = null;
        //    }
        //}


        private async Task<List<UploadFileInfoForServer>> GenerateUploadPartInfo(string url, List<UploadFileInfo> fileList)
        {
            string UploadFileJson = JsonConvert.SerializeObject(fileList);

            HttpResponseMessage response = await _httpClient.PostAsync(url, new StringContent(UploadFileJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<List<UploadFileInfoForServer>>(content));
        }

        public string ComputeContentMd5(Stream input, long partSize)
        {
            using (var md5 = MD5.Create())
            {
                int readSize = (int)partSize;
                long pos = input.Position;
                byte[] buffer = new byte[readSize];
                readSize = input.Read(buffer, 0, readSize);

                var data = md5.ComputeHash(buffer, 0, readSize);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                return sb.ToString();
                // input.Seek(pos, SeekOrigin.Begin);
                //return Convert.ToBase64String(data);
            }
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
        }


        /// <summary>
        /// This method makes a bunch (workItems.Count()) of HttpRequests using Tasks
        /// The work each task performs is a synchronous Http request. Essentially each
        /// Task is performed on a different thread and when all threads have completed
        /// this method returns
        /// </summary>
        /// <param name="url"></param>
        /// <param name="workItems"></param>
        static void CallHttpWebRequestTaskAndWaitOnAll(string url, IEnumerable<Work> workItems)
        {
            var tasks = new List<Task>();
            foreach (var workItem in workItems)
            {
                tasks.Add(Task.Factory.StartNew(wk =>
                {
                    var wrkItem = (Work)wk;
                    wrkItem.ResponseData = GetWebResponse(url, wrkItem.PostParameters);
                }, workItem));
            }
            Task.WaitAll(tasks.ToArray());
        }

        static string GetWebResponse(string url, NameValueCollection parameters)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";

            var sb = new StringBuilder();
            foreach (var key in parameters.AllKeys)
                sb.Append(key + "=" + parameters[key] + "&");
            sb.Length = sb.Length - 1;

            byte[] requestBytes = Encoding.UTF8.GetBytes(sb.ToString());
            httpWebRequest.ContentLength = requestBytes.Length;

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(requestBytes, 0, requestBytes.Length);
            }

            Task<WebResponse> responseTask = Task.Factory.FromAsync<WebResponse>(httpWebRequest.BeginGetResponse, httpWebRequest.EndGetResponse, null);
            using (var responseStream = responseTask.Result.GetResponseStream())
            {
                var reader = new StreamReader(responseStream);
                return reader.ReadToEnd();
            }
        }
    }


    public class Work
    {
        public int Id { get; set; }
        public NameValueCollection PostParameters { get; set; }
        public string ResponseData { get; set; }
        public Exception Exception { get; set; }
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

        public UploadFileInfo(string filePath, string fileSize, string fileMD5, FileType type)
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
    public enum FileType
    {
        Plan,
        Image,
        Video,
        Text
    }
}
