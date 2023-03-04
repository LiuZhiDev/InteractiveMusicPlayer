using System.Windows.Controls;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 段落项.xaml 的交互逻辑
    /// </summary>
    public partial class 段落项 : UserControl
    {
        private string? _文件名;
        public string? 文件名
        {
            get { return _文件名; }
            set { _文件名 = value; 文件名显示.Text = _文件名; }
        }

        private float _BPM;
        public float BPM
        {
            get { return _BPM; }
            set { _BPM = value; BPM显示.Text = _BPM + "BPM"; }
        }
        private string? _章节名;
        public string? 章节名
        {
            get { return _章节名; }
            set { _章节名 = value; 章节名显示.Text = _章节名; }
        }
        private string? _拍子;
        public string? 拍子
        {
            get { return _拍子; }
            set { _拍子 = value; 节拍显示.Text = _拍子; }
        }

        private bool _循环;
        public bool 循环
        {
            get { return _循环; }
            set { _循环 = value; 允许循环.IsChecked = _循环; }
        }

        private double _Offset;
        public double Offset
        {
            get { return _Offset; }
            set { _Offset = value; Offset显示.Text = _Offset.ToString(); }
        }

        public 段落项()
        {
            InitializeComponent();
        }
    }
}
