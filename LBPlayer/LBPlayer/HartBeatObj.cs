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
    }
}
