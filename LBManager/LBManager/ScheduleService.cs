using LBManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBManager.Infrastructure.Models;

namespace LBManager
{
    public class ScheduleService : IScheduleService
    {
        public Task<List<ProgramSchdule>> GetSchedules()
        {
            var tcs = new TaskCompletionSource<IList<Screen>>();
            DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileInfo in folder.GetFiles("*.playprog"))
            {
                .Add(new ScheduleFileInfo(fileInfo));
            }
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
