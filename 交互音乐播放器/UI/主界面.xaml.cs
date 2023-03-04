using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using 交互音乐播放器.UI;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据;
using 交互音乐播放器.数据.播放中数据;

namespace 交互音乐播放器
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class 主界面 : Panuon.WPF.UI.WindowX
    {
        #region 初始变量
        public static 主界面? 当前界面;
        public static 数据.UI管理器? UI;
        public static Slider 实例音量滑条 { get; set; } //暂时不知道怎么取到这个滑条，先定义在这里吧
        #endregion
        #region 构造函数
        public 主界面()
        {
#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            Initialized += 主界面_Initialized;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            InitializeComponent();
        }

        private void 主界面_Initialized(object sender, EventArgs e)
        {
            程序入口();
            测试入口();
        }
        #endregion
        #region 程序与测试入口
        private void 程序入口()
        {
            当前界面 = this;
            默认按钮.主按钮反馈 = 主按钮按下;
            默认按钮.菜单按钮反馈 = 菜单按钮按下;
            系统控制.读取配置文件();
            文本.加载多语言文本();
            文本.设置为中文();
            UI = new UI管理器();
            设置页面(UI管理器.程序页.文件管理器);
            动态更新初始化(UI界面数据.UI动态更新时间);
            测试入口();
        }

        private void 测试入口()
        {


        }
        #endregion
        #region 页面动态更新
        private void 动态更新初始化(int 更新间隔)
        {
            Task 新线程 = new Task(() =>
            {

                while (UI界面数据.程序执行)
                {
                    动态更新();
                    Thread.Sleep(更新间隔);
                }
            });
            新线程.Start();
            系统控制.线程.Add(新线程);
        }

        public void 动态更新()
        {
            设置播放信息(UI界面数据.播放进度条);
            UI界面数据.更新界面数据(this);
        }

        #endregion
        #region 交互方法
        public void 设置播放信息(double 百分比)
        {
            double 总进度背景大小 = 0;
            this.Dispatcher.Invoke(() =>
            {
                if (UI界面数据.音量大小 >= 100) { UI界面数据.音量大小 = 100; }
                if (UI界面数据.音量大小 <= 0) { UI界面数据.音量大小 = 0; 音量控制.Content = $"🔇 {UI界面数据.音量大小}"; }
                if (UI界面数据.音量大小 <= 20 && UI界面数据.音量大小 != 0) { 音量控制.Content = $"🔈 {UI界面数据.音量大小}"; }
                if (UI界面数据.音量大小 > 20 && UI界面数据.音量大小 != 0) { 音量控制.Content = $"🔉 {UI界面数据.音量大小}"; }
                if (UI界面数据.音量大小 >= 70) { 音量控制.Content = $"🔊 {UI界面数据.音量大小}"; }
                音量滑条.Content = UI界面数据.音量大小;
                if (实例音量滑条 != null) { 实例音量滑条.Value = UI界面数据.音量大小; }
              
                总进度背景大小 = 进度条空.ActualWidth;
            });
            var 应该的进度条大小 = 总进度背景大小 * (百分比 * 0.01);
            var 应该的当前时间 = UI界面数据.总播放时间.TotalSeconds * (百分比 * 0.01);

            歌词显示窗.更新歌词();

            this.Dispatcher.Invoke(() =>
            {
                进度条满.Width = 应该的进度条大小;
                播放时间显示.Text = $"{UI界面数据.当前播放时间.ToString(@"mm\:ss")}/{UI界面数据.总播放时间.ToString(@"mm\:ss")}";
                if (UI界面数据.当前小节 == UI界面数据.当前拍 && UI界面数据.当前拍 == 0) { 节拍显示.Visibility = Visibility.Collapsed; }
                else { 节拍显示.Visibility = Visibility.Visible; 节拍显示.Text = $"{UI界面数据.当前小节}.{UI界面数据.当前拍}"; }
            });

            if (音频控制中间件.当前中间件 != null &&
                音频控制中间件.播放数据 != null &&
                音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前流) == false &&
                音频控制中间件.播放数据.当前流!.当前段落!.状态 == 段落信息.播放状态.播放中)
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (UI界面数据.专辑图片 != null && UI界面数据.是否已经更换过文件())
                    {
                        曲名.Text = UI界面数据.曲名;
                        作者.Text = UI界面数据.作者;

                        专辑图片.Source = UI界面数据.专辑图片.ImageSource;
                    }
                    播放与暂停按钮.Content = "\ue6a9";
                });
            }
            if (音频控制中间件.当前中间件 != null &&
                 音频控制中间件.播放数据 != null &&
               音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前流) == false &&
               音频控制中间件.播放数据.当前流!.当前段落!.状态 != 段落信息.播放状态.播放中)
            {
                this.Dispatcher.Invoke(() =>
                {
                    播放与暂停按钮.Content = "\ue6a8";
                });
            }

            if (音频控制中间件.当前中间件 != null && 音频控制中间件.播放数据 != null &&
               音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前流) == false)
            {

                if (UI界面数据.默认按钮颜色 == null) { return; }
                this.Dispatcher.Invoke(() =>
                {
                    var 不透明颜色 = UI界面数据.默认按钮颜色.Clone();
                    var 不透明颜色1 = 不透明颜色.Color + Color.FromArgb(255, 0, 0, 0);
                    默认按钮.按钮文本 = UI界面数据.默认按钮名称;
                    默认按钮.图标 = UI界面数据.默认按钮符号;
                    默认按钮.主按钮.IsEnabled = UI界面数据.默认按钮状态;
                    默认按钮.主按钮.Background = UI界面数据.默认按钮颜色;
                    默认按钮.下拉菜单按钮.Background = UI界面数据.默认按钮颜色;
                    默认按钮.主按钮.BorderBrush = new SolidColorBrush(不透明颜色1);
                    默认按钮.下拉菜单按钮.BorderBrush = new SolidColorBrush(不透明颜色1);
                    if (UI界面数据.默认按钮颜色 != null)
                    {
                        默认按钮.阴影颜色.Color = 不透明颜色1;
                    }

                });
            }

        }

        private void 显隐进度条控制器(UI界面数据.显示状态 状态)
        {
            if (状态 == UI界面数据.显示状态.关闭) { 进度条控制器.Visibility = Visibility.Collapsed; return; }
            if (状态 == UI界面数据.显示状态.启用)
            {
                进度条控制器.Visibility = Visibility.Visible;
                return;
            }
        }

        private void 修改进度控制器位置(MouseEventArgs 鼠标事件)
        {
            Point 鼠标位置 = 鼠标事件.GetPosition(进度条空);
            进度条控制器.Width = 鼠标位置.X;
        }

        public void 定位位置(float 百分比位置)
        {
            if (音频控制中间件.当前中间件 != null && 音频控制中间件.播放数据 != null &&
               !音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前流) &&
               音频控制中间件.播放数据.当前流!.当前段落!.状态 != 段落信息.播放状态.停止)
            {
                音频控制中间件.当前中间件.命令_全部重定位(百分比位置);
            }
        }

        private void 设置页面(UI管理器.程序页 页面)
        {
            UI!.设置当前页面(页面);
            Visibility 文件管理器按钮切换开关 = Visibility.Visible;
            Visibility 实时跳转管理器按钮切换开关 = Visibility.Visible;
            Visibility 脚本编辑器按钮切换开关 = Visibility.Visible;
            string 图标 = "";
            SolidColorBrush 背景颜色 = null!;
            SolidColorBrush 边框颜色 = null!;
            SolidColorBrush 图标颜色 = null!;
            //根据不同页面类型读取不同的值
            if (页面 == UI管理器.程序页.文件管理器)
            {
                文件管理器按钮切换开关 = Visibility.Collapsed;
                背景颜色 = UI管理器.色彩[UI管理器.色彩集.文件管理器背景色];
                边框颜色 = UI管理器.色彩[UI管理器.色彩集.文件管理器边框色];
                图标颜色 = UI管理器.色彩[UI管理器.色彩集.文件管理器图标色];
                图标 = UI管理器.图标[UI管理器.图标集.文件管理器图标];
            }
            if (页面 == UI管理器.程序页.实时管理器)
            {
                实时跳转管理器按钮切换开关 = Visibility.Collapsed;
                背景颜色 = UI管理器.色彩[UI管理器.色彩集.实时跳转管理器背景色];
                边框颜色 = UI管理器.色彩[UI管理器.色彩集.实时跳转管理器边框色];
                图标颜色 = UI管理器.色彩[UI管理器.色彩集.实时跳转管理器图标色];
                图标 = UI管理器.图标[UI管理器.图标集.实时跳转管理器图标];
            }
            if (页面 == UI管理器.程序页.脚本编辑器)
            {
                脚本编辑器按钮切换开关 = Visibility.Collapsed;
                背景颜色 = UI管理器.色彩[UI管理器.色彩集.脚本编辑器背景色];
                边框颜色 = UI管理器.色彩[UI管理器.色彩集.脚本编辑器边框色];
                图标颜色 = UI管理器.色彩[UI管理器.色彩集.脚本编辑器图标色];
                图标 = UI管理器.图标[UI管理器.图标集.脚本编辑器图标];
            }
            当前页面图标背景色.Background = 背景颜色;
            当前页面图标边框色.Background = 边框颜色;
            当前页面图标.Foreground = 图标颜色;
            当前页面图标.Text = 图标;
            文件管理器切换按钮.Visibility = 文件管理器按钮切换开关;
            实时跳转管理器切换按钮.Visibility = 实时跳转管理器按钮切换开关;
            脚本编辑器切换按钮.Visibility = 脚本编辑器按钮切换开关;
            页面名称.Text = 页面.ToString();
        }

        public void 主按钮按下()
        {
            if (音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前段落)
                || 计划.全局事件 == null || 脚本解析器.当前解析 == null)
            {
                return;
            }
            计划.全局事件.Add(计划.事件.按下默认按钮);
            UI界面数据.默认按钮名称 = "处理...";
            UI界面数据.默认按钮状态 = false;

        }

        public void 菜单按钮按下()
        {

        }
        #endregion
        #region 按钮事件
        private void 进度条组_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine("已经移动到进度条内");
            显隐进度条控制器(UI界面数据.显示状态.启用);
        }

        private void 进度条组_MouseLeave(object sender, MouseEventArgs e)
        {
            Console.WriteLine("已经移动到进度条外");
            显隐进度条控制器(UI界面数据.显示状态.关闭);
        }

        private void 进度条组_MouseMove(object sender, MouseEventArgs e)
        {
            修改进度控制器位置(e);
        }

        private void 进度条组_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point 鼠标位置 = e.GetPosition(进度条空);
            UI界面数据.播放进度条 = 鼠标位置.X / 进度条空.ActualWidth * 100;
            定位位置((float)UI界面数据.播放进度条);
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UI界面数据.程序执行 = false;
            foreach (var 线程 in 系统控制.线程)
            {
                线程.WaitAsync(TimeSpan.FromSeconds(2));
            }
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            foreach (var 线程 in 系统控制.线程)
            {
                线程.WaitAsync(TimeSpan.FromSeconds(2));
            }
            Application.Current.Shutdown();
        }

        private void 最大化_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            { this.WindowState = WindowState.Normal; return; }
            else { this.WindowState = WindowState.Maximized; }

        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void 切换到文件管理器_Click(object sender, RoutedEventArgs e)
        {
            设置页面(UI管理器.程序页.文件管理器);
        }

        private void 切换到实时跳转管理器_Click(object sender, RoutedEventArgs e)
        {
            设置页面(UI管理器.程序页.实时管理器);
        }

        private void 切换到脚本编辑器_Click(object sender, RoutedEventArgs e)
        {
            设置页面(UI管理器.程序页.脚本编辑器);
        }

        private void 播放与暂停按钮_Click(object sender, RoutedEventArgs e)
        {
            if (音频控制中间件.播放数据 == null || 音频控制中间件.播放数据.空值或空引用(音乐播放数据.播放数据组.当前段落)) { return; }
            if (音频控制中间件.当前音频控制器 == null) { return; }
            if (音频控制中间件.播放数据.当前流!.当前段落.状态 == 段落信息.播放状态.播放中) { 音频控制中间件.当前音频控制器.命令_暂停所有(); return; }
            if (音频控制中间件.播放数据.当前流!.当前段落.状态 == 段落信息.播放状态.暂停) { 音频控制中间件.当前音频控制器.命令_继续所有(); return; }
            if (音频控制中间件.播放数据.当前流!.当前段落.状态 == 段落信息.播放状态.停止) { 音频控制中间件.当前音频控制器.命令_继续所有(); return; }
        }

        private void 音量控制_Click(object sender, RoutedEventArgs e)
        {
            if (UI界面数据.音量大小 != 0)
            {
                UI界面数据.暂存音量大小 = UI界面数据.音量大小;
                UI界面数据.音量大小 = 0;
            }
            else { UI界面数据.音量大小 = UI界面数据.暂存音量大小; }


            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前段落!.状态 == 段落信息.播放状态.播放中)
            {
                if (音频控制中间件.当前中间件 != null)
                {
                    音频控制中间件.当前中间件.命令_设定总音量(false, UI界面数据.音量大小);
                }
            }
        }

        private void 音量控制_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UI界面数据.音量大小 += e.Delta / 70;

            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前段落!.状态 == 段落信息.播放状态.播放中)
            {
                if (音频控制中间件.当前中间件 != null)
                {
                    音频控制中间件.当前中间件.命令_设定总音量(false, UI界面数据.音量大小);
                }
            }

        }

        private void 停止按钮_Click(object sender, RoutedEventArgs e)
        {
            if (音频控制中间件.当前中间件 != null)
            {
                音频控制中间件.当前中间件.命令_停止所有();
            }
        }

        private void 音量控制_MouseEnter(object sender, MouseEventArgs e)
        {
            浮动控制栏.Visibility = Visibility.Visible;
            
        }

        private void 浮动控制栏_MouseLeave(object sender, MouseEventArgs e)
        {
            浮动控制栏.Visibility = Visibility.Collapsed;
        }

        private void 滑动条数据_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            实例音量滑条 = (Slider)sender;
        }

        private void 滑动条数据_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var 滑条 = (Slider)sender;
            UI界面数据.音量大小 = (int)滑条.Value;
            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前段落!.状态 == 段落信息.播放状态.播放中)
            {
                if (音频控制中间件.当前中间件 != null)
                {
                    音频控制中间件.当前中间件.命令_设定总音量(false, UI界面数据.音量大小);
                }
            }
        }


    }
    #endregion
}
