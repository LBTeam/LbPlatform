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
using CCWin;

namespace LBPlayer
{
    public partial class LBPlayerMain : Skin_Color
    {
        public LBPlayerMain()
        {
            InitializeComponent();
        }
        public void s()
        {
            Config c = new Config();
            c.HeartBeatInterval = 10;
            c.LockUnLockPlayer = true;
            c.WebUrl = "1";
            c.OpenTime = new TimeSpan(22, 45, 00);
            c.CloseTime = new TimeSpan(22, 50, 00);
            c.IsEnableAutoOpenOrClose = true;
            ConfigTool.SaveConfigData(c);

            Config f = ConfigTool.ReadConfigData();
        }
    }
}
