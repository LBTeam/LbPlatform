using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    /// <summary>
    ///{
    ///"err_code": "000000",
    ///"msg": "ok"
    ///}
    /// </summary>
    public class WebCommonResponse
    {
        [JsonProperty("err_code")]
        public string Code { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
