using Com.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.lbplayer
{
    #region 委托
    public delegate void SendMonitorDatePollEventHandler(Object sender, MonitorDatePollEventArgs args);
    public class MonitorDatePollEventArgs
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

        public MonitorDatePollEventArgs(String url, String PollData)
        {
            _PollData = PollData;
            _url = url;
        }
    }
    public delegate void GetMonitorDatePollResponseEventHandler(Object sender, GetMonitorDatePollResponseEventArgs args);
    public class GetMonitorDatePollResponseEventArgs : MonitorDatePollEventArgs
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


        public GetMonitorDatePollResponseEventArgs(bool bSuc, String url, String PollResponseData, String replydata, String errMsg)
            : base(url, PollResponseData)
        {
            _bSuc = bSuc;
            _errMsg = errMsg;
            _replydata = replydata;
        }

    }
    #endregion
    public class MonitorDataPoll: IDisposable
    {
        #region 字段
        private const int MaxFailTime = 3;
        private int _failTimes = 0;
        private Timer _heartTimer = null;
        private HttpClient _httpClient;
        private bool _isStart = false;
        #endregion
        #region 属性
        /// <summary>
        /// 监控数据上传间隔 单位（毫秒）
        /// </summary>
        private int _MonitorDatePollInterval = 60000;

        public int MonitorDatePollInterval
        {
            get { return _MonitorDatePollInterval; }
            set
            {
                _MonitorDatePollInterval = value;
                MonitorDatePollIntervalChange();
            }
        }


        #endregion
        #region 事件
        public event SendMonitorDatePollEventHandler SendMonitorDatePollEvent;
        private void OnSendMonitorDatePoll(Object sender, MonitorDatePollEventArgs args)
        {
            if (SendMonitorDatePollEvent != null)
            {
                try
                {
                    SendMonitorDatePollEvent(sender, args);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnSendMonitorDatePoll Error：" + ex.Message);
                }
            }
        }
        public event GetMonitorDatePollResponseEventHandler GetMonitorDatePollResponseEvent;
        private void OnGetMonitorDatePollResponse(Object sender, GetMonitorDatePollResponseEventArgs args)
        {
            if (GetMonitorDatePollResponseEvent != null)
            {
                try
                {
                    GetMonitorDatePollResponseEvent(sender, args);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("OnGetMonitorDatePollResponse Error：" + ex.Message);
                }
            }
        }
        #endregion
        #region 构造函数
        public MonitorDataPoll()
        {
            _httpClient = new HttpClient();
        }
        #endregion
        #region public函数
        public void Initializer()
        {
            _heartTimer = new Timer(OnMonitorDatePoll, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _heartTimer.Dispose();
        }

        public void Start()
        {
            _isStart = true;
            _heartTimer.Change(0, MonitorDatePollInterval);
        }

        public void Stop()
        {
            _isStart = false;
            _heartTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        //心跳周期变化
        #endregion
        #region private函数
        private void MonitorDatePollIntervalChange()
        {
            if (_isStart)
            {
                _heartTimer.Change(0, MonitorDatePollInterval);
            }
        }
        #endregion
        #region 心跳交互
        private int _isUpNowTime = 1;
        private String _replydata = "";
        private MonitorDatePollEventArgs _args = new MonitorDatePollEventArgs("", "");
        private GetMonitorDatePollResponseEventArgs _replyArgs = null;
        private String _errInfo = "";
        private bool _isPostOk = false;
        private void OnMonitorDatePoll(object state)
        {
            if (Interlocked.Exchange(ref _isUpNowTime, 0) == 1)
            {
                try
                {
                    OnSendMonitorDatePoll(this, _args);
                    if (_args.Url != "")
                    {
                        _isPostOk = _httpClient.Post(_args.Url, _args.PollData, out _replydata, out _errInfo);
                    }
                    else
                    {
                        _isPostOk = false;
                    }
                    _replyArgs = new GetMonitorDatePollResponseEventArgs(_isPostOk, _args.Url, _args.PollData, _replydata, _errInfo);
                    OnGetMonitorDatePollResponse(this, _replyArgs);
                    if (!_isPostOk)
                    {
                        _failTimes++;
                    }
                    else
                    {
                        _failTimes = 0;
                    }
                    if (_failTimes == MaxFailTime)
                    {
                        _httpClient = new HttpClient();
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
