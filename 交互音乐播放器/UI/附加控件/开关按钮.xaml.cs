using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class 开关按钮 : UserControl
    {
        public delegate void 主按钮按下反馈();
        public 主按钮按下反馈? 主按钮反馈;
        public delegate void 菜单按钮按下反馈();
        public 菜单按钮按下反馈? 菜单按钮反馈;
        private string? _按钮文本;
        public string? 按钮文本
        {
            get { return _按钮文本; }
            set { _按钮文本 = value; 按钮文本框.Text = _按钮文本; }
        }

        private string? _图标;
        public string? 图标
        {
            get { return _图标; }
            set { _图标 = value; 菜单图标.Text = _图标; }
        }
        public 开关按钮()
        {
            InitializeComponent();

        }

        private void 生成颜色动画(Border 边框按钮, List<EasingColorKeyFrame> 原点动画颜色, List<EasingColorKeyFrame> 终点动画颜色)
        {

        }

        private void 生成透明度动画(Border 边框按钮, double 目标透明度)
        {

            DoubleAnimation 透明度动画 = new DoubleAnimation { From = 边框按钮.Opacity, To = 目标透明度, Duration = new Duration(TimeSpan.FromSeconds(0.1)) };
            边框按钮.BeginAnimation(Border.OpacityProperty, 透明度动画);

        }

        private void 主按钮_MouseEnter(object sender, MouseEventArgs e)
        {
            生成透明度动画((Border)sender, 1);
        }

        private void 主按钮_MouseLeave(object sender, MouseEventArgs e)
        {
            生成透明度动画((Border)sender, 0.85);
        }

        private void 主按钮_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            生成透明度动画((Border)sender, 0.4);
        }

        private void 主按钮_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            生成透明度动画((Border)sender, 0.8);
            if (主按钮反馈 != null) { 主按钮反馈(); }
            //逻辑代码
            Console.WriteLine("执行逻辑代码1");
        }

        private void 下拉菜单按钮_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            生成透明度动画((Border)sender, 0.4);
        }

        private void 下拉菜单按钮_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            生成透明度动画((Border)sender, 0.8);
            if (菜单按钮反馈 != null) { 菜单按钮反馈(); }
            //逻辑代码
            Console.WriteLine("执行逻辑代码2");
        }

        private void 下拉菜单按钮_MouseEnter(object sender, MouseEventArgs e)
        {
            生成透明度动画((Border)sender, 1);
        }

        private void 下拉菜单按钮_MouseLeave(object sender, MouseEventArgs e)
        {
            生成透明度动画((Border)sender, 0.85);
        }
    }
}
