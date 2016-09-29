using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LBPlayerConfig
{
    [Serializable]
    public class Config
    {
        public string ID="";
        public string Key="";
        public string Mac = "";
        public bool LockUnLockPlayer = false;
        public string WebUrl = "";
        public int HeartBeatInterval = 30000;
        public bool IsEnableAutoOpenOrClose = false;
        public TimeSpan OpenTime=new TimeSpan(00,00,00);
        public TimeSpan CloseTime = new TimeSpan(00,00,00);
        public bool IsEnableScreenCupture = false;
        public int ScreenCuptureX = 0;
        public int ScreenCuptureY = 0;
        public int ScreenCuptureW = 0;
        public int ScreenCuptureH = 0;
        public string FileSavePath;
    }
    public static class ConfigTool
    {
        public static readonly string ConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\LBPlayer"; //配置文件存放目录
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.bin");//配置文件路径
        public static bool SaveConfigData(Config config)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);
                }
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(ConfigPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                formatter.Serialize(stream, config);
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveConfigData Error:"+ex.Message);
                return false;
            }
            return true;
        }
        public static Config ReadConfigData()
        {
            Config obj = new Config();
            if(!File.Exists(ConfigPath))
            {
                SaveConfigData(obj);
            }
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                obj = (Config)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadConfigData Error:" + ex.Message);
                return null;
            }
            return obj;
        }

    }
}
