using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据.播放中数据;

namespace 交互音乐播放器.数据
{
    public static class UI界面数据 
    {

        static public bool 程序执行 { get; set; } = true;
        static public int UI动态更新时间 { get; set; } = 10;
        static public string 初始浏览文件夹 { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        static public List<string> 历史路径 { get; set; } = new List<string>();
        static public int 历史路径指针 { get; set; } = 0;
        static public string 当前浏览文件夹 { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        static public double 播放进度条 { get; set; } = 0;
        static public TimeSpan 当前播放时间 { get; set; } = TimeSpan.FromMilliseconds(0);
        static public TimeSpan 总播放时间 { get; set; } = TimeSpan.FromMilliseconds(0);
        static public float BPM { get; set; } = 60;
        static public int 当前小节 { get; set; } = 4;
        static public int 当前拍 { get; set; } = 4;
        static public string 曲名 { get; set; } = "";
        static public string 唯一ID { get; set; } = "";
        static public string 上次更新唯一ID { get; set; } = "";
        static public string 作者 { get; set; } = "";
        static public int 音量大小 { get; set; } = 100;
        static public int 暂存音量大小 { get; set; } = 100;
        static public ImageBrush? 专辑图片 { get; set; }
        static public string? 默认按钮名称 { get; set; }
        static public bool 默认按钮状态 { get; set; } = true;
        static public string? 默认按钮符号 { get; set; }
        static public SolidColorBrush? 默认按钮颜色 { get; set; }


        public enum 显示状态
        {
            启用,
            关闭
        }
        public static void 更新界面数据(主界面 UI)
        {
            if (音频控制中间件.播放数据 == null) { return; }
            var 更新段落 = 音频控制中间件.播放数据.当前流!.当前段落;
            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前流!.当前段落 != null &&
                    更新段落!.状态 == 段落信息.播放状态.播放中)
            {
                当前播放时间 = 更新段落.播放位置.当前时间;
                总播放时间 = 更新段落.播放位置.总时间;
                播放进度条 = (double)(更新段落.播放位置.已播放字节 / (double)更新段落.播放位置.总字节) * 100;
                当前小节 = 更新段落.节拍信息_显示.小节;
                当前拍 = 更新段落.节拍信息_显示.拍;
                if (音频控制中间件.脚本数据 == null) { Debug.Print("更新界面数据不完整，在中间件中没有绑定脚本数据"); return; }
                作者 = 音频控制中间件.脚本数据.作者!;
                曲名 = 音频控制中间件.脚本数据.名称!;
                更新按钮图标();
                if (音频控制中间件.当前中间件 == null) { Debug.Print("更新界面数据不完整，当前中间件没有实例"); return; }
                专辑图片 = 音频控制中间件.脚本数据.专辑图片;
                唯一ID = 作者 + 曲名;
                音频控制中间件.当前中间件.补例_更新播放内容();

            }
        }

        public static bool 是否已经更换过文件()
        {
            if (上次更新唯一ID == 唯一ID)
            {
                return false;
            }
            上次更新唯一ID = 唯一ID;
            return true;
        }

        private static void 更新默认图标样式(string 当前段落名称)
        {
            默认按钮名称 = 当前段落名称;
            默认按钮符号 = UI管理器.图标[UI管理器.图标集.正常播放图标];
            默认按钮状态 = true;
            默认按钮颜色 = UI管理器.色彩[UI管理器.色彩集.音乐播放正常颜色];
            return;
        }
        public static void 更新按钮图标()
        {
            if (默认按钮状态 == false) { return; }
            var 脚本 = 音频控制中间件.播放数据;
            if (脚本 == null) { Debug.Print("更新按钮图标失败，音频中间件中的播放数据没有实例"); return; }
            if (脚本.空值或空引用(音乐播放数据.播放数据组.当前段落)) { 更新默认图标样式("无脚本"); return; }
            if (脚本.当前段落!.别名 == null) { 更新默认图标样式("无脚本"); return; }
            if (脚本.空值或空引用(音乐播放数据.播放数据组.当前段落)) { return; }
            string 当前段落名称 = 脚本.当前段落.别名;
            if (计划.全局按钮集 == null || 计划.全局按钮集.Count == 0)
            {
                更新默认图标样式(当前段落名称); return;
            }
            if (计划.全局按钮集.Count > 0 && 计划.全局默认按钮 == null)
            { 计划.全局默认按钮 = 计划.全局按钮集.FirstOrDefault().Value; }
            var 默认按钮 = 计划.全局默认按钮;
            if (默认按钮 == null||!默认按钮.允许显示)
            {
                更新默认图标样式(当前段落名称);
                return;
            }
            默认按钮名称 = 默认按钮.按钮显示名称;
            if (默认按钮.按钮类型 == 计划.按钮数据._按钮类型.正常进行)
            { 默认按钮符号 = UI管理器.图标[UI管理器.图标集.正常播放图标]; 默认按钮颜色 = UI管理器.色彩[UI管理器.色彩集.音乐播放正常颜色]; }
            if (默认按钮.按钮类型 == 计划.按钮数据._按钮类型.无限循环)
            { 默认按钮符号 = UI管理器.图标[UI管理器.图标集.循环图标]; 默认按钮颜色 = UI管理器.色彩[UI管理器.色彩集.音乐循环颜色]; }
            if (默认按钮.按钮类型 == 计划.按钮数据._按钮类型.等待操作)
            { 默认按钮符号 = UI管理器.图标[UI管理器.图标集.等待图标]; 默认按钮颜色 = UI管理器.色彩[UI管理器.色彩集.音乐等待颜色]; }
            if (默认按钮.按钮类型 == 计划.按钮数据._按钮类型.结束)
            { 默认按钮符号 = UI管理器.图标[UI管理器.图标集.正常播放图标]; 默认按钮颜色 = UI管理器.色彩[UI管理器.色彩集.音乐结束颜色]; }

            默认按钮状态 = true;
        }
    }
    public class UI管理器
    {
        private static Dictionary<程序页, Page> 子页面 = new Dictionary<程序页, Page>();
        public static Dictionary<色彩集, SolidColorBrush> 色彩 = new Dictionary<色彩集, SolidColorBrush>();
        public static Dictionary<图标集, string> 图标 = new Dictionary<图标集, string>();
        private 程序页 当前运行页;
        public enum 程序页
        {
            文件管理器,
            脚本编辑器,
            实时管理器
        }
        public enum 色彩集
        {
            文件管理器背景色,
            文件管理器图标色,
            文件管理器边框色,
            实时跳转管理器背景色,
            实时跳转管理器图标色,
            实时跳转管理器边框色,
            脚本编辑器背景色,
            脚本编辑器图标色,
            脚本编辑器边框色,
            音乐播放正常颜色,
            音乐等待颜色,
            音乐循环颜色,
            音乐结束颜色
        }
        public enum 图标集
        {
            文件管理器图标,
            实时跳转管理器图标,
            脚本编辑器图标,
            正常播放图标,
            循环图标,
            等待图标
        }
        public UI管理器()
        {
            初始化子页面();
            初始化颜色();
            初始化图标();

        }
        private void 初始化颜色()
        {
            色彩.Add(色彩集.文件管理器图标色, new SolidColorBrush(Color.FromArgb(112, 255, 255, 255)));
            色彩.Add(色彩集.实时跳转管理器图标色, new SolidColorBrush(Color.FromArgb(112, 255, 255, 255)));
            色彩.Add(色彩集.脚本编辑器图标色, new SolidColorBrush(Color.FromArgb(112, 255, 255, 255)));

            色彩.Add(色彩集.文件管理器背景色, new SolidColorBrush(Color.FromArgb(255, 182, 148, 44)));
            色彩.Add(色彩集.脚本编辑器背景色, new SolidColorBrush(Color.FromArgb(255, 46, 117, 182)));
            色彩.Add(色彩集.实时跳转管理器背景色, new SolidColorBrush(Color.FromArgb(255, 112, 48, 160)));

            色彩.Add(色彩集.文件管理器边框色, new SolidColorBrush(Color.FromArgb(140, 255, 217, 102)));
            色彩.Add(色彩集.脚本编辑器边框色, new SolidColorBrush(Color.FromArgb(140, 157, 195, 230)));
            色彩.Add(色彩集.实时跳转管理器边框色, new SolidColorBrush(Color.FromArgb(140, 168, 121, 237)));
            色彩.Add(色彩集.音乐播放正常颜色, new SolidColorBrush(Color.FromArgb(140, 68, 169, 81)));
            色彩.Add(色彩集.音乐等待颜色, new SolidColorBrush(Color.FromArgb(140, 231, 211, 129)));
            色彩.Add(色彩集.音乐结束颜色, new SolidColorBrush(Color.FromArgb(140, 154, 64, 64)));
            色彩.Add(色彩集.音乐循环颜色, new SolidColorBrush(Color.FromArgb(140, 115, 81, 196)));
        }
        private void 初始化图标()
        {
            图标.Add(图标集.文件管理器图标, "\ue9a9");
            图标.Add(图标集.实时跳转管理器图标, "\ue9d3");
            图标.Add(图标集.脚本编辑器图标, "\ue9b1");
            图标.Add(图标集.正常播放图标, "\ue91f");
            图标.Add(图标集.等待图标, "\ue951");
            图标.Add(图标集.循环图标, "\ue972");
        }
        private void 初始化子页面()
        {
            if (子页面.Count > 0) { return; }
            子页面.Add(程序页.文件管理器, new UI.文件管理器());
            子页面.Add(程序页.脚本编辑器, new UI.脚本编辑());
            子页面.Add(程序页.实时管理器, new UI.实时管理器());
        }

        public void 设置当前页面(程序页 页面)
        {
            主界面.当前界面!.内容页.Navigate(子页面[页面]);
            当前运行页 = 页面;
        }
    }
}
