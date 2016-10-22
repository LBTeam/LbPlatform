using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    /// <summary>
    /// 
    /// <Request>
    ///{
    ///"err_code": "000000",
    ///"msg": "ok",
    ///"data": {
    ///"token": "142e81ca955d2e457dae6ad819df772cf554cb5c",
    ///"expire": 7200
    ///}
    ///} 
    /// </Request>
    ///
    /// <Response>
    /// token不存在
    ///{
    ///    "err_code": "010002",
    ///    "msg": "Token not found"
    ///}
    ///
    ///token为空
    ///{
    ///    "err_code": "010004",
    ///    "msg": "Token empty"
    ///}
    ///
    ///token未过期
    ///{
    ///    "err_code": "010005",
    ///    "msg": "Token not expired"
    ///}
    ///
    ///token刷新失败
    ///{
    ///    "err_code": "010106",
    ///    "msg": "Token refresh failed"
    ///}
    /// </Response>
    /// </summary>
    public class RefreshTokenResponse
    {
        [JsonProperty("err_code")]
        public string Code { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public TokenData TokenInfo { get; set; }
    }
}
