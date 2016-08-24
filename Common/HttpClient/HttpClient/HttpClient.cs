using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Com.Net
{

    public class HttpClient
    {

        private HttpWebRequest _httpWebRequest = null;
        private Encoding _encoder = Encoding.UTF8;

        public Encoding Encoder
        {
            get { return _encoder; }
            set { _encoder = value; }
        }
        private WebProxy _proxy = null;
        public WebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }
        public bool Post(string url, String data, out String replydata, out String ErrInfo)
        {
            replydata = "";
            ErrInfo = "";
            bool bRes = true;
            HttpWebResponse httpWebResponse = null;
            Stream responseStream = null;
            StreamReader streamReader = null;
            Stream outStream = null;
            try
            {

                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);


                _httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                _httpWebRequest.KeepAlive = false;//不维持与服务器的请求状态
                _httpWebRequest.Method = "POST";   //设置与服务器交互的谓词
                _httpWebRequest.Accept = "*/*;";   //表示接收所有的数据类型
                //将post数据编码得到相应的字节数组
                byte[] postBytes = _encoder.GetBytes(data);
                //HTTP-GET和POST 使用 application/x-www-form-urlencoded MIME 类型以 URL 编码文本格式传递其参数
                _httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                _httpWebRequest.ContentLength = postBytes.Length;
                if (_proxy != null)
                {
                    _httpWebRequest.Proxy = _proxy;// new WebProxy("192.168.0.188", 3128);
                }

                outStream = _httpWebRequest.GetRequestStream(); //获取与服务器交互的数据流，并将数据写入
                outStream.Write(postBytes, 0, postBytes.Length);
                httpWebResponse = (HttpWebResponse)_httpWebRequest.GetResponse(); //获取服务器的响应
                responseStream = httpWebResponse.GetResponseStream(); //获取服务器的响应数据流并读取，显示在WebBrowser中
                streamReader = new StreamReader(responseStream, _encoder);
                replydata = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                ErrInfo = ex.ToString();
                bRes = false;
            }
            finally
            {
                if (_httpWebRequest != null)
                {
                    _httpWebRequest.Abort();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                if (outStream != null)
                {
                    outStream.Close();
                }
            }
            System.Diagnostics.Debug.WriteLine("Post : " + data);
            return bRes;
        }
        public bool Post(string url, String data, out String replydata, out Exception errInfo)
        {
            replydata = "";
            errInfo = null;
            bool bRes = true;
            //HttpWebRequest httpWRequest = null;
            HttpWebResponse httpWebResponse = null;
            Stream responseStream = null;
            StreamReader streamReader = null;
            Stream outStream = null;
            try
            {

                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);


                _httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                _httpWebRequest.KeepAlive = false;//不维持与服务器的请求状态
                _httpWebRequest.Method = "POST";   //设置与服务器交互的谓词
                _httpWebRequest.Accept = "*/*;";   //表示接收所有的数据类型
                //将post数据编码得到相应的字节数组
                byte[] postBytes = _encoder.GetBytes(data);
                //HTTP-GET和POST 使用 application/x-www-form-urlencoded MIME 类型以 URL 编码文本格式传递其参数
                _httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                _httpWebRequest.ContentLength = postBytes.Length;
                if (_proxy != null)
                {
                    _httpWebRequest.Proxy = _proxy;// new WebProxy("192.168.0.188", 3128);
                }

                outStream = _httpWebRequest.GetRequestStream(); //获取与服务器交互的数据流，并将数据写入
                outStream.Write(postBytes, 0, postBytes.Length);
                httpWebResponse = (HttpWebResponse)_httpWebRequest.GetResponse(); //获取服务器的响应
                responseStream = httpWebResponse.GetResponseStream(); //获取服务器的响应数据流并读取，显示在WebBrowser中

                streamReader = new StreamReader(responseStream, _encoder);
                replydata = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                errInfo = ex;
                bRes = false;
            }
            finally
            {
                if (_httpWebRequest != null)
                {
                    _httpWebRequest.Abort();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                if (outStream != null)
                {
                    outStream.Close();
                }
            }
            System.Diagnostics.Debug.WriteLine("Post : " + data);
            return bRes;
        }
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }
    }
}
