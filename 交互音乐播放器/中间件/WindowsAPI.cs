using System;
using System.IO;
using System.Runtime.InteropServices;

namespace 交互音乐播放器.中间件
{
    /// <summary>
    /// 完成文件浏览器与系统的交互操作
    /// </summary>
    public static class 文件定位器
    {
        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(
        IntPtr hwnd,
        string lpOperation,
        string lpFile,
        string lpParameters,
        string lpDirectory,
        ShowCommands nShowCmd);
        private enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        /// <summary>
        /// 打开系统的文件资源管理器，然后定位到某个特定的文件或文件夹
        /// </summary>
        /// <param name="完整路径"></param>
        public static void 定位文件(string 完整路径)
        {
            if (Directory.Exists(完整路径))
            {
                ShellExecute(IntPtr.Zero, "open", "explorer.exe", 完整路径, "", ShowCommands.SW_NORMAL);
                return;
            }

            ShellExecute(IntPtr.Zero, "open", "explorer.exe", @"/e,/select," + 完整路径, "", ShowCommands.SW_NORMAL);

        }


    }
}
