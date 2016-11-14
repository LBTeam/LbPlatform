using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnUploadFileCompletedEvent: PubSubEvent<OnUploadFileCompletedEventArg>
    {
    }

    public class OnUploadFileCompletedEventArg:EventArgs
    {
        public string FileName { get; set; }
        public OnUploadFileCompletedEventArg(string fileName)
        {
            FileName = fileName;
        }
    }
}
