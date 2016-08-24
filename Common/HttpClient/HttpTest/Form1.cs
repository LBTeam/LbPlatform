using Com.Net;
using JumpKick.HttpLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static int partSize = 50 * 1024 * 1024;

        private void button1_Click(object sender, EventArgs e)
        {
            UploadTransmit u =new UploadTransmit("",new List<UploadFileInfo>());

            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                var fs = File.Open(dlg.FileName, FileMode.Open);
                fs.Seek((long)120, 0);
                

                //var fi = new FileInfo(dlg.FileName);
                //var fileSize = fi.Length;
                //var partCount = fileSize / partSize;
                //if (fileSize % partSize != 0)
                //{
                //    partCount++;
                //}
                //NamedFileStream[] namedFileStreams = new NamedFileStream[partCount];
                //for (var i = 0; i < partCount; i++)
                //{
                //    var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                //    //var skipBytes = (long)partSize * i;
                //   // fs.Seek(skipBytes, 0);
                //    //var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);

                //    NamedFileStream n = new NamedFileStream("TestFile", dlg.FileName, "application/octet-stream", fs);
                //    namedFileStreams[i] = n;
                //}
                Dictionary<String, String> headers = new Dictionary<String, String>();

                headers.Add("Content-Type", "application/octet-stream");

                Http.Put(textBox1.Text).Upload(new[] { new NamedFileStream("myfile", dlg.FileName, "application/octet-stream", File.OpenRead(dlg.FileName)) }, new { }, (bytesSent, totalBytes) =>
                {
                    UpdateText(bytesSent.ToString());
                },
                (totalBytes) => { }).

                OnFail((fail) => {
                    UpdateText(fail.Message.ToString());
                }
                )
                .OnSuccess((result) => 
                {
                    UpdateText("Completed");
                }).Go();
            }
        }
        private void UpdateText(String text)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtResult.Text = text;
            });
        }
    }
}
