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

        private void richTxtControl1_Load(object sender, EventArgs e)
        {
            List<UploadFileInfo> list = new List<UploadFileInfo>();
            UploadTransmit uploadTransmit = new UploadTransmit("http://lbcloud.ddt123.cn/?s=api/Manager/upload", list);
            string md5;
            FileStream fs1 = new FileStream(@"E:\Test\test.playprog", FileMode.Open);
            md5 = uploadTransmit.ComputeContentMd5(fs1, fs1.Length);
            UploadFileInfo u = new UploadFileInfo(@"E:\Test\test.playprog", fs1.Length.ToString(), md5, FileType.Plan);
            fs1.Close();
            u.MediaList = new List<media>();

            FileStream fs2 = new FileStream(@"E:\Test\1.png", FileMode.Open);
            md5 = uploadTransmit.ComputeContentMd5(fs2, fs2.Length);
            u.MediaList.Add(new media(@"E:\Test\1.png", md5));
            fs2.Close();

            FileStream fs3 = new FileStream(@"E:\Test\Wildlife.wmv", FileMode.Open);
            md5 = uploadTransmit.ComputeContentMd5(fs3, fs3.Length);
            u.MediaList.Add(new media(@"E:\Test\Wildlife.wmv", md5));
            fs3.Close();

            FileStream fs4 = new FileStream(@"E:\Test\Wildlife.wmv", FileMode.Open);
            md5 = uploadTransmit.ComputeContentMd5(fs4, fs4.Length);
            UploadFileInfo u2 = new UploadFileInfo(@"E:\Test\Wildlife.wmv", fs4.Length.ToString(), md5, FileType.Image);
            fs4.Close();

            FileStream fs5 = new FileStream(@"E:\Test\Wildlife.wmv", FileMode.Open);
            md5 = uploadTransmit.ComputeContentMd5(fs5, fs5.Length);
            UploadFileInfo u3 = new UploadFileInfo(@"E:\Test\2.png", fs5.Length.ToString(), md5, FileType.Image);
            fs5.Close();
            list.Add(u);
            list.Add(u2);
            list.Add(u3);

            uploadTransmit.UploadFileList = list;

            uploadTransmit.StartUpload();

        }
    }
}
