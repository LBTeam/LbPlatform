using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace LBManager.Infrastructure.Models
{
   public class MediaTempInfo
    {
        private string _mediaName;
        private string _mediaMD5;

        public MediaTempInfo(string _mediaName, string _mediaMD5)
        {
            this._mediaName = _mediaName;
            this._mediaMD5 = _mediaMD5;
        }

        [JsonProperty("MediaName")]
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

        [JsonProperty("MediaMD5")]
        public string MediaMD5
        {
            get
            {
                return _mediaMD5;
            }

            set
            {
                _mediaMD5 = value;
            }
        }
    }
}
