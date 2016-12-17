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
using LBManager.Infrastructure.Utility;
using Newtonsoft.Json;
using System.Threading;
using LBManager.Modules.ScheduleManage.ViewModels;
using LBManager.Utility;
using MaterialDesignThemes.Wpf;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Common.Event;
using Prism.Events;
using Prism.Logging;
using Microsoft.Practices.ServiceLocation;

namespace LBManager
{
    public class ScreenViewModel : BindableBase
    {
        private ILoggerFacade _logger;

        private SubscriptionToken _uploadFileProgressChangedToken;
        private SubscriptionToken _publishScheduleCompletedToken;
        //private LEDScreen _screen = null;
        public ScreenViewModel(Screen screen, ScheduleSummaryListViewModel scheduleList)
        {
            _logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
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


        private ScheduleSummaryViewModel _selectedScheduleSummaryFile = new ScheduleSummaryViewModel();
        public ScheduleSummaryViewModel SelectedScheduleSummaryFile
        {
            get { return _selectedScheduleSummaryFile; }
            set { SetProperty(ref _selectedScheduleSummaryFile, value); }
        }

        private ScheduleSummaryListViewModel _scheduleList;
        public ScheduleSummaryListViewModel ScheduleList
        {
            get { return _scheduleList; }
            set { SetProperty(ref _scheduleList, value); }
        }


        public DelegateCommand PublishScheduleCommand { get; private set; }

        public DelegateCommand PreviewScheduleCommand { get; private set; }

        private async void PublishSchedule()
        {

            var view = new UploadFileProgressBar();

            _uploadFileProgressChangedToken = Messager.Default.EventAggregator.GetEvent<OnUploadFileProgressChangedEvent>().Subscribe(e =>
             {
                 view.progressTxt.Text = string.Format("{0}({1:f}%)", e.FileName, e.UploadedPercent);
             }, ThreadOption.UIThread);

            _publishScheduleCompletedToken = Messager.Default.EventAggregator.GetEvent<OnPublishScheduleCompletedEvent>().Subscribe(e =>
              {
                  DialogHost.CloseDialogCommand.Execute(false, view);
              }, ThreadOption.UIThread);

            var result = await DialogHost.Show(view, "RootDialog", UploadFileProgressOpenedHandler, UploadFileProgressCloseingHandler);

        }

        private void UploadFileProgressCloseingHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false)
            {
                Messager.Default.EventAggregator.GetEvent<OnUploadFileProgressChangedEvent>().Unsubscribe(_uploadFileProgressChangedToken);
                Messager.Default.EventAggregator.GetEvent<OnPublishScheduleCompletedEvent>().Unsubscribe(_publishScheduleCompletedToken);
                return;
            }
            eventArgs.Cancel();
        }

        private void UploadFileProgressOpenedHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            var action = new Action(() => CompletedUpload());
            action.BeginInvoke(null, null);
        }




        private async void CompletedUpload()
        {
            List<UploadMediaFileInfo> uploadFileInfos = new List<UploadMediaFileInfo>();
            UploadScheduleFileInfo uploadScheduleFileInfo;
            Schedule scheduleFile;
            var scheduleFilePath = _selectedScheduleSummaryFile.FilePath;
            var scheduleFileMD5 = FileUtils.ComputeFileMd5(scheduleFilePath);
            using (FileStream fs = File.OpenRead(scheduleFilePath))
            {

                string content;
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                scheduleFile = JsonConvert.DeserializeObject<Schedule>(content);
                if (scheduleFile == null)
                {
                    _logger.Log("播放方案文件已损坏！", Category.Exception, Priority.Medium);
                    throw new ArgumentNullException("播放方案文件已损坏！");
                }
                var mediaList = scheduleFile.GetAllMedia();
                foreach (var mediaItem in mediaList)
                {
                    var uploadMediaFileInfo = new UploadMediaFileInfo(mediaItem.URL,
                                                                 new FileInfo(mediaItem.URL).Length,
                                                                 mediaItem.MD5,
                                                                 UtilityTool.MediaTypeToContentType(mediaItem.Type));
                    uploadFileInfos.Add(uploadMediaFileInfo);
                }
                uploadScheduleFileInfo = new UploadScheduleFileInfo(scheduleFilePath,
                                                                new FileInfo(scheduleFilePath).Length,
                                                                scheduleFileMD5,
                                                                FileContentType.Schedule,
                                                                scheduleFile.Type);
                uploadScheduleFileInfo.MediaList = new List<MediaTempInfo>(mediaList.Select(m => new MediaTempInfo(m.URL, m.MD5)));
                //uploadFileInfos.Add(uploadScheduleFileInfo);
            }


            var uploadPartInfos = await GenerateUploadPartInfos(string.Format("http://lbcloud.ddt123.cn/?s=api/Manager/upload&token={0}", App.SessionToken), uploadScheduleFileInfo, uploadFileInfos);
            foreach (var uploadPartInfo in uploadPartInfos)
            {
                if (uploadPartInfo.Type == FileContentType.Schedule)
                {
                    continue;
                }

                var uploadComplete = UploadFile(uploadPartInfo, scheduleFile.Type);

                var result = await CompleteMultipartUpload(uploadComplete);


            }

            UploadFileInfoForServer schedulePartInfo;

            if (uploadPartInfos.Count == 0)
            {
                var scheduleUploadCompleted = new UploadComplete(scheduleFilePath, FileContentType.Schedule, scheduleFile.Type, scheduleFileMD5, null, new List<string>() { this.Id });
                var scheduleUploadResult = await CompleteMultipartUpload(scheduleUploadCompleted);
            }
            else
            {
                schedulePartInfo = uploadPartInfos.FirstOrDefault(p => p.Type == FileContentType.Schedule);
                if (schedulePartInfo == null)
                {
                    _logger.Log("发布缺少播放方案！", Category.Exception, Priority.Medium);
                    throw new ArgumentNullException("发布缺少播放方案！");
                }
                var scheduleUploadCompleted = UploadFile(schedulePartInfo, scheduleFile.Type);
                var scheduleUploadResult = await CompleteMultipartUpload(scheduleUploadCompleted);
            }
            Messager.Default.EventAggregator.GetEvent<OnPublishScheduleCompletedEvent>().Publish(new OnPublishScheduleCompletedEventArg());
        }

        private void PreviewSchedule()
        {
            ThreadStart threadDelegate = new ThreadStart(Preview);
            Thread showThread = new Thread(threadDelegate);
            showThread.Start();
        }

        private void Preview()
        {
            //if (_screen != null)
            //{
            //    _screen.Free();
            //    _screen = null;
            //    return;
            //}
            //var scheduleFilePath = _selectedScheduleSummaryFile.FilePath;
            //using (FileStream fs = File.OpenRead(scheduleFilePath))
            //{

            //    string content;
            //    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
            //    {
            //        content = reader.ReadToEnd();
            //    }
            //    var scheduleFile = JsonConvert.DeserializeObject<ScheduleFile>(content);
            //    foreach (var mediaItem in scheduleFile.MediaList)
            //    {
            //        if (mediaItem.Type == MediaType.Image)
            //        {
            //            _screen = new LEDScreen(0, 0, _pixelsWidth, _pixelsHeight);
            //            _screen.PlayImage(mediaItem.FilePath);
            //        }
            //        else if (mediaItem.Type == MediaType.Video)
            //        {
            //            _screen = new LEDScreen(0, 0, _pixelsWidth, _pixelsHeight);
            //            _screen.PlayVideo(mediaItem.FilePath);
            //        }
            //    }
            //}
        }


        private UploadComplete UploadFile(UploadFileInfoForServer uploadPartInfo, ScheduleType planType)
        {
            UploadComplete uploadComplete = new UploadComplete();
            uploadComplete.FileName = uploadPartInfo.Name;
            uploadComplete.FileMD5 = uploadPartInfo.MD5;
            uploadComplete.FileType = uploadPartInfo.Type;
            uploadComplete.PlanType = planType;

            if (uploadComplete.FileType == FileContentType.Schedule)
            {
                uploadComplete.Screens = new List<string>() { this.Id };
            }


            using (FileStream fs = File.OpenRead(uploadPartInfo.Name))
            {
                try
                {
                    double totalFileSize = uploadPartInfo.Parts.Sum(p => (double)p.Length);
                    double uploadedFileSize = 0.0;
                    foreach (var part in uploadPartInfo.Parts)
                    {
                        long partSize = part.Length;
                        fs.Seek(part.SeekTo, SeekOrigin.Begin);
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(part.Url);
                        request.ContentType = "application/octet-stream";
                        request.Method = "PUT";

                        using (Stream requestStream = request.GetRequestStream())
                        {
                            long writeTotalBytes = 0;
                            byte[] inData = new byte[1024];
                            int bytesRead = fs.Read(inData, 0, inData.Length);

                            while (writeTotalBytes < partSize)
                            {
                                requestStream.Write(inData, 0, bytesRead);
                                writeTotalBytes += bytesRead;
                                bytesRead = fs.Read(inData, 0, inData.Length);
                            }
                        }

                        fs.Seek(part.SeekTo, SeekOrigin.Begin);

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                uploadedFileSize += part.Length;
                                Messager.Default.EventAggregator
                                                .GetEvent<OnUploadFileProgressChangedEvent>()
                                                .Publish(new OnUploadFileProgressChangedEventArg(Path.GetFileName(uploadPartInfo.Name), (uploadedFileSize / totalFileSize) * 100.0));
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
                                _logger.Log(string.Format("分片{0}上传异常", part.PartNumber), Category.Exception, Priority.Medium);
                               // MessageBox.Show(string.Format("分片{0}上传异常", part.PartNumber));
                            }
                        }

                        fs.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (WebException webException)
                {
                    _logger.Log(string.Format("---上传文件[{0}]发生网络通讯异常---\r\n{1}", uploadPartInfo.Name, webException.Message), Category.Exception, Priority.Medium);
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("---上传文件[{0}]发生异常---\r\n{1}", uploadPartInfo.Name, ex.Message), Category.Exception, Priority.Medium);
                }
            }
            return uploadComplete;
        }

        private async Task<List<UploadFileInfoForServer>> GenerateUploadPartInfos(string url, UploadScheduleFileInfo scheduleFile, List<UploadMediaFileInfo> mediaFileList)
        {
            HttpClient httpClient = new HttpClient();

            string uploadFileInfoJson = JsonConvert.SerializeObject(new { scheduleFile, mediaFileList });

            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(uploadFileInfoJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<List<UploadFileInfoForServer>>(content));
        }

        private async Task<CompleteMultipartUploadResult> CompleteMultipartUpload(UploadComplete uploadComplete)
        {
            HttpClient httpClient = new HttpClient();
            string uploadFileInfoJson = JsonConvert.SerializeObject(uploadComplete);

            HttpResponseMessage response = await httpClient.PostAsync(string.Format("http://lbcloud.ddt123.cn/?s=api/Manager/complete_upload&token={0}", App.SessionToken), new StringContent(uploadFileInfoJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<CompleteMultipartUploadResult>(content));
        }

    }
}
