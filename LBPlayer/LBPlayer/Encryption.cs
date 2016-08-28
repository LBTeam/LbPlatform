using CCWin;
using Com.Utility;
using System;
using System.Collections;
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

    public partial class Encryption : Skin_Color
    {
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private const int WM_POWERBROADCAST = 0x218;         //此消息发送给应用程序来通知它有关电源管理事件
        private const int BROADCAST_QUERY_DENY = 0x424D5144; //广播返回值为阻止事件发生
        private KeyboardHelper _boradHook = new KeyboardHelper();
        private bool _bShift = false;
        private bool _bAlt = false;
        private bool _bCtrl = false;
        private bool _bNumber = false;
        private const int CHAR_ESC = 27;
        private string _password = "";
        public event EventHandler UnLockEvent = null;
        public Encryption()
        {
            InitializeComponent();
        }

        public void Hook_KeyUp(KeyEventArgs e)
        {
            _bShift = e.Shift;
            _bAlt = e.Alt;
            _bCtrl = e.Control;
            if (e.KeyValue >= 96 && e.KeyValue <= 105)
            {
                _bNumber = false;
            }
            if (e.KeyCode == Keys.Enter)
            {
                skinButton_OK_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                textBox_Password.Text = "";
                this.Hide();
            }
        }

        public void Hook_KeyDown(KeyEventArgs e)
        {
            _bShift = e.Shift;
            _bAlt = e.Alt;
            _bCtrl = e.Control;
            if (e.KeyValue >= 96 && e.KeyValue <= 105)
            {
                _bNumber = true;
            }
        }

        public void Hook_KeyPress(KeyPressEventArgs e)
        {
            if (_bAlt || _bCtrl || e.KeyChar == CHAR_ESC || e.KeyChar == '\r' || e.KeyChar == '\t')
            {
                //有Alt或Ctrl组合其他字母或ESC或Enter或Tab按下则不输入
                return;
            }
            if (e.KeyChar == '\b')
            {
                //按下BackSpace键按位删除
                if (textBox_Password.Text.Length > 0)
                {
                    textBox_Password.Text = textBox_Password.Text.Remove(textBox_Password.Text.Length - 1);

                }
            }
            else if (!_bShift)
            {
                textBox_Password.Text = textBox_Password.Text + e.KeyChar.ToString();
            }
            else if ((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z'))
            {
                //主键盘区字母部分，在第二功能下，与当前状态相反
                if (e.KeyChar.ToString() == e.KeyChar.ToString().ToUpperInvariant())
                {
                    textBox_Password.Text = textBox_Password.Text + e.KeyChar.ToString().ToLowerInvariant();
                }
                else if (e.KeyChar.ToString() == e.KeyChar.ToString().ToLowerInvariant())
                {
                    textBox_Password.Text = textBox_Password.Text + e.KeyChar.ToString().ToUpperInvariant();
                }
            }
            else if (!_bNumber && (e.KeyChar >= '0' && e.KeyChar <= '9'))
            {
                //主键盘数字键区的第二功能
                switch (e.KeyChar)
                {
                    case '0':
                        textBox_Password.Text = textBox_Password.Text + ")";
                        break;

                    case '1':
                        textBox_Password.Text = textBox_Password.Text + "!";
                        break;
                    case '2':
                        textBox_Password.Text = textBox_Password.Text + "@";
                        break;
                    case '3':
                        textBox_Password.Text = textBox_Password.Text + "#";
                        break;
                    case '4':
                        textBox_Password.Text = textBox_Password.Text + "$";
                        break;
                    case '5':
                        textBox_Password.Text = textBox_Password.Text + "%";
                        break;
                    case '6':
                        textBox_Password.Text = textBox_Password.Text + "^";
                        break;
                    case '7':
                        textBox_Password.Text = textBox_Password.Text + "&";
                        break;
                    case '8':
                        textBox_Password.Text = textBox_Password.Text + "*";
                        break;
                    case '9':
                        textBox_Password.Text = textBox_Password.Text + "(";
                        break;
                }
            }
            else if (!_bNumber)
            {
                switch (e.KeyChar)
                {
                    //其他第二功能键
                    case '`':
                        textBox_Password.Text = textBox_Password.Text + "~";
                        break;
                    case ',':
                        textBox_Password.Text = textBox_Password.Text + "<";
                        break;
                    case '.':
                        textBox_Password.Text = textBox_Password.Text + ">";
                        break;
                    case '/':
                        textBox_Password.Text = textBox_Password.Text + "?";
                        break;
                    case ';':
                        textBox_Password.Text = textBox_Password.Text + ":";
                        break;
                    case '\'':
                        textBox_Password.Text = textBox_Password.Text + "\"";
                        break;
                    case '[':
                        textBox_Password.Text = textBox_Password.Text + "{";
                        break;
                    case ']':
                        textBox_Password.Text = textBox_Password.Text + "}";
                        break;
                    case '-':
                        textBox_Password.Text = textBox_Password.Text + "_";
                        break;
                    case '=':
                        textBox_Password.Text = textBox_Password.Text + "+";
                        break;
                    case '\\':
                        textBox_Password.Text = textBox_Password.Text + "|";
                        break;
                }

            }
            textBox_Password.Select();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            //在键盘锁定状态下屏蔽键盘上与电源相关的操作
            if (m.Msg == WM_POWERBROADCAST)
            {
                m.Result = (IntPtr)BROADCAST_QUERY_DENY;
                return;
            }
            base.WndProc(ref m);
        }

        public void ShowForm()
        {
            
            this.TopMost = true;
            this.BringToFront();

            this.Show();
        }

        private void skinButton_OK_Click(object sender, EventArgs e)
        {
            if (_password != textBox_Password.Text)
            {
                label_PassWordError.Visible = true;
                textBox_Password.Text = "";
                textBox_Password.Focus();
            }
            else
            {
                label_PassWordError.Visible = false;
                textBox_Password.Text = "";
                textBox_Password.Focus();
                this.Hide();

                if (UnLockEvent != null)
                {
                    UnLockEvent.Invoke(sender, e);
                }
            }
        }
    }
}
