using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using 交互音乐播放器.UI;
using 交互音乐播放器.UI.附加控件;
using 交互音乐播放器.数据.播放中数据;

namespace 交互音乐播放器.数据
{

    public class 音乐播放数据
    {
        /// <summary>
        /// 播放数据的枚举
        /// </summary>
        public enum 播放数据组
        {
            段落组,
            播放流组,
            文件组,
            当前流,
            当前段落,
            当前文件
        }

        /// <summary>
        /// 已加载的段落组，通过文件的别名获取段落信息
        /// </summary>
        public Dictionary<string, 段落信息> 段落组 = new Dictionary<string, 段落信息>();
        /// <summary>
        /// 已加载的播放流组，通过播放流的别名来获取流信息
        /// </summary>
        public Dictionary<string, 流信息> 播放流组 = new Dictionary<string, 流信息>();
        /// <summary>
        /// 已加载的文件组，通过文件的别名来获取文件信息
        /// </summary>
        public Dictionary<string, 文件信息> 文件组 = new Dictionary<string, 文件信息>();
        /// <summary>
        /// 设置或读取当前正在播放的流信息
        /// </summary>
        public 流信息? 当前流 { get; set; } = new 流信息();
        /// <summary>
        /// 设置或读取当前正在播放的段落信息
        /// </summary>
        public 段落信息? 当前段落 { get; set; } = new 段落信息();
        /// <summary>
        /// 设置或读取当前正在播放文件的信息
        /// </summary>
        public 文件信息? 当前文件 { get; set; } = null;
        /// <summary>
        /// 快速检查要访问的对象是否为空值或空引用
        /// </summary>
        /// <param name="数据类型">枚举值</param>
        /// <param name="别名">[可选] 要访问的别名，若是当前的信息则不需要该值</param>
        /// <returns></returns>
        public bool 空值或空引用(播放数据组 数据类型, string 别名 = "")
        {
            if (数据类型 == 播放数据组.当前文件)
            {
                if (当前文件 == null) { return true; }
                return false;
            }
            if (数据类型 == 播放数据组.当前段落)
            {
                if (当前段落 == null) { return true; }
                return false;
            }
            if (数据类型 == 播放数据组.当前流)
            {
                if (当前流 == null) { return true; }
                return false;
            }
            if (数据类型 == 播放数据组.播放流组)
            {
                if (string.IsNullOrEmpty(别名)) { return true; }
                if (播放流组 == null) { return true; }
                if (!播放流组.ContainsKey(别名)) { return true; }
                return false;
            }
            if (数据类型 == 播放数据组.文件组)
            {
                if (string.IsNullOrEmpty(别名)) { return true; }
                if (文件组 == null) { return true; }
                if (!文件组.ContainsKey(别名)) { return true; }
                return false;
            }
            if (数据类型 == 播放数据组.段落组)
            {
                if (string.IsNullOrEmpty(别名)) { return true; }
                if (段落组 == null) { return true; }
                if (!段落组.ContainsKey(别名)) { return true; }
                return false;
            }
            return true;
        }
        public void 重定向当前流引用(string 流名称)
        {
            if (空值或空引用(播放数据组.播放流组, 流名称) || 播放流组[流名称].当前段落 == null) { Debug.Print("重定向到流时，流当前段落没有指向"); return; }
            当前段落 = 播放流组[流名称].当前段落; 当前文件 = 播放流组[流名称].当前段落!.绑定文件;
            当前流 = 播放流组[流名称];
        }

        public void 重定向所有当前引用()
        {
            当前流 = 播放流组.FirstOrDefault().Value;
            当前段落 = 当前流.当前段落;
            当前文件 = 当前段落.绑定文件;
        }

    }
    public class 脚本文件数据
    {
        public object 状态_图片访问锁 = new object();
        public float 默认BPM { get; set; } = -1;
        public int 默认小节节拍分量 { get; set; } = -1;
        public int 默认小节节拍总数 { get; set; } = -1;
        public int 段落数 { get; set; }
        //通过段落编号来获取其Offset
        public List<double> Offset组 { get; set; } = new List<double>();
        //通过段落编号来获取文件路径
        public List<string> 文件组 { get; set; }
        //通过段落编号来获取段落名称，实际上在数据类中是文件别名
        public List<string> 段落名称 { get; set; }
        //通过段落编号来获取循环段落的名称
        public List<string> 循环段落名称 { get; set; }
        //通过段落编号来获取节拍分量
        public List<int> 小节节拍分量组 { get; set; }
        //通过段落编号来获取节拍总数
        public List<int> 小节节拍总数组 { get; set; }
        //通过段落编号来获取BPM
        public List<float> BPM组 { get; set; }
        //通过段落编号来获取是否需要循环
        public List<bool> 循环下标组 { get; set; }

        public string 配置文件版本 { get; set; } = "2.0.0";
        public string 名称 { get; set; }
        public string 作者 { get; set; }
        public string 专辑 { get; set; }
        public bool 拥有配置文件 { get; set; }
        public bool 拥有脚本 { get; set; }
        public string 用户TAG { get; set; }
        public int 文件时长 { get; set; }
        public double 文件大小 { get; set; }
        public string 格式 { get; set; }
        public bool 使用原生脚本 { get; set; }
        public string 脚本基类 { get; set; } = "仅播放";
        public string 脚本文档 { get; set; }
        public ImageBrush 专辑图片 { get; set; } = new ImageBrush();
    }
    public class 脚本文件读写器
    {

        public static void 读取脚本文件(文件项 文件项)
        {
            if (脚本编辑.当前脚本编辑器 == null) { Debug.Print("读取脚本文件 - 还没有加载好脚本文件编辑器"); return; }
            if (文件项.脚本数据.拥有配置文件)
            {
                string 脚本路径 = 获取脚本文件位置(文件项.脚本数据);
                if (!File.Exists(脚本路径))
                {
    
                    脚本路径 = 修正获取脚本文件位置(文件项);
                    if (!File.Exists(脚本路径)) 
                    { MessageBox.Show($"脚本文件：{脚本路径}不存在，且自动寻找失败，若文件路径做过移动可能会出现此问题。"); return; }
                }
                var 脚本文本 = File.ReadAllText(脚本路径);
                try
                {
                    var 脚本字典 = 转换配置文本到字典(脚本文本);
                    if (脚本字典 == null) { Debug.Print("读取脚本文件 - 转换配置到字典失败"); return; }
                    设定属性<脚本文件数据>(文件项.脚本数据, 脚本字典, '|');
                }
                catch (Exception 错误信息)
                {
                    MessageBox.Show($"读取配置文件失败，详细信息如下：\r\n{错误信息.Message}\r\n堆信息\r\n{错误信息.StackTrace}");
                }

            }
            脚本编辑.当前脚本编辑器.载入文件(文件项.脚本数据);
        }

        public static void 读取脚本文件(脚本文件数据 脚本数据)
        {
            if (脚本编辑.当前脚本编辑器 == null) { Debug.Print("读取脚本文件 - 还没有加载好脚本文件编辑器"); return; }
            if (脚本数据.拥有配置文件)
            {
                string 脚本路径 = 获取脚本文件位置(脚本数据);
                var 脚本文本 = File.ReadAllText(脚本路径);
                try
                {
                    var 脚本字典 = 转换配置文本到字典(脚本文本);
                    设定属性<脚本文件数据>(脚本数据, 脚本字典, '|');
                }
                catch (Exception 错误信息)
                {
                    MessageBox.Show($"读取配置文件失败，详细信息如下：\r\n{错误信息.Message}\r\n堆信息\r\n{错误信息.StackTrace}");
                }

            }
            脚本编辑.当前脚本编辑器.载入文件(脚本数据);
        }

        private static Dictionary<string, string>? 转换配置文本到字典(string 脚本文本)
        {

            Dictionary<string, string> 配置字典 = new();
            Regex 配置字典匹配器 = new Regex("(.*)=(.*)");
            var 配置字典匹配集 = 配置字典匹配器.Matches(脚本文本);
            if (配置字典匹配集.Count == 0)
            { return null; }
            for (int i = 0; i < 配置字典匹配集.Count; i++)
            {
                var 键 = 配置字典匹配集[i].Groups[1].Value;
                var 值 = 配置字典匹配集[i].Groups[2].Value.Trim();
                配置字典.Add(键, 值);
            }

            Regex 脚本匹配器 = new Regex("执行脚本].*[\\s\\S]([\\s\\S]+)");
            var 脚本匹配集 = 脚本匹配器.Matches(脚本文本);
            if (脚本匹配集.Count == 0)
            { return 配置字典; }
            配置字典.Add("脚本文档", 脚本匹配集[0].Groups[1].Value.Trim());
            return 配置字典;
        }

        public static void 储存脚本文件(脚本文件数据 脚本数据, string 文件路径)
        {
            StringBuilder 配置文档 = new();
            var 属性集 = 获取属性<脚本文件数据>(脚本数据);
            配置文档.Append("[配置项]\r\n");
            foreach (var 属性 in 属性集)
            {
                配置文档.Append($"{属性.Key}={属性.Value}\r\n");
            }
            配置文档.Append("[执行脚本]\r\n");
            配置文档.Append(脚本数据.脚本文档);
            File.WriteAllText(文件路径, 配置文档.ToString());
            Debug.Print("脚本文件已储存到" + 文件路径);
        }
        public static void 删除脚本文件(脚本文件数据 脚本数据)
        {
            File.Delete(脚本文件读写器.获取脚本文件位置(脚本数据));
        }
        public static string 获取脚本文件位置(脚本文件数据 脚本数据)
        {
            string 文件位置 = "";
            文件位置 = $"{Path.GetDirectoryName(脚本数据.文件组[0])}\\{Path.GetFileNameWithoutExtension(脚本数据.文件组[0])}.脚本"; return 文件位置;
        }

        public static string 修正获取脚本文件位置(文件项 文件项数据)
        {
            string 文件位置 = "";
            文件位置 = $"{UI界面数据.当前浏览文件夹}\\{Path.GetFileNameWithoutExtension(文件项数据.脚本数据.文件组[0])}.脚本"; 
            return 文件位置;
        }


        private static void 建立临时配置文件(脚本文件数据 脚本数据)
        {
            var 属性集 = 获取属性<脚本文件数据>(脚本数据);
            Debug.Print("已读取数据");
        }

        private static Dictionary<string, string> 获取属性<T>(T 目标类)
        {
            Dictionary<string, string> 属性列表 = new();
            if (目标类 == null)
            {
                return 属性列表;
            }
            System.Reflection.PropertyInfo[] 属性集 = 目标类.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (属性集.Length <= 0)
            {
                return 属性列表;
            }
            foreach (System.Reflection.PropertyInfo 属性 in 属性集)
            {
                string 属性名称 = 属性.Name; //名称
                Debug.Print($"属性名称{属性名称}");
                if (属性名称 == "专辑图片") { continue; }
                if (属性名称 == "脚本文档") { continue; }
                if (属性名称.Contains("状态_")) { continue; }
                object? 属性值 = 属性?.GetValue(目标类, null);  //值
                if (属性 == null) { continue; }
                if (属性值 == null) { continue; }
                if (属性.PropertyType.IsValueType || 属性.PropertyType.Name.StartsWith("String"))
                {
                    属性列表.Add(属性名称, 属性值?.ToString());
                }
                if (属性.PropertyType.Name.StartsWith("List"))
                {
                    //为不同情况转换不同的类型
                    if (属性值 == null) { continue; }
                    if (属性值.GetType() == new List<string>(0).GetType())
                    {
                        List<string> 列表 = (List<string>)属性值;
                        StringBuilder 值列 = new StringBuilder();
                        foreach (var 值 in 列表)
                        {
                            值列.Append($"{值}|");
                        }
                        值列 = 值列.Remove(值列.Length - 1, 1);
                        属性列表.Add(属性名称, 值列.ToString());

                    }
                    if (属性值.GetType() == new List<int>(0).GetType())
                    {
                        List<int> 列表 = (List<int>)属性值;
                        StringBuilder 值列 = new StringBuilder();
                        foreach (var 值 in 列表)
                        {
                            值列.Append($"{值}|");
                        }
                        值列 = 值列.Remove(值列.Length - 1, 1);
                        属性列表.Add(属性名称, 值列.ToString());
                    }
                    if (属性值.GetType() == new List<double>(0).GetType())
                    {
                        List<double> 列表 = (List<double>)属性值;
                        StringBuilder 值列 = new StringBuilder();
                        foreach (var 值 in 列表)
                        {
                            值列.Append($"{值}|");
                        }
                        值列 = 值列.Remove(值列.Length - 1, 1);
                        属性列表.Add(属性名称, 值列.ToString());
                    }
                    if (属性值.GetType() == new List<float>(0).GetType())
                    {
                        List<float> 列表 = (List<float>)属性值;
                        StringBuilder 值列 = new StringBuilder();
                        foreach (var 值 in 列表)
                        {
                            值列.Append($"{值}|");
                        }
                        值列 = 值列.Remove(值列.Length - 1, 1);
                        属性列表.Add(属性名称, 值列.ToString());
                    }
                    if (属性值.GetType() == new List<bool>(0).GetType())
                    {
                        List<bool> 列表 = (List<bool>)属性值;
                        StringBuilder 值列 = new StringBuilder();
                        foreach (var 值 in 列表)
                        {
                            值列.Append($"{值}|");
                        }
                        值列 = 值列.Remove(值列.Length - 1, 1);
                        属性列表.Add(属性名称, 值列.ToString());
                    }

                }

            }
            return 属性列表;
        }

        private static void 设定属性<T>(T 数据类的实例, Dictionary<string, string> 要匹配的数据集, char 集合元素分隔符)
        {
            System.Reflection.PropertyInfo[] 属性集 = 数据类的实例.GetType()
                                                                  .GetProperties(BindingFlags.Instance | BindingFlags.Public); //仅实例与公开成员
            foreach (var 属性 in 要匹配的数据集)
            {
                PropertyInfo 待设置属性 = 属性集.Where(x => x.Name == 属性.Key).ToArray().FirstOrDefault();
                if (待设置属性 == null) { continue; }
                Type 数据类 = 数据类的实例.GetType();
                object 待设置值 = new object();
                if (待设置属性.PropertyType.IsGenericType) //若是一个集合类型，则开始集合设置的逻辑
                {
                    //转换原先的字符串数据到新集合中
                    string[] 待设置的集合元素 = 属性.Value.Split(集合元素分隔符);
                    Type 集合类型 = 待设置属性.PropertyType;
                    Type 元素类型 = 待设置属性.PropertyType.GetGenericArguments()[0];
                    dynamic? 新集合 = Activator.CreateInstance(集合类型);
                    foreach (var 待设置元素 in 待设置的集合元素)
                    {
                        dynamic 已转换类型的元素 = Convert.ChangeType(待设置元素, 元素类型);
                        新集合.Add(已转换类型的元素);
                    }
                    待设置值 = 新集合;
                }

                else { 待设置值 = Convert.ChangeType(属性.Value, 待设置属性.PropertyType); } //若不是集合类型，则开始普通属性设置逻辑
                数据类.GetProperty(待设置属性.Name)?.SetValue(数据类的实例, 待设置值, null);
            }
        }

    }

    public class 音乐文件数据辅助
    {
        public static object 锁 = new object();
        /// <summary>
        /// 自动寻找并在脚本项中设置专辑图片，若是MP3，将返回MP3文件的元数据
        /// </summary>
        /// <param name="脚本数据"></param>
        /// <returns></returns>
        public string[] 寻找并设定专辑图片(脚本文件数据 脚本数据)
        {
          
                string[] 元数据集 = new string[0];
                //如果是MP3文件则执行MP3元数据读取
                if (脚本数据.格式 == 系统控制.支援的文件类型.mp3.ToString())
                {
                    MP3信息读取 MP3信息 = new();
                    元数据集 = MP3信息.读取MP3元数据(脚本数据.文件组[0], 脚本数据);
                    if (元数据集[0] != null) { 元数据集[0] = 元数据集[0].Replace("TIT2", ""); }
                    if (元数据集[1] != null) { 元数据集[1] = 元数据集[1].Replace("TPE1", ""); }
                    if (元数据集[2] != null) { 元数据集[2] = 元数据集[2].Replace("TALB", ""); }
                }
                bool 成功读取了MP3文件的图像 = false;
       
                    if (脚本数据.专辑图片.ImageSource != null) { 成功读取了MP3文件的图像 = true; }
              
                if (成功读取了MP3文件的图像) { return 元数据集; }
                //如果是非MP3文件，则执行非MP3元数据读取
                string 专辑图片路径 = 在文件夹中寻找默认专辑图片(Path.GetDirectoryName(脚本数据.文件组[0]), 脚本数据.名称);
                if (专辑图片路径 == null) { return 元数据集; }
                BinaryReader 二进制读取器 = new(File.Open(专辑图片路径, FileMode.Open));
                FileInfo 文件信息 = new(专辑图片路径);
                byte[] 图片数据 = 二进制读取器.ReadBytes((int)文件信息.Length);
                二进制读取器.Close();
                BitmapImage 图片 = new();
                图片.BeginInit();
                图片.StreamSource = new MemoryStream(图片数据);
                图片.EndInit();
                ImageBrush 图片笔刷 = new();
                图片笔刷.ImageSource = 图片;
                脚本数据.专辑图片.ImageSource = 图片笔刷.ImageSource;
                二进制读取器.Dispose();
                return 元数据集;
        }

        private string? 在文件夹中寻找默认专辑图片(string? 文件夹路径, string? 曲名)
        {
            if (文件夹路径 == null || 曲名 == null) { Debug.Print("未指定文件夹路径或曲名"); return null; }
            string 专辑图片位置 = "";
            var JPG文件 = Directory.GetFiles(文件夹路径, "*.jpg");
            var PNG文件 = Directory.GetFiles(文件夹路径, "*.png");
            var BMP文件 = Directory.GetFiles(文件夹路径, "*.bmp");
            if (JPG文件.Length + PNG文件.Length + BMP文件.Length == 0)
            {
                Console.WriteLine("未找到图片文件");
                return null;
            }
            List<string> 图片文件组 = new(JPG文件.Length + PNG文件.Length + BMP文件.Length + 1);
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
    }
}
