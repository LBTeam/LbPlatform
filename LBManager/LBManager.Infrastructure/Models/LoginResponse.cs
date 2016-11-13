using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    /// <summary>
    //{
    //"user": "nova_liangjian@126.com",
    //"pwd": "123123"
    //}
    /// </summary>
    public class LoginRequest
    {
        [JsonProperty("user")]
        public string UserName { get; set; }
        [JsonProperty("pwd")]
        public string Password { get; set; }
    }

    /// <summary>
    //{
    //"err_code": "000000",
    //"msg": "ok",
    //"data": {
    //    "token": "142e81ca955d2e457dae6ad819df772cf554cb5c",
    //    "expire": 7200
    //}
    //}
    /// </summary>
    public class LoginResponse
    {
        [JsonProperty("err_code")]
        public string Code { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public TokenData TokenInfo { get; set; }
    }

    public class TokenData
    {
        [JsonProperty("token")]
        public string Value { get; set; }
        [JsonProperty("expire")]
        public int Expire { get; set; }
    }
}


