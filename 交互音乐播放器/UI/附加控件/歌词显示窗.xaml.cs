using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据;
using static 交互音乐播放器.数据.音乐播放数据;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 歌词显示窗.xaml 的交互逻辑
    /// </summary>
    public partial class 歌词显示窗 : Window,INotifyPropertyChanged
    {
        public static 歌词显示窗 当前显示窗 { get; set; }
        public static bool 是否打开 { get; set; }
        public static string 文件链接 { get; set; }
        public static string 音乐路径 { get; set; }
        public static 数据.播放中数据.段落信息 段落信息 { get; set; }
        public static string 歌词文件链接 { get; set; }
        public static string 上次更新行一 { get; set; }
        public static string 上次更新行二 { get; set; }
        public static int 上次更新时间 { get; set; }
        public static Dictionary<int, string> 歌词键值对 { get; set; } = new Dictionary<int, string>();
        public static Dictionary<int,string> 副语言歌词键值对 { get; set; } = new Dictionary<int, string>();
        public static List<int> 时间键 { get; set; } = new List<int>();
        public static bool 暗色模式 { get; set; } = true;
        public static bool 双行模式 { get; set; } = true;
        public static bool 穿透 { get; set; } = false;
        #region MVVM属性
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _歌词行一;
        public string 歌词行一
        {
            get => _歌词行一;
            set
            {
                _歌词行一 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(歌词行一)));
            }
        }

        private string _歌词行二;
        public string 歌词行二
        {
            get => _歌词行二;
            set
            {
                _歌词行二 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(歌词行二)));
            }
        }

        private string _翻译行一;
        public string 翻译行一
        {
            get => _翻译行一;
            set
            {
                _翻译行一 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(翻译行一)));
            }
        }

        private string _翻译行二;
        public string 翻译行二
        {
            get => _翻译行二;
            set
            {
                _翻译行二 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(翻译行二)));
            }
        }
        #endregion
        public 歌词显示窗()
        {
            当前显示窗 = this;
            是否打开 = true;
            InitializeComponent();
  
            Loaded += 加载完毕;
            this.Closing += 歌词窗口关闭;

        }

        private void 加载完毕(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前文件!=null)
            {
                歌词显示窗.加载基础信息(音频控制中间件.播放数据.当前文件!.路径);
            }
        }

        public static void 加载基础信息(string 文件路径)
        {
            清空数据();
            if (音频控制中间件.播放数据 != null && 音频控制中间件.播放数据.当前文件 != null)
            {
                文件链接 = 音频控制中间件.播放数据.当前文件.路径;

            }
            else { return; }
            var 文件夹链接 = System.IO.Path.GetDirectoryName(文件链接);
            var 文件名 = System.IO.Path.GetFileNameWithoutExtension(文件链接);

            string 歌词文件链接 = $"{文件夹链接}\\{文件名}.lrc";
            if (!File.Exists(歌词文件链接)) { return; }
            音乐路径 = 文件链接;//该值未单一功能化
            文件链接 = 歌词文件链接;//该值未单一功能化
            当前显示窗.读取歌词文件(歌词文件链接);
            //尝试读取第二语言歌词
            var 文件列表 = Directory.GetFiles(文件夹链接, $"*{文件名}*.lrc").ToList();
            文件列表.Remove(歌词文件链接);
            var 文件 = 文件列表.FirstOrDefault();
            if (文件 != null)
            {
                string 第二语言歌词文件链接 = 文件;
                当前显示窗.读取第二语言歌词(第二语言歌词文件链接);
            }

        }

        private static void 清空数据()
        {
            歌词键值对.Clear(); 副语言歌词键值对.Clear();
            当前显示窗.歌词行一 = "";
            当前显示窗.歌词行二 = "";
            当前显示窗.翻译行一 = "";
            当前显示窗.翻译行二 = "";
        }

        public static void 更新歌词()
        {
            if (当前显示窗 == null) { return; }
            if (歌词键值对 == null) { return; }

            var 当前播放时间 = (int)UI界面数据.当前播放时间.TotalSeconds;

            if (歌词键值对.ContainsKey(当前播放时间))
            {

                var 歌词 = $"{歌词键值对[当前播放时间]}";

                if (歌词 == 上次更新行二) { return; }
                if (歌词 == 上次更新行一) { return; }

                当前显示窗.歌词行一 = 歌词;
                if (副语言歌词键值对.Count > 0)
                {
                    var 翻译行歌词 = $"{副语言歌词键值对[当前播放时间]}";
                    当前显示窗.翻译行一 = 翻译行歌词;
                }

                if (!双行模式) { return; }
                var 下一时间序列 = 时间键.IndexOf(当前播放时间);
                if (下一时间序列 + 1 <= 时间键.Count-1)
                {
                    当前显示窗.歌词行二 = 歌词键值对[时间键[下一时间序列+1]];
                    if (副语言歌词键值对.Count > 0)
                    {
                        当前显示窗.翻译行二 = 副语言歌词键值对[时间键[下一时间序列 + 1]];
                    }

                }
                else
                {
                    当前显示窗.歌词行二 = "";
                }
                上次更新行二 = 当前显示窗.歌词行二;
                上次更新行一 = 当前显示窗.歌词行一;
                Debug.Print($"更新歌词 - {当前显示窗.歌词行一}");
            }
        }

        private bool 读取歌词文件(string 文件链接)
        {
            
            var 文件内容 = File.ReadAllText(文件链接);
            string 正则表达式 = "\\[([0-9:.]+)\\](.*)";
            Regex 歌词匹配器 = new(正则表达式);
            var 歌词匹配集 = 歌词匹配器.Matches(文件内容);
            if (歌词匹配集.Count == 0) { return false; }
            时间键.Clear();
            歌词键值对 = new Dictionary<int, string>();
            for (int i = 0; i < 歌词匹配集.Count; i++)
            {
                var 时间文本 = 歌词匹配集[i].Groups[1].Value.Trim();
                if (时间文本.Split(':').Length < 3) { 时间文本 = "00:" + 时间文本; }
                TimeSpan 歌词时间 = new TimeSpan();
                if (!TimeSpan.TryParse(时间文本, out 歌词时间))
                {
                    MessageBox.Show($"在{文件链接}中的\r\n歌词文本的第{i}行有问题，无法被识别", "歌词文本有问题");
                    return false;
                };
                string 歌词内容 = 歌词匹配集[i].Groups[2].Value.Trim();
                if (string.IsNullOrWhiteSpace(歌词内容)) { continue; }
                var 歌词秒数 = (int)歌词时间.TotalSeconds;
                if (!歌词键值对.ContainsKey(歌词秒数))
                {
                    歌词键值对.Add(歌词秒数, 歌词内容);
                    时间键.Add(歌词秒数);
                }
                else
                {
                    歌词键值对[歌词秒数] += 歌词内容;
                }

            }
            时间键.Sort();
            return true;
        }
        private bool 读取第二语言歌词(string 文件链接)
        {

            var 文件内容 = File.ReadAllText(文件链接);
            string 正则表达式 = "\\[([0-9:.]+)\\](.*)";
            Regex 歌词匹配器 = new(正则表达式);
            var 歌词匹配集 = 歌词匹配器.Matches(文件内容);
            if (歌词匹配集.Count == 0) { return false; }
            副语言歌词键值对 = new Dictionary<int, string>();
            for (int i = 0; i < 歌词匹配集.Count; i++)
            {
                var 时间文本 = 歌词匹配集[i].Groups[1].Value.Trim();
                if (时间文本.Split(':').Length < 3) { 时间文本 = "00:" + 时间文本; }
                TimeSpan 歌词时间 = new TimeSpan();
                if (!TimeSpan.TryParse(时间文本, out 歌词时间))
                {
                    MessageBox.Show($"在{文件链接}中的\r\n歌词文本的第{i}行有问题，无法被识别", "歌词文本有问题");
                    return false;
                };
                string 歌词内容 = 歌词匹配集[i].Groups[2].Value.Trim();
                if (string.IsNullOrWhiteSpace(歌词内容)) { continue; }
                var 歌词秒数 = (int)歌词时间.TotalSeconds;
                if (!副语言歌词键值对.ContainsKey(歌词秒数))
                {
                    副语言歌词键值对.Add(歌词秒数, 歌词内容);
                }
                else
                {
                    副语言歌词键值对[歌词秒数] += 歌词内容;
                }

            }
            时间键.Sort();
            return true;
        }

        private void 变换至亮色()
        {
            暗色模式 = false;
            var 歌词行暗色 = (Style)Resources["暗色歌词行"];
            var 翻译行暗色 = (Style)Resources["暗色翻译行"];
            翻译一.Style = 翻译行暗色; 翻译二.Style = 翻译行暗色;
            第一行.Style = 歌词行暗色; 第二行.Style = 歌词行暗色;
            
        }

        private void 变换至暗色()
        {
            暗色模式 = true ;
            var 歌词行亮色 = (Style)Resources["亮色歌词行"];
            var 翻译行亮色 = (Style)Resources["亮色翻译行"];
            翻译一.Style = 翻译行亮色; 翻译二.Style = 翻译行亮色;
            第一行.Style = 歌词行亮色; 第二行.Style = 歌词行亮色;
        }

        public void 切换穿透模式()
        {
            if (!穿透)
            {
                信息行.Visibility = Visibility.Hidden;
                窗口设定.CaptionHeight = 0;
                穿透 = true;
                return;
            }
            else
            {
                信息行.Visibility = Visibility.Visible;
                窗口设定.CaptionHeight = 32;
                穿透 = false;
                return;
            }
        }

        private void 歌词窗口关闭(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            当前显示窗 = null;
            是否打开 = false;
        }


        private void 变换暗亮色(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("切换主题");
            if (暗色模式)
            {
                变换至亮色();
            }
            else
            {
                变换至暗色();
            }
        }

        private void 切换单双行(object sender, RoutedEventArgs e)
        {
            if (双行模式)
            {
                双行模式 = false;
                第二行.Visibility = Visibility.Collapsed;
                翻译二.Visibility = Visibility.Collapsed;
                第一行.HorizontalAlignment = HorizontalAlignment.Right;
                翻译一.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                双行模式 = true ;
                第二行.Visibility = Visibility.Visible;
                翻译二.Visibility = Visibility.Visible;
                第一行.HorizontalAlignment = HorizontalAlignment.Left;
                翻译一.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private void 穿透模式(object sender, RoutedEventArgs e)
        {
            切换穿透模式();

        }

 
    }
}
