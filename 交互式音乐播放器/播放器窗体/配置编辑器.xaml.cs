using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using 交互式音乐播放器.配置文件;
using 交互式音乐播放器.音频中间件;

namespace 交互式音乐播放器.配置
{
    /// <summary>
    /// 新建循环配置文件.xaml 的交互逻辑
    /// </summary>
    public partial class 新建循环配置文件 : Window
    {
        #region 引入INI读取函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
        #endregion
        #region INI操作方法
        /// <summary>
        ///读取一个配置项，如果这个配置项中的值不存在返回一个默认值
        /// </summary>
        /// <param name="配置节名">配置节名</param>
        /// <param name="配置项名">配置项名</param>
        /// <param name="默认返回">如果这个配置项中的值不存在返回一个默认值</param>
        /// <param name="配置项文件">指定一个配置项的文件路径</param>
        /// <returns></returns>
        public string 读配置项(string 配置项文件, string 配置节名, string 配置项名, string 默认返回)
        {
            if (File.Exists(配置项文件))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(配置节名, 配置项名, 默认返回, temp, 1024, 配置项文件);
                return temp.ToString();
            }
            else
            {
                return 默认返回;
            }
        }
        /// <summary>
        /// 写配置项，返回True或者Flase
        /// </summary>
        /// <param name="配置项路径">输入目标文件路径</param>
        /// <param name="配置节名">配置节名 [ ]内的文字</param>
        /// <param name="配置项名">配置项名 = 前的文字</param>
        /// <param name="值">配置值 = 后的文字</param>
        /// <returns></returns>
        public bool 写配置项(string 配置项路径, string 配置节名, string 配置项名, string 值)
        {
            var pat = System.IO.Path.GetDirectoryName(配置项路径);
            if (Directory.Exists(pat) == false)
            {
                Directory.CreateDirectory(pat);
            }
            if (File.Exists(配置项路径) == false)
            {
                File.Create(配置项路径).Close();
            }
            long OpStation = WritePrivateProfileString(配置节名, 配置项名, 值, 配置项路径);
            if (OpStation == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
        #region 变量定义
        string 文件夹路径 = "";
        string 配置文件名称 = "";
        string 音乐名称 = "";
        int BPM值 = 0;
        int 节拍数 = 0;
        string 高级_文件模式 = "";
        string 高级_循环模式 = "";
        List<int> 循环段落 = new List<int>();
        List<CheckBox> 勾选组 = new List<CheckBox>();
        string 节拍特例文本 = "";
        string 段落名称 = "";
        音频信息 信息 = new 音频信息();
        配置档 配置 = new 配置档();
        #endregion
        #region 回调函数
        public delegate void 更新主界面文件列表(string 文件夹路径);
        public 更新主界面文件列表 更新方法;
        #endregion
        public 新建循环配置文件(string 配置文件名)
        {
            InitializeComponent();
            配置启用状态.IsChecked = true;
            配置文件名称 = 配置文件名;
            if (File.Exists(配置文件名) == false)
            {
                Console.WriteLine("正在新建档案...");
                Console.WriteLine("正在写入初始数据...");
                新建循环文件();
                Console.WriteLine("档案新建完毕...");
            }
            Console.WriteLine("打开了 " + 配置文件名);
            音乐名称 = Path.GetFileName(配置文件名).Split(' ')[0];
            文件夹路径 = Path.GetDirectoryName(配置文件名);
            更新循环文件();
        }
        public void 更新循环文件()
        {
            配置档.检测并升级配置档(配置文件名称);
            循环段落.Clear();
            循环段落组.Children.Clear();
            勾选组.Clear();
            //读取普通数据
            曲名框.Text = 读配置项(配置文件名称, "基础信息", "曲名", "");
            作者框.Text = 读配置项(配置文件名称, "基础信息", "作者", "");
            专辑名称.Text = 读配置项(配置文件名称, "基础信息", "专辑", "");
            //读取基本循环数据
            BPM值 = Convert.ToInt32(读配置项(配置文件名称, "基础信息", "BPM", ""));
            节拍数 = Convert.ToInt32(读配置项(配置文件名称, "基础信息", "节拍", ""));
            段落名称 = 读配置项(配置文件名称, "章节配置", "章节名", "");
            节拍特例文本 = 读配置项(配置文件名称, "章节配置", "章节节拍特例", "");
            //读取高级循环数据
            高级_文件模式 = 读配置项(配置文件名称, "交互信息", "文件模式", "多个文件模式");
            高级_循环模式 = 读配置项(配置文件名称, "交互信息", "循环类型", "永续");
            //将普通数据刷新到界面
            BPM数.Text = BPM值.ToString();
            拍数.Text = 节拍数.ToString();
            节拍特例.Text = 节拍特例文本;
            段落名称组.Text = 段落名称;
            文件模式.Text = 高级_文件模式;
            处理模式.Text = 高级_循环模式;
            //更新循环段落信息
            string 循环段落文本 = 读配置项(配置文件名称, "章节配置", "循环章节下标", "");
            string[] 循环段落文本组 = 循环段落文本.Split(',');
            if (循环段落文本.Length > 0)
            {
                //读入配置文件
                foreach (string i in 循环段落文本组)
                {
                    循环段落.Add(Convert.ToInt32(i));
                }
            }
            //读取文件列 ！此处应加上对其他格式音频的支持
            var 播放列表 = Directory.GetFiles(文件夹路径, $"{音乐名称}*.ogg");
            for (int i = 1; i <= 播放列表.Length; i++)
            {
                CheckBox 选项框 = new CheckBox();
                选项框.Content = $"第{i}段 ";
                勾选组.Add(选项框);
            }
            //应用段落信息
            foreach (int i in 循环段落)
            {
                勾选组[i].IsChecked = true;
            }
            //将设置更新到界面
            foreach (var i in 勾选组)
            {
                循环段落组.Children.Add(i);
            }

        }
        public void 写入循环文件()
        {
            //读取循环信息
            string 段落 = "";
            int 遍历数 = 0;
            int 已取得的循环 = 0;
            foreach (CheckBox i in 循环段落组.Children)
            {
                //遍历选项框是否都已经勾选，勾选则写入临时数据
                if (i.IsChecked == true)
                {
                    if (已取得的循环 == 0) //是否为第一次，若为第一次则不写入前面的逗号
                    { 段落 += $"{遍历数}"; 已取得的循环++; }
                    else
                    { 段落 += $",{遍历数}"; 已取得的循环++; }
                }
                遍历数++;
            }
            //写入基础文件
            #region 建立版本信息
            写配置项(配置文件名称, "配置文件", "版本", "1.0");
            #endregion
            #region 建立音频基础信息
            写配置项(配置文件名称, "基础信息", "启用配置", "true");
            写配置项(配置文件名称, "基础信息", "曲名", 曲名框.Text);
            写配置项(配置文件名称, "基础信息", "作者", 作者框.Text);
            写配置项(配置文件名称, "基础信息", "专辑", 专辑名称.Text);
            写配置项(配置文件名称, "基础信息", "封面路径", "");
            #region 建立交互信息配置
            写配置项(配置文件名称, "交互信息", "循环类型", 处理模式.Text);
            写配置项(配置文件名称, "交互信息", "文件模式", 文件模式.Text);
            //写配置项(配置文件名称, "交互信息", "文件模式", "");
            #endregion
            if (BPM数.Text == "0") { MessageBox.Show("保存歌曲信息", "因为仅填写了歌曲信息，已仅对此部分进行更新"); return; }
            写配置项(配置文件名称, "基础信息", "BPM", BPM数.Text);
            写配置项(配置文件名称, "基础信息", "节拍", 拍数.Text);
            #endregion

            #region 建立分段信息配置
            写配置项(配置文件名称, "章节配置", "循环章节下标", 段落);
            //写配置项(配置文件名称, "分段配置", "分段字节", "");
            #endregion
            #region 建立循环信息配置
            写配置项(配置文件名称, "章节配置", "章节名", 段落名称组.Text);
            //写配置项(配置文件名称, "章节配置", "循环章节下标", "1");
            写配置项(配置文件名称, "章节配置", "章节节拍特例", 节拍特例.Text);
            //写配置项(配置文件名称, "章节配置", "章节BPM特例", "");
            //写配置项(配置文件名称, "章节配置", "章节切换拍", "");
            #endregion
            MessageBox.Show("设定已保存", "成功");
        }
        public void 新建循环文件()
        {
            配置档 配置 = new 配置档();
            配置.新建配置档(配置文件名称);
        }
        public void 删除循环文件()
        {
            File.Delete(配置文件名称);
            MessageBox.Show("已删除循环配置文件", "删除完毕！");
            this.Close();
        }
        private void 保存_Click(object sender, RoutedEventArgs e)
        {
            写入循环文件();
            更新循环文件();
            this.Close();
        }

        private void 删除_Click(object sender, RoutedEventArgs e)
        {
            删除循环文件();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (曲名框.Text != "")
            {
                return;
            }
            if (BPM数.Text == "0")
            {
                File.Delete(配置文件名称);
                MessageBox.Show("因为未进行更改，已删除循环配置文件", "操作撤销");
            }

        }


    }
}
