using LBPlayerConfig;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LBPlayerTool
{
    public partial class LBPlayerTool : Form
    {
        private string _registName = "LBPlayerTool";
        private string _processName = @"E:\LB\LBPlayer\LBPlayer\bin\Debug\LBPlayer.exe";
        private System.Threading.Timer _checkTimer = null;
        private static object locker = new object();
        private int _isUpNowTime = 1;
        private Config _config = null;
        #region Public method
        /// <summary>
        /// 构造函数
        /// </summary>
        public LBPlayerTool()
        {
            InitializeComponent();
        }
        #endregion
        #region private method
        private void AutoRunWhenStart(string exeName, string exePath, bool autoRun)
        {
            RegistryKey HKCU = null;
            try
            {
                HKCU = Registry.CurrentUser;
                RegistryKey registryKey = HKCU.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (autoRun)
                {
                    registryKey.SetValue(exeName, exePath); 
                }
                else
                {
                    registryKey.DeleteValue(exePath, false);
                }
                HKCU.Close();
            }
            catch (Exception ex)
            {
                if (HKCU != null)
                {
                    HKCU.Close();
                }
            }
        }
        private bool IsApplicationAutoRun(string registName)
        {
            RegistryKey HKCU = null;
            try
            {
                HKCU = Registry.CurrentUser;
                RegistryKey registryKey = HKCU.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                object getRegistObj = registryKey.GetValue(registName);//获取指定名称的注册表项

                if (getRegistObj == null)//若注册表项为空，说明注册表中不存在这一项
                {
                    return false;
                }
                string path = getRegistObj.ToString();//获取与该注册表项对应的文件路径
                if (path.ToLower() != Application.ExecutablePath.ToLower())
                {

                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                if (HKCU != null)
                {
                    HKCU.Close();
                }
                return false;
            }
        }
        private void LBPlayerTool_Load(object sender, EventArgs e)
        {
            if(!IsApplicationAutoRun(_registName))
            {
                AutoRunWhenStart(_registName, Application.ExecutablePath, true);
            }
            _checkTimer = new System.Threading.Timer(CheckProcess, null, Timeout.Infinite, Timeout.Infinite);
            _checkTimer.Change(0, 1000);
        }
        TimeSpan timeFromClose = new TimeSpan();
        TimeSpan timeFromOpen = new TimeSpan();
        private void CheckProcess(object state)
        {
            if (Interlocked.Exchange(ref _isUpNowTime, 0) == 1)
            {
                try
                {
                    _config = ConfigTool.ReadConfigData();
                    if (_config.IsEnableAutoOpenOrClose)
                    {
                        timeFromClose = _config.CloseTime - DateTime.Now.TimeOfDay;
                        if ((Math.Abs(timeFromClose.TotalSeconds) * 1000 < 3000))
                        {
                            KillRecentProcess(_processName);
                            return;
                        }
                        timeFromOpen = _config.OpenTime - DateTime.Now.TimeOfDay;
                        if ((Math.Abs(timeFromOpen.TotalSeconds) * 1000 < 3000))
                        {
                            StartProcess(_processName);
                            return;
                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("CheckProcess Error" + ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _isUpNowTime, 1);
                }
            }
        }
        private void StartProcess(string processName)
        {
            bool bRunning = false;
            System.Diagnostics.Process[] curProc = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process proc in curProc)
            {
                if (proc.ProcessName == processName)
                {
                    bRunning = true;
                    break;
                }
            }
            if (!bRunning)
            {
                System.Diagnostics.Process.Start(processName);
            }
        }
        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="processName">进程名</param>
        private void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }
        }


        //强制关闭最近打开的某个进程

        private void KillRecentProcess(string processName)

        {
            System.Diagnostics.Process[] Proc = System.Diagnostics.Process.GetProcessesByName(processName);
            System.DateTime startTime = new DateTime();
            int m, killId = 0;
            for (m = 0; m < Proc.Length; m++)
            {
                if (startTime < Proc[m].StartTime)
                {
                    startTime = Proc[m].StartTime;
                    killId = m;
                }
            }
            if (Proc[killId].HasExited == false)
            {
                Proc[killId].Kill();
            }

        }
        #endregion
    }
}
