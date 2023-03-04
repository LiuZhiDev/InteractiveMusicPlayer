using System.Collections.Generic;

namespace 交互音乐播放器.UI
{
    internal static class 文本
    {
        static public Dictionary<文本类型, string> _文本 = new Dictionary<文本类型, string>();
        static Dictionary<文本类型, string> _中文文本 = new Dictionary<文本类型, string>();
        public enum 文本类型
        {
            搜索栏_搜索栏空提示
        }
        static public void 加载多语言文本()
        {
            加载中文();
        }
        static public void 加载中文()
        {
            if (_中文文本.Count > 0) { return; }
            _中文文本.Add(文本类型.搜索栏_搜索栏空提示, "检索列表...");
        }
        static public void 设置为中文()
        {
            _文本 = _中文文本;
        }
    }
}
