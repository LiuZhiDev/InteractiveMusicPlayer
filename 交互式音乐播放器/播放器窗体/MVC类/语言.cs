/// <summary>
/// 该命名空间主要存放播放器中关于各类符号设定、语言设定的信息
/// </summary>
namespace 交互式音乐播放器.语言
{
    /// <summary>
    /// 管理项目中需要使用的图标
    /// </summary>
    public static class 图标
    {

        public static string 文件夹图标 { get; } = @"📁";
        public static string 永续图标 { get; } = @"♾️";
        public static string 章节图标 { get; } = @"➰";
        public static string 无缝式章节图标 { get; } = @"➿";
        public static string 不可循环图标 { get; } = @"〰️";
        public static string 步进循环图标 { get; } = @"🔃";
        public static string 强制循环图标 { get; } = @"↔️";
    }

    /// <summary>
    /// 管理项目中使用音频文件的类型信息
    /// </summary>
    public class 音频类型
    {
        public enum 循环类型
        {
            不可循环,
            列表循环,
            永续,
            章节循环,
            无缝章节循环,
            步进循环,
            强制循环
        }

        public enum 文件模式
        {
            单一文件模式,
            多个文件模式
        }

    }
    /// <summary>
    /// 管理当前项目中所要定义的音频状态
    /// </summary>
    public class 音频状态
    {
        public enum 默认段落
        {
            起始段落,
            循环段落

        }
        public enum 播放状态
        {
            静音,
            播放中,
            暂停中,
            停止中,
            未定义
        }

    }
}
