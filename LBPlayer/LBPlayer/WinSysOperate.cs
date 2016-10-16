using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LBPlayer
{
    public class WinSysOperate
    {
        //设置关机时的一些参数和加载API函数
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }


        [DllImport("kernel32.dll", ExactSpelling = true)]
        // GetCurrentProcess函数返回当前进程的一个句柄
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        // OpenProcessToken函数打开一个进程的访问代号
        public static extern bool OpenProcessToken(IntPtr ProcessHandles, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        // LookupPrivilegeValue函数获得本地唯一的标示符(LUID)，用于在特定的系统中 
        // 表示特定的优先权。 
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        // AdjustTokenPrivileges函数允许或者禁用指定访问记号的优先权。 
        // 允许或者禁用优先权需要TOKEN_ADJUST_PRIVILEGES访问权限。 
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TokPriv1Luid NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        // ExitWindowsEx函数可以注销，关机或者重新启动系统
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool ExitWindowsEx(int flg, int rea);

        private System.Threading.Timer timer;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        private const int EWX_LOGOFF = 0x00000000;     // 注销
        private const int EWX_SHUTDOWN = 0x00000001;   // 关机
        private const int EWX_REBOOT = 0x00000002;     // 重启
        private const int EWX_FORCE = 0x00000004;

        private static bool RebootCommand(int flg)
        {
            //give current process SeShutdownPrivilege
            TokPriv1Luid tp;

            IntPtr hproc = GetCurrentProcess();

            IntPtr htok = IntPtr.Zero;

            if (!OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
            {
                return false;
            }
            tp.Count = 1;

            tp.Luid = 0;

            tp.Attr = SE_PRIVILEGE_ENABLED;

            if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid))
            {
                return false;
            }

            if (!AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                return false;
            }

            //Exit windows
            if (!ExitWindowsEx(flg, 0))
            {
                return false;
            }
            return true;
        }

        public static bool SHUTDOWN()
        {
            return RebootCommand(EWX_SHUTDOWN + EWX_FORCE);
        }
        public static bool REBOOT()
        {
            return RebootCommand(EWX_REBOOT + EWX_FORCE);
        }
        public static bool LOGOFF()
        {
            return RebootCommand(EWX_LOGOFF + EWX_FORCE);
        }
    }
}
