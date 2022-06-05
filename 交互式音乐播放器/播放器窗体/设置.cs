using System;
using System.Collections.Generic;

namespace 交互式音乐播放器.设置类
{
    /// <summary>
    /// 管理软件的主要设定
    /// </summary>
    internal class 设置
    {
        /// <summary>
        /// 设置软件启动后默认导航到的位置，默认为“音乐”文件夹
        /// </summary>
        public static string 默认启动位置 { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        public static int UI更新时间_粗略 { get; set; } = 400;
        public static int UI更新时间_精准 { get; set; } = 100;
        public static int UI更新时间_实时 { get; set; } = 100;
        public static int 默认BPM { get; set; } = 60;
        public static int 默认节拍 { get; set; } = 4;
        public static bool 日志开关 { get; set; } = true;
        public static 语言.音频类型.循环类型 循环模式 = 语言.音频类型.循环类型.不可循环;

    }

    /// <summary>
    /// 管理软件运行时的设定
    /// </summary>
    internal class 运行时
    {
        /// <summary>
        /// 设置应用程序主界面当前访问的文件夹路径
        /// </summary>
        public string 当前文件夹路径 { get; set; } = "";
        /// <summary>
        /// 应用程序已经访问过的路径集
        /// </summary>
        public List<string> 访问过的文件夹路径 { get; set; } = new List<string>();
        /// <summary>
        /// Mp3未匹配到封面，需要执行二次匹配的开关
        /// </summary>
        public static bool 本Mp3未匹配到封面 { get; set; } = false;
        /// <summary>
        /// 子提示窗的状态
        /// </summary>
        public static bool 提示窗打开 { get; set; } = false;
        public static bool 进度条拉动 { get; set; } = false;
        public static int 音量 { get; set; } = 100;
    }

    internal class 日志
    {
        /// <summary>
        /// 向系统输出一个日志
        /// </summary>
        /// <param name="日志"></param>
        public static void 输出(object 日志)
        {
            if (设置.日志开关 == false) { return; }

            Console.WriteLine($"{DateTime.Now}|{日志.ToString()}");
        }

    }
}
