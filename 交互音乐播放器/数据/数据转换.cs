using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using 交互音乐播放器.UI;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.数据;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据.播放中数据;
using 交互音乐播放器.音频逻辑;
using System.Windows;

namespace 交互音乐播放器.数据
{
    public static class 数据转换
    {
        public static double 取每毫秒bit数(int 缓冲区大小, WaveStream 音频流)
        {
            var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                    / 音频流.WaveFormat.BitsPerSample
                    / 音频流.WaveFormat.Channels * 8.0
                    / 音频流.WaveFormat.SampleRate;
            var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
            return 每毫秒bit;
        }

        public static double 取缓冲区大小(WaveStream 音频流)
        {
            var 缓冲区大小 = 音频流.WaveFormat.BlockAlign;
            return 缓冲区大小;
        }

        public static double 取缓冲区执行毫秒(int 缓冲区大小, WaveStream 音频流)
        {
            var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                    / 音频流.WaveFormat.BitsPerSample
                    / 音频流.WaveFormat.Channels * 8.0
                    / 音频流.WaveFormat.SampleRate;
            return 缓冲区时间_毫秒;
        }

        public static double 计算每拍字节(float BPM, TimeSpan 播放总时长, long 文件大小)
        {
            double 播放时长_毫秒 = 播放总时长.TotalMilliseconds;
            double 每毫秒字节 = 文件大小 / 播放时长_毫秒;
            double 每拍毫秒 = (double)60000 / (double)BPM;
            var 每拍比特 = (double)每拍毫秒 * (double)每毫秒字节;
            return 每拍比特;
        }

        public static int 计算每秒字节(double 缓冲区执行毫秒,int 缓冲区大小)
        {
            return (int)Math.Round((1000 / 缓冲区执行毫秒)) * 缓冲区大小;
        }

        public static double 取实时播放时间_毫秒(WaveOutEvent 播放器, WaveStream 音频流)
        {
            var 播放时间 = 播放器.GetPosition() * 1000.0 /
                                    音频流.WaveFormat.BitsPerSample /
                                    音频流.WaveFormat.Channels * 8.0 /
                                    音频流.WaveFormat.SampleRate;
            return 播放时间;
        }

        public static double 文本时间转目标字节(string 输入字符串, WaveStream 音频流)
        {
            var 时间格式 = 输入字符串.Split(':').Length;
            if (时间格式 == 1) { 输入字符串 = $"00:00:{输入字符串}"; }
            if (时间格式 == 2) { 输入字符串 = $"00:{输入字符串}"; }
            TimeSpan 目标原时间 = TimeSpan.FromSeconds(0);
            if (!TimeSpan.TryParse(输入字符串, out 目标原时间)) { MessageBox.Show($"无法将脚本中的时间（{输入字符串}）转换为目标时间，请检查时间格式"); return 音频流.Length; }

            var 目标时间 = 目标原时间.TotalSeconds;
            var 目标点 = 目标时间  * 音频流.WaveFormat.BitsPerSample * 音频流.WaveFormat.Channels / 8.0
                                    * 音频流.WaveFormat.SampleRate;
            return (int)目标点;
        }

        public static double 时间转目标字节(TimeSpan 跳转时间, WaveStream 音频流)
        {
            var 目标时间 = 跳转时间.TotalSeconds;
            var 目标点 = 目标时间 / 100 * 音频流.WaveFormat.BitsPerSample * 音频流.WaveFormat.Channels * 8.0 
                                    * 音频流.WaveFormat.SampleRate;
            return (int)目标点;
        }

        public static int 数字时间转目标字节(double 时间, WaveStream 音频流)
        {
            bool 是负值 = false;
            if (时间 < 0) { 是负值 = true; }
            var 目标时间 = Math.Abs(时间);
            var 目标点 = 目标时间 * 音频流.WaveFormat.BitsPerSample * 音频流.WaveFormat.Channels / 8.0
                                    * 音频流.WaveFormat.SampleRate;
            if (是负值) { 目标点 = -目标点; }
            return (int)目标点;
        }

        public static int 文本节拍数转字节(string 文本, int 每小节拍数, int 偏移)
        {
            var 分隔 = 文本.Split('.'); int 小节 = -1; int 拍 = -1;
            if (!(int.TryParse(分隔[0], out 小节) && int.TryParse(分隔[1], out 拍)))
            {
                Debug.Print("转换失败，节拍数输入不正确");
            }
            return ((小节 - 1) * 每小节拍数 + 拍) + 偏移;
        }

        public static int 文本节拍数转字节(string 文本, string 段落名称, 音乐播放数据 脚本)
        {
            var 分隔 = 文本.Split('.'); int 小节 = -1; int 拍 = -1;
            if (!(int.TryParse(分隔[0], out 小节) && int.TryParse(分隔[1], out 拍)))
            {
                Debug.Print("转换失败，节拍数输入不正确");
            }
            int 段落号 = 脚本.段落组[段落名称].编号;
            int 每小节拍数 = 脚本.段落组[段落名称].节拍信息_读取.每小节拍数;
            if (音频控制器.当前控制器 == null || 音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用( 音乐播放数据.播放数据组.文件组,段落名称)) 
            { Debug.Print("转换失败，文件未加载");return -1; }
            var 总拍数 = ((小节 - 1) * 每小节拍数 + 拍);
            var 总字节 = 总拍数* 脚本.段落组[段落名称].节拍信息_读取.每拍字节;
            if (脚本.段落组[段落名称].绑定文件 == null) { Debug.Print("读取位置转总拍数失败，指定的段落未绑定文件"); return -1; }
            int 偏移 = 数字时间转目标字节(脚本.段落组[段落名称].Offset, 音频控制器.当前控制器.已载文件组[脚本.段落组[段落名称].绑定文件!.别名]);
            return (int)(总字节 + 偏移);
        }

        public static int 节拍数转字节(int 小节, int 拍,string 段落名称, 音乐播放数据 脚本)
        {
            if (音频控制器.当前控制器 == null || 音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.文件组, 段落名称))
            { Debug.Print("转换失败，文件未加载"); return -1; }
            int 段落号 = 脚本.段落组[段落名称].编号;
            int 每小节拍数 = 脚本.段落组[段落名称].节拍信息_读取.每小节拍数;
            if (音频控制器.当前控制器 == null && 音频控制器.当前控制器!.已载文件组.Count-1 < 段落号) { Debug.Print("转换失败，文件未加载"); }
            if (脚本.段落组[段落名称].绑定文件 == null) { Debug.Print("读取位置转总拍数失败，指定的段落未绑定文件"); return -1; }
            int 偏移 = 数字时间转目标字节(脚本.段落组[段落名称].Offset, 音频控制器.当前控制器.已载文件组[脚本.段落组[段落名称].绑定文件!.别名]);
            return ((小节 - 1) * 每小节拍数 + 拍) + 偏移;
        }

        public static int 节拍数转字节(int 小节, int 拍, int 每小节拍数 ,double 每拍字节, int 偏移)
        {
           
            var 总拍数 = ((小节 - 1) * 每小节拍数 + 拍);
            var 总字节 = 总拍数 * 每拍字节;
            return (int)(总字节 + 偏移);
        }

        public static int 读取位置转总拍数(string 段落名称, int 流位置 ,int 偏移 , 音乐播放数据 脚本)
        {
            if (音频控制器.当前控制器 == null ||  音频控制中间件.播放数据 == null ||音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.文件组, 段落名称))
            { Debug.Print("转换失败，文件未加载"); return -1; }
            int 段落号 = 脚本.段落组[段落名称].编号;
            if (脚本.段落组[段落名称].绑定文件 == null) { Debug.Print("读取位置转总拍数失败，指定的段落未绑定文件"); return -1; }
            var 当前文件 = 音频控制器.当前控制器.已载文件组[脚本.段落组[段落名称].绑定文件!.别名];
            var 总拍数 = (int)((流位置 + 数据转换.数字时间转目标字节(脚本.段落组[段落名称].Offset, 当前文件)) / 脚本.段落组[段落名称].节拍信息_读取.每拍字节);
            return 总拍数;
        }

        public static int 读取段落总拍数(string 段落名称, 音乐播放数据 脚本)
        {
            if (音频控制器.当前控制器 == null || 音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.文件组, 段落名称))
            { Debug.Print("转换失败，文件未加载"); return -1; }
            int 段落号 = 脚本.段落组[段落名称].编号;
            if (脚本.段落组[段落名称].绑定文件 == null) { Debug.Print("读取位置转总拍数失败，指定的段落未绑定文件"); return -1; }
            var 当前文件 = 音频控制器.当前控制器.已载文件组[脚本.段落组[段落名称].绑定文件!.别名];
            var 总拍数 = (int)((当前文件.Length + 数据转换.数字时间转目标字节(脚本.段落组[段落名称].Offset, 当前文件)) / 脚本.段落组[段落名称].节拍信息_读取.每拍字节);
            return 总拍数;
        }

        public static (int 小节 , int 拍 , int 总拍数,int 小节最大拍号,double 每拍字节, int 偏移) 读取段落总节拍信息(string 段落名称, 音乐播放数据 脚本)
        {
            int 段落号 = 脚本.段落组[段落名称].编号;
            if (音频控制器.当前控制器 == null ||  音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.文件组, 段落名称))
            { Debug.Print("转换失败，文件未加载"); return (-1,-1,-1,-1,-1,-1); }
            if (脚本.段落组[段落名称].绑定文件 == null) { Debug.Print("读取位置转总拍数失败，指定的段落未绑定文件"); return (-1, -1, -1, -1, -1, -1); }
            var 当前文件 = 音频控制器.当前控制器.已载文件组[脚本.段落组[段落名称].绑定文件!.别名];
            var 偏移字节 = 数据转换.数字时间转目标字节(脚本.段落组[段落名称].Offset,当前文件);
            var 总拍数 = (int)((当前文件.Length + 偏移字节) / 脚本.段落组[段落名称].节拍信息_读取.每拍字节); 
            var 小节 = 总拍数 / 脚本.段落组[段落名称].节拍信息_读取.每小节拍数 + 1;
            var 拍 = 总拍数 % 脚本.段落组[段落名称].节拍信息_读取.每小节拍数 + 1;
            return (小节,拍,总拍数, 脚本.段落组[段落名称].节拍信息_读取.每小节拍数, 脚本.段落组[段落名称].节拍信息_读取.每拍字节, 偏移字节);
        }

        public static 段落信息.Cue数据? 取最近Cue点(List<段落信息.Cue数据> CUE点集 , 音乐播放数据 配置)
        {
            if (配置.空值或空引用(音乐播放数据.播放数据组.当前段落)) { return null; }
            long 当前播放位置 = 配置.当前段落!.播放位置.已播放字节;
            int 最近的Cue点下标 = int.MaxValue;
            long 当前最小偏移值 = long.MaxValue;
            for (int 当前Cue点下标 = 0; 当前Cue点下标 < CUE点集.Count; 当前Cue点下标++)
            {
                当前播放位置 = 配置.当前段落!.播放位置.已播放字节;
                var 偏移值 = CUE点集[当前Cue点下标].出点位置 - 当前播放位置;
                if (偏移值 < 0) { continue; }
                if (偏移值 == -1) { 最近的Cue点下标 = 当前Cue点下标; 当前最小偏移值 = 偏移值; }
                if (偏移值 < 当前最小偏移值) { 最近的Cue点下标 = 当前Cue点下标; 当前最小偏移值 = 偏移值; }
            }
            return CUE点集[最近的Cue点下标];
        }
        

    }
}
