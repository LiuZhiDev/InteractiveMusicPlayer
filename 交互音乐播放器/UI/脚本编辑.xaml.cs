using ICSharpCode.AvalonEdit;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据;

namespace 交互音乐播放器.UI
{
    /// <summary>
    /// 脚本编辑.xaml 的交互逻辑
    /// </summary>
    public partial class 脚本编辑 : Page
    {
        #region 构造函数
        public static 脚本编辑? 当前脚本编辑器;
        public 脚本文件数据? 脚本文件;
        public 脚本编辑()
        {
            InitializeComponent();
            当前脚本编辑器 = this;
            设置脚本编辑器();
            更新脚本模板();
            脚本内容.TextArea.Caret.PositionChanged += 文本更改;
        }
        public void 载入文件(脚本文件数据 载入文件)
        {
            脚本文件 = 载入文件;
            检查脚本文件(脚本文件);
            更新文件视图(载入文件);
            更新数据视图(载入文件);

        }
        #endregion
        #region 脚本编辑器控制
        public void 设置脚本编辑器()
        {
            脚本编辑器控制.应用文档高亮(脚本内容, 系统控制.脚本高亮文档);

        }
        private void 文本更改(object? sender, EventArgs e)
        {
            var 编辑器 = (TextEditor)脚本内容;
            var 光标位置 = 脚本编辑器控制.获取输入光标所在行列(编辑器);
            var 当前行文本 = 脚本编辑器控制.取当前行文本(编辑器, 光标位置.Y坐标);
            var 当前方法名 = 脚本编辑器控制.取当前方法名(当前行文本);
            var 选择块数 = 脚本编辑器控制.当前选择块(当前行文本, 光标位置.X坐标);
            补全组件.智能提示过滤(补全组件.选项集, 补全组件.方法名称列表, 当前方法名);
            补全组件.更新参数提示(选择块数);
            补全组件.当前提示下标 = 选择块数;
            智能提示.IsOpen = true;
            Debug.Print($"{光标位置.X坐标},{光标位置.Y坐标} => 【{当前方法名}】 {当前行文本} | {选择块数}");
        }

        #endregion
        #region 数据与视图更新函数

        public void 检查脚本文件(脚本文件数据 载入文件)
        {
            for (int i = 0; i < 载入文件.文件组.Count; i++)
            {
                if (!File.Exists(载入文件.文件组[i]))
                {
                    //尝试修复文件路径
                    var 文件名 = Path.GetFileName(载入文件.文件组[i]);
                    var 文件夹 = UI界面数据.当前浏览文件夹;
                    载入文件.文件组[i] = $"{文件夹}\\{文件名}";
                    //若修复不成
                    if (!File.Exists(载入文件.文件组[i]))
                    {
                        MessageBox.Show($"{载入文件.名称}\r\n配置文件中存在无效的音乐文件路径，且无法自动寻找到文件，请右键定位到配置文件，删除或修改文件组数据。");
                    }
                }
            }

        }

        public void 更新文件视图(脚本文件数据 载入文件)
        {
            文件项.Children.Clear();
            UI.附加控件.文件项 文件信息 = new 文件项();
            文件信息.设置文件项(载入文件, false);
            文件项.Children.Add(文件信息);
        }
        public void 更新数据视图(脚本文件数据 载入文件)
        {
            曲名.Text = 载入文件.名称;
            作者.Text = 载入文件.作者;
            专辑.Text = 载入文件.专辑;
            BPM.Text = 载入文件.默认BPM.ToString();
            节拍.Text = $"{载入文件.默认小节节拍分量}/{载入文件.默认小节节拍总数}";
            段落集.Children.Clear();
            for (int i = 0; i < 载入文件.文件组!.Count; i++)
            {
                段落项 段落 = new 段落项();
                if (i < 载入文件.段落名称?.Count) { 段落.章节名 = 载入文件.段落名称[i]; }
                else { 段落.章节名 = "未配置段落名"; }

                if (i < 载入文件.小节节拍分量组?.Count && i < 载入文件.小节节拍总数组!.Count)
                { 段落.拍子 = $"{载入文件.小节节拍分量组[i]}/{载入文件.小节节拍总数组[i]}"; }
                else { 段落.拍子 = "4/4"; }

                if (i < 载入文件.BPM组?.Count) { 段落.BPM = 载入文件.BPM组[i]; }
                else { 段落.BPM = 60; }

                if (i < 载入文件.循环下标组?.Count) { 段落.循环 = 载入文件.循环下标组[i]; }
                else { 段落.循环 = false; }
                if (i < 载入文件.Offset组?.Count) { 段落.Offset = 载入文件.Offset组[i]; }
                else { 段落.Offset = 0; }
                段落集.Children.Add(段落);
            }
            if (载入文件.脚本基类 != null) { 脚本库.Text = 载入文件.脚本基类; }
            else { 脚本库.Text = ""; }
            if (载入文件.脚本文档 != null) { 脚本内容.Text = 载入文件.脚本文档; }
            else { 脚本内容.Text = ""; }
        }
        public void 更新并储存数据(脚本文件数据 载入文件)
        {
            //基础信息储存
            StringBuilder 错误信息 = new StringBuilder();
            载入文件.名称 = 曲名.Text;
            载入文件.作者 = 作者.Text;
            载入文件.专辑 = 专辑.Text;
            载入文件.默认BPM = float.Parse(BPM.Text);

            float tmpBPM = -1; var 储存信息 = float.TryParse(BPM.Text, out tmpBPM);
            if (!储存信息) { 错误信息.Append("BPM值 - BPM设置失败，将使用默认值\r\n"); }

            //节拍信息块
            {
                string[] 节拍信息 = 节拍.Text.Split('/');
                int 节拍分量 = -1; int 节拍总数 = -1;
                if (节拍信息.Length != 2) { 错误信息.Append("基础信息节拍值 - 节拍设置失败，将使用默认值\r\n"); }
                else
                {
                    var 储存信息1 = int.TryParse(节拍信息[0], out 节拍分量);
                    var 储存信息2 = int.TryParse(节拍信息[1], out 节拍总数);
                    if (储存信息1 == false || 储存信息2 == false)
                    {
                        节拍分量 = 节拍总数 = -1;
                        错误信息.Append("基础信息节拍值 - 节拍设置失败，将使用默认值\r\n");
                    }
                    载入文件.默认小节节拍分量 = 节拍分量;
                    载入文件.默认小节节拍总数 = 节拍总数;
                }
            }
            //段落配置储存
            {
                //载入文件.文件组
                载入文件.BPM组 = new System.Collections.Generic.List<float>();
                载入文件.段落名称 = new System.Collections.Generic.List<string>();
                载入文件.小节节拍分量组 = new System.Collections.Generic.List<int>();
                载入文件.小节节拍总数组 = new System.Collections.Generic.List<int>();
                载入文件.Offset组 = new System.Collections.Generic.List<double>();
                载入文件.循环下标组 = new System.Collections.Generic.List<bool>();
                foreach (段落项 段落 in 段落集.Children)
                {
                    var 段落名 = 段落.章节名显示.Text;
                    var offset = 段落.Offset显示.Text;
                    string[] 节拍信息 = 段落.节拍显示.Text.Split('/');
                    int 节拍分量 = -1; int 节拍总数 = -1;
                    if (节拍信息.Length != 2) { 错误信息.Append($"{段落名} 节拍值 - 节拍设置失败，将使用默认值\r\n"); }
                    else
                    {
                        var 储存信息1 = int.TryParse(节拍信息[0], out 节拍分量);
                        var 储存信息2 = int.TryParse(节拍信息[1], out 节拍总数);
                        if (储存信息1 == false || 储存信息2 == false)
                        {
                            节拍分量 = 节拍总数 = -1;
                            错误信息.Append($"{段落名} 节拍值 - 节拍设置失败，将使用默认值\r\n");
                        }
                    }
                    float BPM = -1;
                    var 储存信息3 = float.TryParse(段落.BPM显示.Text.Replace("BPM", ""), out BPM);
                    if (储存信息3 == false)
                    {
                        BPM = -1;
                        错误信息.Append($"{段落名} BPM - BPM设置失败，将使用默认值\r\n");
                    }

                    if (段落.允许循环.IsChecked.HasValue) { 载入文件.循环下标组.Add(段落.允许循环.IsChecked.Value); }
                    else { 载入文件.循环下标组.Add(false); }
                    double 偏移值 = 0;
                    if (!double.TryParse(offset, out 偏移值))
                    {
                        偏移值 = 0;
                        错误信息.Append($"{段落名} Offset - 文件时间偏移设置失败，将使用默认值\r\n");
                    }
                    载入文件.BPM组.Add(BPM);
                    载入文件.段落名称.Add(段落名);
                    载入文件.小节节拍分量组.Add(节拍分量);
                    载入文件.小节节拍总数组.Add(节拍总数);
                    载入文件.Offset组.Add(偏移值);
                }
                //脚本储存
            }
            if (脚本库.Text.Length != 0) { 载入文件.脚本基类 = 脚本库.Text; }
            else { 错误信息.Append($"未储存应用模板信息，可能没有编辑"); }
            if (脚本内容.Text.Length != 0) { 载入文件.脚本文档 = 脚本内容.Text; 载入文件.拥有脚本 = true; }
            else { 错误信息.Append($"未储存脚本内容，可能没有编辑"); 载入文件.拥有脚本 = false; }
            //输出错误信息并储存文档
            if (错误信息.Length > 0)
            { MessageBox.Show($"部分信息储存失败\r\n{错误信息}", "脚本处理"); }
            载入文件.拥有配置文件 = true;
            脚本文件读写器.储存脚本文件(载入文件, 脚本文件读写器.获取脚本文件位置(载入文件));
        }
        public void 删除脚本(脚本文件数据 载入文件)
        {
            脚本文件读写器.删除脚本文件(载入文件);
        }
        public void 更新脚本模板()
        {
            if (!System.IO.Directory.Exists("脚本资源\\模板")) { return; }
            var 文件列表 = System.IO.Directory.GetFiles("脚本资源\\模板");
            foreach (var i in 文件列表)
            {
                脚本库.Items.Add(Path.GetFileName(i));
            }
        }
        private void 选择模板脚本(string 脚本文件名)
        {
            var 默认目录 = "脚本资源\\模板\\";
            if (File.Exists(默认目录 + 脚本文件名))
            {
                脚本内容.Text = File.ReadAllText(默认目录 + 脚本文件名);
            }

        }
        public void 打开模板文件夹()
        {
            if (System.IO.Directory.Exists("脚本资源\\模板") == false)
            {
                return;
            }
            中间件.文件定位器.定位文件("脚本资源\\模板");
        }
        #endregion
        #region 按钮方法
        private void 储存按钮_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            更新并储存数据(脚本文件!);
        }

        private void 删除按钮_Click(object sender, RoutedEventArgs e)
        {
            var 结果 = MessageBox.Show($"确定要删除脚本文件吗", "删除脚本文件", MessageBoxButton.OKCancel);
            if (结果 == MessageBoxResult.OK)
            {
                删除脚本(脚本文件!);
            }

        }

        private void 热重载按钮_Click(object sender, RoutedEventArgs e)
        {
            if (脚本解析器.当前解析 == null) { Debug.Print($"未运行脚本，无需热重载"); return; }
            if (脚本文件 == null || 脚本文件.脚本文档 == "") { Debug.Print($"没有脚本文档需要重载"); return; }
            脚本解析器.当前解析.热重载(脚本文件.脚本文档);
        }

        private void 脚本内容_GotFocus(object sender, RoutedEventArgs e)
        {
            智能提示.IsOpen = true;
            Debug.Print("打开提示");
        }

        private void 脚本内容_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            智能提示.IsOpen = true;
        }

        private void 脚本内容_LostFocus(object sender, RoutedEventArgs e)
        {
            if (智能提示.IsFocused) { return; }
            if (智能提示.IsMouseOver) { return; }
            智能提示.IsOpen = false;
            Debug.Print("关闭提示");
        }

        private void 脚本内容_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            智能提示.IsOpen = true;
        }

        private void 节拍器按钮_Click(object sender, RoutedEventArgs e)
        {
            节拍器.IsOpen = true;
            迷你节拍器.Focus();
        }

        private void 脚本内容_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (补全组件.当前提示下标 == 0)
                {
                    if (补全组件.当前选中项目 == null) { return; }
                    if (补全组件.实际未匹配项目 == true) { return; }
                    e.Handled = true;//取消当前键盘事件
                    脚本编辑器控制.替换当前方法名(脚本内容, 补全组件.当前选中项目);
                }
            }


        }

        private void 打开模板文件夹_Click(object sender, RoutedEventArgs e)
        {

            打开模板文件夹();
        }

        private void 脚本库_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var 控件 = (ComboBox)sender;
            if (控件.SelectedValue == null)
            {
                return;
            }
            选择模板脚本(控件.SelectedValue.ToString());
        }


    }
    #endregion
}
