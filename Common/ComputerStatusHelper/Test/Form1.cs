using Com.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
             ComputerStatus c = new ComputerStatus();
            List<CPUModel> a= c.GetCPUTemperature();
            for (int i = 0; i < a.Count; i++)
            {
                textBox1.Text += a[i].Name + ":" + a[i].Temperature;
            }
            string s = "无";
            c.GetFanSpeed(out s);
            textBox1.Text += s;

        }
    }
}
