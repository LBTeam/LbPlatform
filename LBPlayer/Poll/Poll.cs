using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace com.lbplayer
{
    #region 委托
    public delegate void SendPollEventHandler(Object sender, PollEventArgs args);
    public class PollEventArgs
    {
        private String _url = "";

        public String Url
        {
            get { return _url; }
            set { _url = value; }
        }
        private String _PollData = "";
        public String PollData
        {
            get
            {
                return _PollData;
            }
            set
            {
                _PollData = value;
            }
        }

        public PollEventArgs(String url, String PollData)
        {
            _PollData = PollData;
            _url = url;
        }
    }
    public delegate void GetPollResponseEventHandler(Object sender, GetPollResponseEventArgs args);
    public class GetPollResponseEventArgs : PollEventArgs
    {
        private String _errMsg = "";
        public String ErrMsg
        {
            get
            {
                return _errMsg;
            }

        }
        private bool _bSuc = false;
        public bool bSuc
        {
            get
            {
                return _bSuc;
            }
        }
        private String _replydata = "";

        public String Replydata
        {
            get
            {
                return _replydata;
            }
        }


        public GetPollResponseEventArgs(bool bSuc, String url, String PollResponseData, String replydata, String errMsg)
            : base(url, PollResponseData)
        {
            _bSuc = bSuc;
            _errMsg = errMsg;
            _replydata = replydata;
        }

    }
    #endregion
    public class Poll: IDisposable
    {
        #region 字段
        private const int MaxFailTime = 3;
        private int _failTimes = 0;
        private Timer _heartTimer = null;
        private RestClient _httpClient;
        private bool _isStart = false;
        #endregion
        #region 属性
        /// <summary>
        /// 心跳间隔 单位（毫秒）
        /// </summary>
        private int _PollInterval = 1000;

        public int PollInterval
        {
            get { return _PollInterval; }
            set
            {
                _PollInterval = value;
                PollIntervalChange();
            }
        }


        #endregion
        #region 事件
        public event SendPollEventHandler SendPollEvent;
        private void OnSendPoll(Object sender, PollEventArgs args)
        {
            if (SendPollEvent != null)
            {
                try
                {
                    SendPollEvent(sender, args);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnSendPollEvent Error：" + ex.Message);
                }
            }
        }
        public event GetPollResponseEventHandler GetPollResponseEvent;
        private void OnGetPollResponse(Object sender, GetPollResponseEventArgs args)
        {
            if (GetPollResponseEvent != null)
            {
                try
                {
                    GetPollResponseEvent(sender, args);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("OnGetPollResponseEvent Error：" + ex.Message);
                }
            }
        }
        #endregion
        #region 构造函数
        public Poll()
        {
            _httpClient = new RestClient("http://lbcloud.ddt123.cn");
        }
        #endregion
        #region public函数
        public void Initializer()
        {
            _heartTimer = new Timer(OnPoll, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _heartTimer.Dispose();
        }

        public void Start()
        {
            _isStart = true;
            _heartTimer.Change(0, PollInterval);
        }

        public void Stop()
        {
            _isStart = false;
            _heartTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        //心跳周期变化
        #endregion
        #region private函数
        private void PollIntervalChange()
        {
            if (_isStart)
            {
                _heartTimer.Change(0, PollInterval);
            }
        }
        #endregion
        #region 心跳交互
        private int _isUpNowTime = 1;
        private String _replydata = "";
        private PollEventArgs _args = new PollEventArgs("", "");
        private GetPollResponseEventArgs _replyArgs = null;
        private String _errInfo = "";
        private HttpStatusCode _isPostOk = HttpStatusCode.OK;
        private void OnPoll(object state)
        {
            if (Interlocked.Exchange(ref _isUpNowTime, 0) == 1)
            {
                try
                {
                    OnSendPoll(this, _args);
                    if (_args.Url != "")
                    {
                        var request = new RestRequest(_args.Url, Method.POST);
                        request.AddParameter("application/json", _args.PollData, ParameterType.RequestBody);
                        IRestResponse response = _httpClient.Execute(request);
                        if (response.ErrorException != null)
                        {
                            _errInfo = response.ErrorException.Message;
                        }
                        if (string.IsNullOrEmpty(response.ErrorMessage))
                        {
                            _errInfo = response.ErrorMessage;
                        }
                        _replydata = response.Content;
                        _isPostOk = response.StatusCode;
                       // _isPostOk = _httpClient.Post(_args.Url, _args.PollData, out _replydata, out _errInfo);
                    }
                    else
                    {
                        _isPostOk = HttpStatusCode.BadRequest;
                    }
                    _replyArgs = new GetPollResponseEventArgs(_isPostOk == HttpStatusCode.OK, _args.Url, _args.PollData, _replydata, _errInfo);
                    OnGetPollResponse(this, _replyArgs);
                    if (_isPostOk != HttpStatusCode.OK)
                    {
                        _failTimes++;
                    }
                    else
                    {
                        _failTimes = 0;
                    }
                    if (_failTimes == MaxFailTime)
                    {
                        //_httpClient = new HttpClient();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Poll Error"+ex.ToString());
                }
                finally
                {
                    Interlocked.Exchange(ref _isUpNowTime, 1);
                }
            }
        }
        #endregion
    }
}
