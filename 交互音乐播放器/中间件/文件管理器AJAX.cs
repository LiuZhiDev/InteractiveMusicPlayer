using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using 交互音乐播放器.UI.附加控件;

namespace 交互音乐播放器.中间件
{
    internal static class 文件管理器AJAX
    {
        public static Dictionary<string, 文件项> 文件项组 = new Dictionary<string, 文件项>();
        public static List<ListBoxItem> 显示列表 = new List<ListBoxItem>();
        public static void 清空文件组()
        {
            文件项组.Clear();
        }


    }
}
