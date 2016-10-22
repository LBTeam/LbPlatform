using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace Com.Utility
{
    public class KeyboardHelper
    {
        #region API函数
        // Methods
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("user32.dll")]
        private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] IpbKeyState, byte[] IpwTransKey, int fuState);
        [StructLayout(LayoutKind.Sequential)]

        #endregion

        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        public struct MouseHookStruct
        {
            public Point p;
            public IntPtr HWnd;
            public uint wHitTestCode;
            public int dwExtraInfo;
        }

        #region 字段
        // Nested Types
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        // Fields
        private int hKeyboardHook;
        private HookProc KeyboardHookProcedure;
        private int hMouseHook;
        private HookProc MouseHookProcedure;

        private const int WH_KEYBOARD_LL = 0x00d; //键盘钩子的ID
        private const int WM_KEYDOWN = 0x100; //一个键按下的ID
        private const int WM_KEYUP = 0x101;  //一个键释放的ID
        private const int WM_SYSKEYDOWN = 0x104;  //一个系统键按下的ID
        private const int WM_SYSKEYUP = 0x105;  //一个系统键释放的ID

        private const int WH_MOUSE_LL = 0x00e;  //全局鼠标钩子ID
        private const int WM_MOUSEMOVE = 0x200;   //鼠标移动的ID
        private const int WM_MOUSELEFTDOWN = 0x201; //左键按下的ID
        private const int WM_MOUSELEFTUP = 0x202;  //左键键释放的ID
        private const int WM_MOUSERIGHTDOWN = 0x204; //左键按下的ID
        private const int WM_MOUSERIGHTUP = 0x205;  //左键键释放的ID


        public event KeyEventHandler KeyUp;
        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseDown;

        private List<Keys> preKeys = new List<Keys>();
        #endregion
        #region 属性
        private bool _isLockMessage = true;
        /// <summary>
        /// 是否锁定键盘消息
        /// </summary>
        public bool IsLockMessage
        {
            get { return _isLockMessage; }
            set { _isLockMessage = value; }
        }
        #endregion
        public KeyboardHelper()
        {

        }

        #region 锁定键盘
        public bool KeyboardHookStart()
        {
            if (this.hKeyboardHook != 0)
            {
                return false;
            }
            this.KeyboardHookProcedure = new HookProc(this.KeyboardProc);
            this.hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, this.KeyboardHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            if (this.hKeyboardHook == 0)
            {
                this.KeyboardHookStop();
                return false;
            }
            return true;

        }
        public bool KeyboardHookStop()
        {
            bool flag = true;
            if (this.hKeyboardHook != 0)
            {
                flag = UnhookWindowsHookEx(this.hKeyboardHook);
                this.hKeyboardHook = 0;
            }
            if (!flag)
            {
                return false;
            }
            return true;

        }

        private int KeyboardProc(int nCode, int wParam, IntPtr lParam)
        {

            //如果是监听键盘消息的线程钩子，根据lParam值的正负确定按键是按下还是抬起,lParam.ToInt32() > 0时，键盘按下；lParam.ToInt32() < 0时，键盘抬起。
            KeyBoardHookStruct kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));

             if ((nCode >= 0) && (lParam.ToInt32() > 0))
            {

                Keys keyData = (Keys)kbh.vkCode;
                if ((KeyPress != null || KeyDown != null) && (wParam == WM_SYSKEYDOWN || wParam == WM_KEYDOWN))
                {
                    if (IsCtrlShiftAltKeys(keyData) && preKeys.IndexOf(keyData) == -1)
                    {
                        preKeys.Add(keyData);
                    }
                }
                if (KeyDown != null && (wParam == WM_SYSKEYDOWN || wParam == WM_KEYDOWN))
                {
                    KeyDown(this, new KeyEventArgs(GetDownKeys(keyData)));
                }


                if (KeyPress != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (ToAscii(kbh.vkCode, kbh.scanCode, keyState, inBuffer, kbh.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPress(this, e);
                    }
                }
                if ((KeyPress != null || KeyDown != null) && (wParam == WM_SYSKEYUP || wParam == WM_KEYUP))
                {
                    if (IsCtrlShiftAltKeys(keyData))
                    {
                        for (int i = preKeys.Count - 1; i >= 0; i--)
                        {
                            if (preKeys[i] == keyData)
                            {
                                preKeys.RemoveAt(i);
                            }
                        }

                    }
                }
                //当键按下的时候wParam = WM_KEYDOWN，当键抬起的时候wParam = WM_KEYUP
                if (KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    KeyUp(this, new KeyEventArgs(GetDownKeys(keyData)));
                }

                if (keyData != Keys.CapsLock && keyData != Keys.RControlKey && keyData != Keys.LControlKey && keyData != Keys.LMenu && keyData != Keys.RMenu)
                {
                    if (!_isLockMessage)
                    {
                        return CallNextHookEx(this.hKeyboardHook, nCode, wParam, lParam);
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (kbh.vkCode == 91)  // 截获左win(开始菜单键) 
                {
                    return 1;
                }
                if (kbh.vkCode == 92)// 截获右win 
                {
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Control) //截获Ctrl+Esc 
                {
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.F4 && (int)Control.ModifierKeys == (int)Keys.Alt)  //截获alt+f4 
                {
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.Tab && (int)Control.ModifierKeys == (int)Keys.Alt) //截获alt+tab 
                {
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Shift) //截获Ctrl+Shift+Esc 
                {
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.Space && (int)Control.ModifierKeys == (int)Keys.Alt)  //截获alt+空格 
                {
                    return 1;
                }
                if (kbh.vkCode == 241)                  //截获F1 
                {
                    return 1;
                }
                if ((int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Alt + (int)Keys.Delete)      //截获Ctrl+Alt+Delete 
                {
                    return 1;
                }

            }
            return CallNextHookEx(this.hKeyboardHook, nCode, wParam, lParam);
        }

        private Keys GetDownKeys(Keys key)
        {
            Keys rtnKey = Keys.None;
            foreach (Keys keyTemp in preKeys)
            {
                switch (keyTemp)
                {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        rtnKey = rtnKey | Keys.Control;
                        break;
                    case Keys.LMenu:
                    case Keys.RMenu:
                        rtnKey = rtnKey | Keys.Alt;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        rtnKey = rtnKey | Keys.Shift;
                        break;
                    default:
                        break;


                }
            }
            rtnKey = rtnKey | key;
            return rtnKey;
        }

        private bool IsCtrlShiftAltKeys(Keys key)
        {
            switch (key)
            {
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region 锁定鼠标
        public bool MouseHookStart()
        {
            if (this.hMouseHook != 0)
            {
                return false;
            }
            this.MouseHookProcedure = new HookProc(this.MouseHookProc);
            this.hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, this.MouseHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            if (this.hMouseHook == 0)
            {
                this.KeyboardHookStop();
                return false;
            }
            return true;
        }
        public bool MouseHookStop()
        {
            bool flag = true;
            if (this.hMouseHook != 0)
            {
                flag = UnhookWindowsHookEx(this.hMouseHook);
                this.hMouseHook = 0;
            }
            if (!flag)
            {
                return false;
            }
            return true;
        }
        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            MouseHookStruct mbh = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));


            if ((nCode >= 0) && (lParam.ToInt32() > 0))
            {
               
                switch (wParam)
                {
                    case WM_MOUSELEFTDOWN:
                        if (MouseDown != null)
                        {
                            MouseDown.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, mbh.p.X, mbh.p.Y, 0));
                        }
                        break;
                    case WM_MOUSELEFTUP:
                        if (MouseUp != null)
                        {
                            MouseUp.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, mbh.p.X, mbh.p.Y, 0));
                        }
                        break;
                    case WM_MOUSERIGHTDOWN:
                        if (MouseDown != null)
                        {
                            MouseDown.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, mbh.p.X, mbh.p.Y, 0));
                        }

                        break;
                    case WM_MOUSERIGHTUP:
                        if (MouseUp != null)
                        {
                            MouseUp.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, mbh.p.X, mbh.p.Y, 0));
                        }

                        break;
                }
                if (wParam != WM_MOUSEMOVE)
                    return 1;
            }
            return CallNextHookEx(this.hMouseHook, nCode, wParam, lParam);
        }
        #endregion
    }
}
