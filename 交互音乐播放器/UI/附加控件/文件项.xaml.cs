using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using 交互音乐播放器.数据;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 文件项.xaml 的交互逻辑
    /// </summary>
    public partial class 文件项 : UserControl
    {
        #region 属性
        public object 锁 = new object();
        private SolidColorBrush? _循环类型颜色;
        public SolidColorBrush? 循环类型颜色
        {
            get { return _循环类型颜色; }
            set { _循环类型颜色 = value; 循环类型栏.Background = _循环类型颜色; }
        }

        private string? _循环类型;
        public string? 循环类型
        {
            get { return _循环类型; }
            set { _循环类型 = value; 循环类型名称.Text = _循环类型; }
        }

        private string? _速度信息;
        public string? 速度信息
        {
            get { return _速度信息; }
            set { _速度信息 = value; 速度信息栏.Text = _速度信息; }
        }

        private string? _曲目名称;
        public string? 曲目名称
        {
            get { return _曲目名称; }
            set { _曲目名称 = value; 曲目名称信息栏.Text = _曲目名称; }
        }

        private string? _专辑名称;
        public string? 专辑名称
        {
            get { return _专辑名称; }
            set { _专辑名称 = value; 专辑名称信息.Text = _专辑名称; }
        }

        private string? _作者名称;
        public string? 作者名称
        {
            get { return _作者名称; }
            set { _作者名称 = value; 作者名称信息.Text = _作者名称; }
        }


        public 脚本文件数据? 脚本数据;
        #endregion

        public 文件项()
        {
            InitializeComponent();
        }
        public void 设置文件项(脚本文件数据 脚本数据, bool 包含图像)
        {
            this.脚本数据 = 脚本数据;
            曲目名称 = 脚本数据.名称;
            作者名称 = 脚本数据.作者;
            专辑名称 = 脚本数据.专辑;
            速度信息 = $"{脚本数据.默认BPM}BPM {脚本数据.默认小节节拍分量}/{脚本数据.默认小节节拍总数}";
            if (脚本数据.默认BPM == -1)
            {
                速度信息 = $"默认设定";
            }
            if (!脚本数据.拥有配置文件)
            {
                循环类型 = "仅播放";
            }
            if (脚本数据.文件组.Count>=2)
            {
                循环类型颜色 = 数据.UI管理器.色彩[UI管理器.色彩集.音乐结束颜色];
                循环类型 = "默认循环";
            }
            if (脚本数据.拥有配置文件)
            {
                循环类型颜色 = 数据.UI管理器.色彩[UI管理器.色彩集.音乐循环颜色];
                循环类型 = "永续循环";
            }
            if (脚本数据.拥有脚本)
            {
                循环类型颜色 = 数据.UI管理器.色彩[UI管理器.色彩集.音乐等待颜色];
                循环类型 = "脚本控制";
            }

            if (包含图像 && 脚本数据.专辑图片.ImageSource != null)
            {

                 专辑背景.Source = 脚本数据.专辑图片.ImageSource;

            }
        }
        public void 设置专辑图像()
        {

        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
