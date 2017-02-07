using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBPlayer
{
    public static class ApplicationConfig
    {
        public static readonly Uri BaseURL = new Uri("http://lbcloud.ddt123.cn");
        public const string BindingURL = "?s=api/Player/bind_player";
        public const string PlayBackURL = "?s=api/Player/record";
        public const string PollURL = "http://lbcloud.ddt123.cn/?s=api/Player/heartbeat";
        public const string CmdBackURL = "?s=api/Player/cmd_result";
        public const string MonitorInfoURL = "?s=api/Player/monitor";
        public const string GetPicUploadURL = "?s=api/Player/picture";
        public const string WebSocketURL = "ws://123.56.240.172:9501";

        private static string _mediaFilePath = string.Empty;
        private static string _playLogFilePath = string.Empty;
        private static string _scheduleFilePath = string.Empty;
        private static string _offlineScheduleFilePath = string.Empty;
        private static string _pictureFilePath = string.Empty;

        public static string GetMediaFilePath()
        {
            return _mediaFilePath;
        }

        public static void SetMediaFilePath(string filePath)
        {
            _mediaFilePath = filePath;
        }

        public static string GetPlayLogFilePath()
        {
            return _playLogFilePath;
        }

        public static void SetPlayLogFilePath(string filePath)
        {
            _playLogFilePath = filePath;
        }

        public static string GetScheduleFilePath()
        {
            return _scheduleFilePath;
        }

        public static void SetScheduleFilePath(string filePath)
        {
            _scheduleFilePath = filePath;
        }

        public static string GetOfflineScheduleFilePath()
        {
            return _offlineScheduleFilePath;
        }

        public static void SetOfflineScheduleFilePath(string filePath)
        {
            _offlineScheduleFilePath = filePath;
        }

        public static string GetPictureFilePath()
        {
            return _pictureFilePath;
        }

        public static void SetPictureFilePath(string filePath)
        {
            _pictureFilePath = filePath;
        }
    }
}
