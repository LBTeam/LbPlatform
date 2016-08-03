using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;

namespace Com.Utility
{
    public class CPUModel
    {
        private string _name;
        private string _temperature;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public string Temperature
        {
            get
            {
                return _temperature;
            }

            set
            {
                _temperature = value;
            }
        }
    }
    public class UpdateVisitor : IVisitor
    {

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_INFO
    {
        public uint dwLength;
        public uint dwMemoryLoad;//正在使用
        public uint dwTotalPhys;//物理内存大小
        public uint dwAvailPhys;//可使用物理内存
        public uint dwTotalPageFile;//交换文件总大小
        public uint dwAvailPageFile;
        public uint dwTotalVirtual;//总虚拟内存
        public uint dwAvailVirtual;
    }
    public class ComputerStatus : IDisposable
    {
        #region 字段
        private readonly string DebugInfoHead = AppDomain.CurrentDomain.FriendlyName + "--" + "ComputerStatus.cs" + ":>>>>";
        private PerformanceCounter _performanceCounter;
        private bool _bFirstGetCpuUtilization = true;
        #endregion

        public ComputerStatus()
        {
            try
            {
                _performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", ".");
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(DebugInfoHead + "初始化PerformanceCounter时出现异常：" + e.Message);
            }
            
        }
        
        public List<CPUModel> GetCPUTemperature()
        {
            List<CPUModel> listTemp = new List<CPUModel>();
            Computer myComputer = new Computer();
            try
            {
                
                UpdateVisitor updateVisitor = new UpdateVisitor();

                myComputer.CPUEnabled = true;
                myComputer.Accept(updateVisitor);

                myComputer.Open();

                foreach (var hardwareItem in myComputer.Hardware)
                {
                    hardwareItem.Update();
                    if (hardwareItem.HardwareType == HardwareType.CPU)
                    {
                        foreach (var sensor in hardwareItem.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                CPUModel cm = new CPUModel();
                                cm.Name = sensor.Name;
                                cm.Temperature = Convert.ToString(sensor.Value);
                                listTemp.Add(cm);
                            }
                            
                        }
                        
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("获取CPU温度异常：" + ex.ToString());
                return listTemp;
            }
            finally
            {
                myComputer.Close();
            }
            return listTemp;
        }

        /// <summary>
        /// 获取CPU利用率
        /// </summary>
        /// <param name="bContinuous">若为连续获取则为true,若只取一次则为false</param>
        /// <param name="cpuUtilization">CPU利用率</param>
        /// <returns>是否获取成功</returns>
        public bool GetCpuUtilization(out float cpuUtilization)
        {
            cpuUtilization = -1;
            if (_performanceCounter == null)
            {
                return false;
            }
            try
            {
                if (_bFirstGetCpuUtilization)
                {
                    cpuUtilization = _performanceCounter.NextValue(); //cpu占用率
                    Thread.Sleep(500);
                }
                cpuUtilization = _performanceCounter.NextValue(); //cpu占用率
                _bFirstGetCpuUtilization = false;
                return true;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(DebugInfoHead + "获取CPU使用率时出现异常：" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取磁盘空间
        /// </summary>
        /// <param name="driveDirectoryName">磁盘名</param>
        /// <param name="freeBytesAvailable">可用空间</param>
        /// <param name="driveTotalSize">总空间</param>
        /// <returns></returns>
        public bool GetDriveSpace(string driveDirectoryName, out long freeBytesAvailable, out long driveTotalSize)
        {
            freeBytesAvailable = 0;
            driveTotalSize = 0;
            if (!Directory.Exists(driveDirectoryName))
            {
                return false;
            }
            DriveInfo drive = new DriveInfo(driveDirectoryName);
            driveTotalSize = drive.TotalSize;
            freeBytesAvailable = drive.AvailableFreeSpace;
            return true;
        }

        #region 获取内存状态
        [DllImport("kernel32")]
        private static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);
        public bool GetMemoryStatus(ref MEMORY_INFO meminfo)
        {
            meminfo = new MEMORY_INFO();
            try
            {
                GlobalMemoryStatus(ref meminfo);
                return true;
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(DebugInfoHead + "获取内存信息时出现异常：" + e.Message);
                return false;
            }
        }
        #endregion

        public void Dispose()
        {
            if (_performanceCounter != null)
            {
                _performanceCounter.Dispose();
            }
        }
    }
}
