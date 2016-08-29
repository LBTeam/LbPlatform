using LBManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBManager.Infrastructure.Models;
using JumpKick.HttpLib;
using System.Diagnostics;
using Newtonsoft.Json;

namespace LBManager
{
    public class ScreenService : IScreenService
    {
        public Task<IList<Screen>> GetScreens()
        {
            var tcs = new TaskCompletionSource<IList<Screen>>();
            Http.Get("http://lbcloud.ddt123.cn/?s=api/Manager/screens")
                .OnSuccess((result) =>
                {
                    Debug.WriteLine(result);
                    var screens = JsonConvert.DeserializeObject<List<Screen>>(result);
                    tcs.SetResult(screens);
                })
                .OnFail((fail) =>
                {
                    Debug.WriteLine(fail);
                })
                .Go();

            return tcs.Task;
        }

    }
}
