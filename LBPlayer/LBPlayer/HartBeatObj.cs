using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LBPlayer
{
    public class HartBeatRequestObj
    {
        private string _id;
        private string _key;
        private string _mac;

        public HartBeatRequestObj()
        {

        }

        public HartBeatRequestObj(string id, string key, string mac)
        {
            this._id = id;
            this._key = key;
            this._mac = mac;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }
    }
    public class HartBeatResponseObj
    {
        private string _err_code;
        private string _msg;
        private string _id;
        private string _key;
        private string _mac;
        private List<Cmd> _data;

        public HartBeatResponseObj(string _err_code, string _msg, string _id, string _key, string _mac, List<Cmd> _data)
        {
            this._err_code = _err_code;
            this._msg = _msg;
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
            this._data = _data;
        }
        [JsonProperty("err_code")]
        public string Err_code
        {
            get
            {
                return _err_code;
            }

            set
            {
                _err_code = value;
            }
        }
        [JsonProperty("msg")]
        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }
        [JsonProperty("Id")]
        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }
        [JsonProperty("Key")]
        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }
        [JsonProperty("Mac")]
        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }
        [JsonProperty("data")]
        public List<Cmd> Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }
    }
    public class Cmd
    {
        private CmdType _cmdType;
        private string _cmdParam;
        private string _cmdId;

        public Cmd(CmdType _cmdType, string _cmdParam, string cmdId)
        {
            this._cmdType = _cmdType;
            this._cmdParam = _cmdParam;
            this._cmdId = cmdId;
        }
        public Cmd()
        {

        }
        [JsonProperty("CmdType")]
        public CmdType CmdType
        {
            get
            {
                return _cmdType;
            }

            set
            {
                _cmdType = value;
            }
        }
        [JsonProperty("CmdParam")]
        public string CmdParam
        {
            get
            {
                return _cmdParam;
            }

            set
            {
                _cmdParam = value;
            }
        }
        [JsonProperty("CmdId")]
        public string CmdId
        {
            get
            {
                return _cmdId;
            }

            set
            {
                _cmdId = value;
            }
        }
    }
    public class PlanCmdPar
    {
        private string _programId;
        private string _programName;
        private string _programUrl;
        private List<Media> _medias;

        public string ProgramId
        {
            get
            {
                return _programId;
            }

            set
            {
                _programId = value;
            }
        }

        public string ProgramName
        {
            get
            {
                return _programName;
            }

            set
            {
                _programName = value;
            }
        }

        public string ProgramUrl
        {
            get
            {
                return _programUrl;
            }

            set
            {
                _programUrl = value;
            }
        }

        public List<Media> Medias
        {
            get
            {
                return _medias;
            }

            set
            {
                _medias = value;
            }
        }
    }
    public class Media
    {
        private string _mediaId;
        private string _mediaName;
        private string _mediaUrl;

        public string MediaId
        {
            get
            {
                return _mediaId;
            }

            set
            {
                _mediaId = value;
            }
        }

        public string MediaName
        {
            get
            {
                return _mediaName;
            }

            set
            {
                _mediaName = value;
            }
        }

        public string MediaUrl
        {
            get
            {
                return _mediaUrl;
            }

            set
            {
                _mediaUrl = value;
            }
        }
    }
    public enum CmdType
    {
        DownloadPlan,
        Lock,
        SetHartBeatPeriod,
        UploadMonitor,
        SoftWareRunTime,
        SetScreenLocation,
        SetScreenWorkTime,
        EmergencyPlan,
        OfflinePlan,
        WebSocketReConnection

    }
    public class Bind
    {
        private string _id;
        private string _key;
        private string _mac;

        public Bind(string _id, string _key, string _mac)
        {
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }
    }
    public class PlayBack
    {
        private string _id;
        private string _key;
        private string _mac;
        private string _mediaId;
        private DateTime _startTime;
        private DateTime _endTime;

        public PlayBack(string _id, string _key, string _mac, string _mediaId, DateTime _startTime, DateTime _endTime)
        {
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
            this._mediaId = _mediaId;
            this._startTime = _startTime;
            this._endTime = _endTime;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }

        public string MediaId
        {
            get
            {
                return _mediaId;
            }

            set
            {
                _mediaId = value;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }

            set
            {
                _startTime = value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }

            set
            {
                _endTime = value;
            }
        }
    }
    public class SystemResult
    {
        private SystemCode _err_code;
        private string _msg;

        [JsonProperty("err_code")]
        public SystemCode Err_code
        {
            get
            {
                return _err_code;
            }

            set
            {
                _err_code = value;
            }
        }

        [JsonProperty("msg")]
        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }
    }
    public enum SystemCode
    {
        OK = 000000,

    }
    public class CmdResult
    {
        private string _id;
        private string _key;
        private string _mac;
        private string _cmdId;
        private string _cmdRes;

        public CmdResult(string _id, string _key, string _mac, string _cmdId, string _cmdRes)
        {
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
            this._cmdId = _cmdId;
            this._cmdRes = _cmdRes;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }

        public string CmdId
        {
            get
            {
                return _cmdId;
            }

            set
            {
                _cmdId = value;
            }
        }

        public string CmdRes
        {
            get
            {
                return _cmdRes;
            }

            set
            {
                _cmdRes = value;
            }
        }
    }
    public class MonitorResult
    {
        private string _id;
        private string _key;
        private string _mac;
        private MonitorInfo _monitor;

        public MonitorResult(string _id, string _key, string _mac, MonitorInfo _monitor)
        {
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
            this._monitor = _monitor;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }

        public MonitorInfo Monitor
        {
            get
            {
                return _monitor;
            }

            set
            {
                _monitor = value;
            }
        }
    }
    public class MonitorInfo
    {
        private string _cpu_usage;
        private string _disk_usage;
        private string _memory_usage;
        private string _cpu_temperature;
        private string _fan_speed;

        public MonitorInfo(string _cpu_usage, string _disk_usage, string _memory_usage, string _cpu_temperature, string _fan_speed)
        {
            this._cpu_usage = _cpu_usage;
            this._disk_usage = _disk_usage;
            this._memory_usage = _memory_usage;
            this._cpu_temperature = _cpu_temperature;
            this._fan_speed = _fan_speed;
        }

        public string Cpu_usage
        {
            get
            {
                return _cpu_usage;
            }

            set
            {
                _cpu_usage = value;
            }
        }

        public string Disk_usage
        {
            get
            {
                return _disk_usage;
            }

            set
            {
                _disk_usage = value;
            }
        }

        public string Memory_usage
        {
            get
            {
                return _memory_usage;
            }

            set
            {
                _memory_usage = value;
            }
        }

        public string Cpu_temperature
        {
            get
            {
                return _cpu_temperature;
            }

            set
            {
                _cpu_temperature = value;
            }
        }

        public string Fan_speed
        {
            get
            {
                return _fan_speed;
            }

            set
            {
                _fan_speed = value;
            }
        }
    }
    public class ScreenSet
    {
        private string _id;
        private string _key;
        private string _name;
        private string _size_x;
        private string _size_y;
        private string _resolu_x;
        private string _resolu_y;

        public ScreenSet(string _id, string _key, string _name, string _size_x, string _size_y, string _resolu_x, string _resolu_y)
        {
            this._id = _id;
            this._key = _key;
            this._name = _name;
            this._size_x = _size_x;
            this._size_y = _size_y;
            this._resolu_x = _resolu_x;
            this._resolu_y = _resolu_y;
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

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

        public string Size_x
        {
            get
            {
                return _size_x;
            }

            set
            {
                _size_x = value;
            }
        }

        public string Size_y
        {
            get
            {
                return _size_y;
            }

            set
            {
                _size_y = value;
            }
        }

        public string Resolu_x
        {
            get
            {
                return _resolu_x;
            }

            set
            {
                _resolu_x = value;
            }
        }

        public string Resolu_y
        {
            get
            {
                return _resolu_y;
            }

            set
            {
                _resolu_y = value;
            }
        }
    }
    public class WorkTime
    {
        private TimeSpan _start;
        private TimeSpan _end;

        public WorkTime(TimeSpan _start, TimeSpan _end)
        {
            this._start = _start;
            this._end = _end;
        }

        public TimeSpan Start
        {
            get
            {
                return _start;
            }

            set
            {
                _start = value;
            }
        }

        public TimeSpan End
        {
            get
            {
                return _end;
            }

            set
            {
                _end = value;
            }
        }
    }
    public class RunTime
    {
        private bool _switch;
        private TimeSpan _enable;
        private TimeSpan _disable;
        

        public TimeSpan Enable
        {
            get
            {
                return _enable;
            }

            set
            {
                _enable = value;
            }
        }

        public TimeSpan Disable
        {
            get
            {
                return _disable;
            }

            set
            {
                _disable = value;
            }
        }

        public bool Switch
        {
            get
            {
                return _switch;
            }

            set
            {
                _switch = value;
            }
        }
    }
    public class LockSystem
    {
        private string _password;
        private bool _enable;

        public LockSystem(string _password, bool _enable)
        {
            this._password = _password;
            this._enable = _enable;
        }

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        public bool Enable
        {
            get
            {
                return _enable;
            }

            set
            {
                _enable = value;
            }
        }
    }
    public class PollInterval
    {
        private int _cycle;

        public int Cycle
        {
            get
            {
                return _cycle;
            }

            set
            {
                _cycle = value;
            }
        }
    }

    public class MonitorPollInterval
    {
        private int _cycle;

        public int Cycle
        {
            get
            {
                return _cycle;
            }

            set
            {
                _cycle = value;
            }
        }
    }

    public class WebSocketAccept
    {
        private string _act;
        private string _id;
        private string _key;
        private string _mac;

        public WebSocketAccept(string _act, string _id, string _key, string _mac)
        {
            this._act = _act;
            this._id = _id;
            this._key = _key;
            this._mac = _mac;
        }

        public string Act
        {
            get
            {
                return _act;
            }

            set
            {
                _act = value;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }

            set
            {
                _mac = value;
            }
        }
    }
    public class WebSocketMsg
    {
        private Accept _act;
        private string _msg;

        public Accept Act
        {
            get
            {
                return _act;
            }

            set
            {
                _act = value;
            }
        }

        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }
    }
    public enum Accept
    {
        notice =1,
        studow =2
    }
    public class ReConnectionWebSocket
    {
        private string _host;

        public string Host
        {
            get
            {
                return _host;
            }

            set
            {
                _host = value;
            }
        }
    }

    public class UploadPicInfo
    {
        private string _err_code;
        private string _msg;
        private string _url;

        public string Err_code
        {
            get
            {
                return _err_code;
            }

            set
            {
                _err_code = value;
            }
        }

        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
            }
        }
    }
}
