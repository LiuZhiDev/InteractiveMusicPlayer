using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 交互音乐播放器.数据.播放中数据
{
    public class 计划
    {
        public enum 事件
        {
            切换段落,
            切换至循环段落,
            切换至循环等待段落,
            再次循环,
            播放文件,
            自动处理所有Cue点,
            到达指定CUE点并切换,
            到达指定CUE点并执行脚本,
            播放至指定CUE点并执行脚本,
            处理两点循环,
            播放完毕,
            按下默认按钮,
            切换至可控制段落
        }
        public enum 等待
        {
            切换至循环等待段落,
        }
        public class 按钮数据
        {
            public enum _按钮类型
            {
                正常进行,
                等待操作,
                结束,
                无限循环
            }
            public string? 绑定的脚本 { get; set; }
            public bool 允许显示 { get; set; }
            public string? 按钮显示名称 { get; set; }
            public string? 可用段落 { get; set; }
            public _按钮类型 按钮类型 { get; set; }
        }
        /// <summary>
        /// 全局触发的事件信息，通过查询事件表来执行相应的方法
        /// </summary>
        public static List<事件> 全局事件 { get; set; } = new List<事件>();
        /// <summary>
        /// 全局触发的等待信息，通过查询等待表来执行相应的方法
        /// </summary>
        public static List<等待> 全局等待 { get; set; } = new List<等待>();
        /// <summary>
        /// 执行该段落时默认使用的按钮，如果为无，则不显示
        /// </summary>
        public static 按钮数据? 全局默认按钮 { get; set; }
        /// <summary>
        /// 获取本播放集的所有按钮，若没有绑定，则为Null
        /// 结果为<按钮名称,按钮信息>
        /// [未优化] 应该编写方法进行查询与访问
        /// </summary>
        public static Dictionary<string, 按钮数据>? 全局按钮集 { get; set; } = new();
        
        
    }
}
