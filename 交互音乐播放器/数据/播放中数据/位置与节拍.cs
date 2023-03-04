using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 交互音乐播放器.数据.播放中数据
{
    /// <summary>
    /// 关于节拍的信息
    /// </summary>
    public class 节拍
    {
        private int _小节;
        private int _拍;
        /// <summary>
        /// 节拍中的小节，从一个强拍到下一个强拍之间的部分即称一小节。
        /// </summary>
        public int 小节 { get { return _小节; } set { _小节 = value; 计算结构体其他信息(); } }
        /// <summary>
        /// 在听觉上感到音量有周期性的强弱
        /// </summary>
        public int 拍 { get { return _拍; } set { _拍 = value; 计算结构体其他信息(); } } 
        /// <summary>
        /// 每小节一共多少拍数
        /// </summary>
        public int 每小节拍数 { get; set; }
        /// <summary>
        /// 自动计算的总拍数，获取总共的节拍数量
        /// </summary>
        public int 总拍数 { get; set; }
        /// <summary>
        /// 每一拍所要存储的字节数量
        /// </summary>
        public double 每拍字节 { get; set; } = -1;
        public float BPM { get; set; } = -1;

        public 节拍(int 初始小节 , int 初始拍号 , int 小节最大拍数,int 每拍的字节数)
        {
            _小节 = 初始小节;
            _拍 = 初始拍号;
            this.每小节拍数 = 0;
            this.总拍数 = 0;
            this.每拍字节 = 0;
            计算结构体其他信息();
        }

        private void 计算结构体其他信息()
        {
            总拍数 = (小节 - 1) * 每小节拍数 + 拍;
        }

    }

    public class 位置
    {
        public TimeSpan 当前时间 { get; set; } = new TimeSpan(0);
        public TimeSpan 总时间 { get; set; } = new TimeSpan(0);
        public TimeSpan 剩余时间 { get; set; } = new TimeSpan(0);
        public long 已播放字节 { get; set; } = 0;
        public long 总字节 { get; set; } = 0;
        public long 剩余字节 { get; set; } = 0;
        public 位置()
        {

        }
    }
}
