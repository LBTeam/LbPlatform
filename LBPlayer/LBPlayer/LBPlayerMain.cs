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
using Com.Net;
using System.Threading;
using LBManager.Infrastructure.Models;
using WebSocketSharp;
using System.Diagnostics;
using JumpKick.HttpLib;
using System.Net;

namespace LBPlayer
{
    public partial class LBPlayerMain : Skin_Color
    {
        #region 字段（锁定）
        private KeyboardHelper _hook = new KeyboardHelper();
        private const int WM_POWERBROADCAST = 0x218;         //此消息发送给应用程序来通知它有关电源管理事件
        private const int BROADCAST_QUERY_DENY = 0x424D5144; //广播返回值为阻止事件发生
        private readonly string TASKMGR_NAME = "taskmgr.exe";
        private FileStream _TaskMgrStream;
        private ToolTipWindow _toolTip_Mouse = new ToolTipWindow();
        private Encryption _keyPasswordWindow = new Encryption();

        #endregion
        #region 字段
        
        private ScreenCapture _screenCapture;
        private string _playLogPath = "";
        private string _lbPlanPath = "";
        private string _mediaPath = "";
        private string _offlinePlanPath = "";
        private string _picPath = "";
        private Config _config =null;
        private string _privewPic = "Privew.jpg";
        private Poll _poll;
        private List<Cmd> _cmdList;
        private string _cmdSavePath;
        private System.Windows.Forms.Timer _queryTimer;   
        private const int QueryTimerInterval = 10000;
        private string BindingURL = "http://lbcloud.ddt123.cn/?s=api/Player/bind_player";
        private string PlayBackURL = "http://lbcloud.ddt123.cn/?s=api/Player/record";
        private string PollURL = "http://lbcloud.ddt123.cn/?s=api/Player/heartbeat";
        private string CmdBackURL = "http://lbcloud.ddt123.cn/?s=api/Player/cmd_result";
        private string MonitorInfoURL = "http://lbcloud.ddt123.cn/?s=api/Player/monitor";
        private string GetPicUploadURL = "http://lbcloud.ddt123.cn/?s=api/Player/picture";
        private string WebSocketURL = "ws://123.56.240.172:9501";
        private WebSocket _webSocket;
        private MonitorDataPoll _monitorDataPoll;
        private ComputerStatus _computerStatus;
        private System.Threading.Timer _captureTimer;
        private ScreenCapture _screenCaptrue = new ScreenCapture();
        #endregion
        #region 构造函数
        public LBPlayerMain()
        {
            InitializeComponent();
           
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
            Debug.WriteLine("心跳完成："+DateTime.Now);
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
            Debug.WriteLine("开始心跳："+DateTime.Now);
            HartBeatRequestObj obj = new HartBeatRequestObj();
            obj.Id = _config.ID;
            obj.Key = _config.Key;
            obj.Mac = _config.Mac;
            args.Url = PollURL;
            args.PollData = JsonConvert.SerializeObject(obj);
            //SetControlText(skinLabel8, "开始心跳");
        }
        #endregion
        #region 长连接
        private void initialWebSocket(string url)
        {
            _webSocket = new WebSocket(url);
            _webSocket.OnClose +=new EventHandler<CloseEventArgs>(_webSocket_OnClose);
            _webSocket.OnError += new EventHandler<WebSocketSharp.ErrorEventArgs>(_webSocket_OnError);
            _webSocket.OnOpen +=new EventHandler(_webSocket_OnOpen);
            _webSocket.OnMessage += new EventHandler<MessageEventArgs>(_webSocket_OnMessage);
            _webSocket.Connect();
        }

        private void _webSocket_OnMessage(object sender, MessageEventArgs e)
        {
            WebSocketMsg wsm = JsonConvert.DeserializeObject<WebSocketMsg>(e.Data);
            switch (wsm.Act)
            {
                case Accept.notice:
                    Debug.WriteLine("收到紧急通知，通知内容："+wsm.Msg);
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
            _playLogPath = Path.Combine(_config.FileSavePath, "PlayLog");
            if (!Directory.Exists(_playLogPath))
            {
                Directory.CreateDirectory(_playLogPath);
            }
            _lbPlanPath = Path.Combine(_config.FileSavePath, "Plan");
            if (!Directory.Exists(_lbPlanPath))
            {
                Directory.CreateDirectory(_lbPlanPath);
            }
            _mediaPath = Path.Combine(_config.FileSavePath, "Media");
            if (!Directory.Exists(_mediaPath))
            {
                Directory.CreateDirectory(_mediaPath);
            }
            _offlinePlanPath = Path.Combine(_config.FileSavePath, "OfflinePlan");
            if (!Directory.Exists(_offlinePlanPath))
            {
                Directory.CreateDirectory(_offlinePlanPath);
            }
            _picPath = Path.Combine(_config.FileSavePath, "Pic");
            if (!Directory.Exists(_picPath))
            {
                Directory.CreateDirectory(_picPath);
            }
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
                if(macList[i]== _config.Mac)
                {
                    skinComboBox_Mac.SelectedIndex = i;
                }
            }
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
                        break;
                    case CmdType.OfflinePlan:
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
            }
            catch (Exception ex)
            {
                File.Delete(_cmdSavePath);
                _cmdList = new List<Cmd>();
                return;
            }
        }

        private void InitialMonitorDataUpload()
        {
            _computerStatus = new ComputerStatus();
            _monitorDataPoll = new MonitorDataPoll();
            _monitorDataPoll.MonitorDatePollInterval = _config.MonitorDateInterval;

            _monitorDataPoll.SendMonitorDatePollEvent += new SendMonitorDatePollEventHandler(_monitorDataPoll_SendMonitorDatePollEvent);
            _monitorDataPoll.GetMonitorDatePollResponseEvent += new GetMonitorDatePollResponseEventHandler(_monitorDataPoll_GetMonitorDatePollResponseEvent);
            _monitorDataPoll.Initializer();
            _monitorDataPoll.Start();
            
        }

        private void _monitorDataPoll_GetMonitorDatePollResponseEvent(object sender, GetMonitorDatePollResponseEventArgs args)
        {
            Debug.WriteLine("上传监控完成：" + DateTime.Now);
           
        }

        private void _monitorDataPoll_SendMonitorDatePollEvent(object sender, MonitorDatePollEventArgs args)
        {
            Debug.WriteLine("上传监控开始：" + DateTime.Now);
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

                args.Url = MonitorInfoURL;
                args.PollData = JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                return;
            }
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
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");//获取网卡硬件地址  
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
            InitialConfigValue();
            InitialCmdList();
            StartCmdTimer();
            initialPoll();
            initialWebSocket(WebSocketURL);
            InitialLock();
            InitialMonitorDataUpload();
            initialMonitorPic();
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
            _screenCapture.CaptrueScreenRegionToFile((int)skinNumericUpDown_X.Value, (int)skinNumericUpDown_Y.Value, (int)skinNumericUpDown_W.Value, (int)skinNumericUpDown_H.Value, Path.Combine(_picPath, _privewPic));
            skinPictureBox_pic.Image = Image.FromFile(Path.Combine(_picPath, _privewPic));
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
            Bind bind = new Bind(skinTextBox_Id.Text.Trim(), skinTextBox_Key.Text.Trim(), skinComboBox_Mac.SelectedItem.ToString());
            HttpClient httpClient = new HttpClient();
            string json = JsonConvert.SerializeObject(bind);
            string replaydata, errordata;
            bool bSuc=httpClient.Post(BindingURL, JsonConvert.SerializeObject(bind), out replaydata, out errordata);
            if (!bSuc)
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SystemResult sr;
            sr = JsonConvert.DeserializeObject<SystemResult>(replaydata);
            if(sr.Err_code!= SystemCode.OK)
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _config.ID = skinTextBox_Id.Text.Trim();
            _config.Key = skinTextBox_Key.Text.Trim();
            _config.Mac = skinComboBox_Mac.SelectedItem.ToString();
            if (ConfigTool.SaveConfigData(_config))
            {
                MessageBoxEx.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }
        #endregion
        #region 下载方案
        private int _fileCount = 0;
        private bool _bDownloading = false;
        private int _completeCount = 0;
        private PlanCmdPar _PlanCmdParTemp = null;
        private void DownloadPlan(Cmd cmd)
        {
            if(cmd==null||cmd.CmdType!= CmdType.DownloadPlan|| _bDownloading==true)
            {
                return;
            }
            _bDownloading = true;
            PlanCmdPar planCmdPar=JsonConvert.DeserializeObject<PlanCmdPar>(DecodeBase64((Encoding.UTF8),cmd.CmdParam));
            _PlanCmdParTemp = planCmdPar;
            _fileCount = planCmdPar.Medias.Count + 1;

            DownloadTransmit downloadTransmit = new DownloadTransmit();
            downloadTransmit.Completed +=new Completed(DownloadTransmit_Completed);
            downloadTransmit.Download(planCmdPar.ProgramUrl, Path.Combine(_lbPlanPath, Path.GetFileName(planCmdPar.ProgramName)));
            for (int i = 0; i < planCmdPar.Medias.Count; i++)
            {
                downloadTransmit.Download(planCmdPar.Medias[i].MediaUrl, Path.Combine(_mediaPath, Path.GetFileName(planCmdPar.Medias[i].MediaName)));
            }
        }
        private void DownloadTransmit_Completed(object obj)
        {
            _completeCount++;
            if (_fileCount == _completeCount)
            {
                _fileCount = 0;
                _completeCount = 0;
                _bDownloading = false;
                ThreadStart threadDelegate = new ThreadStart(PlayMedia);
                Thread showThread = new Thread(threadDelegate);
                showThread.Start();
            }
        }
        private PlanCmdPar _cureentPlan = null;
        private LEDScreen _screen = null;
        private Object thisLock = new Object();
        private void PlayMedia()
        {
            //lock (thisLock)
            //{
                if (_screen != null)
                {
                    if (_cureentPlan != null && _cureentPlan.ProgramName == _PlanCmdParTemp.ProgramName)
                    {
                        return;
                    }
                    else
                    {
                        _screen.Free();
                        _screen = null;
                    }
                }
                Thread.Sleep(1000);
                var scheduleFilePath = Path.Combine(_lbPlanPath, Path.GetFileName(_PlanCmdParTemp.ProgramName));
                using (FileStream fs = File.OpenRead(scheduleFilePath))
                {

                    string content;
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        content = reader.ReadToEnd();
                    }
                    var scheduleFile = JsonConvert.DeserializeObject<ScheduleFile>(content);
                    foreach (var mediaItem in scheduleFile.MediaList)
                    {
                        if (mediaItem.Type == LBManager.Infrastructure.Models.FileType.Image)
                        {
                            _screen = new LEDScreen(0, 0, 1024, 768);
                            _screen.PlayImage(Path.Combine(_mediaPath, Path.GetFileName(_PlanCmdParTemp.Medias[0].MediaName)));
                            _cureentPlan = _PlanCmdParTemp;
                        }
                        else if (mediaItem.Type == LBManager.Infrastructure.Models.FileType.Video)
                        {
                            _screen = new LEDScreen(0, 0, 1024, 768);
                            _screen.PlayVideo(Path.Combine(_mediaPath, Path.GetFileName(_PlanCmdParTemp.Medias[0].MediaName)));
                            _cureentPlan = _PlanCmdParTemp;
                        }
                    }
                }
          //  }
           
        }

        #endregion
        #region 未知命令处理
        private void UnKnowCmd(Cmd cmd)
        {
            return;
        }
        #endregion
        #region 上传播放记录
        private bool UploadPlayBack(PlayBack playBack)
        {
            string data= JsonConvert.SerializeObject(playBack);
            HttpClient httpClient = new HttpClient();
            string replaydata, errordata;
            bool bSuc=httpClient.Post(PlayBackURL, data, out replaydata, out errordata);
            if(!bSuc)
            {
                return false;
            }
            SystemResult sr;
            sr = JsonConvert.DeserializeObject<SystemResult>(replaydata);
            if(sr.Err_code!= SystemCode.OK)
            {
                return false;
            }
            return true;
        }
        #endregion
        #region 回复命令结果
        private bool UploadCmdResult(CmdResult cmdRes)
        {
            string data = JsonConvert.SerializeObject(cmdRes);
            HttpClient httpClient = new HttpClient();
            string replaydata, errordata;
            bool bSuc = httpClient.Post(CmdBackURL, data, out replaydata, out errordata);
            if (!bSuc)
            {
                return false;
            }
            SystemResult sr;
            sr = JsonConvert.DeserializeObject<SystemResult>(replaydata);
            if (sr.Err_code != SystemCode.OK)
            {
                return false;
            }
            return true;
        }
        #endregion
        #region 上传监控数据
        private bool UploadMonitorInfo(MonitorResult monitor)
        {
            string data = JsonConvert.SerializeObject(monitor);
            HttpClient httpClient = new HttpClient();
            string replaydata, errordata;
            bool bSuc = httpClient.Post(MonitorInfoURL, data, out replaydata, out errordata);
            if (!bSuc)
            {
                return false;
            }
            SystemResult sr;
            sr = JsonConvert.DeserializeObject<SystemResult>(replaydata);
            if (sr.Err_code != SystemCode.OK)
            {
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
                return false;
            }
            return true;
        }
        #endregion
        #region 重连WebSocket命令
        private void SetWebSocketReConnection(Cmd cmd)
        {
            ReConnectionWebSocket rcws = JsonConvert.DeserializeObject<ReConnectionWebSocket>(DecodeBase64(Encoding.UTF8, cmd.CmdParam));
            initialWebSocket(rcws.Host);
            CmdResult cr = new CmdResult(_config.ID, _config.Key, _config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 屏幕参数设置
        private void SetScreenInfo(Cmd cmd)
        {
            ScreenSet screenSet=JsonConvert.DeserializeObject<ScreenSet>(DecodeBase64(Encoding.UTF8,cmd.CmdParam));
            _config.Size_X = screenSet.Size_x;
            _config.Size_Y = screenSet.Size_y;
            _config.Resoul_X = screenSet.Resolu_x;
            _config.Resoul_Y = screenSet.Resolu_y;
            ConfigTool.SaveConfigData(_config);

            CmdResult cr = new CmdResult(_config.ID, _config.Key,_config.Mac, cmd.CmdId, true.ToString());
            UploadCmdResult(cr);
            DeleteCmd(cmd);
        }
        #endregion
        #region 设置工作时间
        private void SetWorkTime(Cmd cmd)
        {
            WorkTime workTime = JsonConvert.DeserializeObject<WorkTime>(DecodeBase64(Encoding.UTF8, cmd.CmdParam));
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
            RunTime runTime = JsonConvert.DeserializeObject<RunTime>(DecodeBase64(Encoding.UTF8, cmd.CmdParam));
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
            LockSystem lockSystem = JsonConvert.DeserializeObject<LockSystem>(DecodeBase64((Encoding.UTF8), cmd.CmdParam));
            _config.LockUnLockPlayer = lockSystem.Enable;
            _config.LockPwd = lockSystem.Password;
            ConfigTool.SaveConfigData(_config);
            if(lockSystem.Enable)
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
            PollInterval pollInterval = JsonConvert.DeserializeObject<PollInterval>(DecodeBase64((Encoding.UTF8), cmd.CmdParam));
            _config.HeartBeatInterval = pollInterval.Cycle*1000;
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
            MonitorPollInterval pollInterval = JsonConvert.DeserializeObject<MonitorPollInterval>(DecodeBase64((Encoding.UTF8), cmd.CmdParam));
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
                _screenCaptrue.CaptrueScreenRegionToFile(_config.ScreenCuptureX, _config.ScreenCuptureY, _config.ScreenCuptureW, _config.ScreenCuptureH, Path.Combine(_picPath, nowTime.Ticks + ".jpg"), 30);
                UploadImage();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void UploadImage()
        {
            if(bUploding)
            {
                return;
            }
            bUploding = true;
            List<string> imageFiles;
            GetImageList(out imageFiles);
            if (imageFiles == null|| imageFiles.Count==0)
            {
                return;
            }
            HartBeatRequestObj obj = new HartBeatRequestObj();
            obj.Id = _config.ID;
            obj.Key = _config.Key;
            obj.Mac = _config.Mac;
            HttpClient httpClient = new HttpClient();
            string replyData, errorData;
            for (int i = 0; i < imageFiles.Count; i++)
            {
                if(!httpClient.Post(GetPicUploadURL, JsonConvert.SerializeObject(obj), out replyData, out errorData))
                {
                    return;
                }
                UploadPicInfo picInfo = JsonConvert.DeserializeObject<UploadPicInfo>(replyData);

                if (UploadFile(picInfo.Url, imageFiles[i]))
                {
                    try
                    {
                        File.Delete(imageFiles[i]);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
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
                byte[] inData = new byte[4096];
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
            DirectoryInfo imageDestDirInfo = new DirectoryInfo(_picPath);
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
    }
}

