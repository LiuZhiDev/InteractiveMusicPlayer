using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 交互音乐播放器.数据;
using System.Diagnostics;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 智能补全组件.xaml 的交互逻辑
    /// </summary>
    public partial  class 智能补全组件 : UserControl
    {
        public class 方法
        {
            public 方法()
            {
                this.方法名称 = "";
                this.方法格式 = "";
                this.参数列表 = new List<string>();
                this.文档注释 = "";
            }
            public 方法(string 方法名称,string 方法格式,List<string> 参数列表,string 文档注释)
            {
                this.方法名称 = 方法名称;
                this.方法格式 = 方法格式;
                this.参数列表 = 参数列表;
                this.文档注释 = 文档注释;
            }
            public string 方法名称 { get; set; }
            public string 方法格式 { get; set; }
            public List<string> 参数列表 { get; set; }
            public string 文档注释 { get; set; }
        }
        public static 智能补全组件 当前补全;
        public List<string> 方法名称列表 = new List<string>();
        public Dictionary<string, 方法> 方法信息列表 = new Dictionary<string, 方法>();
        IEnumerable<string> 显示选项;
        public bool 实际未匹配项目;
        public string? 当前选中项目 = "";
        public int 当前提示下标 = 0;
        public 智能补全组件()
        {
            Loaded += 智能补全组件_Initialized;
            当前补全 = this;
            InitializeComponent();
        }

        private void 智能补全组件_Initialized(object? sender, EventArgs e)
        {
            初始化文档(系统控制.脚本数据文档, 选项集);
        }

        public void 初始化文档(string 文档提示路径, ListBox UI控件)
        {
            方法名称列表.Clear();
            方法信息列表.Clear();
            string[] 文档 = File.ReadAllLines(文档提示路径);
            foreach (var 方法行 in 文档)
            {
                string[] 信息组 = 方法行.Split('|');
                方法 方法 = new 方法();
                方法.方法名称 = 信息组[0];
                方法.方法格式 = 信息组[1];
                方法.文档注释 = 信息组[2];
                方法.参数列表 = new List<string>();
                for (int i = 3; i < 信息组.Length; i++)
                {
                    方法.参数列表.Add(信息组[i]);
                }
                方法名称列表.Add(方法.方法名称);
                方法信息列表.Add(方法.方法名称, 方法);
            }
            显示选项 = 方法名称列表.ToList<string>();
       
            UI控件.ItemsSource = 显示选项;
        }

        public void 判空隐藏(ListBox UI控件)
        {
            int 最大可选项 = UI控件.Items.Count;
            if (最大可选项 == 0) { UI控件.Visibility = Visibility.Hidden; }
            if (最大可选项 > 0) { UI控件.Visibility = Visibility.Visible; 智能提示重置(UI控件); }
        }

        public void 智能提示重置(ListBox UI控件)
        {
            int 当前选中项 = UI控件.SelectedIndex;
            int 最大可选项 = UI控件.Items.Count;
            当前选中项 = 0;
            if (当前选中项 + 1 > 最大可选项 - 1) { 当前选中项 = 最大可选项 - 1; }
            if (实际未匹配项目) { UI控件.SelectedIndex = -1; return; }
            UI控件.SelectedIndex = 当前选中项;
            UI控件.ScrollIntoView(UI控件.SelectedItem);
        }

        public void 智能提示过滤(ListBox UI控件, List<string> 原列表, string 过滤文本)
        {
            if (string.IsNullOrEmpty(过滤文本))
            {
                UI控件.ItemsSource = 原列表;
                实际未匹配项目 = true;
                判空隐藏(UI控件);
                return;
            }
            实际未匹配项目 = false;
            Thread 文件更新线程 = new Thread(() => {
                Thread 列表过滤 = new Thread(() =>
                {
                    显示选项 = 原列表.Where(x => x.Contains(过滤文本));
                    if (显示选项.FirstOrDefault() == null) { 实际未匹配项目 = true; 显示选项 = 原列表; }
                });
                列表过滤.Start();
                Application.Current.Dispatcher.Invoke(() => { UI控件.ItemsSource = 显示选项; 判空隐藏(UI控件); });
            });
            文件更新线程.Start();
          
         
        }

        public string 智能提示选定(KeyEventArgs 按键, ListBox UI控件, TextEditor 动态文本框)
        {
            if (UI控件 == null|| UI控件.SelectedItem == null) { return ""; }
            string? 当前选中项 = UI控件.SelectedItem.ToString();
            if (string.IsNullOrEmpty(当前选中项)) { return ""; }
            动态文本框.AppendText(当前选中项);
            return 当前选中项;
        }

        public void 更新参数提示(int 当前提示下标)
        {
            if (string.IsNullOrEmpty(当前选中项目)) { return; }
            if (当前提示下标 == 0)
            {
                语法提示器.提示内容.Text = 方法信息列表[当前选中项目].文档注释;
            }
            if (方法信息列表[当前选中项目].参数列表.Count == 0)
            {
                //是空参数方法
                语法提示器.提示内容.Text = $"{方法信息列表[当前选中项目].文档注释}\r\n该方法不需要传入参数即可调用";
                return;
            
            }
            if (当前提示下标 > 0)
            {
                if (当前提示下标 > 方法信息列表[当前选中项目].参数列表.Count - 1) 
                {
                   
                    语法提示器.提示内容.Text = 方法信息列表[当前选中项目].参数列表[方法信息列表[当前选中项目].参数列表.Count - 1];
                    return;
                }
                语法提示器.提示内容.Text = 方法信息列表[当前选中项目].参数列表[当前提示下标-1];
            }
        }

        private void 选项集_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var 控件 = (ListBox)sender;
            if (控件.SelectedValue == null) { 语法提示器.Visibility = Visibility.Collapsed; return; }
            当前选中项目 = 控件.SelectedValue.ToString();
            if (string.IsNullOrEmpty(当前选中项目)) { 语法提示器.Visibility = Visibility.Collapsed; return; }
            语法提示器.语法标题.Text = 方法信息列表[当前选中项目].方法名称;
            语法提示器.语法内容.Text = 方法信息列表[当前选中项目].方法格式.Replace(',','|');
            更新参数提示(当前提示下标);
            
            文本提示.IsOpen = true;
            文本提示.Visibility = Visibility.Visible;
            语法提示器.Visibility = Visibility.Visible;
            
            Debug.Print($"当前选中项 {当前选中项目}");
        }
    }
}
