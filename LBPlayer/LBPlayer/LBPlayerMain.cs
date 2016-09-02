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
        private const int QueryTimerInterval = 3000;
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
            HartBeatResponseObj hartBeatResponseObj;
            //throw new NotImplementedException();
            SetControlText(skinLabel8, "心跳完成");
            hartBeatResponseObj = JsonConvert.DeserializeObject<HartBeatResponseObj>(args.Replydata);

            HartBeatHandle(hartBeatResponseObj);

        }
        /// <summary>
        /// 处理心跳结果
        /// </summary>
        /// <param name="hartBeatResponseObj"></param>
        private void HartBeatHandle(HartBeatResponseObj hartBeatResponseObj)
        {
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
            HartBeatRequestObj obj = new HartBeatRequestObj();
            obj.Id = "1";
            obj.Key = "1";
            obj.Mac = "1";
            args.Url = "http://lbcloud.ddt123.cn/?s=api/Player/heartbeat";
            args.PollData = JsonConvert.SerializeObject(obj);

            //throw new NotImplementedException();
            SetControlText(skinLabel8, "开始心跳");
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
            rect = Screen.GetBounds(this);
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
            _cmdList = XmlUtil.XmlDeserializeFromFile<List<Cmd>>(_cmdSavePath, Encoding.UTF8);
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
            RegistryKey keyLocalMachine = Registry.LocalMachine;
            RegistryKey key;
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
                    _keyPasswordWindow.Password = "123456";
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
            _config.ID = skinTextBox_Id.Text.Trim();
            _config.Key = skinTextBox_Key.Text.Trim();
            _config.Mac = skinComboBox_Mac.SelectedItem.ToString();
            if (ConfigTool.SaveConfigData(_config))
            {
                MessageBoxEx.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBoxEx.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        #endregion
        #region 执行命令
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
            }
        }

        private void UnKnowCmd(Cmd cmd)
        {

        }
        #endregion

        
    }
}
