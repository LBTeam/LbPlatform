using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Net
{
    public class DownloadTransmit
    {
        public event ProgressChanged ProgressChanged = delegate { };
        public event Completed Completed = delegate { };

        public void Download(string url,string savePath)
        {
            Http.Get(url).DownloadTo(savePath, onProgressChanged: (bytesCopied, totalBytes) =>
            {
                ProgressChanged(bytesCopied, totalBytes);
                //UpdateText(bytesCopied.ToString());
            },
            onSuccess: (headers) =>
            {
                Completed(headers);//UpdateText("Download Complete");
            }).Go();
        }
    }
}
