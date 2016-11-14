using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Common.Event
{
    public class OnUploadFileProgressChangedEvent : PubSubEvent<OnUploadFileProgressChangedEventArg>
    {
    }

    public class OnUploadFileProgressChangedEventArg : EventArgs
    {
        public string FileName { get; set; }
        public double UploadedPercent { get; set; }
        public OnUploadFileProgressChangedEventArg(string fileName,double uploadedPercent)
        {
            FileName = fileName;
            UploadedPercent = uploadedPercent;
        }
    }
}
