using System.Collections.Generic;
using System.Windows.Controls;

namespace 交互式音乐播放器.控件
{
    /// <summary>
    /// 音频文件项.xaml 的交互逻辑
    /// </summary>
    public partial class 音频文件项 : UserControl
    {
        /// <summary>
        /// 获得此音乐的一个或多个文件路径表
        /// </summary>
        public List<string> 文件路径 { get; set; } = new List<string>(5);
        public string 配置文件路径 { get; set; }
        public 文件浏览中间件.文件信息.项目类型 文件格式 { get; set; } = 文件浏览中间件.文件信息.项目类型.未知;
        public 音频文件项()
        {
            InitializeComponent();
        }
    }
}
