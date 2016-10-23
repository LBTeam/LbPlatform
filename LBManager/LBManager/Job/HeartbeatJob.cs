using Common.Logging;
using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Models;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Job
{
    [PersistJobDataAfterExecution]
    public class HeartbeatJob : IJob
    {
        private ILog log = LogManager.GetLogger(typeof(HeartbeatJob));
        public const string HeartbeatState = "HeartbeatState";

        public async void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.JobDetail.JobDataMap;
            HeartbeatStatus status = (HeartbeatStatus)data.Get(HeartbeatState);
            if (status == HeartbeatStatus.OK || status == HeartbeatStatus.STOP)
            {
                JobKey jobKey = context.JobDetail.Key;
                log.InfoFormat("任务: {0} 执行时间：{1}", jobKey, DateTime.Now.ToString());
                Console.WriteLine(string.Format("任务: {0} 执行时间：{1}", jobKey, DateTime.Now.ToString()));
                var response = await Heartbeat();

                if (response.Code == "000000")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.OK));
                   data[HeartbeatState] = HeartbeatStatus.OK;
                }
                else if (response.Code == "010002")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.TokenInvalid));
                    data[HeartbeatState] = HeartbeatStatus.TokenInvalid;
                }
                else if (response.Code == "010003")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.TokenExpired));
                   data[HeartbeatState] = HeartbeatStatus.TokenExpired;
                }
            }
        }

        private async Task<HeartbeatResponse> Heartbeat()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(string.Format("http://lbcloud.ddt123.cn/?s=api/Manager/heartbeat&token={0}", App.SessionToken));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<HeartbeatResponse>(content));
        }
    }
}
