using Microsoft.WindowsAPICodePack.Dialogs;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using 交互式音乐播放器.控件;
using 交互式音乐播放器.文件浏览中间件;
using 交互式音乐播放器.设置类;
using 交互式音乐播放器.语言;
using 交互式音乐播放器.配置文件;
using 交互式音乐播放器.音频中间件;

namespace 交互式音乐播放器
{
    public partial class UI信息
    {
        public Dictionary<string, 音频文件项> 当前列表项 { get; set; } = new Dictionary<string, 音频文件项>(20);


    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Panuon.UI.Silver.WindowX
    {
        #region 初始化类
        设置类.运行时 运行变量 = new 设置类.运行时();
        UI信息 UI信息 = new UI信息();
        public static 音频信息 播放中;
        public static ListBox 总播放列表;
        //初始化信息类
        配置档 配置 = new 配置档();
        #endregion
        #region 程序入口点
        public MainWindow()
        {
            InitializeComponent();
            总播放列表 = 文件与文件夹集;
            测试口();
            程序启动();
        }
        private void 测试口()
        {
            设置类.设置.日志开关 = false;

        }
        private void 程序启动()
        {
            打开文件夹(设置类.设置.默认启动位置);

        }
        #endregion
        #region 与音频中间件的信息传输
        private void 播放音频文件()
        {
            停止所有播放线程();
            if(文件与文件夹集.SelectedIndex==-1)
            {
                MessageBox.Show("请选择播放列表中的文件，再点击播放", "提示");
                return;
            }
            else
            { 
            播放中 = new 音频信息();
            }
            var tmp = (音频文件项)文件与文件夹集.SelectedItem;
            循环模式菜单更新(tmp);
            音频文件播放.播放下一音频 = 播放下一音频;
            音频中间件.音频文件播放 音频播放 = new 音频中间件.音频文件播放(tmp, this, 播放中, UI更新);



        }
        public void 播放下一音频()
        {
            var 总列表 = MainWindow.总播放列表;
            if (总列表.Items.Count == 0) { 设置类.日志.输出("已经没有播放项目了"); return; }
            if (总列表.SelectedIndex == -1) { 总列表.SelectedIndex = 0; }
            if (总列表.Items.Count - 1 > 总列表.SelectedIndex + 1)
            {
                总列表.SelectedIndex += 1;
            }
            else
            {
                总列表.SelectedIndex = 0;
                播放下一音频();
                return;

            }

            var 类型 = 总列表.SelectedItem.GetType();
            if (类型 == typeof(控件.音频文件项))
            {
                设置类.日志.输出("准备播放音频文件");
                播放音频文件();
            }
            else
            {
                播放下一音频();
            }
        }
        #endregion
        #region 按钮操作封装
        private void 打开文件夹(string 完整路径)
        {

            if (!Directory.Exists(完整路径))
            {
                设置类.日志.输出("无此文件夹");
                MessageBox.Show("无此文件夹");
                return;
            }

            var 文件夹列表 = 读取文件夹(完整路径);
            删除_清空UI音频项(UI信息.当前列表项);
            更新界面(文件夹列表);
            运行变量.访问过的文件夹路径.Add(完整路径);
            运行变量.当前文件夹路径 = 完整路径;

        }
        private void 上一文件夹()
        {
            if (运行变量.访问过的文件夹路径.Count < 2)
            {
                设置类.日志.输出("访问失败，现在访问过的文件夹少于2");
                return;
            }
            int 当前编号 = 运行变量.访问过的文件夹路径.IndexOf(运行变量.当前文件夹路径);
            int 上一编号 = 当前编号 - 1;
            if (上一编号 < 0)
            {
                设置类.日志.输出("访问失败，已经到了最初访问的文件夹");
                return;
            }
            打开文件夹(运行变量.访问过的文件夹路径[上一编号]);
        }
        private void 下一文件夹()
        {
            if (运行变量.访问过的文件夹路径.Count < 2)
            {
                设置类.日志.输出("访问失败，现在访问过的文件夹少于2");
                return;
            }
            int 当前编号 = 运行变量.访问过的文件夹路径.IndexOf(运行变量.当前文件夹路径);
            int 下一编号 = 当前编号 + 1;
            if (下一编号 > 运行变量.访问过的文件夹路径.Count)
            {
                设置类.日志.输出("访问失败，已经到了最后访问的文件夹");
                return;
            }
            打开文件夹(运行变量.访问过的文件夹路径[下一编号]);
        }


        private void 停止所有播放线程()
        {
            if (播放中 != null)
            {
                播放线程管理.关闭所有播放内容();
            }
        }

        #endregion
        #region IO操作_读取文件

        private void 更新界面(List<文件信息> 文件信息)
        {

            foreach (文件信息 项 in 文件信息)
            {
                //在界面更新文件夹信息
                if (项.文件类型 == 文件浏览中间件.文件信息.项目类型.文件夹)
                {
                    新建_新建UI文件夹项(项);
                }
                //在界面更新音频文件信息
                if (项.文件类型 == 文件浏览中间件.文件信息.项目类型.MP3文件)
                {
                    var MP3音频项 = 读取MP3(项);
                    新建_添加UI音频项(UI信息, MP3音频项);
                }
                if (项.文件类型 == 文件浏览中间件.文件信息.项目类型.OGG文件)
                {
                    //查重
                    if (UI信息.当前列表项.ContainsKey(生成ID(项.显示名称, 项.完整路径)))
                    {
                        var tmp = UI信息.当前列表项[生成ID(项.显示名称, 项.完整路径)];
                        tmp.文件路径.Add(项.完整路径);
                        continue;
                    }
                    var OGG音频项 = 读取OGG(项);
                    新建_添加UI音频项(UI信息, OGG音频项);
                }
                if (项.文件类型 == 文件浏览中间件.文件信息.项目类型.WAV文件)
                {

                    var WAV音频项 = 读取WAV(项);
                    新建_添加UI音频项(UI信息, WAV音频项);
                }


            }


        }
        private List<文件信息> 读取文件夹(string 目录位置)
        {
            var 文件与文件夹组 = new List<文件信息>();
            #region 更新文件夹信息
            文件浏览器路径.Text = Path.GetFileName(目录位置);
            if (Directory.Exists(目录位置) == false)
            {
                MessageBox.Show("不存在该文件夹，也可能是权限不足无法读取", "读取失败");
                return 文件与文件夹组;
            }
            string[] 文件夹组 = null;
            try
            {
                文件夹组 = Directory.GetDirectories(目录位置);
            }
            catch
            {
                MessageBox.Show("您所打开的文件需要更高的权限才能浏览", "权限不足");
                return 文件与文件夹组;
            }

            //将文件夹信息写入内存 
            foreach (string 完整路径 in 文件夹组)
            {
                文件与文件夹组.Add(new 文件信息()
                {
                    完整路径 = 完整路径,
                    文件类型 = 文件信息.项目类型.文件夹,
                    显示名称 = Path.GetFileNameWithoutExtension(完整路径)
                });
            }
            #endregion
            #region 更新文件信息
            //获取目录下所有文件
            string[] 文件集 = Directory.GetFiles(目录位置);
            foreach (string 文件 in 文件集)
            {
                string 扩展名 = Path.GetExtension(文件);
                string 文件名 = Path.GetFileNameWithoutExtension(文件);
                string 完整路径 = Path.GetFullPath(文件);
                文件浏览中间件.文件信息.项目类型 文件类型 = 文件信息.项目类型.未知;
                if (扩展名 == ".mp3") { 文件类型 = 文件信息.项目类型.MP3文件; }
                if (扩展名 == ".ogg") { 文件类型 = 文件信息.项目类型.OGG文件; }
                if (扩展名 == ".wav") { 文件类型 = 文件信息.项目类型.WAV文件; }
                if (扩展名 == ".txt") { 文件类型 = 文件信息.项目类型.配置文件; }
                文件与文件夹组.Add(new 文件信息() { 文件类型 = 文件类型, 完整路径 = 完整路径, 显示名称 = 文件名.Split(' ')[0] });
            }
            #endregion
            return 文件与文件夹组;

        }

        #endregion
        #region 格式文件读取
        private 音频文件项 读取MP3(文件信息 项)
        {

            音频文件项 音频文件 = new 音频文件项();
            MP3信息读取 读Mp3 = new MP3信息读取();
            var tmpa = 音频文件.专辑图片.ImageSource;
            string[] Mp3信息集 = 读Mp3.ReadMp3(项.完整路径, 音频文件);
            string 曲名, 专辑, 作者;
            if (Mp3信息集[0] != null) { 曲名 = Mp3信息集[0].Replace("TIT2", ""); }
            else { 曲名 = 项.显示名称; }
            if (Mp3信息集[1] != null) { 作者 = Mp3信息集[1].Replace("TPE1", ""); }
            else { 作者 = "佚名"; }
            if (Mp3信息集[2] != null) { 专辑 = Mp3信息集[2].Replace("TALB", ""); }
            else { 专辑 = ""; }
            音频文件.曲名.Text = 曲名;
            音频文件.详细信息.Text = $"{专辑}{作者}";
            音频文件.文件路径.Add(项.完整路径);
            if (设置类.设置.循环模式 != 音频类型.循环类型.列表循环)
            {
                音频文件.循环类型.Content = 音频类型.循环类型.不可循环;
                ButtonHelper.SetIcon(音频文件.循环类型, 语言.图标.不可循环图标);
            }
            音频文件.文件格式 = 文件信息.项目类型.MP3文件;


            音频文件.配置文件路径 = $"{Path.GetDirectoryName(项.完整路径)}\\{项.显示名称} 配置.txt";
            if (File.Exists(音频文件.配置文件路径)) { 配置.读取音频配置(音频文件); }
            if (tmpa == 音频文件.专辑图片.ImageSource)
            {
                #region 自动匹配专辑图片
                string 专辑图片路径 = 专辑图片寻找(Path.GetDirectoryName(项.完整路径), 项.显示名称);
                if (File.Exists(专辑图片路径))
                {
                    BinaryReader binReader = new BinaryReader(File.Open(专辑图片路径, FileMode.Open));
                    FileInfo fileInfo = new FileInfo(专辑图片路径);
                    byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                    binReader.Close();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(bytes);
                    bitmap.EndInit();
                    音频文件.专辑图片.ImageSource = bitmap;

                }

                #endregion
                设置类.运行时.本Mp3未匹配到封面 = false;
            }
            return 音频文件;

        }
        private 音频文件项 读取OGG(文件信息 项)
        {

            音频文件项 音频文件 = new 音频文件项();
            音频文件.文件路径.Add(项.完整路径);
            #region 自动匹配专辑图片
            string 专辑图片路径 = 专辑图片寻找(Path.GetDirectoryName(项.完整路径), 项.显示名称);
            if (File.Exists(专辑图片路径))
            {
                BinaryReader binReader = new BinaryReader(File.Open(专辑图片路径, FileMode.Open));
                FileInfo fileInfo = new FileInfo(专辑图片路径);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
                音频文件.专辑图片.ImageSource = bitmap;

            }
            #endregion
            音频文件.曲名.Text = 项.显示名称;
            音频文件.详细信息.Text = "Ogg文件";
            //【接入点】此处可以添加读取专辑作者信息的代码
            音频文件.循环类型.Content = 音频类型.循环类型.永续;
            音频文件.文件格式 = 文件信息.项目类型.OGG文件;
            音频文件.配置文件路径 = $"{Path.GetDirectoryName(项.完整路径)}\\{项.显示名称} 配置.txt";
            if (File.Exists(音频文件.配置文件路径)) { 配置.读取音频配置(音频文件); }
            ButtonHelper.SetIcon(音频文件.循环类型, 语言.图标.永续图标);
            return 音频文件;
        }
        private 音频文件项 读取WAV(文件信息 项)
        {
            音频文件项 音频文件 = new 音频文件项();
            音频文件.曲名.Text = 项.显示名称;
            音频文件.详细信息.Text = "Wav文件 - 暂未支持";
            音频文件.循环类型.Content = 音频类型.循环类型.不可循环;
            音频文件.文件路径.Add(项.完整路径);
            音频文件.文件格式 = 文件信息.项目类型.WAV文件;
            return 音频文件;
        }
        public string 专辑图片寻找(string 文件夹路径, string 曲名)
        {
            string 专辑图片位置 = "";
            var JPG文件 = Directory.GetFiles(文件夹路径, "*.jpg");
            var PNG文件 = Directory.GetFiles(文件夹路径, "*.png");
            var BMP文件 = Directory.GetFiles(文件夹路径, "*.bmp");
            if (JPG文件.Length + PNG文件.Length + BMP文件.Length == 0)
            {
                Console.WriteLine("未找到图片文件");
                return null;
            }
            List<string> 图片文件组 = new List<string>(JPG文件.Length + PNG文件.Length + BMP文件.Length + 1);
            图片文件组.AddRange(JPG文件); 图片文件组.AddRange(PNG文件); 图片文件组.AddRange(BMP文件);
            var 结果集 = 图片文件组.FindAll(文件名 => 文件名.Contains(曲名));
            if (结果集.Count > 0)
            {
                专辑图片位置 = 结果集[0];
            }
            else
            {
                专辑图片位置 = 图片文件组[0];
            }
            return 专辑图片位置;
        }
        #endregion
        #region 列表增删改查
        /// <summary>
        /// 在UI列表中新建一个文件夹项目
        /// </summary>
        /// <param name="文件"></param>
        public void 新建_新建UI文件夹项(文件信息 文件)
        {
            string 显示列 = $"{语言.图标.文件夹图标} {文件.显示名称}";
            文件与文件夹集.Items.Add(显示列);

        }
        /// <summary>
        /// 新建一个UI音频项，但不加入UI控制
        /// </summary>
        /// <param name="曲名">歌曲的名称</param>
        /// <param name="详细信息">歌曲的详细信息，如歌手、专辑</param>
        /// <param name="循环标识">该歌曲文件使用循环方式的标志</param>
        /// <param name="类型名称">该歌曲文件使用循环方式的名称</param>
        /// <param name="文件路径集">该歌曲文件的路径集合</param>
        /// <param name="专辑图片">该歌曲的专辑图片</param>
        /// <returns></returns>
        public 音频文件项 新建_新建UI音频项(string 曲名, string 详细信息, string 循环标识, string 类型名称, List<string> 文件路径集, ImageBrush 专辑图片 = null)
        {
            if (专辑图片 == null)
            {

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Environment.CurrentDirectory + "\\Resources\\默认专辑图.png");
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                ImageBrush imageBrush = new ImageBrush();
                ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                imageBrush.ImageSource = (ImageSource)imageSourceConverter.ConvertFrom(stream);
                专辑图片 = imageBrush;

            }
            音频文件项 控件 = new 音频文件项();
            ButtonHelper.SetIcon(控件.循环类型, 循环标识);
            控件.循环类型.Content = 类型名称;
            控件.曲名.Text = 曲名;
            控件.详细信息.Text = 详细信息;
            控件.文件路径 = 文件路径集;
            控件.配置文件路径 = $"{Path.GetFileNameWithoutExtension(文件路径集[0])} 配置.txt";
            if (!File.Exists(控件.配置文件路径)) { 控件.配置文件路径 = null; }
            //【接入点】此处可以添加进行类型判定所需的代码
            控件.专辑图片 = 专辑图片;
            return 控件;
        }
        /// <summary>
        /// 向UI组件添加一个UI音频项目
        /// </summary>
        /// <param name="UI">当前控制的UI信息类</param>
        /// <param name="音频项">已经新建好的音频文件项</param>
        /// <returns>返回该UI控件的ID（曲名+文件后缀名）</returns>
        public string 新建_添加UI音频项(UI信息 UI, 音频文件项 音频项)
        {
            文件与文件夹集.Items.Add(音频项);
            string ID = 生成ID(音频项.曲名.Text, Path.GetExtension(音频项.文件路径[0]));
            try
            {
                UI.当前列表项.Add(ID, 音频项);
            }
            catch
            {
                ID += DateTime.Now.Ticks;
                UI.当前列表项.Add(ID, 音频项);
            }
            return ID;
        }
        public void 删除_删除UI音频项(Dictionary<string, 音频文件项> 列表, string ID)
        {
            文件与文件夹集.Items.Remove(列表[ID]);
            列表.Remove(ID);
        }
        public void 删除_清空UI音频项(Dictionary<string, 音频文件项> 列表)
        {
            文件与文件夹集.Items.Clear();
            列表.Clear();
        }
        public void 删除_仅清空UI()
        {
            文件与文件夹集.Items.Clear();
        }
        #endregion
        #region 唯一码生成
        public string 生成ID(string 曲名, string 完整文件路径)
        {
            string ID = 曲名 + Path.GetExtension(完整文件路径);
            return ID;

        }
        #endregion
        #region UI更新
        /// <summary>
        /// 将一个音频信息类传入，将自动将信息更新到UI界面中
        /// 每访问一次，更新一次，由中间件来控制更新
        /// </summary>
        /// <param name="状态"></param>
        public void UI更新(音频信息 状态)
        {
            播放中 = 状态;
            #region 更新时间信息
            double 时间百分比 = (状态.当前时长.TotalSeconds / 状态.持续时长.TotalSeconds) * 100;
            if (状态.当前时长.TotalSeconds == 0) { return; }
            this.Dispatcher.Invoke((ThreadStart)delegate ()
            {
                时间信息.Text = $"{状态.当前时长.ToString(@"mm\:ss")}/{状态.持续时长.ToString(@"mm\:ss")}";
                //没拉动进度条时更新
                if (!设置类.运行时.进度条拉动)
                {
                    进度条.Value = 时间百分比;
                }
            });
            #endregion
            #region 更新显示开关
            this.Dispatcher.Invoke((ThreadStart)delegate ()
            {
                if (状态 == null || 状态.音频类型 == 音频类型.循环类型.不可循环 || 状态.音频类型 == 音频类型.循环类型.永续 ||  状态.音频类型 == 音频类型.循环类型.步进循环)
                {
                    小节信息.Visibility = Visibility.Collapsed;
                }
                else
                {
                    小节信息.Visibility = Visibility.Visible;
                }
            });
            #endregion
            #region 更新小节信息
            if (状态.当前小节 == -1 && 状态.当前拍数 == -1)
            {

                this.Dispatcher.Invoke((ThreadStart)delegate ()
                {
                    小节信息.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                this.Dispatcher.Invoke((ThreadStart)delegate ()
                {
                    小节信息.Visibility = Visibility.Visible;
                    小节信息.Text = $"{状态.当前小节}节 {状态.当前拍数}拍";
                    日志.输出($"输出了文本{状态.当前小节}节{状态.当前拍数}拍");
                });
            }
            #endregion
            #region 更新章节信息
            if (状态 != null && 状态.章节信息.Count > 状态.章节_当前编号)
            {
                this.Dispatcher.Invoke((ThreadStart)delegate ()
                {

                    if (状态.音频类型 == 音频类型.循环类型.无缝章节循环) { 段落切换按钮.Content = 状态.章节信息[状态.章节_当前编号].ToString(); }
                    if (状态.音频类型 == 音频类型.循环类型.步进循环) { 段落切换按钮.Content = 状态.章节信息[状态.章节_当前编号].ToString(); }
                    if (状态.正在切换章节 == false) { 段落切换按钮.IsEnabled = true; } else { 段落切换按钮.Content = "切换中..."; }
                });
            }
            this.Dispatcher.Invoke((ThreadStart)delegate ()
            {
                if (状态.音频类型 == 音频类型.循环类型.永续 && 状态.播放列表.Count - 1 == 状态.章节_当前编号) { 段落切换按钮.Content = 语言.音频状态.默认段落.循环段落; }
                if (状态.音频类型 == 音频类型.循环类型.永续 && !(状态.播放列表.Count - 1 == 状态.章节_当前编号)) { 段落切换按钮.Content = 语言.音频状态.默认段落.起始段落; }
            });
            #endregion
            #region 更新循环信息
            this.Dispatcher.Invoke((ThreadStart)delegate ()
            {
                循环次数.Text = 状态.循环次数.ToString() + " 循环";
            });
            #endregion

        }
        /// <summary>
        /// 更新右键点击循环模式所显示的循环选项
        /// </summary>
        /// <param name="信息">要更新的UI组件</param>
        public void 循环模式菜单更新(音频文件项 信息)
        {

            if (信息.文件格式 == 文件信息.项目类型.MP3文件 && !File.Exists(信息.配置文件路径))
            {
                循环_步进.Visibility = Visibility.Collapsed;
                循环_无缝章节.Visibility = Visibility.Collapsed;
                循环_永续.Visibility = Visibility.Collapsed;
            }
            if (信息.文件格式 == 文件信息.项目类型.OGG文件)
            {
                循环_步进.Visibility = Visibility.Collapsed;
                循环_无缝章节.Visibility = Visibility.Collapsed;
                循环_永续.Visibility = Visibility.Visible;
            }

        }
        /// <summary>
        /// 管理在文件浏览器中，右键点击某选项后弹出的右键菜单项
        /// </summary>
        /// <param name="信息">要更新的UI组件</param>
        public void 右键菜单更新(音频文件项 信息)
        {
            if (信息.配置文件路径 != null)
            {
                日志.输出(信息.配置文件路径);
            }

            if (信息.文件格式 == 文件信息.项目类型.MP3文件 && !File.Exists(信息.配置文件路径))
            {
                管理配置文件.Visibility = Visibility.Collapsed;
                新建配置文件.Visibility = Visibility.Collapsed;
            }
            if (信息.文件格式 == 文件信息.项目类型.OGG文件 && !File.Exists(信息.配置文件路径))
            {
                管理配置文件.Visibility = Visibility.Collapsed;
                新建配置文件.Visibility = Visibility.Visible;
            }
            if (信息.文件格式 == 文件信息.项目类型.OGG文件 && File.Exists(信息.配置文件路径))
            {
                管理配置文件.Visibility = Visibility.Visible;
                新建配置文件.Visibility = Visibility.Collapsed;
            }

        }
        /// <summary>
        /// 管理在文件浏览器中，右键点击某选项后弹出的右键菜单项
        /// 由于不传入任何UI组件，将关闭所有可以互操作的菜单项
        /// </summary>
        public void 右键菜单更新()
        {

            管理配置文件.Visibility = Visibility.Collapsed;
            新建配置文件.Visibility = Visibility.Collapsed;
        }
        #endregion
        #region UI控制代码
        private void 进度条_MouseEnter(object sender, MouseEventArgs e)
        {
            SliderHelper.SetThumbBorderThickness((Slider)sender, new Thickness(0));
        }

        private void 进度条_MouseLeave(object sender, MouseEventArgs e)
        {
            SliderHelper.SetThumbBorderThickness((Slider)sender, new Thickness(15));
        }

        private void 进度条_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            音频中间件.播放状态管理.调整播放进度(进度条.Value / 100.0);
            设置类.运行时.进度条拉动 = false;
        }

        private void 进度条_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            设置类.运行时.进度条拉动 = true;
        }
        private void 文件与文件夹集_MouseDoubleClick(object sender, MouseButtonEventArgs e = null)
        {
            if (文件与文件夹集.SelectedIndex == -1) { return; }
            文件与文件夹集.IsEnabled = false;
            #region 界面更新操作...
            if (文件与文件夹集.SelectedItem == null)
            {
                设置类.日志.输出("未选择项目");
                return;
            }
            var 类型 = 文件与文件夹集.SelectedItem.GetType();
            if (类型 == "".GetType())
            {
                设置类.日志.输出("文件夹");
                string 文件夹路径 = 运行变量.当前文件夹路径 + @"\" + 文件与文件夹集.SelectedItem.ToString().Replace($"{语言.图标.文件夹图标} ", "");
                打开文件夹(文件夹路径);
            }
            if (类型 == typeof(音频文件项))
            {
                设置类.日志.输出("音频文件");
                播放音频文件();
            }
            #endregion
            文件与文件夹集.IsEnabled = true;
        }

        private void 后退按钮_Click(object sender, RoutedEventArgs e)
        {
            上一文件夹();
        }

        private void 前进按钮_Click(object sender, RoutedEventArgs e)
        {
            下一文件夹();
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            打开文件夹(运行变量.当前文件夹路径);
        }

        private void 上级按钮_Click(object sender, RoutedEventArgs e)
        {
            打开文件夹(Path.GetDirectoryName(运行变量.当前文件夹路径));
        }

        private void 文件夹选择按钮_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog 文件夹选择器 = new CommonOpenFileDialog();
            文件夹选择器.IsFolderPicker = true;//设置为选择文件夹
            CommonFileDialogResult 结果 = 文件夹选择器.ShowDialog();
            if (结果 == CommonFileDialogResult.Ok)
            {
                打开文件夹(文件夹选择器.FileName);
            }
        }

        private void 搜索框_LostFocus(object sender, RoutedEventArgs e)
        {
            if (搜索框.Text.Length <= 0)
            {
                搜索框.Text = "在文件中搜索...";
            }
        }

        private void 搜索框_GotFocus(object sender, RoutedEventArgs e)
        {
            搜索框.Text = "";
        }

        private void 搜索框_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (搜索框.Text.Length == 0) { return; }
            var query = UI信息.当前列表项.Keys.Where(x => x.Contains(搜索框.Text));
            if (query.Count<string>() == 0)
            {
                设置类.日志.输出("搜索无匹配项");
                return;
            }
            foreach (var item in query)
            {

                文件与文件夹集.SelectedItem = UI信息.当前列表项[item];
                文件与文件夹集.ScrollIntoView(UI信息.当前列表项[item]);
                return;

            }

        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            停止所有播放线程();
        }

        private void 循环_强制_Click(object sender, RoutedEventArgs e)
        {
            if (播放中 != null)
            {
                设置类.设置.循环模式 = 音频类型.循环类型.强制循环;
                循环模式.Content = "强制循环";
                设置类.日志.输出("切换为强制循环");
            }

        }
        private void 循环_关闭_Click(object sender, RoutedEventArgs e)
        {
            if (播放中 != null)
            {
                设置类.设置.循环模式 = 音频类型.循环类型.强制循环;
                循环模式.Content = "强制循环";
                设置类.日志.输出("切换为强制循环");
            }
        }


        private void 循环_顺序_Click(object sender, RoutedEventArgs e)
        {
            if (播放中 != null)
            {
                设置类.设置.循环模式 = 音频类型.循环类型.列表循环;
                循环模式.Content = "列表循环";
                设置类.日志.输出("切换为列表循环");
                ButtonHelper.SetIcon(循环模式, 语言.图标.文件夹图标);
            }

        }

        private void 循环_永续_Click(object sender, RoutedEventArgs e)
        {
            if (播放中 != null)
            {
                设置类.设置.循环模式 = 音频类型.循环类型.永续;
                循环模式.Content = "永续";
                设置类.日志.输出("切换为永续");
                ButtonHelper.SetIcon(循环模式, 语言.图标.永续图标);
            }
        }
        private void 停止按钮_Click(object sender, RoutedEventArgs e)
        {
            停止所有播放线程();
        }

        private void 播放暂停按钮_Click(object sender, RoutedEventArgs e)
        {
            if (播放中 == null)
            {
                文件与文件夹集_MouseDoubleClick(new object());
                return;

            }
            if (播放中.音频状态 == 语言.音频状态.播放状态.播放中)
            {
                播放状态管理.暂停处理();
                设置类.日志.输出("正在暂停");
                播放暂停按钮.Content = Regex.Unescape("\ue902");
                return;
            }
            if (播放中.音频状态 == 语言.音频状态.播放状态.暂停中)
            {
                播放状态管理.播放处理();
                设置类.日志.输出("正在继续");
                播放暂停按钮.Content = Regex.Unescape("\ue91a");
                return;
            }
            if (播放中.音频状态 == 语言.音频状态.播放状态.停止中)
            {
                播放音频文件();
                设置类.日志.输出("正在重新播放");
                播放暂停按钮.Content = Regex.Unescape("\ue91a");
                return;
            }
        }

        private void 音量按钮_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            音量提示.Visibility = Visibility.Visible;
            设置类.运行时.音量 += e.Delta / 60;
            if (设置类.运行时.音量 >= 100) { 设置类.运行时.音量 = 100; }
            if (设置类.运行时.音量 <= 0) { 设置类.运行时.音量 = 0; 音量按钮.Content = "🔇"; }
            if (设置类.运行时.音量 <= 20 && 设置类.运行时.音量 != 0) { 音量按钮.Content = "🔈"; }
            if (设置类.运行时.音量 >= 50) { 音量按钮.Content = "🔉"; }
            if (设置类.运行时.音量 >= 80) { 音量按钮.Content = "🔊"; }
            音量提示.Text = $"音量：{设置类.运行时.音量}";
            if (播放中 != null)
            {
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.MP3文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.MP3文件, 运行时.音量);
                }
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.OGG文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.OGG文件, 运行时.音量);

                }
            }
        }

        private void 音量按钮_Click(object sender, RoutedEventArgs e)
        {
            if (设置类.运行时.音量 == 0)
            {

                设置类.运行时.音量 = 50;
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.MP3文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.MP3文件, 运行时.音量);
                }
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.OGG文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.OGG文件, 运行时.音量);
                }
                音量按钮.Content = "🔉";

            }
            else
            {
                设置类.运行时.音量 = 0;
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.MP3文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.MP3文件, 运行时.音量);
                }
                if (播放中 != null && 播放中.文件格式 == 文件信息.项目类型.OGG文件)
                {
                    音频中间件.播放状态管理.调整音量(文件信息.项目类型.OGG文件, 运行时.音量);
                }
                音量按钮.Content = "🔇";
            }
        }

        private void 文件与文件夹集_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var Controltmp = (ListBox)sender;
            if (Controltmp.SelectedItem == null) { return; }
            if (Controltmp.SelectedItem.GetType() == typeof(音频文件项))
            {
                var ItemTemp = (音频文件项)Controltmp.SelectedItem;
                右键菜单更新(ItemTemp);
            }
            if (Controltmp.SelectedItem.GetType() == "".GetType())
            {
                右键菜单更新();
                return;
            }

        }

        private void 打开所在文件夹_Click(object sender, RoutedEventArgs e)
        {
            var FilePath = "";
            if (文件与文件夹集.SelectedItem == null) { return; }
            if (文件与文件夹集.SelectedItem.GetType() == typeof(音频文件项))
            {
                var ItemTemp = (音频文件项)文件与文件夹集.SelectedItem;
                FilePath = ItemTemp.文件路径[0];
            }
            if (文件与文件夹集.SelectedItem.GetType() == "".GetType())
            {
                FilePath = 运行变量.当前文件夹路径;

            }

            文件浏览中间件.系统交互 资源管理器 = new 系统交互();
            资源管理器.定位文件(FilePath);
        }

        private void 管理配置文件_Click(object sender, RoutedEventArgs e)
        {
            var 音频 = (音频文件项)文件与文件夹集.SelectedItem;
            配置.新建循环配置文件 配置文件窗 = new 配置.新建循环配置文件(音频.配置文件路径);
            配置文件窗.Show();
        }

        private void 段落切换按钮_Click(object sender, RoutedEventArgs e)
        {

            段落切换按钮.IsEnabled = false;
            音频中间件.播放状态管理.执行段落切换();
        }




        #endregion

    }
}
