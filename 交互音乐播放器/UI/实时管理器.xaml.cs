using System.Windows;
using System.Windows.Controls;
using 交互音乐播放器.UI.附加控件;

namespace 交互音乐播放器.UI
{
    /// <summary>
    /// 实时管理器.xaml 的交互逻辑
    /// </summary>
    public partial class 实时管理器 : Page
    {
        public 实时管理器()
        {
            InitializeComponent();
        }

        private void 歌词控制_Click(object sender, RoutedEventArgs e)
        {
            if (歌词显示窗.是否打开)
            {
                歌词显示窗.当前显示窗.Close();
            }
            else
            {
                交互音乐播放器.UI.附加控件.歌词显示窗 歌词显示 = new UI.附加控件.歌词显示窗();
                歌词显示.Show();
            }


        }

        private void 切换歌词穿透模式_Click(object sender, RoutedEventArgs e)
        {
            歌词显示窗.当前显示窗.切换穿透模式();
        }
    }
}
