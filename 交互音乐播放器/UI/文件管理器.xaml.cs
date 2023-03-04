using Microsoft.WindowsAPICodePack.Dialogs;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.数据;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.中间件;
using System.Threading;

namespace 交互音乐播放器.UI
{
    /// <summary>
    /// 文件管理器.xaml 的交互逻辑
    /// </summary>
    public partial class 文件管理器 : Page
    {
        public static 文件管理器? 当前文件管理器;
        public object 锁定 = new object();
        public enum 文件名称信息
        {
            曲名,
            作者,
            完整路径
        }

        public 文件管理器()
        {
            Initialized += 文件管理器_Initialized;
            InitializeComponent();
            当前文件管理器 = this;
        }

        private void 文件管理器_Initialized(object? sender, EventArgs e)
        {
            程序初始化();
        }

        private void 程序初始化()
        {

            打开文件与文件夹(UI界面数据.初始浏览文件夹);

        }

        #region 增删改查


        public void 打开文件与文件夹(string 文件夹路径)
        {
            var 调用栈 = new StackFrame(1);
            MethodBase? 调用者 = 调用栈.GetMethod();
            #region 读取文件夹信息
            DirectoryInfo 文件夹信息 = new DirectoryInfo(文件夹路径);
            try
            { 文件夹信息.GetAccessControl(); }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "无法访问文件夹");
                return;
            }
            var 当前文件夹名称 = Path.GetFileName(文件夹路径);
            文件夹.Text = 当前文件夹名称;
            this.文件列表.Items.Clear();
            var 文件夹列表 = Directory.GetDirectories(文件夹路径);
            foreach (var 路径 in 文件夹列表)
            {
                文件夹项 文件夹 = new 文件夹项();
                文件夹.文件夹名称 = System.IO.Path.GetFileName(路径);
                文件夹.文件夹路径 = 路径;
                ListBoxItem Item = new ListBoxItem();
                Item.Content = 文件夹;
                ListBoxItemHelper.SetCornerRadius(Item, new CornerRadius(14));
                Item.Tag = 文件夹.文件夹显示名称;
                this.文件列表.Items.Add(Item);
            }
            #endregion

            //多线程处理
            UI界面数据.当前浏览文件夹 = 文件夹路径;
            var 文件列表 = Directory.GetFiles(文件夹路径);
            Dictionary<string, 数据.脚本文件数据> 文件表 = new Dictionary<string, 数据.脚本文件数据>();
            foreach (var 文件路径 in 文件列表)
            {
                if (!系统控制.检查是否为支援类型(System.IO.Path.GetExtension(文件路径))) { continue; }
                var 文件名信息 = 识别文件名信息(文件路径);
                添加文件与文件组(文件表, 文件名信息); //添加一些基本的信息
            }
            foreach (var 文件 in 文件表)
            {
                var 显示项 = 建立文件项(文件.Value); //读取专辑图片、脚本文件、MP3tag
                ListBoxItem Item = new ListBoxItem();
                Item.Content = 显示项;
                Item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                ListBoxItemHelper.SetCornerRadius(Item, new CornerRadius(14));
                Item.Tag = 文件.Value.名称;
                this.文件列表.Items.Add(Item); 
            }

            if (调用者 == null) { Debug.Print("打开文件夹调用者是未知的"); 导航_更新按钮状态();  return; }
            if (调用者.Name == "刷新按钮_Click")
            {
                return;
            }
            if (调用者.Name != "导航_后退" && 调用者.Name != "导航_前进")
            {
                导航_添加历史文件夹(文件夹路径);
            }

            导航_更新按钮状态();
            GC.Collect();
            Debug.Print($"双击事件触发结束");
        }

        public Dictionary<文件名称信息, string> 识别文件名信息(string 文件路径)
        {
            Dictionary<文件名称信息, string> 文件信息 = new Dictionary<文件名称信息, string>();
            var 文件名称 = System.IO.Path.GetFileNameWithoutExtension(文件路径);
            if (文件名称.Contains("_"))
            {
                var 下划线分离 = 文件名称.Split("_");
                文件名称 = 下划线分离[0];
            }
            if (文件名称.Contains(" - "))
            {
                var 横杠分离 = 文件名称.Split(new string[] { " - " }, StringSplitOptions.None);
                文件信息.Add(文件名称信息.作者, 横杠分离[0]);
                文件信息.Add(文件名称信息.曲名, 横杠分离[1]);
                文件信息.Add(文件名称信息.完整路径, 文件路径);
            }

            else { 文件信息.Add(文件名称信息.曲名, 文件名称); 文件信息.Add(文件名称信息.作者, ""); 文件信息.Add(文件名称信息.完整路径, 文件路径); }
            return 文件信息;
        }

        public void 添加文件与文件组(Dictionary<string, 数据.脚本文件数据> 文件表, Dictionary<文件名称信息, string> 文件名信息)
        {
            数据.脚本文件数据? 文件信息;
            string 键名 = $"{文件名信息[文件名称信息.作者]} {文件名信息[文件名称信息.曲名]}";
            //若已经存在相同曲目 则添加
            if (文件表.TryGetValue(键名, out 文件信息))
            {
                文件信息.文件组.Add(文件名信息[文件名称信息.完整路径]);
            }
            //否则新建
            else
            {
                文件表.TryAdd($"{文件名信息[文件名称信息.作者]} {文件名信息[文件名称信息.曲名]}",
                new 脚本文件数据
                {
                    作者 = 文件名信息[文件名称信息.作者],
                    名称 = 文件名信息[文件名称信息.曲名],
                    文件组 = new List<string> { 文件名信息[文件名称信息.完整路径] },
                    格式 = Path.GetExtension(文件名信息[文件名称信息.完整路径]).Remove(0, 1),
                });
                string 配置文件路径 = 脚本文件读写器.获取脚本文件位置(文件表[$"{文件名信息[文件名称信息.作者]} {文件名信息[文件名称信息.曲名]}"]);
                bool 是否有配置文件 = File.Exists(配置文件路径);
                文件表[$"{文件名信息[文件名称信息.作者]} {文件名信息[文件名称信息.曲名]}"].拥有配置文件 = 是否有配置文件;
            }

        }

        public 文件项 建立文件项(脚本文件数据 脚本数据)
        {
            文件项 显示项 = new 文件项();
            音乐文件数据辅助 辅助类 = new 音乐文件数据辅助();

                var 文件元数据 = 辅助类.寻找并设定专辑图片(脚本数据);

                if (脚本数据.拥有配置文件 == false)
                {
                    if (脚本数据.格式 == 数据.系统控制.支援的文件类型.mp3.ToString())
                    {
                        if (文件元数据[0] != null) { 脚本数据.名称 = 文件元数据[0]; }
                        if (文件元数据[1] != null) { 脚本数据.作者 = 文件元数据[1]; }
                        if (文件元数据[2] != null) { 脚本数据.专辑 = 文件元数据[2]; }
                    }
                }

                    AJAX更新数据(脚本数据,显示项);

            if (脚本数据.拥有配置文件 == true)
            {
                脚本文件读写器.读取脚本文件(脚本数据);
            }

            显示项.设置文件项(脚本数据,false);
            return 显示项;

        }


        public void AJAX更新数据(脚本文件数据 文件数据,文件项 待设置文件项)
        {
            待设置文件项.设置文件项(文件数据,true);
            
        }

        public void 从文件获取信息(string 文件路径, 脚本文件数据 脚本配置)
        {
            FileInfo 文件信息 = new FileInfo(文件路径);

        }

        public void 导航_添加历史文件夹(string 文件夹路径)
        {

            int 最大历史 = 数据.系统控制.最大浏览历史;
            if (数据.UI界面数据.历史路径.Count == 最大历史)
            {
                数据.UI界面数据.历史路径.RemoveAt(0);
                数据.UI界面数据.历史路径指针 -= 1;

            }
            if (数据.UI界面数据.历史路径指针 + 1 != 数据.UI界面数据.历史路径.Count && 数据.UI界面数据.历史路径.Count != 0)
            {
                数据.UI界面数据.历史路径.RemoveRange(数据.UI界面数据.历史路径指针 + 1, 数据.UI界面数据.历史路径.Count - (数据.UI界面数据.历史路径指针 + 1));
            }
            数据.UI界面数据.历史路径.Add(文件夹路径);
            数据.UI界面数据.历史路径指针 = 数据.UI界面数据.历史路径.Count - 1;
        }

        public void 导航_后退()
        {
            if (数据.UI界面数据.历史路径指针 == 0) { MessageBox.Show("当前已是最先浏览的文件夹"); return; }
            数据.UI界面数据.历史路径指针 -= 1;
            打开文件与文件夹(数据.UI界面数据.历史路径[数据.UI界面数据.历史路径指针]);
        }

        public void 导航_更新按钮状态()
        {
            if (数据.UI界面数据.历史路径指针 == 数据.UI界面数据.历史路径.Count - 1) { 前进导航按钮.IsEnabled = false; }
            else { 前进导航按钮.IsEnabled = true; }
            if (数据.UI界面数据.历史路径指针 == 0) { 后退导航按钮.IsEnabled = false; }
            else { 后退导航按钮.IsEnabled = true; }
        }

        public void 导航_前进()
        {
            if (数据.UI界面数据.历史路径指针 + 1 == 数据.UI界面数据.历史路径.Count) { MessageBox.Show("当前已是最后浏览的文件夹"); return; }
            数据.UI界面数据.历史路径指针 += 1;
            打开文件与文件夹(数据.UI界面数据.历史路径[数据.UI界面数据.历史路径指针]);
        }

        public void 导航_向上(string 文件夹路径)
        {
            var 新路径 = Path.GetDirectoryName(文件夹路径);
            if (新路径 == null) { return; }
            打开文件与文件夹(新路径);
        }

        public void 导航_打开文件夹()
        {
            CommonOpenFileDialog 文件夹选择器 = new CommonOpenFileDialog();
            文件夹选择器.IsFolderPicker = true;//设置为选择文件夹
            CommonFileDialogResult 结果 = 文件夹选择器.ShowDialog();
            if (结果 == CommonFileDialogResult.Ok)
            {
                打开文件与文件夹(文件夹选择器.FileName);
            }
        }

        public void 导航_搜索文件(string 搜索文本)
        {
            if (搜索文本 == 文本._文本[文本.文本类型.搜索栏_搜索栏空提示])
            {
                return;
            }
            var 列表 = 文件列表.Items.SourceCollection;
            int 查找指针 = 0;
            foreach (ListBoxItem 项 in 列表)
            {
                if (项.Tag == null) { return; }
                if (项.Tag.ToString()!.Contains(搜索文本, StringComparison.OrdinalIgnoreCase))
                {
                    文件列表.SelectedIndex = 查找指针;
                    文件列表.ScrollIntoView(项);
                    break;
                }
                查找指针++;
            }

        }

        public void 命令_打开文件(文件项 文件项)
        {
            var 脚本数据 = 文件项.脚本数据;
            音频控制中间件 音频 = new(文件项.脚本数据);
        }

        public void 命令_加载配置档(文件项 文件项)
        {
            var 脚本数据 = 文件项.脚本数据;
            数据.脚本文件读写器.读取脚本文件(文件项);
        }

        public void 命令_定位到所在文件夹(string 文件路径)
        {
            if (File.Exists(文件路径))
            {
                文件定位器.定位文件(文件路径);
                return;
            }
            if (Directory.Exists(文件路径))
            {
                文件定位器.定位文件(文件路径);
                return;
            }

        }

        #endregion
        #region 按钮控制事件
        private void 搜索栏_LostFocus(object sender, RoutedEventArgs e)
        {
            if (搜索栏.Text.Trim().Length == 0)
            {
                搜索栏.Text = 文本._文本[文本.文本类型.搜索栏_搜索栏空提示];
            }
        }

        private void 搜索栏_GotFocus(object sender, RoutedEventArgs e)
        {
            if (搜索栏.Text == 文本._文本[文本.文本类型.搜索栏_搜索栏空提示])
            {
                搜索栏.Text = "";
            }
        }


        private void 文件列表_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            lock (锁定)
            {
                this.文件列表.IsEnabled = false;
                var 文件列表 = (ListBox)sender;
                var 选中项 = (ListBoxItem)文件列表.SelectedItem;
                if (选中项 == null) { this.文件列表.IsEnabled = true; return; }
                var 类型名 = 选中项.Content.GetType().Name;
                if (类型名.Contains("文件项"))
                {
                    var 文件 = (文件项)选中项.Content;
                    命令_打开文件(文件);
                }
                if (类型名.Contains("文件夹项"))
                {
                    var 文件夹 = (文件夹项)选中项.Content;
                    打开文件与文件夹(文件夹.文件夹路径);
                }
                this.文件列表.IsEnabled = true;
            }
        }

        private void 向上按钮_Click(object sender, RoutedEventArgs e)
        {
            导航_向上(数据.UI界面数据.当前浏览文件夹);
        }

        private void 打开文件夹按钮_Click(object sender, RoutedEventArgs e)
        {
            导航_打开文件夹();
        }

        private void 后退导航按钮_Click(object sender, RoutedEventArgs e)
        {
            导航_后退();
        }

        private void 前进导航按钮_Click(object sender, RoutedEventArgs e)
        {
            导航_前进();
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            this.文件列表.IsEnabled = true;
            打开文件与文件夹(数据.UI界面数据.当前浏览文件夹);
        }

        private void 文件列表_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var 文件列表 = (ListBox)sender;
            var 选中项 = (ListBoxItem)文件列表.SelectedItem;
            if (选中项 == null) { Debug.Print("未选中项"); return; }
            var 类型名 = 选中项.Content.GetType().Name;
            if (类型名.Contains("文件项"))
            {
                命令_加载配置档((文件项)选中项.Content);
            }
            if (类型名.Contains("文件夹项"))
            {
            }
        }

        private void 打开所在文件夹_Click(object sender, RoutedEventArgs e)
        {
            var 文件列表 = this.文件列表;
            var 选中项 = (ListBoxItem)文件列表.SelectedItem;
            if (选中项 == null) { Debug.Print("未选中项"); return; }
            var 类型名 = 选中项.Content.GetType().Name;
            if (类型名.Contains("文件项"))
            {
                var 文件 = (文件项)选中项.Content;
                命令_定位到所在文件夹(文件.脚本数据.文件组[0]);
            }
            if (类型名.Contains("文件夹项"))
            {
                var 文件 = (文件夹项)选中项.Content;
                命令_定位到所在文件夹(文件.文件夹路径);
            }
        }

        private void 打开配置文件所在文件夹_Click(object sender, RoutedEventArgs e)
        {
            var 文件列表 = this.文件列表;
            var 选中项 = (ListBoxItem)文件列表.SelectedItem;
            if (选中项 == null) { Debug.Print("未选中项"); return; }
            var 类型名 = 选中项.Content.GetType().Name;
            if (类型名.Contains("文件项"))
            {
                var 文件 = (文件项)选中项.Content;
                命令_定位到所在文件夹(脚本文件读写器.获取脚本文件位置(文件.脚本数据));
            }
        }

        private void 搜索栏_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
                导航_搜索文件(搜索栏.Text);
        }
    }
    #endregion
}
