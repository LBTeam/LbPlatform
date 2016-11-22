using LBManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBManager.Infrastructure.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using RestSharp;

namespace LBManager
{
    public class ScreenService : IScreenService
    {
        public Task<IList<Screen>> GetScreens()
        {
            var tcs = new TaskCompletionSource<IList<Screen>>();
            var httpClient = new RestClient("http://lbcloud.ddt123.cn/?s=api");
            var httpRequest = new RestRequest();
            httpRequest.Resource = string.Format("Manager/screens&token={0}", App.SessionToken);
            httpClient.ExecuteAsync(httpRequest,response =>{
                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response.  Check inner details for more info.";
                    var twilioException = new ApplicationException(message, response.ErrorException);
                    throw twilioException;
                }
                var screens = JsonConvert.DeserializeObject<List<Screen>>(response.Content);
                tcs.SetResult(screens);
            });
            return tcs.Task;
        }

    }
}
