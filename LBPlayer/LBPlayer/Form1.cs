using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LBPlayerConfig;

namespace LBPlayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public void s()
        {
            Config c = new Config();
            c.HeartBeatInterval = 10;
            c.LockUnLockPlayer = true;
            c.WebUrl = "1";
            c.OpenTime = new TimeSpan(22, 00, 00);
            c.CloseTime = new TimeSpan(23, 59, 00);
            c.IsEnableAutoOpenOrClose = true;
            ConfigTool.SaveConfigData(c);

            Config f = ConfigTool.ReadConfigData();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            s();
        }
    }
}
