using System.Windows.Controls;
using System.Windows.Input;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 文件夹按钮.xaml 的交互逻辑
    /// </summary>
    public partial class 文件夹项 : UserControl
    {
        private string? _文件夹名称;
        public string? 文件夹名称
        {
            get { return _文件夹名称; }
            set { _文件夹名称 = value; 文件夹显示名称.Text = _文件夹名称; }
        }
        public string? 文件夹路径;
        public 文件夹项()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
