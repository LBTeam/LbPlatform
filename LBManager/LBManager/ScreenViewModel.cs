using LBManager.Infrastructure.Interfaces;
using LBManager.Infrastructure.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LBManager.Infrastructure.Utils;
using Newtonsoft.Json;

namespace LBManager
{
    public class ScreenViewModel : BindableBase
    {
        public ScreenViewModel(Screen screen, ProgramScheduleListViewModel scheduleList)
        {
            _id = screen.Id;
            _name = screen.Name;
            _width = screen.Width;
            _height = screen.Height;
            _pixelsWidth = screen.PixelsWidth;
            _pixelsHeight = screen.PixelsHeight;
            ScheduleList = scheduleList;
            PublishScheduleCommand = new DelegateCommand(() => { PublishSchedule(); });
            PreviewScheduleCommand = new DelegateCommand(() => { PreviewSchedule(); });
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        private int _pixelsWidth;
        public int PixelsWidth
        {
            get { return _pixelsWidth; }
            set { SetProperty(ref _pixelsWidth, value); }
        }

        private int _pixelsHeight;
        public int PixelsHeight
        {
            get { return _pixelsHeight; }
            set { SetProperty(ref _pixelsHeight, value); }
        }


        private ScheduleFileInfo _selectedScheduleFile = new ScheduleFileInfo();
        public ScheduleFileInfo SelectedScheduleFile
        {
            get { return _selectedScheduleFile; }
            set { SetProperty(ref _selectedScheduleFile, value); }
        }

        private ProgramScheduleListViewModel _scheduleList;
        public ProgramScheduleListViewModel ScheduleList
        {
            get { return _scheduleList; }
            set { SetProperty(ref _scheduleList, value); }
        }


        public DelegateCommand PublishScheduleCommand { get; private set; }

        public DelegateCommand PreviewScheduleCommand { get; private set; }

        private async void PublishSchedule()
        {
            List<UploadFileInfo> uploadFileInfos = new List<UploadFileInfo>();

            var scheduleFilePath = _selectedScheduleFile.FilePath;
            using (FileStream fs = File.OpenRead(scheduleFilePath))
            {

                string content;
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                var scheduleFile = JsonConvert.DeserializeObject<ScheduleFile>(content);
                foreach (var mediaItem in scheduleFile.MediaList)
                {
                    var uploadMediaFileInfo = new UploadFileInfo(mediaItem.FilePath,
                                                                 new FileInfo(mediaItem.FilePath).Length,
                                                                 mediaItem.MD5,
                                                                 mediaItem.Type);
                    uploadFileInfos.Add(uploadMediaFileInfo);
                }
                var uploadScheduleFileInfo = new UploadFileInfo(scheduleFilePath,
                                                                new FileInfo(scheduleFilePath).Length, 
                                                                FileUtils.ComputeFileMd5(scheduleFilePath),
                                                                FileType.Plan);
                uploadScheduleFileInfo.MediaList = new List<MediaTempInfo>(scheduleFile.MediaList.Select(m => new MediaTempInfo(m.FilePath, m.MD5)));
                uploadFileInfos.Add(uploadScheduleFileInfo);
            }

            var uploadPartInfos = await GenerateUploadPartInfos("http://lbcloud.ddt123.cn/?s=api/Manager/upload", uploadFileInfos);
            foreach (var uploadPartInfo in uploadPartInfos)
            {
                if (uploadPartInfo.Type == FileType.Plan)
                {
                    continue;
                }

                var uploadComplete = UploadFile(uploadPartInfo);

                var result = await CompleteMultipartUpload(uploadComplete);
            }

            var schedulePartInfo = uploadPartInfos.FirstOrDefault(p => p.Type == FileType.Plan);
            var scheduleUploadCompleted = UploadFile(schedulePartInfo);
            var scheduleUploadResult = await CompleteMultipartUpload(scheduleUploadCompleted);
        }

        private UploadComplete UploadFile(UploadFileInfoForServer uploadPartInfo)
        {
            UploadComplete uploadComplete = new UploadComplete();
            uploadComplete.FileName = uploadPartInfo.Name;
            uploadComplete.FileMD5 = uploadPartInfo.MD5;
            uploadComplete.FileType = uploadPartInfo.Type;
            if (uploadComplete.FileType == FileType.Plan)
            {
                uploadComplete.Screens = new List<string>() {this.Id};
            }

            using (FileStream fs = File.OpenRead(uploadPartInfo.Name))
            {
                foreach (var part in uploadPartInfo.Parts)
                {
                    long partSize = part.Length;
                    fs.Seek(part.SeekTo, SeekOrigin.Begin);
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(part.Url);
                    request.ContentType = "application/octet-stream";
                    request.Method = "PUT";

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        long writeTotalBytes = 0;
                        byte[] inData = new byte[4096];
                        int bytesRead = fs.Read(inData, 0, inData.Length);

                        while (writeTotalBytes < partSize)
                        {
                            requestStream.Write(inData, 0, bytesRead);
                            writeTotalBytes += bytesRead;
                            bytesRead = fs.Read(inData, 0, inData.Length);
                        }
                    }

                    fs.Seek(part.SeekTo, SeekOrigin.Begin);

                    using (var response = (HttpWebResponse) request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            PartComplete pc = new PartComplete();
                            pc.PartNumber = part.PartNumber;
                            pc.MD5 = FileUtils.ComputeContentMd5(fs, part.Length);
                            if (uploadComplete.Parts == null)
                            {
                                uploadComplete.Parts = new List<PartComplete>();
                            }
                            uploadComplete.Parts.Add(pc);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("分片{0}上传异常", part.PartNumber));
                        }
                    }

                    fs.Seek(0, SeekOrigin.Begin);
                }
            }
            return uploadComplete;
        }

        private void PreviewSchedule()
        {

        }


        private async Task<List<UploadFileInfoForServer>> GenerateUploadPartInfos(string url, List<UploadFileInfo> fileList)
        {
            HttpClient httpClient = new HttpClient();
            string uploadFileInfoJson = JsonConvert.SerializeObject(fileList);

            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(uploadFileInfoJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<List<UploadFileInfoForServer>>(content));
        }

        private async Task<CompleteMultipartUploadResult> CompleteMultipartUpload(UploadComplete uploadComplete)
        {
            HttpClient httpClient = new HttpClient();
            string uploadFileInfoJson = JsonConvert.SerializeObject(uploadComplete);

            HttpResponseMessage response = await httpClient.PostAsync("http://lbcloud.ddt123.cn/?s=api/Manager/complete_upload", new StringContent(uploadFileInfoJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<CompleteMultipartUploadResult>(content));
        }

    }
}
