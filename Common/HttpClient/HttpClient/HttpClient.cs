using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.Net
{
    
    public class HttpClient
    {
        #region 字段
        private WebProxy _webProxy = null;
        private Encoding _encoder = Encoding.UTF8;
        #endregion

        #region 属性
        public WebProxy WebProxy
        {
            get
            {
                return _webProxy;
            }

            set
            {
                _webProxy = value;
            }
        }

        public Encoding Encoder
        {
            get
            {
                return _encoder;
            }

            set
            {
                _encoder = value;
            }
        }
        #endregion

        #region 事件
        public event UploadFileCompletedEventHandler UploadFileCompletedEvent;
        public event UploadProgressChangedEventHandler UploadProgressChangedEvent;
        public event DownloadStringCompletedEventHandler DownloadStringCompletedEvent;
        public event DownloadProgressChangedEventHandler DownloadProgressChangedEvent;
        #endregion

        #region 公有函数
        public HttpClient()
        {
            _webProxy = new WebProxy();
            
        }

        public bool PostHttp(string url,string requestData,string responseDate)
        {
            return true;
        }

        public void AsyncDownloadFile()
        {
            
        }
        public void SyncDownloadFile()
        {

        }

        public void AsyncUploadFile()
        {

        }
        public void SyncUploadFile()
        {

        }
        #endregion
    }
}
