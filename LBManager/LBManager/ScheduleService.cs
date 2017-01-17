using LBManager.Infrastructure.Interfaces;
using LBManager.Infrastructure.Models;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Polly;
using Prism.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ScheduleService : IScheduleService
    {
        private static string mediaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media");
        private ILoggerFacade _logger;
        public ScheduleService()
        {
            _logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
        }

        public Task<bool> BackupSchedules(string backupRequest)
        {
            var tcs = new TaskCompletionSource<bool>();
            var httpClient = new RestClient("http://lbcloud.ddt123.cn/?s=api");
            var httpRequest = new RestRequest(Method.POST);
            httpRequest.AddJsonBody(backupRequest);
            httpRequest.Resource = string.Format("Manager/backup&token={0}", App.SessionToken);
            httpClient.ExecuteAsync(httpRequest, response =>
            {
                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response.  Check inner details for more info.";
                    var twilioException = new ApplicationException(message, response.ErrorException);
                    tcs.SetResult(false);
                    //throw twilioException;
                }
                var webResponse = JsonConvert.DeserializeObject<WebCommonResponse>(response.Content);
                if (webResponse.Code == "000000")
                {
                    tcs.SetResult(true);
                }
                else
                {
                    _logger.Log(string.Format("备份播放方案发生错误：错误代码为【{0}】", webResponse.Message), Category.Exception, Priority.Medium);
                    tcs.SetResult(false);
                }

            });
            return tcs.Task;
        }

        public Task<bool> GetBackedUpSchedules()
        {
            var tcs = new TaskCompletionSource<bool>();
            var httpClient = new RestClient("http://lbcloud.ddt123.cn/?s=api");
            var httpRequest = new RestRequest();
            httpRequest.Resource = string.Format("Manager/plans&token={0}", App.SessionToken);
            httpClient.ExecuteAsync(httpRequest, response =>
            {
                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response.  Check inner details for more info.";
                    var twilioException = new ApplicationException(message, response.ErrorException);
                    tcs.SetResult(false);
                    //throw twilioException;
                }
                var fetchScheduleResponse = JsonConvert.DeserializeObject<List<FetchBackedUpScheduleResponse>>(response.Content);
                bool result = false;
                foreach (var item in fetchScheduleResponse)
                {
                    if (FetchSchedule(item.ProgramName, item.ProgramUrl) == false)
                    {
                        result = false;
                        break;
                    }
                    else
                    {
                        result = true;
                    }
                }
                tcs.SetResult(result);
            });
            return tcs.Task;
        }

        private bool FetchSchedule(string scheduleName, string url)
        {
            var policy = Policy.Handle<Exception>().WaitAndRetry(
               retryCount: 5, // Retry 3 times
               sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(2000), // Wait 2000ms between each try.
               onRetry: (exception, calculatedWaitDuration) => // Capture some info for logging!
                {
                    _logger.Log(string.Format("{0}下载异常，开始下载重试！\r\n{1}", url, exception.Message), Category.Debug, Priority.Medium);
                });

            try
            {
                policy.Execute(() =>
                {
                    string filePath = Path.Combine(mediaDirectory, scheduleName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    using (FileStream fileStream = File.Create(Path.Combine(mediaDirectory, scheduleName)))
                    {
                        //打开网络连接
                        HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                        //if (SPosition > 0)
                        //    myRequest.AddRange((int)SPosition);             //设置Range值
                        //                                                    //向服务器请求,获得服务器的回应数据流
                        using (Stream myStream = myRequest.GetResponse().GetResponseStream())
                        {
                            byte[] btContent = new byte[1024];
                            int intSize = 0;
                            intSize = myStream.Read(btContent, 0, 1024);
                            while (intSize > 0)
                            {
                                fileStream.Write(btContent, 0, intSize);
                                intSize = myStream.Read(btContent, 0, 1024);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}下载异常!！\r\n{1}", url, ex.Message), Category.Exception, Priority.Medium);
                return false;
            }
            return true;
        }
    }
}
