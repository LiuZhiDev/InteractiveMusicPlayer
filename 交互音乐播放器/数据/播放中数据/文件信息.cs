using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 交互音乐播放器.数据.播放中数据
{
    /// <summary>
    /// 储存包含文件别名、路径等关于文件的基础信息
    /// </summary>
    public class 文件信息
    {
        /// <summary>
        /// 文件的编号
        /// </summary>
        public int 编号 { get; set; } = -1;
        /// <summary>
        /// 文件的路径
        /// </summary>
        public string 路径 { get; set; } = "未定义路径";
        /// <summary>
        /// 文件的别名，是访问文件的唯一标识
        /// </summary>
        public string 别名 { get; set; } = "未定义别名";
        /// <summary>
        /// 返回音频文件的实例
        /// </summary>
        public object? 文件实例 { get; set; }
    }
}
