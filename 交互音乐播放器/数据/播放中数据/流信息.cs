using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace 交互音乐播放器.数据.播放中数据
{
    /// <summary>
    /// 储存当前播放流正使用的信息，包括段落等
    /// </summary>
    public class 流信息
    {
        /// <summary>
        /// 流所正在使用的段落，引用至段落信息
        /// </summary>
        public 段落信息 当前段落 { get; set; } = new 段落信息();
        /// <summary>
        /// 获取本流的别名
        /// </summary>
        public string 流别名 { get; set; } = "未设置流别名";
        /// <summary>
        /// 获取流编号
        /// </summary>
        public int 流编号 { get; set; } = -1;
        /// <summary>
        /// 设置或获取播放器的偏移值，因为播放器播放数据为连续数据，需要使用偏移获取正确的位置.
        /// 单位 字节
        /// </summary>
        public long 播放器偏移 { get; set; } = 0;
        /// <summary>
        /// 在绑定的段落读取的字节数，用于计算播放器偏移
        /// </summary>
        public long 段落读取字节数 { get; set; } = 0;
        /// <summary>
        /// 绑定一个流，在要更换流的时候执行
        /// </summary>
        public 流信息? 下一流信息 { get; set; }
        /// <summary>
        /// 返回流的实例
        /// </summary>
        public object? 流实例 { get; set; }
        /// <summary>
        /// 返回播放器的实例
        /// </summary>
        public object? 播放器实例 { get; set; }
    }
}
