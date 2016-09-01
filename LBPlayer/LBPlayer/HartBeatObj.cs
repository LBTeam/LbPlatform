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

        public Cmd(CmdType _cmdType, string _cmdParam,string cmdId)
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
    }
}
