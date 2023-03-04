using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace 交互音乐播放器.效果器
{
    public static class 淡入淡出
    {
        public class 段落信息
        {
            public enum 效果模式
            {
                淡入,
                淡出,
                完全静音,
                混流,
                不启用
            }
            public 效果模式 模式 { get; set; } = 效果模式.不启用;
            public int 流编号 { get; set; } = -1;
            public int 段落编号 { get; set; } = -1;
            public int 已处理字节 { get; set; } = -1;
            public int 总处理字节 { get; set; } = -1;
            public float 音量值 { get; set; } = -1;
        }

        //如果是挂载给流，那么直接挂载到所有流就可以了，所以只用挂在段落
        public static List<段落信息> 挂载段落 { get; set; } = new List<段落信息>();
        public static List<int> 流编号集 { get; set; } = new List<int>();
        public static Dictionary<string, int> 效果别名到挂载号 { get; set; } = new Dictionary<string, int>();
        public static Dictionary<int, int> 签名到到挂载号 { get; set; } = new Dictionary<int, int>();

        public static void 重置所有挂载()
        {
            挂载段落.Clear();
            流编号集.Clear();
            效果别名到挂载号.Clear();
            签名到到挂载号.Clear();
        }

        public static int? 获取段落号(string 效果别名)
        {
            if (!效果别名到挂载号.ContainsKey(效果别名)) { return null; }
            return 效果别名到挂载号[效果别名];
        }

        public static 段落信息? 获取处理状态(string 效果别名)
        {
            if (!效果别名到挂载号.ContainsKey(效果别名)) { return null; }
            var 编号 = 效果别名到挂载号[效果别名];
            return 挂载段落[编号];
        }

        public static bool 解除挂载(string 效果别名)
        {
            if (!效果别名到挂载号.ContainsKey(效果别名)) { Debug.Print("未挂载该效果器"); return false; }
            var 编号 = 效果别名到挂载号[效果别名];
            效果别名到挂载号.Remove(效果别名);
            var 挂载 = 挂载段落[编号];
            var 流编号 = 挂载.流编号; int 段落编号 = 挂载.段落编号;
            签名到到挂载号.Remove(生成流与段落签名(流编号, 段落编号));
            挂载段落.RemoveAt(编号);
            return true;
        }

        public static int? 添加挂载(string 效果别名, 段落信息.效果模式 效果模式, int 流编号, int 段落编号, int 总处理字节)
        {
            
            float 音量初始值 = 0;
            if (效果模式 == 段落信息.效果模式.淡出) { 音量初始值 = 1; }
            段落信息 段落 = new 段落信息() { 模式 = 效果模式, 流编号 = 流编号, 段落编号 = 段落编号, 已处理字节 = 0, 总处理字节 = 总处理字节, 音量值 = 音量初始值 };
            挂载段落.Add(段落); var 挂载编号 = 挂载段落.Count - 1;
            if (效果别名到挂载号.ContainsKey(效果别名)) 
            { Debug.Print("已挂载相同名称的效果器，将替换挂载。"); 效果别名到挂载号[效果别名] = 挂载编号; 签名到到挂载号[生成流与段落签名(流编号, 段落编号)] = 挂载编号; }
            else
            {
                if (!签名到到挂载号.ContainsKey(生成流与段落签名(流编号, 段落编号)))
                {
                    效果别名到挂载号.Add(效果别名, 挂载编号); 签名到到挂载号.Add(生成流与段落签名(流编号, 段落编号), 挂载编号);
                }
                else
                {
                    效果别名到挂载号.Add(效果别名, 挂载编号);
                }
                
            }
            if (!流编号集.Contains(流编号)) { 流编号集.Add(流编号); }
            return 挂载编号;
        }

        public static void 使用效果(byte[] 音频缓冲区, int 偏移值, int 读取大小, int 流编号, int 当前段落号, WaveFormat 格式信息)
        {
            if (!流编号集.Contains(流编号)) { Debug.Print("该流不在处理范围内，尝试加入该流进行处理"); }
            var 段落签名 = 生成流与段落签名(流编号, 当前段落号); int 挂载号 = -1;
            if (签名到到挂载号.ContainsKey(段落签名)) { 挂载号 = 签名到到挂载号[段落签名]; }
            if(挂载号 == -1) { Debug.Print("段落没有被正确挂载，无法使用效果"); }
            var 挂载信息 = 挂载段落[挂载号];
            var 音量开始值 = (float)挂载信息.已处理字节 / (float)挂载信息.总处理字节;
            var 音量目标值 = ((float)挂载信息.已处理字节 + (float)音频缓冲区.Length) / (float)挂载信息.总处理字节;
            var 每字节增减值 = (float)Math.Round( 1f / (float)挂载信息.总处理字节,7);

            var 处理大小 = 挂载信息.总处理字节 - 挂载信息.已处理字节;
            if (处理大小 > 音频缓冲区.Length) { 处理大小 = 音频缓冲区.Length; }

            int 额外大小 = 音频缓冲区.Length - 处理大小;
            if (挂载信息.模式 == 段落信息.效果模式.淡入)
            {
                if (挂载信息.已处理字节 >= 挂载信息.总处理字节) { Debug.Print("已淡入，跳过处理");return; }
                淡入淡出流(音频缓冲区, 偏移值, 处理大小, 音量开始值, 每字节增减值,格式信息.BlockAlign);
                if (额外大小 > 0)
                {
                    Debug.Print("是额外数据，不再处理额外部分");
                }
                if (额外大小 + 处理大小 == 音频缓冲区.Length) { Debug.Print("已完整处理淡入"); }
                Debug.Print($"{流编号}.{当前段落号} 淡入 - {挂载信息.已处理字节}/{挂载信息.总处理字节} + {音频缓冲区.Length} <{处理大小}/{额外大小}> [{每字节增减值}] - {音量开始值}/{音量目标值}");
            }

            if (挂载信息.模式 == 段落信息.效果模式.淡出)
            {
                if (挂载信息.已处理字节 >= 挂载信息.总处理字节) 
                { 
                    静音(音频缓冲区, 偏移值, 额外大小, 格式信息.BlockAlign);  
                    Debug.Print("已淡出，静音"); 
                    return; 
                }
                淡入淡出流(音频缓冲区, 偏移值, 处理大小, 1- 音量开始值, -每字节增减值, 格式信息.BlockAlign);
                if (额外大小 > 0)
                {
                    静音(音频缓冲区, 处理大小, 额外大小, 格式信息.BlockAlign);
                    Debug.Print("是额外数据，已静音");
                }
                if (额外大小 + 处理大小 == 音频缓冲区.Length) { Debug.Print("已完整处理淡出"); }
                Debug.Print($"{流编号}.{当前段落号} 淡出 - {挂载信息.已处理字节}/{挂载信息.总处理字节} + {音频缓冲区.Length} <{处理大小}/{额外大小}> [{-每字节增减值}] - {1-音量开始值}/{1-音量目标值}");
            }
            if (挂载信息.模式 == 段落信息.效果模式.完全静音)
            {
                静音(音频缓冲区, 处理大小, 额外大小, 格式信息.BlockAlign);
            }

            挂载信息.已处理字节 += 处理大小;
            //从音量开始值不断累加或累减每字节增加值，这样就不用再判定是否已完成累加。
            //首先调用特效函数传入【处理大小】与每字节增减值，这样处理大小被处理后，之后流就不会再被处理
            //对于额外大小部分，若是淡入，则之后不处理，若是淡出，则应用静音函数，传入【处理大小】
            //最后判断处理大小是否等于音频缓冲区大小，如果等于，则本次处理没问题，累加到【挂载信息.已处理字节】中


        }

        public static int 生成流与段落签名(int 流编号, int 段落编号)
        {
            return 流编号 * 10000 + 段落编号;
        }

        #region 特效函数

        public static void 混流(byte[] 缓冲流A, byte[] 缓冲流B, int 偏移值, int 处理大小, int 块大小)
        {


        }

        private static void 淡入淡出流(byte[] 缓冲流, int 偏移值, int 处理大小,float 音量开始值,float 每字节增减值,int 块大小)
        {
            
            unsafe
            {
                float 左声道A, 右声道A, 左声道B, 右声道B;
                fixed (byte* 缓冲区指针 = 缓冲流)
                {
                    fixed (byte* 临时缓冲区指针 = 缓冲流)
                    {
                        float* 缓冲区数据A = (float*)(缓冲区指针      + 偏移值);
                        float* 缓冲区数据B = (float*)(临时缓冲区指针  + 偏移值);
                        int 流终点 = 处理大小 / 块大小 * 2;
                        for (int i = 0; i < 流终点; i += 2)
                        {
                            float 音量倍率 = 音量开始值 + 每字节增减值*2;
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
        private static void 静音(byte[] 缓冲流, int 偏移值, int 处理大小, int 块大小)
        {

            unsafe
            {
                float 左声道A, 右声道A, 左声道B, 右声道B;
                fixed (byte* 缓冲区指针 = 缓冲流)
                {
                    fixed (byte* 临时缓冲区指针 = 缓冲流)
                    {
                        float* 缓冲区数据A = (float*)(缓冲区指针 + 偏移值);
                        float* 缓冲区数据B = (float*)(临时缓冲区指针 + 偏移值);
                        int 流终点 = 处理大小 / 块大小 * 2;
                        for (int i = 0; i < 流终点; i += 2)
                        {
                            float 音量倍率 = 0;
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

        #endregion

    }
}
