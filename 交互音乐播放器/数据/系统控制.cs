using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace 交互音乐播放器.数据
{
    internal static class 系统控制
    {
        public static List<Task> 线程 = new List<Task>();
        private static List<string> 文件类型支援表 = new List<string>();
        public static string 脚本高亮文档 = "脚本资源\\语法定义.xshd";
        public static string 脚本数据文档 = "脚本资源\\脚本命令列表.txt";
        public static string 设置文档 = "设置.设置";
        public static int 最大浏览历史 = 10;
        public enum 支援的文件类型
        {
            ogg,
            mp3,
            wav,
            flac
        }
        static 系统控制()
        {
            初始化支援文件类型表();
          
        }
        public static void 初始化支援文件类型表()
        {
            foreach (string 文件类型名称 in Enum.GetNames(typeof(支援的文件类型)))
            {
                文件类型支援表.Add("." + 文件类型名称);
            }
        }
        public static bool 检查是否为支援类型(string 文件类型名)
        {
            return 文件类型支援表.Contains(文件类型名);
        }

        public static void 读取配置文件()
        {
            if (!File.Exists(设置文档))
            {
                File.WriteAllText(设置文档,"启始路径=音乐文件夹");
            }
            var 配置集 = File.ReadAllLines(设置文档, System.Text.Encoding.UTF8);
            foreach (var 配置 in 配置集)
            {
                var 配置内容 = 配置.Split('=');
                var 配置项 = 配置内容[0].Trim();
                var 配置值 = 配置内容[1].Trim();
                设定程序(配置项, 配置值);
            }
        }

        public static void 设定程序(string 配置项, string 配置值)
        {
            if (配置项 == "启始路径")
            {
                if (配置值 == "音乐文件夹") { return; }
                if (配置值 == "程序目录") { UI界面数据.初始浏览文件夹 = Environment.CurrentDirectory; }
                if (配置值 == "上级目录") 
                { 
                    UI界面数据.初始浏览文件夹 = Path.GetDirectoryName(Environment.CurrentDirectory);
                }
            }
        }



    }
}
