using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LBPlayerConfig;
using CCWin;
using Com.Utility;
using System.IO;
using Microsoft.Win32;
using System.Management;
using com.lbplayer;
using Newtonsoft.Json;
using System.Threading;
using LBManager.Infrastructure.Models;
using WebSocketSharp;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;
using LbPlayer.Logger;
using Quartz;
using LBPlayer.Job;
using LBManager.Infrastructure.Utility;
using Polly;
using RestSharp;

namespace LBPlayer
{
    public partial class LBPlayerMain : Skin_Color
    {
        #region 字段（锁定）

        private KeyboardHelper _hook = new KeyboardHelper();
        private const int WM_POWERBROADCAST = 0x218; //此消息发送给应用程序来通知它有关电源管理事件
        private const int BROADCAST_QUERY_DENY = 0x424D5144; //广播返回值为阻止事件发生
        private readonly string TASKMGR_NAME = "taskmgr.exe";
        private FileStream _TaskMgrStream;
        private ToolTipWindow _toolTip_Mouse = new ToolTipWindow();
        private Encryption _keyPasswordWindow = new Encryption();

        #endregion

        #region 字段

        private ScreenCapture _screenCapture;





        private Config _config = null;
        private string _privewPic = "Privew.jpg";
        private Poll _poll;
        private List<Cmd> _cmdList;
        private string _cmdSavePath;
        private System.Windows.Forms.Timer _queryTimer;
        private const int QueryTimerInterval = 10000;

        private WebSocket _webSocket;
        private MonitorDataPoll _monitorDataPoll;
        private ComputerStatus _computerStatus;
        private System.Threading.Timer _captureTimer;
        private ScreenCapture _screenCaptrue = new ScreenCapture();
        private int HeartBeatFailCount = 0;
        private ScreenPlayer _screenPlayer;

        #endregion

        #region 构造函数

        public LBPlayerMain()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        #endregion

        #region 心跳

        /// <summary>
        /// 心跳结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _poll_GetPollResponseEvent(object sender, GetPollResponseEventArgs args)
        {
            Log4NetLogger.LogDebug("心跳完成：" + DateTime.Now);
            if (!args.bSuc)
            {
                HeartBeatFailCount++;
                return;
            }
            HeartBeatFailCount = 0;
            HartBeatResponseObj hartBeatResponseObj;
            //throw new NotImplementedException();
            hartBeatResponseObj = JsonConvert.DeserializeObject<HartBeatResponseObj>(args.Replydata);

            HartBeatHandle(hartBeatResponseObj);

        }

        /// <summary>
        /// 处理心跳结果
        /// </summary>
        /// <param name="hartBeatResponseObj"></param>
        private void HartBeatHandle(HartBeatResponseObj hartBeatResponseObj)
        {
            //_cmdList.Clear();
            if (hartBeatResponseObj == null || hartBeatResponseObj.Data == null || hartBeatResponseObj.Data.Count == 0)
            {
                return;
            }
            for (int i = 0; i < hartBeatResponseObj.Data.Count; i++)
            {
                _cmdList.Add(hartBeatResponseObj.Data[i]);
            }
            XmlUtil.XmlSerializeToFile(_cmdList, _cmdSavePath, Encoding.UTF8);
        }

        /// <summary>
        /// 心跳开始事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _poll_SendPollEvent(object sender, PollEventArgs args)
        {
            Log4NetLogger.LogDebug("开始心跳：" + DateTime.Now);
            HartBeatRequestObj obj = new HartBeatRequestObj();
            obj.Id = _config.ID;
            obj.Key = _config.Key;
            obj.Mac = _config.Mac;
            args.Url = ApplicationConfig.PollURL;
            args.PollData = JsonConvert.SerializeObject(obj);
            //SetControlText(skinLabel8, "开始心跳");
        }

        #endregion

        #region 长连接

        private void initialWebSocket(string url)
        {
            _webSocket = new WebSocket(url);
            _webSocket.OnClose += new EventHandler<CloseEventArgs>(_webSocket_OnClose);
            _webSocket.OnError += new EventHandler<WebSocketSharp.ErrorEventArgs>(_webSocket_OnError);
            _webSocket.OnOpen += new EventHandler(_webSocket_OnOpen);
            _webSocket.OnMessage += new EventHandler<MessageEventArgs>(_webSocket_OnMessage);
            _webSocket.Connect();
            Log4NetLogger.LogDebug("初始化长连接");
        }

        private void _webSocket_OnMessage(object sender, MessageEventArgs e)
        {
            WebSocketMsg wsm = JsonConvert.DeserializeObject<WebSocketMsg>(e.Data);
            switch (wsm.Act)
            {
                case Accept.notice:
                    Debug.WriteLine("收到紧急通知，通知内容：" + wsm.Msg);
                    break;
                case Accept.studow:
                    Debug.WriteLine("收到关机通知");
                    WinSysOperate.SHUTDOWN();
                    break;
                default:
                    break;
            }
        }

        private void _webSocket_OnOpen(object sender, EventArgs e)
        {
            WebSocketAccept wsa = new WebSocketAccept("bind", _config.ID, _config.Key, _config.Mac);

            _webSocket.Send(JsonConvert.SerializeObject(wsa));

        }

        private void _webSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {

        }

        private void _webSocket_OnClose(object sender, CloseEventArgs e)
        {

        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        private void LoadConfig()
        {
            _config = new Config();
            _config = ConfigTool.ReadConfigData();
            Log4NetLogger.LogDebug(string.Format("---初始化配置文件---\r\n{0}", JsonConvert.SerializeObject(_config)));
        }

        /// <summary>
        /// 初始化工作目录
        /// </summary>
        private void initialWorkPath()
        {
            if (_config.FileSavePath == null || _config.FileSavePath == "")
            {
                _config.FileSavePath = Path.Combine(Path.GetPathRoot(Application.ExecutablePath), "LBPlay");
                ConfigTool.SaveConfigData(_config);
            }

            string playLogPath = Path.Combine(_config.FileSavePath, "PlayLog");
            ApplicationConfig.SetPlayLogFilePath(playLogPath);
            if (!Directory.Exists(playLogPath))
            {
                Directory.CreateDirectory(playLogPath);
            }

            string scheduleFilePath = Path.Combine(_config.FileSavePath, "Plan");
            ApplicationConfig.SetScheduleFilePath(scheduleFilePath);
            if (!Directory.Exists(scheduleFilePath))
            {
                Directory.CreateDirectory(scheduleFilePath);
            }

            string mediaPath = Path.Combine(_config.FileSavePath, "Media");
            ApplicationConfig.SetMediaFilePath(mediaPath);
            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            string offlineScheduleFilePath = Path.Combine(_config.FileSavePath, "OfflinePlan");
            ApplicationConfig.SetOfflineScheduleFilePath(offlineScheduleFilePath);
            if (!Directory.Exists(offlineScheduleFilePath))
            {
                Directory.CreateDirectory(offlineScheduleFilePath);
            }

            string pictureFilePath = Path.Combine(_config.FileSavePath, "Pic");
            ApplicationConfig.SetPictureFilePath(pictureFilePath);
            if (!Directory.Exists(pictureFilePath))
            {
                Directory.CreateDirectory(pictureFilePath);
            }
            Log4NetLogger.LogDebug("初始化工作目录");
        }

        /// <summary>
        /// 初始化控件值
        /// </summary>
        private void InitialConfigValue()
        {
            //工作目录
            skinTextBox_workPath.Text = _config.FileSavePath;
            //桌面截图
            Rectangle rect = new Rectangle();
            rect = System.Windows.Forms.Screen.GetBounds(this);
            if (_config.ScreenCuptureW == 0 || _config.ScreenCuptureH == 0)
            {
                _config.ScreenCuptureW = rect.Width;
                _config.ScreenCuptureH = rect.Height;
                ConfigTool.SaveConfigData(_config);
            }
            skinNumericUpDown_W.Maximum = rect.Width;
            skinNumericUpDown_H.Maximum = rect.Height;
            skinNumericUpDown_X.Maximum = rect.Width;
            skinNumericUpDown_Y.Maximum = rect.Height;
            skinNumericUpDown_W.Value = _config.ScreenCuptureW;
            skinNumericUpDown_H.Value = _config.ScreenCuptureH;
            skinNumericUpDown_X.Value = _config.ScreenCuptureX;
            skinNumericUpDown_Y.Value = _config.ScreenCuptureY;

            //配置
            skinTextBox_Id.Text = _config.ID;
            skinTextBox_Key.Text = _config.Key;
            List<string> macList = null;
            GetMacAddress(out macList);
            for (int i = 0; i < macList.Count; i++)
            {
                skinComboBox_Mac.Items.Add(macList[i]);
                if (macList[i] == _config.Mac)
                {
                    skinComboBox_Mac.SelectedIndex = i;
                }
            }

            Log4NetLogger.LogDebug("初始化控件值");
        }

        /// <summary>
        /// 初始化心跳
        /// </summary>
        private void initialPoll()
        {
            _poll = new Poll();
            _poll.PollInterval = _config.HeartBeatInterval;
            _poll.SendPollEvent += new SendPollEventHandler(_poll_SendPollEvent);
            _poll.GetPollResponseEvent += new GetPollResponseEventHandler(_poll_GetPollResponseEvent);
            _poll.Initializer();
            _poll.Start();
            Log4NetLogger.LogDebug("初始化心跳");
        }

        /// <summary>
        /// 启动命令轮询
        /// </summary>
        private void StartCmdTimer()
        {
            _queryTimer = new System.Windows.Forms.Timer();
            _queryTimer.Interval = QueryTimerInterval;
            _queryTimer.Tick += new EventHandler(QueryTimer_Tick);
            _queryTimer.Enabled = true;
            Log4NetLogger.LogDebug("启动命令轮询");
        }

        /// <summary>
        /// 轮询命令事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < _cmdList.Count; i++)
            {
                Log4NetLogger.LogDebug(string.Format("开始处理{0}命令...", _cmdList[i].CmdType));
                switch (_cmdList[i].CmdType)
                {
                    case CmdType.DownloadPlan:
                        DownloadPlan(_cmdList[i]);
                        break;
                    case CmdType.Lock:
                        SetComputerLock(_cmdList[i]);
                        break;
                    case CmdType.SetHartBeatPeriod:
                        SetPollInterval(_cmdList[i]);
                        break;
                    case CmdType.UploadMonitor:
                        SetMonitorInterval(_cmdList[i]);
                        break;
                    case CmdType.SoftWareRunTime:
                        SetSoftWareRunTime(_cmdList[i]);
                        break;
                    case CmdType.SetScreenLocation:
                        SetScreenInfo(_cmdList[i]);
                        break;
                    case CmdType.SetScreenWorkTime:
                        SetWorkTime(_cmdList[i]);
                        break;
                    case CmdType.EmergencyPlan:
                        DownloadEmergencyPlan(_cmdList[i]);
                        break;
                    case CmdType.OfflinePlan:
                        DownloadOfflinePlan(_cmdList[i]);
                        break;
                    case CmdType.WebSocketReConnection:
                        SetWebSocketReConnection(_cmdList[i]);
                        break;
                    default:
                        UnKnowCmd(_cmdList[i]);
                        break;
                }
            }
        }

        /// <summary>
        /// 初始化命令列表
        /// </summary>
        private void InitialCmdList()
        {
            _cmdSavePath = Path.Combine(ConfigTool.ConfigDir, "Cmd.xml");
            if (!File.Exists(_cmdSavePath))
            {
                _cmdList = new List<Cmd>();
                return;
            }
            try
            {
                _cmdList = XmlUtil.XmlDeserializeFromFile<List<Cmd>>(_cmdSavePath, Encoding.UTF8);
                Log4NetLogger.LogDebug(string.Format("---从配置中读取命令列表---\r\n{0}", JsonConvert.SerializeObject(_cmdList)));
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("[读取命令列表错误]：{0}", ex.Message));
                File.Delete(_cmdSavePath);
                _cmdList = new List<Cmd>();
                return;
            }
        }

        /// <summary>
        /// 初始化监控图片上传
        /// </summary>
        private void InitialMonitorDataUpload()
        {
            _computerStatus = new ComputerStatus();
            _monitorDataPoll = new MonitorDataPoll();
            _monitorDataPoll.MonitorDatePollInterval = _config.MonitorDateInterval;

            _monitorDataPoll.SendMonitorDatePollEvent +=
                new SendMonitorDatePollEventHandler(_monitorDataPoll_SendMonitorDatePollEvent);
            _monitorDataPoll.GetMonitorDatePollResponseEvent +=
                new GetMonitorDatePollResponseEventHandler(_monitorDataPoll_GetMonitorDatePollResponseEvent);
            _monitorDataPoll.Initializer();
            _monitorDataPoll.Start();
            Log4NetLogger.LogDebug("初始化监控图片上传");
        }

        /// <summary>
        /// 监控图片上传完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _monitorDataPoll_GetMonitorDatePollResponseEvent(object sender,
            GetMonitorDatePollResponseEventArgs args)
        {
            Log4NetLogger.LogDebug("上传监控完成");
            // Debug.WriteLine("上传监控完成：" + DateTime.Now);

        }

        /// <summary>
        /// 监控图片上传开始事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _monitorDataPoll_SendMonitorDatePollEvent(object sender, MonitorDatePollEventArgs args)
        {
            Log4NetLogger.LogDebug("上传监控开始");
            // Debug.WriteLine("上传监控开始：" + DateTime.Now);
            try
            {
                float cupUtilization;
                long freeBytes, totalBytes;
                string fanSpeed;

                _computerStatus.GetCpuUtilization(out cupUtilization);
                _computerStatus.GetDriveSpace(Path.GetPathRoot(_config.FileSavePath), out freeBytes, out totalBytes);

                MEMORY_INFO memoryInfo = new MEMORY_INFO();
                _computerStatus.GetMemoryStatus(ref memoryInfo);
                List<CPUModel> cpuList = _computerStatus.GetCPUTemperature();
                _computerStatus.GetFanSpeed(out fanSpeed);
                string CPUUusage = cupUtilization.ToString();
                string diskUsage = ((float)freeBytes / (float)totalBytes * 100).ToString();
                string memoryUsage = memoryInfo.dwMemoryLoad.ToString();
                string CPUTem = cpuList[0].Temperature;
                MonitorInfo m = new MonitorInfo(CPUUusage, diskUsage, memoryUsage, CPUTem, fanSpeed);
                MonitorResult obj = new MonitorResult(_config.ID, _config.Key, _config.Mac, m);

                args.Url = new Uri(ApplicationConfig.BaseURL, ApplicationConfig.MonitorInfoURL).ToString();
                // ApplicationConfig.BaseURL + "/" + ApplicationConfig.MonitorInfoURL;
                args.PollData = JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("[监控图片上传错误]：{0}", ex.Message));
                return;
            }
        }

        private void initialPlay()
        {
            LEDScreenDisplayer.GetInstance()
                .Initialize(int.Parse(_config.Size_X), int.Parse(_config.Size_Y), int.Parse(_config.Resoul_X),
                    int.Parse(_config.Resoul_Y));
            LEDScreenDisplayer.GetInstance().OnMediaDisplayCompletedEvent += LBPlayerMain_OnMediaDisplayCompletedEvent;
            Log4NetLogger.LogDebug("初始化播放组件...");
            Thread.Sleep(200);
            GenerateLEDSchedule(_config.CurrentPlanPath);
            //Task.Delay(TimeSpan.FromMilliseconds(1000))
            //   .ContinueWith((t, _) => GenerateLEDSchedule(_config.CurrentPlanPath);, null, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void LBPlayerMain_OnMediaDisplayCompletedEvent(string info)
        {
            Action action = new Action((() =>
            {
                var playBackInfo = GetPlayBackInfo(info);
                bool result = UploadPlayBack(playBackInfo);
                if (result)
                {
                    Log4NetLogger.LogDebug($"成功上传播放记录:{info}");
                }
            }));
            action.BeginInvoke(null, null);
        }

        private PlayBack GetPlayBackInfo(string info)
        {
            var obj = JsonConvert.DeserializeObject<Rootobject>(info);
            return new PlayBack(_config.ID, _config.Key, _config.Mac, obj.MediaName, obj.MediaMD5, obj.StartTime,
                obj.EndTime);
        }

        #endregion

        #region 下载紧急插播

        private bool _bDownloadEmergencyPlan = false;

        private void DownloadEmergencyPlan(Cmd cmd)
        {
            if (cmd == null || cmd.CmdType != CmdType.EmergencyPlan || _bDownloadEmergencyPlan == true)
            {
                return;
            }
            _bDownloadEmergencyPlan = true;
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, false.ToString());
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            PlanCmdPar planCmdPar = JsonConvert.DeserializeObject<PlanCmdPar>(cmdContent);
            string scheduleFilePath = Path.Combine(ApplicationConfig.GetScheduleFilePath(),
                Path.GetFileName(planCmdPar.ProgramName));
            if (!DownloadFile(scheduleFilePath, planCmdPar.ProgramUrl, string.Empty))
            {
                UploadCmdResult(cr);
                DeleteCmd(cmd);
                _bDownloadEmergencyPlan = false;
                return;
            }
            for (int i = 0; i < planCmdPar.Medias.Count; i++)
            {
                if (!DownloadMediaFile(Path.Combine(ApplicationConfig.GetMediaFilePath(),
                                  Path.GetFileName(planCmdPar.Medias[i].MediaName)),
                                  planCmdPar.Medias[i].MediaUrl,
                                  string.Empty))
                {
                    UploadCmdResult(cr);
                    DeleteCmd(cmd);
                    _bDownloadEmergencyPlan = false;
                    return;
                }
            }
            cr.CmdRes = true.ToString();

            if (UploadCmdResult(cr))
            {
                DeleteCmd(cmd);
                _config.LastEmergencyPlanPath = scheduleFilePath;
                ConfigTool.SaveConfigData(_config);
                _bDownloading = false;

                GenerateLEDEmergencySchedule(_config.LastEmergencyPlanPath);
                // Start();
            }
            else
            {
                _bDownloading = false;
            }
        }

        private void GenerateLEDEmergencySchedule(string lastEmergencyPlanPath)
        {
            if (string.IsNullOrEmpty(lastEmergencyPlanPath) || !File.Exists(lastEmergencyPlanPath))
            {
                Log4NetLogger.LogDebug(string.Format("---插播排期文件{0}不存在---", lastEmergencyPlanPath));
                return;
            }
           
            var emergencySchedule = ScheduleParser(lastEmergencyPlanPath);
            if (emergencySchedule == null)
            {
                Log4NetLogger.LogDebug(string.Format("获取当前排期{0}失败。", lastEmergencyPlanPath));
                return;
            }


            Action displayAction = new Action(() =>
            {
                DisplayScheduleManager.GetInstance().ApplyEmergencySchedule(emergencySchedule);

            });
            displayAction.BeginInvoke(null, null);
        }

        #endregion

        #region 下载离线策略

        private bool _bDownloadOfflinePlan = false;

        private void DownloadOfflinePlan(Cmd cmd)
        {
            if (cmd == null || cmd.CmdType != CmdType.OfflinePlan || _bDownloadOfflinePlan == true)
            {
                return;
            }
            _bDownloadOfflinePlan = true;
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, false.ToString());
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            PlanCmdPar planCmdPar = JsonConvert.DeserializeObject<PlanCmdPar>(cmdContent);
            if (
                !DownloadFile(
                    Path.Combine(ApplicationConfig.GetOfflineScheduleFilePath(),
                        Path.GetFileName(planCmdPar.ProgramName)), planCmdPar.ProgramUrl))
            {
                UploadCmdResult(cr);
                DeleteCmd(cmd);
                _bDownloadOfflinePlan = false;
                return;
            }
            for (int i = 0; i < planCmdPar.Medias.Count; i++)
            {
                if (
                    !DownloadFile(
                        Path.Combine(ApplicationConfig.GetMediaFilePath(),
                            Path.GetFileName(planCmdPar.Medias[i].MediaName)), planCmdPar.Medias[i].MediaUrl))
                {
                    UploadCmdResult(cr);
                    DeleteCmd(cmd);
                    _bDownloadOfflinePlan = false;
                    return;
                }
            }
            cr.CmdRes = true.ToString();
            UploadCmdResult(cr);
            DeleteCmd(cmd);
            _config.CurrentOfflinePlanPath = Path.Combine(ApplicationConfig.GetOfflineScheduleFilePath(),
                Path.GetFileName(planCmdPar.ProgramName));
            ConfigTool.SaveConfigData(_config);
            _bDownloadOfflinePlan = false;
        }

        #endregion

        #region 私有函数

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <param name="macList">返回MAC地址列表</param>
        /// <returns></returns>
        private bool GetMacAddress(out List<string> macList)
        {

            macList = new List<string>();
            try
            {
                int iOsVersion = Environment.OSVersion.Version.Major; //取得系统的版本
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"); //获取网卡硬件地址  
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    string macOriginal = null;
                    if (iOsVersion >= 6) //大于或者等于6表示是Vista系统或者以上
                    {
                        if (mo["MacAddress"] != null)
                        {
                            macOriginal = mo["MacAddress"].ToString();
                        }
                    }
                    else
                    {
                        if ((bool)mo["IPEnabled"] == true)
                        {
                            macOriginal = mo["MacAddress"].ToString();
                        }
                    }
                    if (macOriginal != null)
                    {
                        string macHandled = macOriginal.Replace(":", "-"); //将mac地址中的":"替换为"-"，ftp服务器上不支持":"
                        if (!macList.Contains(macHandled))
                        {
                            macList.Add(macHandled);
                        }
                    }
                    mo.Dispose();
                }
                moc = null;
                if (macList.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 64位字符串解密
        /// </summary>
        /// <param name="encode"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private string DecodeBase64(Encoding encode, string result)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        private delegate void SetControlTextDelegate(System.Windows.Forms.Control setControl, string text);

        private void SetControlText(System.Windows.Forms.Control setControl, string text)
        {
            if (!this.InvokeRequired)
            {
                setControl.Text = text;
                return;
            }
            SetControlTextDelegate setTextDelegate = new SetControlTextDelegate(SetControlText);
            this.Invoke(setTextDelegate, new object[] { setControl, text });
        }

        private bool DownloadFile(string strFileName, string url, string md5 = "")
        {
            //打开上次下载的文件
            long SPosition = 0;
            //实例化流对象
            // FileStream FStream;

            if (File.Exists(strFileName))
            {
                if (!string.IsNullOrEmpty(md5) && FileUtils.ComputeFileMd5(strFileName) == md5)
                {
                    Log4NetLogger.LogError(string.Format("---{0}文件已存在！！---", strFileName));
                    return true;
                }
                else
                {
                    Log4NetLogger.LogError(string.Format("---{0}文件不存在！！---", strFileName));
                    File.Delete(strFileName);
                }
            }

            var policy = Policy.Handle<Exception>().WaitAndRetry(
                retryCount: 5, // Retry 3 times
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(2000), // Wait 2000ms between each try.
                onRetry: (exception, calculatedWaitDuration) => // Capture some info for logging!
                {
                    Log4NetLogger.LogDebug(string.Format("{0}下载异常，开始下载重试！\r\n{1}", strFileName, exception.Message));
                });

            try
            {
                policy.Execute(() =>
                {
                    using (FileStream fileStream = File.Create(strFileName))
                    {
                        //打开网络连接
                        HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                        if (SPosition > 0)
                            myRequest.AddRange((int)SPosition); //设置Range值
                        //向服务器请求,获得服务器的回应数据流
                        using (Stream myStream = myRequest.GetResponse().GetResponseStream())
                        {
                            byte[] btContent = new byte[1024];
                            int intSize = 0;
                            intSize = myStream.Read(btContent, 0, 1024);
                            while (intSize > 0)
                            {
                                fileStream.Write(btContent, 0, intSize);
                                intSize = myStream.Read(btContent, 0, 1024);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("{0}下载异常!！\r\n{1}", strFileName, ex.Message));
                return false;
            }

            return true;
        }

        private bool DownloadMediaFile(string strFileName, string url, string md5 = "")
        {
            //打开上次下载的文件
            long SPosition = 0;
            //实例化流对象
            // FileStream FStream;

            if (File.Exists(strFileName))
            {
                if (!string.IsNullOrEmpty(md5) && FileUtils.ComputeFileMd5(strFileName) == md5)
                {
                    return true;
                }
                else
                {
                    return true;
                    // File.Delete(strFileName);
                }
            }

            var policy = Policy.Handle<Exception>().WaitAndRetry(
                retryCount: 5, // Retry 3 times
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(2000), // Wait 2000ms between each try.
                onRetry: (exception, calculatedWaitDuration) => // Capture some info for logging!
                {
                    Log4NetLogger.LogDebug(string.Format("{0}下载异常，开始下载重试！\r\n{1}", strFileName, exception.Message));
                });

            try
            {
                policy.Execute(() =>
                {
                    using (FileStream fileStream = File.Create(strFileName))
                    {
                        //打开网络连接
                        HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                        if (SPosition > 0)
                            myRequest.AddRange((int)SPosition); //设置Range值
                        //向服务器请求,获得服务器的回应数据流
                        using (Stream myStream = myRequest.GetResponse().GetResponseStream())
                        {
                            byte[] btContent = new byte[512];
                            int intSize = 0;
                            intSize = myStream.Read(btContent, 0, 512);
                            while (intSize > 0)
                            {
                                fileStream.Write(btContent, 0, intSize);
                                intSize = myStream.Read(btContent, 0, 512);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("{0}下载异常!！\r\n{1}", strFileName, ex.Message));
                return false;
            }

            return true;
        }

        private bool DownloadFileAsync(string strFileName, string url, string md5 = "")
        {
            //打开上次下载的文件
            long SPosition = 0;
            //实例化流对象
            // FileStream FStream;

            if (File.Exists(strFileName))
            {
                if (!string.IsNullOrEmpty(md5) && FileUtils.ComputeFileMd5(strFileName) == md5)
                {
                    return true;
                }
                else
                {
                    File.Delete(strFileName);
                }
            }

            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(
                retryCount: 5, // Retry 3 times
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(2000), // Wait 2000ms between each try.
                onRetry: (exception, calculatedWaitDuration) => // Capture some info for logging!
                {
                    Log4NetLogger.LogDebug(string.Format("{0}下载异常，开始下载重试！\r\n{1}", strFileName, exception.Message));
                });

            try
            {
                policy.Execute(() =>
                {
                    using (FileStream fileStream = File.Create(strFileName))
                    {
                        //打开网络连接
                        HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                        if (SPosition > 0)
                            myRequest.AddRange((int)SPosition); //设置Range值
                        //向服务器请求,获得服务器的回应数据流

                        using (Stream myStream = myRequest.GetResponse().GetResponseStream())
                        {
                            byte[] btContent = new byte[512];
                            int intSize = 0;
                            intSize = myStream.Read(btContent, 0, 512);
                            while (intSize > 0)
                            {
                                fileStream.Write(btContent, 0, intSize);
                                intSize = myStream.Read(btContent, 0, 512);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("{0}下载异常!！\r\n{1}", strFileName, ex.Message));
                return false;
            }

            return true;
        }

        private Schedule ScheduleParser(string path)
        {
            Schedule schedule = null;
            if (!File.Exists(path))
            {
                return schedule;
            }

            try
            {
                string scheduleContent = File.ReadAllText(path, Encoding.UTF8);
                if (scheduleContent == null || scheduleContent == "")
                {
                    return schedule;
                }
                schedule = JsonConvert.DeserializeObject<Schedule>(scheduleContent);
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("解析排期时出现异常：\r\n{0}", ex.Message));
                return schedule;
            }
            return schedule;
        }

        #endregion

        #region 锁定

        /// <summary>
        /// 解锁事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnLockEvent(object sender, EventArgs e)
        {
            //解除锁定时,关闭任务管理器流对象
            if (_TaskMgrStream != null)
            {
                _TaskMgrStream.Close();
            }
            if (_toolTip_Mouse != null)
            {
                _toolTip_Mouse.Hide();
            }
            _hook.KeyboardHookStop();
            _hook.MouseHookStop();
            //RegistryKey keyLocalMachine = Registry.LocalMachine;
            //RegistryKey key;
            //key = keyLocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\USBSTOR", true);
            //key.SetValue("Start", 3);

            //RegistryKey currentUser = Registry.CurrentUser;
            //RegistryKey system = currentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
            ////如果system项不存在就创建这个项
            //if (system == null)
            //{
            //    system = currentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            //}
            //system.SetValue("DisableTaskmgr",0, RegistryValueKind.DWord);
            //currentUser.Close();

        }

        /// <summary>
        /// 锁定事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLockEvent(object sender, EventArgs e)
        {
            if (_TaskMgrStream != null)
            {
                _TaskMgrStream.Close();
            }
            //taskmgr.exe（任务管理器）
            _TaskMgrStream = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\"
                                       + TASKMGR_NAME,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);

            _hook.KeyboardHookStart();
            _hook.MouseHookStart();
            RegistryKey keyLocalMachine = Registry.LocalMachine;
            RegistryKey key;
            //key = keyLocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\USBSTOR", true);
            //key.SetValue("Start", 4);

            //RegistryKey currentUser = Registry.CurrentUser;
            //RegistryKey system = currentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
            ////如果system项不存在就创建这个项
            //if (system == null)
            //{
            //    system = currentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            //}
            //system.SetValue("DisableTaskmgr", 1, RegistryValueKind.DWord);
            //currentUser.Close();

        }

        private void Hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (!InvokeRequired)
            {

                if (_keyPasswordWindow.Visible)
                {
                    //当前处于解锁密码输入状态
                    _keyPasswordWindow.Hook_KeyUp(e);

                }
                else if (e.KeyCode == Keys.Escape)
                {
                    _keyPasswordWindow.Password = _config.LockPwd;
                    _keyPasswordWindow.StartPosition = FormStartPosition.Manual;
                    int x;
                    int y;
                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;

                        x = (rect.Width - _keyPasswordWindow.Width) / 2;
                        y = (rect.Height - _keyPasswordWindow.Height) / 2;
                    }
                    else
                    {
                        x = this.Location.X + this.Width / 2 - _keyPasswordWindow.Width / 2;
                        y = this.Location.Y + this.Height / 2 - _keyPasswordWindow.Height / 2;
                    }

                    _keyPasswordWindow.Location = new Point(x, y);
                    _keyPasswordWindow.ShowForm();
                    _toolTip_Mouse.Hide();
                }
            }
            else
            {
                KeyEventHandler cs = new KeyEventHandler(Hook_KeyUp);
                Invoke(cs, new object[] { sender, e });
            }

        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            //在键盘锁定状态下屏蔽键盘上与电源相关的操作
            if (m.Msg == WM_POWERBROADCAST)
            {
                m.Result = (IntPtr)BROADCAST_QUERY_DENY;
                return;
            }
            base.WndProc(ref m);
        }

        private void Hook_KeyDown(object sender, KeyEventArgs e)
        {
            if (!InvokeRequired)
            {
                if (_keyPasswordWindow.Visible)
                {
                    _toolTip_Mouse.Hide();
                    //当前处于解锁密码输入状态
                    _keyPasswordWindow.Hook_KeyDown(e);

                }

                else
                {
                    string strText = "你的鼠标和键盘已被锁定按ESC解锁！";
                    _toolTip_Mouse.Hide();
                    _toolTip_Mouse.Show(strText, MousePosition.X + 15, MousePosition.Y, 3000);
                }
            }
            else
            {
                KeyEventHandler cs = new KeyEventHandler(Hook_KeyDown);
                Invoke(cs, new object[] { sender, e });
            }
        }

        private void Hook_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!InvokeRequired)
            {
                if (_keyPasswordWindow.Visible)
                {
                    _toolTip_Mouse.Hide();
                    //当前处于解锁密码输入状态
                    _keyPasswordWindow.Hook_KeyPress(e);

                }

                else
                {

                    string strText = "你的鼠标和键盘已被锁定按ESC解锁！";
                    _toolTip_Mouse.Hide();
                    _toolTip_Mouse.Show(strText, MousePosition.X + 15, MousePosition.Y, 3000);
                }

            }
            else
            {
                KeyPressEventHandler cs = new KeyPressEventHandler(Hook_KeyPress);
                Invoke(cs, new object[] { sender, e });
            }
        }

        private void Hook_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.InvokeRequired)
            {
                return;
            }
            if (!_keyPasswordWindow.Visible)
            {
                string strText = "你的鼠标和键盘已被锁定按ESC解锁！";
                _toolTip_Mouse.Hide();
                _toolTip_Mouse.Show(strText, e.X + 15, e.Y, 3000);
            }
            else
            {
                _toolTip_Mouse.Hide();
            }

        }

        /// <summary>
        /// 初始化锁定
        /// </summary>
        private void InitialLock()
        {
            #region 锁定相关

            if (_hook == null)
            {
                _hook = new KeyboardHelper();
            }
            _keyPasswordWindow.UnLockEvent += new EventHandler(OnUnLockEvent);
            _hook.KeyUp += new KeyEventHandler(Hook_KeyUp); //本地解锁
            _hook.KeyDown += new KeyEventHandler(Hook_KeyDown);
            _hook.KeyPress += new KeyPressEventHandler(Hook_KeyPress);
            _hook.MouseDown += new MouseEventHandler(Hook_MouseDown);
            if (_config.LockUnLockPlayer)
            {
                if (_TaskMgrStream != null)
                {
                    _TaskMgrStream.Close();
                }
                //taskmgr.exe（任务管理器）
                _TaskMgrStream = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\"
                                           + TASKMGR_NAME,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);

                _hook.KeyboardHookStart();
                _hook.MouseHookStart();
                // RegistryKey keyLocalMachine = Registry.LocalMachine;
                //RegistryKey key;
                //key = keyLocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\USBSTOR", true);
                //key.SetValue("Start", 4);

                //RegistryKey currentUser = Registry.CurrentUser;
                //RegistryKey system = currentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                ////如果system项不存在就创建这个项
                //if (system == null)
                //{
                //    system = currentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                //}
                //system.SetValue("DisableTaskmgr", 1, RegistryValueKind.DWord);
                //currentUser.Close();
            }

            #endregion
        }

        #endregion

        #region 窗体事件

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LBPlayerMain_Load(object sender, EventArgs e)
        {
            LoadConfig();
            initialWorkPath();
            _screenCapture = new ScreenCapture();
            DisplayScheduleManager.GetInstance().StartScheduler();
            InitialConfigValue();
            InitialCmdList();
            StartCmdTimer();
            initialPoll();

            initialWebSocket(ApplicationConfig.WebSocketURL);
            InitialLock();
            InitialMonitorDataUpload();
            initialMonitorPic();
            initialPlay();
            this.Location = new Point(int.Parse(_config.Resoul_X), 0);
        }

        /// <summary>
        /// 桌面截屏预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton3_Click(object sender, EventArgs e)
        {
            if (this.skinPictureBox_pic.Image != null)
            {
                this.skinPictureBox_pic.Image.Dispose();
            }
            skinPictureBox_pic.Width = int.Parse(skinNumericUpDown_W.Value.ToString());
            skinPictureBox_pic.Height = int.Parse(skinNumericUpDown_H.Value.ToString());
            _screenCapture.CaptrueScreenRegionToFile((int)skinNumericUpDown_X.Value, (int)skinNumericUpDown_Y.Value,
                (int)skinNumericUpDown_W.Value, (int)skinNumericUpDown_H.Value,
                Path.Combine(ApplicationConfig.GetPictureFilePath(), _privewPic));
            skinPictureBox_pic.Image = Image.FromFile(Path.Combine(ApplicationConfig.GetPictureFilePath(), _privewPic));
        }

        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton_lock_Click(object sender, EventArgs e)
        {
            InitialLock();
        }

        /// <summary>
        /// 配置页面保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton_Ok_Click(object sender, EventArgs e)
        {
            if (skinTextBox_Id.Text.Trim() == "")
            {
                MessageBoxEx.Show("请填写ID", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (skinTextBox_Key.Text.Trim() == "")
            {
                MessageBoxEx.Show("请填写Key", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (skinComboBox_Mac.SelectedItem.ToString() == "")
            {
                MessageBoxEx.Show("请选择MAC", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Bind bind = new Bind(skinTextBox_Id.Text.Trim(), skinTextBox_Key.Text.Trim(),
                skinComboBox_Mac.SelectedItem.ToString());
            var httpClient = new RestClient(ApplicationConfig.BaseURL);
            var request = new RestRequest(ApplicationConfig.BindingURL, Method.POST);
            request.AddJsonBody(bind);
            var response = httpClient.Execute(request);
            if (response.ErrorException != null)
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sr = JsonConvert.DeserializeObject<BindResult>(response.Content);
            if (sr.Err_code != SystemCode.OK)
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _config.ID = skinTextBox_Id.Text.Trim();
            _config.Key = skinTextBox_Key.Text.Trim();
            _config.Mac = skinComboBox_Mac.SelectedItem.ToString();
            SetScreenInfo(sr.ScreenInfo); //modify by lixc
            if (ConfigTool.SaveConfigData(_config))
            {
                MessageBoxEx.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void skinButton_b_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            skinTextBox_workPath.Text = fbd.SelectedPath;
        }

        private void skinButton_SaveConf_Click(object sender, EventArgs e)
        {

            try
            {
                _config.FileSavePath = skinTextBox_workPath.Text;
                ConfigTool.SaveConfigData(_config);
                MessageBoxEx.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        #endregion

        #region 下载方案

        private bool _bDownloading = false;

        private void DownloadPlan(Cmd cmd)
        {
            if (cmd == null || cmd.CmdType != CmdType.DownloadPlan || _bDownloading == true)
            {
                return;
            }
            _bDownloading = true;
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, false.ToString());
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);

            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));

            PlanCmdPar planCmdPar = JsonConvert.DeserializeObject<PlanCmdPar>(cmdContent);
            string scheduleFilePath = Path.Combine(ApplicationConfig.GetScheduleFilePath(),
                Path.GetFileName(planCmdPar.ProgramName));
            if (!DownloadFile(scheduleFilePath, planCmdPar.ProgramUrl))
            {
                UploadCmdResult(cr);
                DeleteCmd(cmd);
                _bDownloading = false;
                return;
            }
            for (int i = 0; i < planCmdPar.Medias.Count; i++)
            {
                if (
                    !DownloadMediaFile(
                        Path.Combine(ApplicationConfig.GetMediaFilePath(),
                            Path.GetFileName(planCmdPar.Medias[i].MediaName)), planCmdPar.Medias[i].MediaUrl))
                {
                    UploadCmdResult(cr);
                    DeleteCmd(cmd);
                    _bDownloading = false;
                    return;
                }
            }
            cr.CmdRes = true.ToString();
            if (UploadCmdResult(cr))
            {
                DeleteCmd(cmd);
                _config.CurrentPlanPath = scheduleFilePath;
                ConfigTool.SaveConfigData(_config);
                _bDownloading = false;

                GenerateLEDSchedule(_config.CurrentPlanPath);
                // Start();
            }
            else
            {
                _bDownloading = false;
            }
        }

        private void GenerateLEDSchedule(string scheduleFilePath)
        {
            if (!File.Exists(scheduleFilePath))
            {
                Log4NetLogger.LogDebug(string.Format("---排期文件{0}不存在---", scheduleFilePath));
            }
            var currentSchedule = ScheduleParser(scheduleFilePath);
            if (currentSchedule == null)
            {
                Log4NetLogger.LogDebug(string.Format("获取当前排期{0}失败。", currentSchedule));
                return;
            }

            Action displayAction = new Action(() =>
            {
                switch (currentSchedule.Type)
                {
                    case ScheduleType.Common:
                        DisplayScheduleManager.GetInstance().ApplyMainSchedule(currentSchedule);
                        break;
                    case ScheduleType.Emergency:
                        DisplayScheduleManager.GetInstance().ApplyMainSchedule(currentSchedule);
                        break;
                    default:
                        break;
                }

            });
            displayAction.BeginInvoke(null, null);


        }

        private void Start()
        {
            Action playAction = new Action(StartPlay);
            playAction.BeginInvoke(null, null);
        }

        private void StartPlay()
        {
            //if (_config.CurrentPlanPath == null || _config.CurrentPlanPath == "")
            //{
            //    return;
            //}
            //Schedule sch;
            //if (!ScheduleParser(_config.CurrentPlanPath, out sch))
            //{
            //    return;
            //}
            //List<PlayInfoWrapper> playInfoList = new List<PlayInfoWrapper>();
            //List<LBManager.Infrastructure.Models.Media> mediaList = sch.GetAllMedia();
            //if (mediaList == null || mediaList.Count <= 0)
            //{
            //    return;
            //}
            //for (int i = 0; i < mediaList.Count; i++)
            //{
            //    PlayInfoWrapper playInfo = new PlayInfoWrapper(Path.Combine(_mediaPath, Path.GetFileNameWithoutExtension(mediaList[i].URL) + "_" + mediaList[i].MD5 + Path.GetExtension(mediaList[i].URL)),
            //                                                   mediaList[i].LoopCount,
            //                                                   int.Parse(_config.Size_X),
            //                                                   int.Parse(_config.Size_Y),
            //                                                   int.Parse(_config.Resoul_X),
            //                                                   int.Parse(_config.Resoul_Y));
            //    playInfoList.Add(playInfo);
            //}
            //try
            //{
            //    Log4NetLogger.LogDebug(string.Format("***开始播放***"));
            //    _screenPlayer.Play(playInfoList);
            //}
            //catch (Exception ex)
            //{
            //    Log4NetLogger.LogError(string.Format("[播放时出现错误]:{0}", ex.Message));
            //    return;
            //}
            //return;
        }

        #endregion

        #region 未知命令处理

        private void UnKnowCmd(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令(Unknow)内容---\r\n{1}", cmd.CmdType, cmdContent));
            return;
        }

        #endregion

        #region 上传播放记录

        private bool UploadPlayBack(PlayBack playBack)
        {
            //string data = JsonConvert.SerializeObject(playBack);
            var httpClient = new RestClient(ApplicationConfig.BaseURL);
            var request = new RestRequest(ApplicationConfig.PlayBackURL, Method.POST);
            request.AddJsonBody(playBack);
            var response = httpClient.Execute(request);
            if (response.ErrorException != null)
            {
                Log4NetLogger.LogError(string.Format("上传播放记录出现错误:\r\n{1}", response.ErrorMessage));
                return false;
            }

            try
            {
                var sr = JsonConvert.DeserializeObject<SystemResult>(response.Content);
                if (sr.Err_code != SystemCode.OK)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("上传播放记录出现错误:\r\n{1}", response.Content));
                return false;
            }

        }
        #endregion
        #region 回复命令结果
        private bool UploadCmdResult(CmdResult cmdRes)
        {
            //string data = JsonConvert.SerializeObject(cmdRes);

            var httpClient = new RestClient(ApplicationConfig.BaseURL);
            var request = new RestRequest(ApplicationConfig.CmdBackURL, Method.POST);
            request.AddJsonBody(cmdRes);
            var response = httpClient.Execute(request);
            if (response.ErrorException != null)
            {
                Log4NetLogger.LogError(string.Format("回复命令[id:{0}]结果出现错误:\r\n{1}", cmdRes.CmdId, response.ErrorMessage));
                return false;
            }

            var sr = JsonConvert.DeserializeObject<SystemResult>(response.Content);
            if (sr.Err_code != SystemCode.OK)
            {
                Log4NetLogger.LogError(string.Format("回复命令[id:{0}]结果解析出现异常，回复数据为：\r\n{1}", cmdRes.CmdId, response.Content));
                return false;
            }
            return true;
        }
        #endregion
        #region 上传监控数据
        private bool UploadMonitorInfo(MonitorResult monitor)
        {
            // string data = JsonConvert.SerializeObject(monitor);
            var httpClient = new RestClient(ApplicationConfig.BaseURL);
            var request = new RestRequest(ApplicationConfig.MonitorInfoURL, Method.POST);
            request.AddJsonBody(monitor);
            var response = httpClient.Execute(request);
            if (response.ErrorException != null)
            {
                Log4NetLogger.LogError(string.Format("向服务端上传监控信息时，出现错误！\r\n{0}", response.ErrorMessage));
                return false;
            }

            try
            {
                var sr = JsonConvert.DeserializeObject<SystemResult>(response.Content);
                if (sr.Err_code != SystemCode.OK)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(ex.Message);
                return false;
            }

            return true;
        }
        #endregion
        #region 删除命令
        private bool DeleteCmd(Cmd cmd)
        {
            try
            {
                _cmdList.Remove(cmd);
                XmlUtil.XmlSerializeToFile(_cmdList, _cmdSavePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log4NetLogger.LogError(string.Format("删除命令时发生异常:\r\n{0}", ex.Message));
                return false;
            }
            return true;
        }
        #endregion
        #region 重连WebSocket命令
        private void SetWebSocketReConnection(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            ReConnectionWebSocket rcws = JsonConvert.DeserializeObject<ReConnectionWebSocket>(cmdContent);
            initialWebSocket(rcws.Host);
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 屏幕参数设置
        private void SetScreenInfo(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogInfo(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            ScreenSet screenSet = JsonConvert.DeserializeObject<ScreenSet>(cmdContent);
            _config.Size_X = screenSet.Size_x;
            _config.Size_Y = screenSet.Size_y;
            _config.Resoul_X = screenSet.Resolu_x;
            _config.Resoul_Y = screenSet.Resolu_y;
            ConfigTool.SaveConfigData(_config);

            LEDScreenDisplayer.GetInstance().UpdateScreenWindow(int.Parse(_config.Size_X), int.Parse(_config.Size_Y), int.Parse(_config.Resoul_X), int.Parse(_config.Resoul_Y));
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }

        private void SetScreenInfo(ScreenSet screenInfo)
        {
            //ScreenSet screenSet = JsonConvert.DeserializeObject<ScreenSet>(DecodeBase64(Encoding.UTF8, cmd.CmdParam));
            _config.Size_X = screenInfo.Size_x;
            _config.Size_Y = screenInfo.Size_y;
            _config.Resoul_X = screenInfo.Resolu_x;
            _config.Resoul_Y = screenInfo.Resolu_y;
            // ConfigTool.SaveConfigData(_config);
        }
        #endregion
        #region 设置工作时间
        private void SetWorkTime(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            WorkTime workTime = JsonConvert.DeserializeObject<WorkTime>(cmdContent);
            _config.StartWorkTime = workTime.Start;
            _config.EndWorkTime = workTime.End;
            ConfigTool.SaveConfigData(_config);
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 设置软件开关时间
        private void SetSoftWareRunTime(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            RunTime runTime = JsonConvert.DeserializeObject<RunTime>(cmdContent);
            _config.IsEnableAutoOpenOrClose = runTime.Switch;
            _config.OpenTime = runTime.Enable;
            _config.CloseTime = runTime.Disable;
            ConfigTool.SaveConfigData(_config);
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 设置电脑锁定
        private void SetComputerLock(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);

            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));

            LockSystem lockSystem = JsonConvert.DeserializeObject<LockSystem>(cmdContent);
            _config.LockUnLockPlayer = lockSystem.Enable;
            _config.LockPwd = lockSystem.Password;
            ConfigTool.SaveConfigData(_config);
            if (lockSystem.Enable)
            {
                InitialLock();
            }
            else
            {
                if (_TaskMgrStream != null)
                {
                    _TaskMgrStream.Close();
                }
                if (_toolTip_Mouse != null)
                {
                    _toolTip_Mouse.Hide();
                }
                _hook.KeyboardHookStop();
                _hook.MouseHookStop();
            }
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 设置交互周期
        private void SetPollInterval(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            PollInterval pollInterval = JsonConvert.DeserializeObject<PollInterval>(cmdContent);
            _config.HeartBeatInterval = pollInterval.Cycle * 1000;
            ConfigTool.SaveConfigData(_config);
            _poll.PollInterval = _config.HeartBeatInterval;
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 设置监控数据上传周期
        private void SetMonitorInterval(Cmd cmd)
        {
            string cmdContent = DecodeBase64((Encoding.UTF8), cmd.CmdParam);
            Log4NetLogger.LogDebug(string.Format("---{0}命令内容---\r\n{1}", cmd.CmdType, cmdContent));
            MonitorPollInterval pollInterval = JsonConvert.DeserializeObject<MonitorPollInterval>(cmdContent);
            _config.MonitorDateInterval = pollInterval.Cycle * 1000;
            ConfigTool.SaveConfigData(_config);
            _monitorDataPoll.MonitorDatePollInterval = _config.MonitorDateInterval;
            _captureTimer.Change(0, _config.MonitorDateInterval);
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }

        #endregion
        #region 监控图片上传
        bool bUploding = false;
        private void initialMonitorPic()
        {
            _captureTimer = new System.Threading.Timer(CaptureTick, null, Timeout.Infinite, Timeout.Infinite);
            _captureTimer.Change(0, _config.MonitorDateInterval);
        }
        private void CaptureTick(object state)
        {
            try
            {
                DateTime nowTime = DateTime.Now;
                _screenCaptrue.CaptrueScreenRegionToFile(_config.ScreenCuptureX, _config.ScreenCuptureY, _config.ScreenCuptureW, _config.ScreenCuptureH, Path.Combine(ApplicationConfig.GetPictureFilePath(), nowTime.Ticks + ".jpg"), 30);
                UploadImage();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void UploadImage()
        {
            if (bUploding)
            {
                return;
            }
            bUploding = true;
            List<string> imageFiles;
            GetImageList(out imageFiles);
            if (imageFiles == null || imageFiles.Count == 0)
            {
                bUploding = false;
                return;
            }
            HartBeatRequestObj obj = new HartBeatRequestObj();
            obj.Id = _config.ID;
            obj.Key = _config.Key;
            obj.Mac = _config.Mac;
            var httpClient = new RestClient(ApplicationConfig.BaseURL);
            for (int i = 0; i < imageFiles.Count; i++)
            {
                var request = new RestRequest(ApplicationConfig.GetPicUploadURL, Method.POST);
                request.AddJsonBody(obj);
                var response = httpClient.Execute(request);
                if (response.ErrorException != null)
                {
                    Log4NetLogger.LogError(string.Format("向服务端请求监控图片上传信息时，出现错误！\r\n{0}", response.ErrorMessage));
                    return;
                }

                try
                {
                    UploadPicInfo picInfo = JsonConvert.DeserializeObject<UploadPicInfo>(response.Content);

                    if (UploadFile(picInfo.Url, imageFiles[i]))
                    {
                        File.Delete(imageFiles[i]);
                    }
                }
                catch (Exception ex)
                {
                    bUploding = false;
                }

            }
            bUploding = false;
        }
        private bool UploadFile(string url, string FilePath)
        {
            using (FileStream fs = File.OpenRead(FilePath))
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/octet-stream";
                request.Method = "PUT";

                using (Stream requestStream = request.GetRequestStream())
                {
                    long writeTotalBytes = 0;
                    byte[] inData = new byte[1024];
                    int bytesRead = fs.Read(inData, 0, inData.Length);

                    while (writeTotalBytes < fs.Length)
                    {
                        requestStream.Write(inData, 0, bytesRead);
                        writeTotalBytes += bytesRead;
                        bytesRead = fs.Read(inData, 0, inData.Length);
                    }
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        private object _upLoadingListLockObj = new object();
        private void GetImageList(out List<string> imageFiles)
        {
            imageFiles = new List<string>();
            DirectoryInfo imageDestDirInfo = new DirectoryInfo(ApplicationConfig.GetPictureFilePath());
            FileInfo[] images = imageDestDirInfo.GetFiles();
            if (images.Length == 0)
            {
                return;
            }
            for (int i = 0; i < images.Length; i++)
            {
                lock (_upLoadingListLockObj)
                {
                    imageFiles.Add(images[i].FullName);
                }
            }
        }
        #endregion

        private void LBPlayerMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }

    public class Rootobject
    {
        public string MediaName { get; set; }
        public string MediaMD5 { get; set; }
        [JsonConverter(typeof(MyDateTimeConverter))]
        public DateTime StartTime { get; set; }
        [JsonConverter(typeof(MyDateTimeConverter))]
        public DateTime EndTime { get; set; }
    }

    public class MyDateTimeConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var t = (long)reader.Value;
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(t);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

