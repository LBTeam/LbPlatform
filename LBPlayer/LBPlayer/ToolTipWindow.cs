using CCWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LBPlayer
{
    public partial class ToolTipWindow : Skin_Color
    {
        public ToolTipWindow()
        {
            InitializeComponent();
        }

        public void Show(string text, int x, int y, int duration)
        {

            SizeF StringSize = new SizeF(0, 0);
            StringSize = TextRenderer.MeasureText(text, skinLabel_tip.Font);
            skinLabel_tip.Width = (int)StringSize.Width + 5;
            skinLabel_tip.Height = (int)StringSize.Height + 5;
            skinLabel_tip.Location = new Point(5, 5);

            Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;

            skinLabel_tip.Text = text;
            this.Width = skinLabel_tip.Width + 10;
            this.Height = skinLabel_tip.Height + 10;
            if (rect.Width < this.Width + x)
            {
                //超出屏幕范围，则初始位置屏幕为准进行计算，
                this.Location = new Point(rect.Width - this.Width - 10, y);
            }
            else
            {
                this.Location = new Point(x, y);
            }
            if (duration < 1)
            {
                timer1.Interval = 1;
            }
            timer1.Interval = duration;
            timer1.Enabled = true;

            this.Show();
            for (int i = 0; i <= 100; i++)
            {
                this.Opacity = i;

            }

        }
    }
}
