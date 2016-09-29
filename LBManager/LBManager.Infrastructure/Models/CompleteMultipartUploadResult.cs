using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LBManager.Infrastructure.Models
{
    public class CompleteMultipartUploadResult
    {
        [JsonProperty("err_code")]
        public string Code { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
