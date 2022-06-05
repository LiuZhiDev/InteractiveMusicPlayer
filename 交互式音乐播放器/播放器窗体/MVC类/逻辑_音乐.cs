using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;

namespace 交互式音乐播放器.流
{
    /// 该命名空间为存放音频播放的架构
    /// 可以把所有关于音频播放的主要实现类储存到该命名空间下
    namespace 高低切换
    {
        public class 高低切换式控制
        {
            public bool 正在播放 = false;
            public static int 已完成实例 = 0;
            public static int 当前章节 = 0;
            public int 切换至章节 = -1;
            public static Dictionary<int, WaveOutEvent> 播放器组 = new Dictionary<int, WaveOutEvent>();
            public static Dictionary<int, 高低切换式> 实例组 = new Dictionary<int, 高低切换式>();
            List<Thread> 线程组 = new List<Thread>();
            enum 自动切换状态
            {
                从前到后,
                从后到前,
                不可切换
            }
            enum 边界激活状态
            {
                最后一项,
                第一项
            }
            object 线程锁 = new object();
            static 自动切换状态 切换状态 { get; set; }
            static 边界激活状态 边界状态 { get; set; }
            public 高低切换式控制(List<string> 播放列表, bool LOOP模式 = true)
            {
                //初始化静态变量
                已完成实例 = 0;
                当前章节 = 0;
                播放器组 = new Dictionary<int, WaveOutEvent>();
                实例组 = new Dictionary<int, 高低切换式>();
                切换状态 = 自动切换状态.从前到后;
                边界状态 = 边界激活状态.第一项;
                //构造初始化线程
                foreach (string 文件路径 in 播放列表)
                {
                    Thread 线程 = new Thread(() =>
                    {
                        初始化音乐(播放列表.IndexOf(文件路径), 文件路径, LOOP模式);
                    });
                    线程组.Add(线程);
                }
                //执行初始化线程
                foreach (var 线程 in 线程组)
                {
                    线程.Start();
                }
                //执行播放线程
                Console.WriteLine($"等待{已完成实例} = {播放列表.Count}");
                while (已完成实例 != 播放列表.Count)
                {
                    Console.WriteLine($"{已完成实例} = {播放列表.Count}");
                    Thread.Sleep(200);
                }
                Console.WriteLine($"等待{已完成实例} = {播放列表.Count}");
                for (int i = 0; i < 播放器组.Count; i++)
                {
                    播放器组[i].Play();
                }

            }

            private void 初始化音乐(int 加入下标, string 文件路径, bool LOOP模式 = true)
            {
                lock (线程锁)
                {
                    NAudio.Vorbis.VorbisWaveReader OGG音频 = new NAudio.Vorbis.VorbisWaveReader(文件路径);
                    WaveOutEvent 播放器 = new WaveOutEvent();
                    高低切换式 实例 = new 高低切换式(OGG音频, 播放器);
                    高低切换式.运行状态.Loop模式 = LOOP模式;
                    播放器.Init(实例);
                    实例组.Add(加入下标, 实例);
                    播放器组.Add(加入下标, 播放器);
                    if (加入下标 != 0) { 实例.设定静音 = true; }
                    已完成实例++;
                }

            }

            public static void 高低切换(int 待切换章节)
            {
                #region 判断操作是否合法
                if (实例组.Count == 0)
                {
                    return;
                }
                if (待切换章节 > 实例组.Count - 1)
                {
                    Console.WriteLine("要切换的章节编号大于现有章节数");
                    return;
                }
                //如果在当前章节则不切换代码暂未实现
                #endregion
                实例组[待切换章节].设定静音 = false;
                var 当前章节状态 = 实例组[当前章节].淡入淡出设定;
                var 待切换章节状态 = 实例组[待切换章节].淡入淡出设定;
                实例组[当前章节].已淡入淡出段数 = 1;
                实例组[待切换章节].已淡入淡出段数 = 1;

                if (当前章节状态 == 高低切换式.运行状态.淡入淡出状态.不启动)
                {
                    实例组[当前章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡出;
                    实例组[待切换章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡入;
                }
                if (当前章节状态 == 高低切换式.运行状态.淡入淡出状态.启动淡入)
                {
                    实例组[当前章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡出;
                    实例组[待切换章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡入;
                }
                if (当前章节状态 == 高低切换式.运行状态.淡入淡出状态.启动淡出)
                {
                    实例组[当前章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡入;
                    实例组[待切换章节].淡入淡出设定 = 高低切换式.运行状态.淡入淡出状态.启动淡出;
                }
                /* 只打开设定组声音
                foreach (var 实例 in 实例组)
                {
                    if (实例.Key != 切换到章节) { 实例.Value.设定静音 = true; }
                    else { 实例.Value.设定静音 = false; }
                }
                */
                当前章节 = 待切换章节;
            }
            public static void 自动高低切换(int 章节号 = -1)
            {
                #region 状态判定
                //判断边界状态
                if (当前章节 == 已完成实例 - 1)
                {
                    边界状态 = 边界激活状态.最后一项;
                }
                if (当前章节 == 0)
                {
                    边界状态 = 边界激活状态.第一项;
                }
                //判断当前切换状态
                //已是最后一个
                if (当前章节 == 已完成实例 - 1)
                {
                    切换状态 = 自动切换状态.从后到前;
                }
                //如果是第一个
                if (当前章节 == 0)
                {
                    切换状态 = 自动切换状态.从前到后;
                }
                //如果是中间
                if (边界状态 == 边界激活状态.第一项 && 当前章节 != 0 && 当前章节 != 已完成实例 - 1)
                {
                    切换状态 = 自动切换状态.从前到后;
                }
                if (边界状态 == 边界激活状态.最后一项 && 当前章节 != 0 && 当前章节 != 已完成实例 - 1)
                {
                    切换状态 = 自动切换状态.从后到前;
                }
                #endregion
                #region 执行
                int 应切换章节 = 当前章节;
                if (切换状态 == 自动切换状态.从前到后)
                {
                    应切换章节++;
                }
                if (切换状态 == 自动切换状态.从后到前)
                {
                    应切换章节--;
                }
                高低切换(应切换章节);
                

                #endregion

            }
            public void 音量修改(double 音量值)
            {
                float 音量 = (float)(音量值 / 100f);
                播放器组[0].Volume = 音量;
            }
            public void 全部停止()
            {
                高低切换式.运行状态.状态 = 高低切换式.运行状态.播放状态.停止;
                实例组.Clear();
                foreach (var 播放器 in 播放器组)
                {
                    播放器.Value.Dispose();
                }
                播放器组.Clear();

            }
        }
        public class 高低切换式 : IWaveProvider
        {
            #region 状态引用
            public VorbisWaveReader OGG读取器;
            public WaveOutEvent 播放器;
            private static float 当前音量 = 1;
            public 运行状态.淡入淡出状态 淡入淡出设定 { get; set; } = 运行状态.淡入淡出状态.不启动;
            public long 播放字节位置 { get; set; } = 0;
            public long 总字节位置 { get; set; } = 0;
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 总时长 { get; set; }

            private static bool 正在淡出 = false;
            private static bool 已淡出 = false;
            public float 已淡入淡出段数 = 1;
            public bool 设定静音 = false;
            public int 循环次数 = 0;
            public class 运行状态
            {

                public static bool Loop模式 = true;
                public static bool 自动淡入淡出 = false;
                public static float 淡出段数 = 10;
                public static 播放状态 状态 = 播放状态.播放;
                public enum 淡入淡出状态
                {
                    启动淡出,
                    启动淡入,
                    不启动
                }
                public enum 播放状态
                {
                    暂停,
                    播放,
                    停止
                }
            }
            #endregion
            #region 构造与析构函数
            public 高低切换式(VorbisWaveReader 传入MP3读取器, WaveOutEvent 传入播放器)
            {
                OGG读取器 = 传入MP3读取器;
                播放器 = 传入播放器;
                WaveFormat = OGG读取器.WaveFormat;
                Thread 外部处理 = new Thread(外部状态处理);
                外部处理.Start();
            }
            ~高低切换式()
            {
                Console.WriteLine("播放类已析构");
            }
            #endregion
            #region 接口必要函数
            /// <summary>
            ///  接口必须实现的函数，当播放器init后，NAudio将自动调用此方法来获取波形数据，当读入0时将停止
            ///  该接口中需要对一个读取器进行递归调用Read获取数据
            /// </summary>
            /// <param name="缓冲区">NAudio内部的缓冲区</param>
            /// <param name="起始位置">NAudio中设定音频播放的起始位置，需要输入字节数</param>
            /// <param name="读取大小">指示要将  缓冲区  从  起始位置  读取多大的音频，最大  不可超过  缓冲区  的  最大长度</param>
            /// <returns>返回此次读取的波形大小</returns>
            public int Read(byte[] 缓冲区, int 起始位置, int 读取大小)
            {

                int 实际读取字节 = 0; //实际读取字节数
                                //循环逻辑，若文件剩余大小已不足以完成本次读取，则把读取器的位置设为0
                if (运行状态.Loop模式 == true && OGG读取器.Length - OGG读取器.Position <= 读取大小 - 实际读取字节)
                {
                    实际读取字节 += OGG读取器.Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器.Length - OGG读取器.Position));
                    OGG读取器.Position = 0;
                    循环次数++;
                }
                实际读取字节 += OGG读取器.Read(缓冲区, 起始位置 + 实际读取字节, 读取大小 - 实际读取字节);
                特效应用(缓冲区, 起始位置, 读取大小);
                更新外部信息();
                return 实际读取字节;

            }


            /// <summary>
            /// 接口必须实现的函数，获取当前播放流的类型
            /// 修改此处或许决定播放的文件
            /// 请设置为其读取器的.WaveFormat
            /// </summary>
            public WaveFormat WaveFormat { get; private set; }
            #endregion
            #region 外部交互逻辑
            public void 特效应用(byte[] 缓冲区, int 起始位置, int 读取大小)
            {
                if (设定静音)
                {
                    静音(缓冲区, 起始位置, 读取大小);
                }
                if (运行状态.自动淡入淡出)
                {
                    Console.WriteLine("准备淡出");
                    淡出流(缓冲区, 起始位置, 读取大小);

                }
                if (淡入淡出设定 == 运行状态.淡入淡出状态.启动淡出)
                {
                    Console.WriteLine("启动手动淡出");
                    淡出流(缓冲区, 起始位置, 读取大小);
                }

                if (淡入淡出设定 == 运行状态.淡入淡出状态.启动淡入)
                {
                    Console.WriteLine("启动手动淡出");
                    淡入流(缓冲区, 起始位置, 读取大小);
                }

            }
            public int 播放暂停()
            {
                if (运行状态.状态 == 运行状态.播放状态.停止)
                {
                    播放器.Stop();
                    播放器.Dispose();
                    OGG读取器.Dispose();
                    return 0;
                }
                if (运行状态.状态 == 运行状态.播放状态.暂停)
                {
                    播放器.Pause();
                    return 1;
                }
                if (运行状态.状态 == 运行状态.播放状态.播放)
                {
                    播放器.Play();
                    return 1;
                }
                return 1;
            }
            public void 外部状态处理()
            {
                while (播放器.PlaybackState == PlaybackState.Stopped)
                {

                }
                while (播放器.PlaybackState != PlaybackState.Stopped)
                {
                    播放暂停();
                    Thread.Sleep(100);
                }
                Console.WriteLine("外部状态处理结束");
                高低切换式.运行状态.状态 = 高低切换式.运行状态.播放状态.播放;
            }
            public void 更新外部信息()
            {
                播放字节位置 = 播放器.GetPosition();
                总字节位置 = OGG读取器.Length;
                当前时长 = OGG读取器.CurrentTime;
                总时长 = OGG读取器.TotalTime;

            }
            #endregion
            #region 特效函数
            public void 静音(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = 0;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡出流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)1 - (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 >= 1)
                                {

                                    音量倍率 = 1f;
                                    已淡出 = true;


                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            /*
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点, float 拍数)
            {


                if (信息.已在淡入)
                {
                    //  Console.WriteLine("淡入被拒绝");
                    return;
                }
                unsafe
                {
                    信息.已在淡入 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / 信息.每采样字节 * 2;

                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)信息.淡入_字节修改量 / ((float)信息.每拍Bit * (float)0.25 * 拍数);

                                if (音量倍率 >= 1)
                                {
                                    //需添加 音频类型.音频状态.满音量
                                    信息.音频状态 = 语言.音频状态.播放状态.播放中;
                                    设置类.日志.输出("若在此处发生Bug，请恢复代码");
                                    信息.当前UI绑定线程 = 信息.线程号;
                                    信息.已在淡入 = false;
                                    return;
                                }

                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;
                                信息.淡入_字节修改量 += 2;

                            }
                        }
                    }

                }
                信息.已在淡入 = false;
            }
            */
            #endregion

        }

    }
    namespace 永续
    {
        public class 永续控制
        {
            Dictionary<int, VorbisWaveReader> OGG读取器组 = new Dictionary<int, VorbisWaveReader>();
            public static WaveOutEvent 播放器;
            public static 永续式 永续流;
            /// <summary>
            /// 储存永续模式播放信息的类
            /// </summary>
            public class 信息
            {
                public int 缓冲区大小 { get; set; }
                public double 当前播放bit_实时 { get; set; }
                public double 当前播放时间_实时 { get; set; }
                public double 每拍毫秒 { get; set; }
                public int 每拍毫秒_去除小数 { get; set; }
                public double 缓冲区时间_毫秒 { get; set; }
                public double 每毫秒bit { get; set; }
                public double 每拍Bit { get; set; }
                public int 当前拍 { get; set; }
                public int 当前小节 { get; set; }
                public int 当前拍数 { get; set; }
                public double 下一拍的字节 { get; set; }
                public TimeSpan 当前时长 { get; set; }
                public TimeSpan 持续时长 { get; set; }
                public int 循环次数 { get; set; }
            }
            object 线程锁 = new object();

            /// <summary>
            /// 启动一个永续模式播放的音乐实例
            /// </summary>
            /// <param name="播放列表">用列表储存的一个或多个文件名</param>
            /// <param name="LOOP模式">是否使用Loop永续循环</param>
            public 永续控制(List<string> 播放列表, bool LOOP模式 = true)
            {
                //初始化流
                foreach (string 文件路径 in 播放列表)
                {
                    初始化音乐(播放列表.IndexOf(文件路径), 文件路径, LOOP模式);
                }
                //建立播放器并开始播放
                播放器 = new WaveOutEvent();
                永续流 = new 永续式(OGG读取器组, 播放器, this);
                永续式.运行状态.Loop模式 = LOOP模式;
                播放器.Init(永续流);
                播放器.Play();

            }
            /// <summary>
            /// 初始化OGG音乐
            /// </summary>
            /// <param name="加入下标">输入要加入的音乐ID，将作为该音乐播放的唯一识别号</param>
            /// <param name="文件路径">音乐的完整文件路径</param>
            /// <param name="LOOP模式">是否使用Loop循环播放，如果选择不使用，将在循环最后一个文件后结束</param>
            private void 初始化音乐(int 加入下标, string 文件路径, bool LOOP模式 = true)
            {
                lock (线程锁)
                {
                    NAudio.Vorbis.VorbisWaveReader OGG音频 = new NAudio.Vorbis.VorbisWaveReader(文件路径);
                    OGG读取器组.Add(加入下标, OGG音频);
                }

            }

            /// <summary>
            /// 传入一个内部信息类，获取实时播放的信息
            /// </summary>
            /// <param name="状态">内部的信息类</param>
            /// <param name="BPM">该音乐的BPM信息，默认为60</param>
            /// <param name="章节拍数">该音乐的拍数信息，默认为4</param>
            public void 更新信息(信息 状态, int BPM = 60, int 章节拍数 = 4)
            {
                状态.缓冲区大小 = OGG读取器组[永续流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    OGG读取器组[永续流.当前播放章节].WaveFormat.BitsPerSample /
                                    OGG读取器组[永续流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    OGG读取器组[永续流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / OGG读取器组[永续流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[永续流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[永续流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)章节拍数);
                状态.当前拍数 = 状态.当前拍 % 章节拍数;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = OGG读取器组[永续流.当前播放章节].CurrentTime;
                状态.持续时长 = OGG读取器组[永续流.当前播放章节].TotalTime;
                状态.循环次数 = 永续流.循环次数;
            }

        }
        public class 永续式 : IWaveProvider
        {
            #region 状态引用
            public Dictionary<int, VorbisWaveReader> OGG读取器组;
            WaveOutEvent 播放器;
            private static float 当前音量 = 1;
            public 运行状态.淡入淡出状态 淡入淡出设定 { get; set; } = 运行状态.淡入淡出状态.不启动;
            public long 播放字节位置 { get; set; } = 0;
            public long 总字节位置 { get; set; } = 0;
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 总时长 { get; set; }
            public int 循环次数 { get; set; }
            public int 当前播放章节 { get; set; }
            private static bool 正在淡出 = false;
            private static bool 已淡出 = false;
            public float 已淡入淡出段数 = 1;
            public bool 设定静音 = false;

            public class 运行状态
            {

                public static bool Loop模式 = true;
                public static bool 自动淡入淡出 = false;
                public static float 淡出段数 = 10;
                public static 播放状态 状态 = 播放状态.播放;
                public enum 淡入淡出状态
                {
                    启动淡出,
                    启动淡入,
                    不启动
                }
                public enum 播放状态
                {
                    暂停,
                    播放,
                    停止
                }
            }
            #endregion
            #region 构造与析构函数
            public 永续式(Dictionary<int, VorbisWaveReader> 传入OGG读取器组, WaveOutEvent 传入播放器, 永续控制 控制类)
            {
                OGG读取器组 = 传入OGG读取器组;
                播放器 = 传入播放器;
                当前播放章节 = 0;
                WaveFormat = 传入OGG读取器组[当前播放章节].WaveFormat;

                Thread 外部处理 = new Thread(外部状态处理);
                外部处理.Start();

            }
            ~永续式()
            {
                Console.WriteLine("播放类已析构");
            }
            #endregion
            #region 接口必要函数
            /// <summary>
            ///  接口必须实现的函数，当播放器init后，NAudio将自动调用此方法来获取波形数据，当读入0时将停止
            ///  该接口中需要对一个读取器进行递归调用Read获取数据
            /// </summary>
            /// <param name="缓冲区">NAudio内部的缓冲区</param>
            /// <param name="起始位置">NAudio中设定音频播放的起始位置，需要输入字节数</param>
            /// <param name="读取大小">指示要将  缓冲区  从  起始位置  读取多大的音频，最大  不可超过  缓冲区  的  最大长度</param>
            /// <returns>返回此次读取的波形大小</returns>
            public int Read(byte[] 缓冲区, int 起始位置, int 读取大小)
            {

                int 实际读取字节 = 0; //实际读取字节数
                //循环逻辑，若文件剩余大小已不足以完成本次读取，则先更换流，然后把读取器的位置设为0
                if (OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position <= 读取大小 - 实际读取字节)
                {
                    //如果关闭了Loop模式，则在最后一个文件播放完成后结束播放
                    if (运行状态.Loop模式 == false && 当前播放章节 == OGG读取器组.Count - 1)
                    {
                        实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                        return 0;
                    }
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                    更换流();
                    OGG读取器组[当前播放章节].Position = 0;

                }
                //如果刚刚更换了流，这里会把下一章节的数据补充进缓冲区的剩余部分，避免造成空数据
                实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 读取大小 - 实际读取字节);
                特效应用(缓冲区, 起始位置, 读取大小);
                更新外部信息();
                return 实际读取字节; //无论如何都要返回已经读取了这么多字节

            }


            /// <summary>
            /// 接口必须实现的函数，获取当前播放流的类型
            /// 修改此处或许决定播放的文件
            /// 请设置为其读取器的.WaveFormat
            /// </summary>
            public WaveFormat WaveFormat { get; private set; }
            #endregion
            #region 外部交互逻辑
            public void 特效应用(byte[] 缓冲区, int 起始位置, int 读取大小)
            {


            }
            public int 播放暂停()
            {
                if (运行状态.状态 == 运行状态.播放状态.停止)
                {
                    播放器.Stop();
                    播放器.Dispose();
                    for (int i = 0; i < OGG读取器组.Count; i++)
                    {
                        OGG读取器组[i].Dispose();

                    }
                    return 0;
                }
                if (运行状态.状态 == 运行状态.播放状态.暂停)
                {
                    播放器.Pause();
                    return 1;
                }
                if (运行状态.状态 == 运行状态.播放状态.播放)
                {
                    播放器.Play();
                    return 1;
                }
                return 1;
            }
            public void 外部状态处理()
            {
                while (播放器.PlaybackState == PlaybackState.Stopped)
                {

                }
                while (播放器.PlaybackState != PlaybackState.Stopped)
                {
                    播放暂停();
                    Thread.Sleep(100);
                }
                Console.WriteLine("外部状态处理结束");
                永续式.运行状态.状态 = 永续式.运行状态.播放状态.播放;
            }
            public void 更新外部信息()
            {
                播放字节位置 = 播放器.GetPosition();
                总字节位置 = OGG读取器组[当前播放章节].Length;
                当前时长 = OGG读取器组[当前播放章节].CurrentTime;
                总时长 = OGG读取器组[当前播放章节].TotalTime;

            }
            #endregion
            #region 特效函数
            public void 静音(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = 0;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡出流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)1 - (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 >= 1)
                                {

                                    音量倍率 = 1f;
                                    已淡出 = true;


                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }

            #endregion
            #region 流处理
            public void 更换流()
            {
                if (当前播放章节 + 1 > OGG读取器组.Count - 1) { 循环次数++; return; }
                OGG读取器组[当前播放章节] = OGG读取器组[当前播放章节 + 1];
                当前播放章节++;
            }
            #endregion
        }

    }
    namespace 交互式直切_Beta
    {
        /// <summary>
        /// 储存永续模式播放信息的类
        /// </summary>
        public class 状态
        {
            public int 章节BPM { get; set; }
            public int 章节节拍 { get; set; }
            public int 缓冲区大小 { get; set; }
            public double 当前播放bit_实时 { get; set; }
            public double 当前播放时间_实时 { get; set; }
            public double 每拍毫秒 { get; set; }
            public int 每拍毫秒_去除小数 { get; set; }
            public double 缓冲区时间_毫秒 { get; set; }
            public double 每毫秒bit { get; set; }
            public double 每拍Bit { get; set; }
            public int 当前拍 { get; set; }
            public int 当前小节 { get; set; }
            public int 当前拍数 { get; set; }
            public double 下一拍的字节 { get; set; }
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 持续时长 { get; set; }
            public int 循环次数 { get; set; }
        }
        public class 交互式无缝循环控制
        {
            Dictionary<int, VorbisWaveReader> OGG读取器组 = new Dictionary<int, VorbisWaveReader>();
            WaveOutEvent 播放器;
            交互式无缝循环 交互流;
            List<int> 循环下标;


            object 线程锁 = new object();

            /// <summary>
            /// 启动一个永续模式播放的音乐实例
            /// </summary>
            /// <param name="播放列表">用列表储存的一个或多个文件名</param>
            /// <param name="LOOP模式">是否使用Loop永续循环</param>
            public 交互式无缝循环控制(List<string> 播放列表, List<int> 循环下标, 状态 传入状态, bool LOOP模式 = true)
            {
                //初始化流
                foreach (string 文件路径 in 播放列表)
                {
                    初始化音乐(播放列表.IndexOf(文件路径), 文件路径, LOOP模式);
                }
                //建立播放器并开始播放
                播放器 = new WaveOutEvent();
                状态 状态 = 传入状态;
                交互流 = new 交互式无缝循环(OGG读取器组, 播放器, 循环下标, 状态, this);
                交互式无缝循环.运行状态.Loop模式 = LOOP模式;
                播放器.Init(交互流);
                播放器.Play();

            }
            /// <summary>
            /// 初始化OGG音乐
            /// </summary>
            /// <param name="加入下标">输入要加入的音乐ID，将作为该音乐播放的唯一识别号</param>
            /// <param name="文件路径">音乐的完整文件路径</param>
            /// <param name="LOOP模式">是否使用Loop循环播放，如果选择不使用，将在循环最后一个文件后结束</param>
            private void 初始化音乐(int 加入下标, string 文件路径, bool LOOP模式 = true)
            {
                lock (线程锁)
                {
                    NAudio.Vorbis.VorbisWaveReader OGG音频 = new NAudio.Vorbis.VorbisWaveReader(文件路径);
                    OGG读取器组.Add(加入下标, OGG音频);
                }

            }

            /// <summary>
            /// 传入一个内部信息类，获取实时播放的信息
            /// </summary>
            /// <param name="状态">内部的信息类</param>
            /// <param name="BPM">该音乐的BPM信息，默认为60</param>
            /// <param name="章节拍数">该音乐的拍数信息，默认为4</param>
            public void 更新信息(状态 状态)
            {
                状态.缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)状态.章节BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)状态.章节节拍);
                状态.当前拍数 = 状态.当前拍 % 状态.章节节拍;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = OGG读取器组[交互流.当前播放章节].CurrentTime;
                状态.持续时长 = OGG读取器组[交互流.当前播放章节].TotalTime;
                状态.循环次数 = 交互流.循环次数;
            }

        }
        internal class 交互式无缝循环 : IWaveProvider
        {
            #region 状态引用
            Dictionary<int, VorbisWaveReader> OGG读取器组;
            WaveOutEvent 播放器;
            交互式无缝循环 交互流;
            List<int> 循环下标;
            状态 播放状态;
            private static float 当前音量 = 1;
            public 运行状态.淡入淡出状态 淡入淡出设定 { get; set; } = 运行状态.淡入淡出状态.不启动;
            public long 播放字节位置 { get; set; } = 0;
            public long 总字节位置 { get; set; } = 0;
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 总时长 { get; set; }
            public int 循环次数 { get; set; }
            public int 当前播放章节 { get; set; }
            private static bool 正在淡出 = false;
            private static bool 已淡出 = false;
            public float 已淡入淡出段数 = 1;
            public bool 设定静音 = false;
            public bool 切换章节 = false;

            public class 运行状态
            {

                public static bool Loop模式 = true;
                public static bool 自动淡入淡出 = false;
                public static float 淡出段数 = 10;
                public static 播放状态 状态 = 播放状态.播放;
                public enum 淡入淡出状态
                {
                    启动淡出,
                    启动淡入,
                    不启动
                }
                public enum 播放状态
                {
                    暂停,
                    播放,
                    停止
                }
            }
            #endregion
            #region 构造与析构函数
            public 交互式无缝循环(Dictionary<int, VorbisWaveReader> 传入OGG读取器组, WaveOutEvent 传入播放器, List<int> 传入循环下标, 状态 传入状态, 交互式无缝循环控制 控制类)
            {
                OGG读取器组 = 传入OGG读取器组;
                播放器 = 传入播放器;
                当前播放章节 = 0;
                播放状态 = 传入状态;
                循环下标 = 传入循环下标;
                WaveFormat = 传入OGG读取器组[当前播放章节].WaveFormat;
                交互流 = this;


                Thread 外部处理 = new Thread(外部状态处理);
                外部处理.Start();

            }
            ~交互式无缝循环()
            {
                Console.WriteLine("播放类已析构");
            }
            #endregion
            #region 接口必要函数
            /// <summary>
            ///  接口必须实现的函数，当播放器init后，NAudio将自动调用此方法来获取波形数据，当读入0时将停止
            ///  该接口中需要对一个读取器进行递归调用Read获取数据
            /// </summary>
            /// <param name="缓冲区">NAudio内部的缓冲区</param>
            /// <param name="起始位置">NAudio中设定音频播放的起始位置，需要输入字节数</param>
            /// <param name="读取大小">指示要将  缓冲区  从  起始位置  读取多大的音频，最大  不可超过  缓冲区  的  最大长度</param>
            /// <returns>返回此次读取的波形大小</returns>
            public int Read(byte[] 缓冲区, int 起始位置, int 读取大小)
            {
                int 实际读取字节 = 0; //实际读取字节数
                /* 实现条件
                 * 获得当前读取起始位置的节拍数
                 * 获得当前读取结束位置的节拍数
                 * 获得当前读取起始位置的总bit数
                 * 获得当前读取结束位置的总bit数
                 * 获得下一个节拍的Bit位置
                 * 如果，下一节拍是切换点，且bit位置已经在当前读取范围内（小于结束位置）那么执行下列操作
                 * ① 将下一节拍的bit位置 减去 当前起始位置的Bit 获得需要读取的bit数，并且读取它
                 * ② 更换至下一个流
                 * ③ 把下一章节的数据补充进缓冲区的剩余部分，实现无缝衔接
                 * 
                 */

                #region 虚拟切换点设置
                int 切换小节 = 2;
                int 切换拍数 = 4;
                int 切换总小节 = (切换小节 - 1) * 播放状态.章节节拍 + 切换拍数 - 1;

                #endregion
                #region
                var 每拍毫秒 = (double)60 * (double)1000 / (double)播放状态.章节BPM;
                var 缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
                var 每拍Bit = 每拍毫秒 * 每毫秒bit;
                int 起始位置总比特 = (int)OGG读取器组[当前播放章节].Position;
                int 起始位置总节拍 = (int)Math.Floor((double)起始位置总比特 / (double)每拍Bit);
                int 起始位置拍 = 起始位置总节拍 % 播放状态.章节节拍 + 1;
                int 起始位置小节 = (int)Math.Floor(((double)起始位置总节拍) / (double)播放状态.章节节拍);
                int 结束位置总比特 = (int)OGG读取器组[当前播放章节].Position + 读取大小;
                int 结束位置总节拍 = (int)Math.Floor((double)结束位置总比特 / (double)每拍Bit);
                int 结束位置拍 = 结束位置总节拍 % 播放状态.章节节拍 + 1;
                int 结束位置小节 = (int)Math.Floor(((double)结束位置总节拍) / (double)播放状态.章节节拍);
                double 下个节拍Bit = (起始位置总节拍 + 1) * 每拍Bit;
                Console.WriteLine($"起始：{起始位置小节}小节{起始位置拍}拍 | {起始位置总比特} \r\n结束：{结束位置小节}小节{结束位置拍}拍 | {结束位置总比特} > {下个节拍Bit}");
                //如果当前章节在循环，并即将在下一拍切换，且剩余流已不足以支撑到下一小节前
                if (循环下标.Contains(当前播放章节) && 起始位置总节拍 + 1 == 切换总小节 && 下个节拍Bit - 起始位置总比特 < 读取大小)
                {
                    Console.WriteLine($"即将进行切换");
                    int 本拍剩余Bit = (int)Math.Floor(下个节拍Bit - 起始位置总比特);
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 本拍剩余Bit);
                    Console.WriteLine($"读取了剩下的{实际读取字节}");
                    切换章节 = true;
                    更换流();
                }
                #endregion

                //循环逻辑，若文件剩余大小已不足以完成本次读取，则先更换流，然后把读取器的位置设为0
                if (OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position <= 读取大小 - 实际读取字节)
                {
                    //如果关闭了Loop模式，则在最后一个文件播放完成后结束播放
                    if (运行状态.Loop模式 == false && 当前播放章节 == OGG读取器组.Count - 1)
                    {
                        实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                        return 0;
                    }
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                    更换流();
                    OGG读取器组[当前播放章节].Position = 0;

                }
                //如果刚刚更换了流，这里会把下一章节的数据补充进缓冲区的剩余部分，避免造成空数据
                实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 读取大小 - 实际读取字节);
                特效应用(缓冲区, 起始位置, 读取大小);

                return 实际读取字节; //无论如何都要返回已经读取了这么多字节

            }


            /// <summary>
            /// 接口必须实现的函数，获取当前播放流的类型
            /// 修改此处或许决定播放的文件
            /// 请设置为其读取器的.WaveFormat
            /// </summary>
            public WaveFormat WaveFormat { get; private set; }
            #endregion
            #region 外部交互逻辑
            public void 特效应用(byte[] 缓冲区, int 起始位置, int 读取大小)
            {


            }
            public int 播放暂停()
            {
                if (运行状态.状态 == 运行状态.播放状态.停止)
                {
                    播放器.Stop();
                    播放器.Dispose();
                    for (int i = 0; i < OGG读取器组.Count; i++)
                    {
                        OGG读取器组[i].Dispose();

                    }
                    return 0;
                }
                if (运行状态.状态 == 运行状态.播放状态.暂停)
                {
                    播放器.Pause();
                    return 1;
                }
                if (运行状态.状态 == 运行状态.播放状态.播放)
                {
                    播放器.Play();
                    return 1;
                }
                return 1;
            }
            public void 外部状态处理()
            {
                while (播放器.PlaybackState == PlaybackState.Stopped)
                {

                }
                while (播放器.PlaybackState != PlaybackState.Stopped)
                {
                    播放暂停();
                    Thread.Sleep(100);
                }
                Console.WriteLine("外部状态处理结束");
                交互式无缝循环.运行状态.状态 = 交互式无缝循环.运行状态.播放状态.播放;
            }
            public void 更新外部信息(状态 状态)
            {
                状态.缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)状态.章节BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)状态.章节节拍);
                状态.当前拍数 = 状态.当前拍 % 状态.章节节拍;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = OGG读取器组[交互流.当前播放章节].CurrentTime;
                状态.持续时长 = OGG读取器组[交互流.当前播放章节].TotalTime;
                状态.循环次数 = 交互流.循环次数;
            }
            #endregion
            #region 特效函数
            public void 静音(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = 0;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡出流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)1 - (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 >= 1)
                                {

                                    音量倍率 = 1f;
                                    已淡出 = true;


                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }

            #endregion
            #region 流处理
            public void 更换流()
            {
                if (当前播放章节 + 1 > OGG读取器组.Count - 1) { 循环次数++; return; }
                if (切换章节)
                {
                    OGG读取器组[当前播放章节] = OGG读取器组[当前播放章节 + 1];
                    切换章节 = false;
                    Console.WriteLine($"手动切换到了新流");
                    当前播放章节++;
                    Console.WriteLine($"当前流编号{ 当前播放章节}");
                    return;
                }
                if (循环下标.Contains(当前播放章节)) { 循环次数++; return; }
                OGG读取器组[当前播放章节] = OGG读取器组[当前播放章节 + 1];
                当前播放章节++;
                Console.WriteLine($"当前流编号{ 当前播放章节}");
            }
            #endregion

        }

    }
    namespace 交互式无缝循环
    {
        /// <summary>
        /// 储存永续模式播放信息的类
        /// </summary>
        public class 状态
        {
            public static 播放状态 当前状态 = 播放状态.播放;
            public enum 播放状态
            {
                暂停,
                播放,
                停止
            }
            public int 章节BPM { get; set; }
            public int 章节节拍 { get; set; }
            public int 缓冲区大小 { get; set; }
            public double 当前播放bit_实时 { get; set; }
            public double 当前播放时间_实时 { get; set; }
            public double 每拍毫秒 { get; set; }
            public int 每拍毫秒_去除小数 { get; set; }
            public double 缓冲区时间_毫秒 { get; set; }
            public double 每毫秒bit { get; set; }
            public double 每拍Bit { get; set; }
            public int 当前拍 { get; set; }
            public int 当前小节 { get; set; }
            public int 当前拍数 { get; set; }
            public double 下一拍的字节 { get; set; }
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 持续时长 { get; set; }
            public int 循环次数 { get; set; }
        }
        public class 交互式无缝循环控制
        {
            Dictionary<int, VorbisWaveReader> OGG读取器组 = new Dictionary<int, VorbisWaveReader>();
            public static WaveOutEvent 播放器;
            public static 交互式无缝循环 交互流;
            List<int> 循环下标;


            object 线程锁 = new object();

            /// <summary>
            /// 启动一个永续模式播放的音乐实例
            /// </summary>
            /// <param name="播放列表">用列表储存的一个或多个文件名</param>
            /// <param name="LOOP模式">是否使用Loop永续循环</param>
            public 交互式无缝循环控制(List<string> 播放列表, List<int> 循环下标, 状态 传入状态, bool LOOP模式 = true)
            {
                //初始化流
                foreach (string 文件路径 in 播放列表)
                {
                    初始化音乐(播放列表.IndexOf(文件路径), 文件路径, LOOP模式);
                }
                //建立播放器并开始播放
                播放器 = new WaveOutEvent();
                状态 状态 = 传入状态;
                交互流 = new 交互式无缝循环(OGG读取器组, 播放器, 循环下标, 状态, 0, true);
                交互流.递交新流 = 更换控制流;
                交互式无缝循环.运行状态.Loop模式 = LOOP模式;
                播放器.Init(交互流);
                播放器.Play();

            }
            /// <summary>
            /// 初始化OGG音乐
            /// </summary>
            /// <param name="加入下标">输入要加入的音乐ID，将作为该音乐播放的唯一识别号</param>
            /// <param name="文件路径">音乐的完整文件路径</param>
            /// <param name="LOOP模式">是否使用Loop循环播放，如果选择不使用，将在循环最后一个文件后结束</param>
            private void 初始化音乐(int 加入下标, string 文件路径, bool LOOP模式 = true)
            {
                lock (线程锁)
                {
                    NAudio.Vorbis.VorbisWaveReader OGG音频 = new NAudio.Vorbis.VorbisWaveReader(文件路径);
                    OGG读取器组.Add(加入下标, OGG音频);
                }

            }

            /// <summary>
            /// 传入一个内部信息类，获取实时播放的信息
            /// </summary>
            /// <param name="状态">内部的信息类</param>
            /// <param name="BPM">该音乐的BPM信息，默认为60</param>
            /// <param name="章节拍数">该音乐的拍数信息，默认为4</param>
            public void 更新信息(状态 状态)
            {
                状态.缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)状态.章节BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)状态.章节节拍);
                状态.当前拍数 = 状态.当前拍 % 状态.章节节拍;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = OGG读取器组[交互流.当前播放章节].CurrentTime;
                状态.持续时长 = OGG读取器组[交互流.当前播放章节].TotalTime;
                状态.循环次数 = 交互流.循环次数;
            }

            public void 更换控制流(WaveOutEvent 传入播放器, 交互式无缝循环 传入流)
            {
                播放器 = 传入播放器;
                交互流 = 传入流;
                //建立播放器并开始播放
                交互式无缝循环.运行状态.Loop模式 = true;
                传入播放器.Init(交互流);
                传入播放器.Play();

            }
            public static void 准备切换章节()
            {
                交互流.准备切换章节();

            }

        }
        public class 交互式无缝循环 : IWaveProvider
        {
            #region 初始化类
            public Dictionary<int, VorbisWaveReader> OGG读取器组;
            public WaveOutEvent 播放器;
            交互式无缝循环 交互流;

            List<int> 循环下标;
            public 状态 播放状态;
            public delegate void 委托递交新流(WaveOutEvent 传入播放器, 交互式无缝循环 传入流);
            public 委托递交新流 递交新流;
            #endregion
            #region 属性与状态
            private static float 当前音量 = 1;
            public 运行状态.淡入淡出状态 淡入淡出设定 { get; set; } = 运行状态.淡入淡出状态.不启动;
            public long 播放字节位置 { get; set; } = 0;
            public long 总字节位置 { get; set; } = 0;
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 总时长 { get; set; }
            public int 循环次数 { get; set; }
            public int 当前播放章节 { get; set; }
            private static bool 正在淡出 = false;
            private static bool 已淡出 = false;
            public double 切换点Bit;
            public int 偏移字节 = 0;
            public float 已淡入淡出字节数 = 1;
            public float 总淡入淡出字节数 = 1;
            public bool 设定静音 = false;
            public Dictionary<int, int> 章节循环次数 = new Dictionary<int, int>();

            /// <summary>
            /// 准备切换章节
            /// </summary>
            public bool 切换章节 = false;


            int 切换总节拍;

            #endregion

            public class 运行状态
            {

                public static bool Loop模式 = true;
                public static bool 自动淡入淡出 = false;
                public static float 淡出段数 = 10;

                public enum 淡入淡出状态
                {
                    启动淡出,
                    启动淡入,
                    不启动
                }

                public static int 切换拍数 { get; set; } = 4;

            }

            #region 构造与析构函数
            public 交互式无缝循环(Dictionary<int, VorbisWaveReader> 传入OGG读取器组, WaveOutEvent 传入播放器, List<int> 传入循环下标, 状态 传入状态, int 播放章节, bool 是否淡入)
            {

                OGG读取器组 = 传入OGG读取器组;
                播放器 = 传入播放器;
                当前播放章节 = 播放章节;
                播放状态 = 传入状态;
                循环下标 = 传入循环下标;
                WaveFormat = 传入OGG读取器组[当前播放章节].WaveFormat;
                交互流 = this;
                更新淡入淡出所需字节();
                Thread 外部处理 = new Thread(外部状态处理);
                外部处理.Start();
                if (是否淡入)
                {
                    淡入淡出设定 = 运行状态.淡入淡出状态.启动淡入;
                }


            }
            ~交互式无缝循环()
            {
                Console.WriteLine("播放类已析构");
            }
            #endregion
            #region 接口必要函数
            /// <summary>
            ///  接口必须实现的函数，当播放器init后，NAudio将自动调用此方法来获取波形数据，当读入0时将停止
            ///  该接口中需要对一个读取器进行递归调用Read获取数据
            /// </summary>
            /// <param name="缓冲区">NAudio内部的缓冲区</param>
            /// <param name="起始位置">NAudio中设定音频播放的起始位置，需要输入字节数</param>
            /// <param name="读取大小">指示要将  缓冲区  从  起始位置  读取多大的音外部状态处理结束频，最大  不可超过  缓冲区  的  最大长度</param>
            /// <returns>返回此次读取的波形大小</returns>
            public int Read(byte[] 缓冲区, int 起始位置, int 读取大小)
            {
                int 实际读取字节 = 0; //实际读取字节数
                /* 实现条件
                 * 获得当前读取起始位置的节拍数
                 * 获得当前读取结束位置的节拍数
                 * 获得当前读取起始位置的总bit数
                 * 获得当前读取结束位置的总bit数
                 * 获得下一个节拍的Bit位置
                 * 如果，下一节拍是切换点，且bit位置已经在当前读取范围内（小于结束位置）那么执行下列操作
                 * ① 将下一节拍的bit位置 减去 当前起始位置的Bit 获得需要读取的bit数，并且读取它
                 * ② 更换至下一个流
                 * ③ 把下一章节的数据补充进缓冲区的剩余部分，实现无缝衔接
                 * 
                 */
                Console.WriteLine($"{实时位置()} {OGG读取器组[当前播放章节].Position} {实时位置() - OGG读取器组[当前播放章节].Position}");
                #region 虚拟切换点设置


                #endregion
                #region 实时计算
                var 每拍毫秒 = (double)60 * (double)1000 / (double)播放状态.章节BPM;
                var 缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
                var 每拍Bit = 每拍毫秒 * 每毫秒bit;
                int 起始位置总比特 = (int)OGG读取器组[当前播放章节].Position;
                int 起始位置总节拍 = (int)Math.Floor((double)起始位置总比特 / (double)每拍Bit);
                int 起始位置拍 = 起始位置总节拍 % 播放状态.章节节拍 + 1;
                int 起始位置小节 = (int)Math.Floor(((double)起始位置总节拍) / (double)播放状态.章节节拍);
                int 结束位置总比特 = (int)OGG读取器组[当前播放章节].Position + 读取大小;
                int 结束位置总节拍 = (int)Math.Floor((double)结束位置总比特 / (double)每拍Bit);
                int 结束位置拍 = 结束位置总节拍 % 播放状态.章节节拍 + 1;
                int 结束位置小节 = (int)Math.Floor(((double)结束位置总节拍) / (double)播放状态.章节节拍);
                double 下个节拍Bit = (起始位置总节拍 + 1) * 每拍Bit;
                播放状态.当前拍 = 起始位置拍;
                播放状态.当前小节 = 起始位置小节;

                //Console.WriteLine($"{当前播放章节}起始：{起始位置小节}小节{起始位置拍}拍 | {起始位置总比特} \r\n结束：{结束位置小节}小节{结束位置拍}拍 | {结束位置总比特} > {下个节拍Bit}");

                #endregion
                //淡出流逻辑，如果当前读取的范围已经包含了切换点的上一拍，那么读完切换点的上一拍前数据后，马上淡出之后的数据
                if (切换章节 && 淡入淡出设定 == 运行状态.淡入淡出状态.不启动 && 结束位置总比特 > 切换点Bit - 每拍Bit)
                {
                    var 剩余Bit = (int)(切换点Bit - 每拍Bit) - 起始位置总比特;
                    Console.WriteLine($"淡出点位于 {切换点Bit - 每拍Bit} 现距离切换点剩余 {切换点Bit - 起始位置总比特}将正常读取剩下的{剩余Bit} 强制剩余{起始位置总比特 + 剩余Bit}+{每拍Bit} = {切换点Bit}");
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 剩余Bit);
                    Console.WriteLine($"对剩余的Bit应用淡出效果");
                    淡入淡出设定 = 运行状态.淡入淡出状态.启动淡出;
                    特效应用(缓冲区, 起始位置 + 实际读取字节, 缓冲区.Length - 实际读取字节);
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 缓冲区.Length - 实际读取字节);
                    return 实际读取字节;
                }
                //淡出流逻辑，若刚刚已经淡出到静音则完全停止
                if (设定静音) { return 0; }
                //切换流逻辑，如果当前章节在循环，并即将在下一拍切换，且剩余流已不足以支撑到下一小节前
                if (循环下标.Contains(当前播放章节) && 起始位置总节拍 + 1 == 切换总节拍 && 下个节拍Bit - 起始位置总比特 < 读取大小)
                {
                    Console.WriteLine($"即将进行切换");
                    int 本拍剩余Bit = (int)Math.Floor(下个节拍Bit - 起始位置总比特);
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 本拍剩余Bit);
                    Console.WriteLine($"读取了剩下的{实际读取字节}");

                }

                //循环逻辑，若文件剩余大小已不足以完成本次读取，则先更换流，然后把读取器的位置设为0
                if (OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position <= 读取大小 - 实际读取字节)
                {
                    //如果关闭了Loop模式，则在最后一个文件播放完成后结束播放
                    if (运行状态.Loop模式 == false && 当前播放章节 == OGG读取器组.Count - 1)
                    {
                        实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                        return 0;
                    }
                    实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(OGG读取器组[当前播放章节].Length - OGG读取器组[当前播放章节].Position));
                    更换流();
                    OGG读取器组[当前播放章节].Position = 0;

                }
                //如果刚刚更换了流，这里会把下一章节的数据补充进缓冲区的剩余部分，避免造成空数据
                实际读取字节 += OGG读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 读取大小 - 实际读取字节);
                特效应用(缓冲区, 起始位置, 读取大小);

                return 实际读取字节; //无论如何都要返回已经读取了这么多字节

            }


            /// <summary>
            /// 接口必须实现的函数，获取当前播放流的类型
            /// 修改此处或许决定播放的文件
            /// 请设置为其读取器的.WaveFormat
            /// </summary>
            public WaveFormat WaveFormat { get; private set; }
            #endregion
            #region 实时判定线程
            private void 切换章节线程()
            {
                var 每拍毫秒 = (double)60 * (double)1000 / (double)播放状态.章节BPM;
                var 缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
                var 每拍Bit = 每拍毫秒 * 每毫秒bit;
                切换点Bit = 每拍Bit * 切换总节拍;
                Thread 切换章节 = new Thread(() =>
                {
                    bool 已发送淡出指令 = false;
                    bool 已发送切换流指令 = false;
                    WaveOutEvent 新播放器 = new WaveOutEvent();
                    while (true)
                    {
                        if (播放器.PlaybackState == PlaybackState.Stopped) { break; }
                        var 当前位置 = 实时位置();
                        var 剩余大小 = 切换点Bit - 当前位置;


                        if (已发送切换流指令 == false && 剩余大小 < 每拍Bit)
                        {
                            Thread 新播放 = new Thread(() =>
                            {
                                递交新流(新播放器, new 交互式无缝循环(OGG读取器组, 新播放器, 循环下标, 播放状态, 当前播放章节 + 1, true));
                            });
                            新播放.Start();
                            已发送切换流指令 = true;
                            return;
                        }
                        Thread.Sleep(1);
                    }

                    Console.WriteLine("因为章节已停止播放，已停止实时判断");
                });
                切换章节.Start();

            }
            private int 实时位置()
            {
                var 原始字节 = 播放器.GetPosition();
                for (int i = 0; i < OGG读取器组.Count; i++)
                {
                    if (OGG读取器组.ContainsKey(i))
                    {
                        原始字节 -= OGG读取器组[i].Length * 取出循环次数(i);

                    }

                }
                return (int)原始字节;

            }
            #endregion
            #region 外部交互逻辑
            public void 准备切换章节()
            {
                if (!循环下标.Contains(当前播放章节)) { return; }
                /* 功能实现
                 * 寻找出最合适的切换点
                 * 在切换点之前一拍的位置，以一拍的大小开始淡出流（不是一次Read的大小）
                 * 在切换点到达的时候，新建流，并设置该流淡出形式开始
                 */
                Console.WriteLine("准备切换");
                var 每拍毫秒 = (double)60 * (double)1000 / (double)播放状态.章节BPM;
                var 缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
                var 每拍Bit = 每拍毫秒 * 每毫秒bit;
                int 结束位置总比特 = (int)OGG读取器组[当前播放章节].Position + OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                int 结束位置总节拍 = (int)Math.Floor((double)结束位置总比特 / (double)每拍Bit);
                int 结束位置拍 = 结束位置总节拍 % 播放状态.章节节拍 + 1;
                int 结束位置小节 = (int)Math.Floor(((double)结束位置总节拍) / (double)播放状态.章节节拍);
                切换总节拍 = (结束位置小节 + 1) * 播放状态.章节节拍 + 播放状态.章节节拍;
                if (切换总节拍 - 结束位置总节拍 > 播放状态.章节节拍 + 1)
                {
                    切换总节拍 -= 播放状态.章节节拍;
                }
                切换章节 = true;
                切换章节线程();
            }

            public void 更新淡入淡出所需字节()
            {
                var 每拍毫秒 = (double)60 * (double)1000 / (double)播放状态.章节BPM;
                var 缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                var 缓冲区时间_毫秒 = (缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                var 每毫秒bit = 缓冲区大小 / 缓冲区时间_毫秒;
                var 每拍Bit = 每拍毫秒 * 每毫秒bit;

                总淡入淡出字节数 = (float)每拍Bit;
                淡入淡出设定 = 运行状态.淡入淡出状态.启动淡入;


            }
            public void 特效应用(byte[] 缓冲区, int 起始位置, int 读取大小)
            {
                if (设定静音)
                {
                    静音(缓冲区, 起始位置, 读取大小);
                }

                if (淡入淡出设定 == 运行状态.淡入淡出状态.启动淡入)
                {
                    淡入流(缓冲区, 起始位置, 读取大小);
                    if (已淡入淡出字节数 >= 总淡入淡出字节数)
                    {
                        淡入淡出设定 = 运行状态.淡入淡出状态.不启动;
                        已淡入淡出字节数 = 0;
                    }
                }

                if (淡入淡出设定 == 运行状态.淡入淡出状态.启动淡出)
                {
                    淡出流(缓冲区, 起始位置, 读取大小);
                    Console.WriteLine($"{已淡入淡出字节数}/{总淡入淡出字节数}");
                    if (已淡入淡出字节数 >= 总淡入淡出字节数)
                    {

                        已淡入淡出字节数 = 0;
                        设定静音 = true;
                    }
                }


            }
            public int 播放暂停()
            {
                if (状态.当前状态 == 状态.播放状态.停止)
                {
                    播放器.Stop();
                    播放器.Dispose();
                    for (int i = 0; i < OGG读取器组.Count; i++)
                    {
                        OGG读取器组[i].Dispose();

                    }
                    return 0;
                }
                if (状态.当前状态 == 状态.播放状态.暂停)
                {
                    播放器.Pause();
                    return 1;
                }
                if (状态.当前状态 == 状态.播放状态.播放)
                {
                    播放器.Play();
                    return 1;
                }
                return 1;
            }
            public void 外部状态处理()
            {
                while (播放器.PlaybackState == PlaybackState.Stopped)
                {

                }
                while (播放器.PlaybackState != PlaybackState.Stopped)
                {
                    播放暂停();
                    Thread.Sleep(100);
                }
                Console.WriteLine("外部状态处理结束");
                状态.当前状态 = 状态.播放状态.播放;
            }
            public void 更新外部信息(状态 状态)
            {
                状态.缓冲区大小 = OGG读取器组[交互流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)状态.章节BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.BitsPerSample
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.Channels * 8.0
                                    / OGG读取器组[交互流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)状态.章节节拍);
                状态.当前拍数 = 状态.当前拍 % 状态.章节节拍;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = OGG读取器组[交互流.当前播放章节].CurrentTime;
                状态.持续时长 = OGG读取器组[交互流.当前播放章节].TotalTime;
                状态.循环次数 = 交互流.循环次数;
            }
            #endregion
            #region 特效函数
            public void 静音(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = 0;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }

                        }
                    }

                }


            }
            public void 淡出流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)(临时缓冲区指针 + 流开始点);//流开始点为后加
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)1 - (float)已淡入淡出字节数 / 总淡入淡出字节数;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;

                                已淡入淡出字节数 += 4f;
                            }

                        }
                    }

                }


            }
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)已淡入淡出字节数 / 总淡入淡出字节数;

                                if (音量倍率 >= 1)
                                {

                                    音量倍率 = 1f;
                                    已淡出 = true;


                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;

                                已淡入淡出字节数 += 8f;

                            }

                        }
                    }

                }


            }

            #endregion
            #region 流处理
            public void 更换流()
            {
                if (当前播放章节 + 1 > OGG读取器组.Count - 1)
                {
                    循环次数++;
                    增加循环次数(当前播放章节);
                    return;
                }
                if (切换章节)
                {
                    切换章节 = false;
                    Console.WriteLine($"手动切换到了新流");
                    当前播放章节++;
                    Console.WriteLine($"当前流编号{ 当前播放章节}");
                    return;
                }
                if (循环下标.Contains(当前播放章节))
                { 循环次数++; 增加循环次数(当前播放章节); return; }

                偏移字节 += (int)OGG读取器组[当前播放章节].Length;
                增加循环次数(当前播放章节);
                当前播放章节++;

                Console.WriteLine($"当前流编号{当前播放章节}");
            }
            public void 增加循环次数(int 循环章节)
            {
                if (章节循环次数.ContainsKey(循环章节))
                {
                    章节循环次数[循环章节] = 章节循环次数[循环章节] + 1;
                    Console.WriteLine($"改 章节{循环章节}已循环{章节循环次数[循环章节]}次");
                }
                else
                {
                    章节循环次数.Add(循环章节, 1);
                    Console.WriteLine($"增 章节{循环章节}已循环{章节循环次数[循环章节]}次");
                }
            }
            public int 取出循环次数(int 循环章节)
            {
                if (章节循环次数.ContainsKey(循环章节))
                {
                    return 章节循环次数[循环章节];
                }
                else
                {
                    return 0;
                }

            }

            #endregion

        }

    }
    namespace MP3
    {
        public class 永续控制
        {
            Dictionary<int, Mp3FileReader> MP3读取器组 = new Dictionary<int, Mp3FileReader>();
            WaveOutEvent 播放器;
            public static 永续式 永续流;
            /// <summary>
            /// 储存永续模式播放信息的类
            /// </summary>
            public class 信息
            {
                public int 缓冲区大小 { get; set; }
                public double 当前播放bit_实时 { get; set; }
                public double 当前播放时间_实时 { get; set; }
                public double 每拍毫秒 { get; set; }
                public int 每拍毫秒_去除小数 { get; set; }
                public double 缓冲区时间_毫秒 { get; set; }
                public double 每毫秒bit { get; set; }
                public double 每拍Bit { get; set; }
                public int 当前拍 { get; set; }
                public int 当前小节 { get; set; }
                public int 当前拍数 { get; set; }
                public double 下一拍的字节 { get; set; }
                public TimeSpan 当前时长 { get; set; }
                public TimeSpan 持续时长 { get; set; }
                public int 循环次数 { get; set; }
            }
            object 线程锁 = new object();

            /// <summary>
            /// 启动一个永续模式播放的音乐实例
            /// </summary>
            /// <param name="播放列表">用列表储存的一个或多个文件名</param>
            /// <param name="LOOP模式">是否使用Loop永续循环</param>
            public 永续控制(List<string> 播放列表, bool LOOP模式 = true)
            {
                //初始化流
                foreach (string 文件路径 in 播放列表)
                {
                    初始化音乐(播放列表.IndexOf(文件路径), 文件路径, LOOP模式);
                }
                //建立播放器并开始播放
                播放器 = new WaveOutEvent();
                永续流 = new 永续式(MP3读取器组, 播放器, this);
                永续式.运行状态.Loop模式 = LOOP模式;
                播放器.Init(永续流);
                播放器.Play();

            }
            /// <summary>
            /// 初始化OGG音乐
            /// </summary>
            /// <param name="加入下标">输入要加入的音乐ID，将作为该音乐播放的唯一识别号</param>
            /// <param name="文件路径">音乐的完整文件路径</param>
            /// <param name="LOOP模式">是否使用Loop循环播放，如果选择不使用，将在循环最后一个文件后结束</param>
            private void 初始化音乐(int 加入下标, string 文件路径, bool LOOP模式 = true)
            {
                lock (线程锁)
                {
                    NAudio.Wave.Mp3FileReader MP3音频 = new NAudio.Wave.Mp3FileReader(文件路径);
                    MP3读取器组.Add(加入下标, MP3音频);
                }

            }

            /// <summary>
            /// 传入一个内部信息类，获取实时播放的信息
            /// </summary>
            /// <param name="状态">内部的信息类</param>
            /// <param name="BPM">该音乐的BPM信息，默认为60</param>
            /// <param name="章节拍数">该音乐的拍数信息，默认为4</param>
            public void 更新信息(信息 状态, int BPM = 60, int 章节拍数 = 4)
            {
                状态.缓冲区大小 = MP3读取器组[永续流.当前播放章节].WaveFormat.BlockAlign;
                状态.当前播放bit_实时 = 播放器.GetPosition();
                状态.当前播放时间_实时 = 播放器.GetPosition() * 1000.0 /
                                    MP3读取器组[永续流.当前播放章节].WaveFormat.BitsPerSample /
                                    MP3读取器组[永续流.当前播放章节].WaveFormat.Channels * 8.0 /
                                    MP3读取器组[永续流.当前播放章节].WaveFormat.SampleRate;
                状态.每拍毫秒 = (double)60 * (double)1000 / (double)BPM;
                状态.每拍毫秒_去除小数 = Convert.ToInt32(Math.Floor(状态.每拍毫秒));
                状态.缓冲区时间_毫秒 = (状态.缓冲区大小) * 1000.0
                                    / MP3读取器组[永续流.当前播放章节].WaveFormat.BitsPerSample
                                    / MP3读取器组[永续流.当前播放章节].WaveFormat.Channels * 8.0
                                    / MP3读取器组[永续流.当前播放章节].WaveFormat.SampleRate;
                状态.每毫秒bit = 状态.缓冲区大小 / 状态.缓冲区时间_毫秒;
                状态.每拍Bit = 状态.每拍毫秒 * 状态.每毫秒bit;
                状态.当前拍 = (int)Math.Floor((double)状态.当前播放bit_实时 / (double)状态.每拍Bit);
                状态.当前小节 = (int)Math.Floor((double)状态.当前拍 / (double)章节拍数);
                状态.当前拍数 = 状态.当前拍 % 章节拍数;
                状态.下一拍的字节 = 状态.当前播放bit_实时 + 状态.每拍Bit;
                状态.当前时长 = MP3读取器组[永续流.当前播放章节].CurrentTime;
                状态.持续时长 = MP3读取器组[永续流.当前播放章节].TotalTime;
                状态.循环次数 = 永续流.循环次数;
            }

            public void 设置音量(double 音量值)
            {
                播放器.Volume = (float)音量值 / 100f;


            }
        }
        public class 永续式 : IWaveProvider
        {
            #region 状态引用
            public Dictionary<int, Mp3FileReader> MP3读取器组;
            public WaveOutEvent 播放器;
            private static float 当前音量 = 1;
            public 运行状态.淡入淡出状态 淡入淡出设定 { get; set; } = 运行状态.淡入淡出状态.不启动;
            public long 播放字节位置 { get; set; } = 0;
            public long 总字节位置 { get; set; } = 0;
            public TimeSpan 当前时长 { get; set; }
            public TimeSpan 总时长 { get; set; }
            public int 循环次数 { get; set; }
            public int 当前播放章节 { get; set; }
            private static bool 正在淡出 = false;
            private static bool 已淡出 = false;
            public float 已淡入淡出段数 = 1;
            public bool 设定静音 = false;

            public class 运行状态
            {

                public static bool Loop模式 = true;
                public static bool 自动淡入淡出 = false;
                public static float 淡出段数 = 10;
                public static 播放状态 状态 = 播放状态.播放;
                public enum 淡入淡出状态
                {
                    启动淡出,
                    启动淡入,
                    不启动
                }
                public enum 播放状态
                {
                    暂停,
                    播放,
                    停止
                }
            }
            #endregion
            #region 构造与析构函数
            public 永续式(Dictionary<int, Mp3FileReader> 传入OGG读取器组, WaveOutEvent 传入播放器, 永续控制 控制类)
            {
                MP3读取器组 = 传入OGG读取器组;
                播放器 = 传入播放器;
                当前播放章节 = 0;
                WaveFormat = 传入OGG读取器组[当前播放章节].WaveFormat;

                Thread 外部处理 = new Thread(外部状态处理);
                外部处理.Start();

            }
            ~永续式()
            {
                Console.WriteLine("播放类已析构");
            }
            #endregion
            #region 接口必要函数
            /// <summary>
            ///  接口必须实现的函数，当播放器init后，NAudio将自动调用此方法来获取波形数据，当读入0时将停止
            ///  该接口中需要对一个读取器进行递归调用Read获取数据
            /// </summary>
            /// <param name="缓冲区">NAudio内部的缓冲区</param>
            /// <param name="起始位置">NAudio中设定音频播放的起始位置，需要输入字节数</param>
            /// <param name="读取大小">指示要将  缓冲区  从  起始位置  读取多大的音频，最大  不可超过  缓冲区  的  最大长度</param>
            /// <returns>返回此次读取的波形大小</returns>
            public int Read(byte[] 缓冲区, int 起始位置, int 读取大小)
            {

                int 实际读取字节 = 0; //实际读取字节数
                //循环逻辑，若文件剩余大小已不足以完成本次读取，则先更换流，然后把读取器的位置设为0
                if (MP3读取器组[当前播放章节].Length - MP3读取器组[当前播放章节].Position <= 读取大小 - 实际读取字节)
                {
                    //如果关闭了Loop模式，则在最后一个文件播放完成后结束播放
                    if (运行状态.Loop模式 == false && 当前播放章节 == MP3读取器组.Count - 1)
                    {
                        实际读取字节 += MP3读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(MP3读取器组[当前播放章节].Length - MP3读取器组[当前播放章节].Position));
                        return 0;
                    }
                    实际读取字节 += MP3读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, (int)(MP3读取器组[当前播放章节].Length - MP3读取器组[当前播放章节].Position));
                    更换流();
                    MP3读取器组[当前播放章节].Position = 0;

                }
                //如果刚刚更换了流，这里会把下一章节的数据补充进缓冲区的剩余部分，避免造成空数据
                实际读取字节 += MP3读取器组[当前播放章节].Read(缓冲区, 起始位置 + 实际读取字节, 读取大小 - 实际读取字节);
                特效应用(缓冲区, 起始位置, 读取大小);
                更新外部信息();
                return 实际读取字节; //无论如何都要返回已经读取了这么多字节

            }


            /// <summary>
            /// 接口必须实现的函数，获取当前播放流的类型
            /// 修改此处或许决定播放的文件
            /// 请设置为其读取器的.WaveFormat
            /// </summary>
            public WaveFormat WaveFormat { get; private set; }
            #endregion
            #region 外部交互逻辑
            public void 特效应用(byte[] 缓冲区, int 起始位置, int 读取大小)
            {


            }
            public int 播放暂停()
            {
                if (运行状态.状态 == 运行状态.播放状态.停止)
                {
                    播放器.Stop();
                    播放器.Dispose();
                    for (int i = 0; i < MP3读取器组.Count; i++)
                    {
                        MP3读取器组[i].Dispose();

                    }
                    return 0;
                }
                if (运行状态.状态 == 运行状态.播放状态.暂停)
                {
                    播放器.Pause();
                    return 1;
                }
                if (运行状态.状态 == 运行状态.播放状态.播放)
                {
                    播放器.Play();
                    return 1;
                }
                return 1;
            }
            public void 外部状态处理()
            {
                while (播放器.PlaybackState == PlaybackState.Stopped)
                {

                }
                while (播放器.PlaybackState != PlaybackState.Stopped)
                {
                    播放暂停();
                    Thread.Sleep(100);
                }
                Console.WriteLine("外部状态处理结束");
                永续式.运行状态.状态 = 永续式.运行状态.播放状态.播放;
            }
            public void 更新外部信息()
            {
                播放字节位置 = 播放器.GetPosition();
                总字节位置 = MP3读取器组[当前播放章节].Length;
                当前时长 = MP3读取器组[当前播放章节].CurrentTime;
                总时长 = MP3读取器组[当前播放章节].TotalTime;

            }
            #endregion
            #region 特效函数
            public void 静音(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = 0;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡出流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)1 - (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 <= 0.01)
                                {

                                    音量倍率 = 0.0f;
                                    已淡出 = true;

                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }
            public void 淡入流(byte[] 缓冲流, int 流开始点, int 流结束点)
            {

                unsafe
                {
                    正在淡出 = true;
                    float 左声道A, 右声道A, 左声道B, 右声道B;
                    fixed (byte* 缓冲区指针 = 缓冲流)
                    {
                        fixed (byte* 临时缓冲区指针 = 缓冲流)
                        {
                            float* 缓冲区数据A = (float*)(缓冲区指针 + 流开始点);
                            float* 缓冲区数据B = (float*)临时缓冲区指针;
                            int 流终点 = 流结束点 / WaveFormat.BlockAlign * 2;
                            for (int i = 0; i < 流终点; i += 2)
                            {
                                float 音量倍率 = (float)已淡入淡出段数 / 运行状态.淡出段数;

                                if (音量倍率 >= 1)
                                {

                                    音量倍率 = 1f;
                                    已淡出 = true;


                                }
                                当前音量 = 音量倍率;
                                左声道A = 缓冲区数据A[i] * 音量倍率;
                                右声道A = 缓冲区数据A[i + 1] * 音量倍率;
                                左声道B = 缓冲区数据B[i] * 音量倍率;
                                右声道B = 缓冲区数据B[i + 1] * 音量倍率;


                                缓冲区数据A[i] = 左声道A;
                                缓冲区数据B[i + 1] = 右声道B;


                            }
                            已淡入淡出段数 += 1;
                        }
                    }

                }


            }

            #endregion
            #region 流处理
            public void 更换流()
            {
                if (当前播放章节 + 1 > MP3读取器组.Count - 1) { 循环次数++; return; }

                当前播放章节++;
            }
            #endregion
        }

    }

}
