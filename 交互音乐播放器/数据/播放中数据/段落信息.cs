using System.Collections.Generic;
using static 交互音乐播放器.数据.音乐播放数据;

namespace 交互音乐播放器.数据.播放中数据
{
    /// <summary>
    /// 储存段落的信息
    /// </summary>
    public class 段落信息
    {
        public class _循环等待数据
        {
            /// <summary>
            /// 循环等待数据的等待状态集
            /// </summary>
            public enum 等待状态
            {
                未开始等待,
                正在等待,
                脚本执行,
                结束
            }
            /// <summary>
            /// 当前等待状态，默认为未开始等待
            /// </summary>
            public 等待状态 当前等待状态 { get; set; } = 等待状态.未开始等待;
            /// <summary>
            /// 绑定的脚本，循环等待按键后一定会执行一个脚本，默认为“无脚本”
            /// </summary>
            public string 绑定的脚本 { get; set; } = "无脚本";
            /// <summary>
            /// 循环等待会要求用户按下按键，此处会新建一个按键，需要绑定按键的显示名称
            /// 默认为“默认按钮”
            /// </summary>
            public string 按钮显示名称 { get; set; } = "默认按钮";
        }
        public class Cue数据
        {
            public string Cue点名称 { get; set; } = "未配置点名称";
            public long 出点位置 { get; set; }
            public string 入点段落 { get; set; }
            public long 入点位置 { get; set; }
            public string? 绑定的脚本 { get; set; }
        }
        public enum 播放状态
        {
            播放中,
            暂停,
            停止
        }


        /// <summary> 
        /// 获取该段落的别名信息（在脚本执行时的别名）
        /// </summary>
        public string 别名 { get; set; } = "未设置名称";
        /// <summary>
        /// 获取该段落的编号
        /// </summary>
        public int 编号 { get; set; } = -1;
        /// <summary>
        /// 获取该段落在文件组中的真实编号、不受用户创建所影响
        /// </summary>
        public int 文件编号 { get; set; } = -1;
        /// <summary>
        /// 获取在该段落下，每个缓冲区所执行的时间（毫秒为单位）
        /// 可以使用【数据转换】将其他值转换为字节
        /// </summary>
        public double 缓冲区时间 { get; set; } = -1;
        /// <summary>
        /// 获取在该段落下，每次要读取的音频流大小（字节为单位）
        /// 可以使用【数据转换】将其他值转换为字节
        /// </summary>
        public int 缓冲区大小 { get; set; } = -1;
        /// <summary>
        /// 获取在该段落下，用于在计算节拍时对齐节拍值（字节为单位）
        /// 可以使用【数据转换】将其他值转换为字节
        /// </summary>
        public double Offset { get; set; } = -1;
        /// <summary>
        /// 获取该段落是否为Loop段落（可以从结尾无缝衔接到开头）
        /// </summary>
        public bool 启用循环 { get; set; } = false;
        /// <summary>
        /// 获取或设置该段落绑定的流，若没有绑定流，则为Null。
        /// 若已经有流正在读取，则可以访问到该流的信息。
        /// </summary>
        public 流信息? 绑定流 { get; set; }
        /// <summary>
        /// 获取或设置该段落绑定的文件，若没有绑定文件，则为Null。
        /// </summary>
        public 文件信息? 绑定文件 { get; set; }
        /// <summary>
        /// 获取该段落是否正在循环等待按键，若正在循环等待按键，则会一直循环该段落
        /// [未优化] 若是跨段落则应增加额外的处理逻辑
        /// </summary>
        public bool 循环等待 { get; set; } = false;
        /// <summary>
        /// 获取该段落的循环等待数据，若没有绑定，则为Null。
        /// </summary>
        public _循环等待数据? 循环等待数据 { get; set; }
        /// <summary>
        /// 获取当前该段落的节拍信息，此处为后台读取数据的节拍情况
        /// </summary>
        public 节拍 节拍信息_读取 { get; set; } = new 节拍(0, 0, 4, 0);
        /// <summary>
        /// 获取当前该段落的节拍信息，此处为音频前端播放数据的节拍情况
        /// </summary>
        public 节拍 节拍信息_显示 { get; set; } = new 节拍(0, 0, 4, 0);
        /// <summary>
        /// 获取当前段落的时间、字节信息
        /// </summary>
        public 位置 播放位置 { get; set; } = new 位置();
        /// <summary>
        /// 获取该段落绑定的下一个段落信息，若没有绑定，则为Null。
        /// </summary>
        public 段落信息? 下一段落链路 { get; set; }
        /// <summary>
        /// 获取该段落的Cue点信息集合，若没有绑定，则为Null。
        /// 结构为<Cue点标识符,Cue数据>
        /// [未优化] 应该编写方法进行查询与访问
        /// </summary>
        public Dictionary<string, Cue数据>? Cue点集 { get; set; }
        /// <summary>
        /// 段落的下一个Cue点信息，若没有绑定，则为Null
        /// </summary>
        public Cue数据? 下个Cue点 { get; set; }
        /// <summary>
        /// 段落的两点循环Cue信息，若没有绑定，则为Null
        /// </summary>
        public Cue数据? 两点循环Cue { get; set; }
        /// <summary>
        /// 获取当前段落的状态
        /// </summary>
        public 播放状态 状态 { get; 
            set;
        } = 播放状态.停止;
        /// <summary>
        /// 获取当前段落的循环次数
        /// </summary>
        public int 循环次数 { get; set; } = 0;
        /// <summary>
        /// 指示该段落已经完成初始化加载，用于计算播放器偏移。
        /// </summary>
        public bool 已加载一次 { get; set; }
        /// <summary>
        /// 指示该段落将不使用系统自带的默认切换逻辑，完全使用脚本控制，若脚本未加以控制，则播完停止
        /// </summary>
        public bool 禁用默认切换逻辑 { get; set; }
        /// <summary>
        /// 指示该段落将不使用系统自带的逻辑赋给当前段落，完全使用脚本控制，若脚本未加以控制，则会出现意外情况
        /// </summary>
        public bool 禁用默认赋值当前段落 { get; set; }
        /// <summary>
        /// 指示该段落已经通过Cue点读取部分字节，播放器的偏移需要重新计算
        /// </summary>
        public bool 已通过Cue点读取部分字节 { get; set; }
        /// <summary>
        /// 指示该段落已经通过Cue点读取部分字节的大小
        /// </summary>
        public double 已通过Cue点读取大小 { get; set; }
    }
}
