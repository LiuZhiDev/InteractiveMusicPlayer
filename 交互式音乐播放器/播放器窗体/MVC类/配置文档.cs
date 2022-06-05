using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace 交互式音乐播放器.配置文件
{
    /// <summary>
    /// 执行有关配置文件（INI）的读写操作
    /// </summary>
    static class 配置
    {
        #region 读写配置项
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
        /// <summary>
        ///读取一个配置项，如果这个配置项中的值不存在返回一个默认值
        /// </summary>
        /// <param name="配置节名">配置节名</param>
        /// <param name="配置项名">配置项名</param>
        /// <param name="默认返回">如果这个配置项中的值不存在返回一个默认值</param>
        /// <param name="配置项文件">指定一个配置项的文件路径</param>
        /// <returns></returns>
        public static string 读配置项(string 配置项文件, string 配置节名, string 配置项名, string 默认返回)
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
        public static bool 写配置项(string 配置项路径, string 配置节名, string 配置项名, string 值)
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
    }
    /// <summary>
    /// 执行有关MP3音频格式文件的配置文档（ID3）的读取操作
    /// </summary>
    internal class MP3信息读取
    {
        public string[] ReadMp3(string path, 交互式音乐播放器.控件.音频文件项 音频文件项)
        {
            int mp3TagID = 0;
            string[] tags = new string[6];
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            // fs.Read(buffer, 0, 128);
            string mp3ID = "";

            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            //int size = (buffer[6] & 0x7F) * 0X200000 * (buffer[7] & 0x7f) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9]);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                mp3TagID = 1;
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }

                tags = ReadFrame(fs, size, 音频文件项);


                return tags;

            }
            return tags;
        }
        public string[] ReadFrame(FileStream fs, int size, 交互式音乐播放器.控件.音频文件项 音频文件项)
        {
            string[] ID3V2 = new string[6];
            byte[] buffer = new byte[10];
            while (size > 0)
            {
                //fs.Read(buffer, 0, 1);
                //if (buffer[0] == 0)
                //{
                //    size--;
                //    continue;
                //}
                //fs.Seek(-1, SeekOrigin.Current);
                //size++;
                //读取标签帧头的10个字节
                fs.Read(buffer, 0, 10);
                size -= 10;
                //得到标签帧ID
                string FramID = Encoding.Default.GetString(buffer, 0, 4);
                //计算标签帧大小，第一个字节代表帧的编码方式
                int frmSize = 0;

                frmSize = buffer[4] * 0x1000000 + buffer[5] * 0x10000 + buffer[6] * 0x100 + buffer[7];
                if (frmSize == 0)
                {
                    //就说明真的没有信息了
                    break;
                }
                //bFrame 用来保存帧的信息
                byte[] bFrame = new byte[frmSize];
                fs.Read(bFrame, 0, frmSize);
                size -= frmSize;
                string str = GetFrameInfoByEcoding(bFrame, bFrame[0], frmSize - 1);
                if (FramID.CompareTo("TIT2") == 0)
                {
                    ID3V2[0] = "TIT2" + str;
                }
                else if (FramID.CompareTo("TPE1") == 0)
                {
                    ID3V2[1] = "TPE1" + str;
                }
                else if (FramID.CompareTo("TALB") == 0)
                {
                    ID3V2[2] = "TALB" + str;
                }
                else if (FramID.CompareTo("TIME") == 0)
                {
                    ID3V2[3] = "TYER" + str;
                }
                else if (FramID.CompareTo("COMM") == 0)
                {
                    ID3V2[4] = "COMM" + str;
                }
                else if (FramID.CompareTo("APIC") == 0)
                {


                    int i = 0;
                    while (true)
                    {
                        if (i + 1 > bFrame.Length)
                        {
                            break;
                        }
                        if (255 == bFrame[i] && 216 == bFrame[i + 1])
                        {
                            //在
                            break;

                        }
                        i++;
                    }


                    byte[] imge = new byte[frmSize - i];
                    fs.Seek(-frmSize + i, SeekOrigin.Current);
                    fs.Read(imge, 0, imge.Length);
                    MemoryStream ms = new MemoryStream(imge);
                    System.Drawing.Image img = null;
                    try
                    {
                        img = System.Drawing.Image.FromStream(ms);
                    }
                    catch
                    {
                        设置类.日志.输出("该文件可能不是图片，已跳过");
                        return ID3V2;
                    }


                    FileStream save = new FileStream($"{System.IO.Directory.GetCurrentDirectory()}\\专辑封面.jpeg", FileMode.Create);
                    img.Save(save, System.Drawing.Imaging.ImageFormat.Jpeg);
                    save.Close();
                    save.Dispose();
                    img.Dispose();

                    BinaryReader binReader = new BinaryReader(File.Open($"{System.IO.Directory.GetCurrentDirectory()}\\专辑封面.jpeg", FileMode.Open));
                    FileInfo fileInfo = new FileInfo($"{System.IO.Directory.GetCurrentDirectory()}\\专辑封面.jpeg");
                    byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                    binReader.Close();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(bytes);
                    bitmap.EndInit();
                    音频文件项.专辑图片.ImageSource = bitmap;


                    //}
                }
                else
                {

                }


            }

            return ID3V2;
        }

        public string GetFrameInfoByEcoding(byte[] b, byte conde, int length)
        {
            string str = "";
            switch (conde)
            {
                case 0:
                    str = Encoding.GetEncoding("ISO-8859-1").GetString(b, 1, length);
                    break;
                case 1:
                    str = Encoding.GetEncoding("UTF-16LE").GetString(b, 1, length);
                    break;
                case 2:
                    str = Encoding.GetEncoding("UTF-16BE").GetString(b, 1, length);
                    break;
                case 3:
                    str = Encoding.UTF8.GetString(b, 1, length);
                    break;
            }
            return str;
        }

    }

    /// <summary>
    /// 执行有关音频的配置文件的读写操作
    /// </summary>
    public class 配置档
    {

        #region 配置档升级模块
        public static void 检测并升级配置档(string 配置文件)
        {
            if (!File.Exists(配置文件)) { return; }
            //读取配置版本
            var 版本号 = 配置.读配置项(配置文件, "配置文件", "版本", "初版");
            if (版本号 == "初版")
            {
                //调用初版升级程序
                配置档升级_初版(配置文件);
            }

            if (版本号 == "1.0")
            {
                return;
            }
            MessageBox.Show("配置文件升级", "配置文件已经成功升级到了最新版本");
        }

        private static void 配置档升级_初版(string 配置文件)
        {
            if (!File.Exists(配置文件)) { return; }
            #region 读取旧版数据
            var BPM值 = Convert.ToInt32(配置.读配置项(配置文件, "默认配置", "BPM", "-1"));
            var 节拍数 = Convert.ToInt32(配置.读配置项(配置文件, "默认配置", "节拍", "-1"));
            var 段落名称 = 配置.读配置项(配置文件, "段落名称", "名称", "读取失败");
            var 节拍特例文本 = 配置.读配置项(配置文件, "节拍特例", "节拍特例", "读取失败");
            string 循环段落文本 = 配置.读配置项(配置文件, "循环", "循环文件下标", "");
            File.Delete(配置文件);
            #endregion
            #region 转换旧信息
            //节拍数转换
            var tmp = 节拍特例文本.Split(',');
            List<int> valuetmp = new List<int>();
            foreach (string i in tmp)
            {
                valuetmp.Add(Convert.ToInt32(i));
            }
            valuetmp.Sort();
            int maxvalue = valuetmp.Max();
            int s = 0;
            List<int> finaltmp = new List<int>();
            for (int i = 0; i < maxvalue; i++)
            {
                if (s >= valuetmp.Count) { break; }
                if (i == valuetmp[s]) { finaltmp.Add(valuetmp[s + 1]); s += 2; }
                else { finaltmp.Add(节拍数); }


            }
            节拍特例文本 = "";
            foreach (int i in finaltmp)
            {
                节拍特例文本 += $"{i},";

            }
            //删除末尾逗号
            节拍特例文本 = 节拍特例文本.Substring(0, 节拍特例文本.Length - 1);
            #endregion
            #region 建立版本信息
            配置.写配置项(配置文件, "配置文件", "版本", "1.0");
            #endregion
            #region 建立音频基础信息
            配置.写配置项(配置文件, "基础信息", "启用配置", "false");
            配置.写配置项(配置文件, "基础信息", "曲名", "");
            配置.写配置项(配置文件, "基础信息", "作者", "");
            配置.写配置项(配置文件, "基础信息", "专辑", "");
            配置.写配置项(配置文件, "基础信息", "封面路径", "");
            配置.写配置项(配置文件, "基础信息", "BPM", BPM值.ToString());
            配置.写配置项(配置文件, "基础信息", "节拍", 节拍数.ToString());

            #endregion
            #region 建立交互信息配置
            配置.写配置项(配置文件, "交互信息", "循环类型", 语言.音频类型.循环类型.无缝章节循环.ToString());
            配置.写配置项(配置文件, "交互信息", "文件模式", 语言.音频类型.文件模式.多个文件模式.ToString());
            #endregion
            #region 建立循环信息配置
            配置.写配置项(配置文件, "分段配置", "分段字节", "");
            配置.写配置项(配置文件, "章节配置", "循环章节下标", 循环段落文本);
            配置.写配置项(配置文件, "章节配置", "章节名", 段落名称);
            配置.写配置项(配置文件, "章节配置", "章节节拍特例", 节拍特例文本);
            配置.写配置项(配置文件, "章节配置", "章节BPM特例", "");
            配置.写配置项(配置文件, "章节配置", "章节切换拍", "");
            #endregion
            设置类.日志.输出($"配置档{配置文件}从最初版升级到了1.0版本");
        }
        #endregion

        public void 新建配置档(string 配置文件)
        {
            #region 建立版本信息
            配置.写配置项(配置文件, "配置文件", "版本", "1.0");
            #endregion
            #region 建立音频基础信息
            配置.写配置项(配置文件, "基础信息", "启用配置", "true");
            配置.写配置项(配置文件, "基础信息", "曲名", "");
            配置.写配置项(配置文件, "基础信息", "作者", "");
            配置.写配置项(配置文件, "基础信息", "专辑", "");
            配置.写配置项(配置文件, "基础信息", "封面路径", "");
            配置.写配置项(配置文件, "基础信息", "BPM", "0");
            配置.写配置项(配置文件, "基础信息", "节拍", "0");
            #endregion
            #region 建立交互信息配置
            配置.写配置项(配置文件, "交互信息", "循环类型", "");
            配置.写配置项(配置文件, "交互信息", "文件模式", "");
            #endregion
            #region 建立分段信息配置
            配置.写配置项(配置文件, "分段配置", "循环文件下标", "");
            配置.写配置项(配置文件, "分段配置", "分段字节", "");
            #endregion
            #region 建立循环信息配置
            配置.写配置项(配置文件, "章节配置", "章节名", "起始段落|循环段落");
            配置.写配置项(配置文件, "章节配置", "循环章节下标", "1");
            配置.写配置项(配置文件, "章节配置", "章节节拍特例", "4,4");
            配置.写配置项(配置文件, "章节配置", "章节BPM特例", "");
            配置.写配置项(配置文件, "章节配置", "章节切换拍", "");
            #endregion


        }
        /// <summary>
        /// 读取一个音频的所有配置信息到音频信息模块
        /// </summary>
        /// <param name="信息">需要传入配置项的信息</param>
        public static void 读取音频配置(控件.音频文件项 控件, 音频中间件.音频信息 信息)
        {
            string 配置文件 = 控件.配置文件路径;

            if (配置.读配置项(配置文件, "交互信息", "循环类型", "") == "") { 检测并升级配置档(配置文件); }
            #region 建立音频基础信息
            信息.配置文件启用 = Convert.ToBoolean(配置.读配置项(配置文件, "基础信息", "启用配置", "false"));
            信息.曲名 = 配置.读配置项(配置文件, "基础信息", "曲名", "");
            信息.BPM = Convert.ToInt32(配置.读配置项(配置文件, "基础信息", "BPM", 设置类.设置.默认节拍.ToString()));
            信息.当前章节拍数 = Convert.ToInt32(配置.读配置项(配置文件, "基础信息", "节拍", 设置类.设置.默认节拍.ToString()));
            信息.文件格式 = 控件.文件格式;
            #endregion

            #region 建立交互信息配置
            信息.音频类型 = (语言.音频类型.循环类型)Enum.Parse(typeof(语言.音频类型.循环类型), 配置.读配置项(配置文件, "交互信息", "循环类型", 语言.音频类型.循环类型.永续.ToString()));
            try
            {
                信息.文件模式 = (语言.音频类型.文件模式)Enum.Parse(typeof(语言.音频类型.文件模式), 配置.读配置项(配置文件, "交互信息", "文件模式", 语言.音频类型.文件模式.多个文件模式.ToString()));
            }
            catch
            {
                信息.文件模式 = 语言.音频类型.文件模式.多个文件模式;
            }
    
            #endregion
            #region 建立分段信息配置
            string 循环段落文本 = 配置.读配置项(配置文件, "章节配置", "循环章节下标", "");
            string[] 循环段落文本组 = 循环段落文本.Split(',');
            if (循环段落文本.Length > 0)
            {
                //读入配置文件
                foreach (string i in 循环段落文本组)
                {
                    信息.循环文件下标.Add(Convert.ToInt32(i));
                }
            }
            string 段落名称文本 = 配置.读配置项(配置文件, "章节配置", "章节名", "");
            string[] 段落名称文本组 = 段落名称文本.Split('|');
            if (段落名称文本组.Length > 0)
            {
                //读入配置文件
                foreach (string i in 段落名称文本组)
                {
                    信息.章节信息.Add(i);
                }
            }

            string 字节分割文本 = 配置.读配置项(配置文件, "章节配置", "分段字节", "");
            string[] 字节分割文本组 = 字节分割文本.Split(',');
            if (字节分割文本组.Length > 0 && 字节分割文本.Length > 0)
            {
                //读入配置文件
                foreach (string i in 字节分割文本组)
                {
                    信息.字节分割.Add(Convert.ToInt32(i));
                }
            }
            #endregion


        }
        /// <summary>
        /// 读取一个音频的部分配置信息（仅用于UI加载）
        /// </summary>
        /// <param name="控件">需要传入的控件</param>
        public void 读取音频配置(控件.音频文件项 控件)
        {
            var 配置文件 = 控件.配置文件路径;
            #region 建立音频基础信息
            var 曲名 = 配置.读配置项(配置文件, "基础信息", "曲名", "");
            var 作者 = 配置.读配置项(配置文件, "基础信息", "作者", "");
            var 专辑 = 配置.读配置项(配置文件, "基础信息", "专辑", "");
            var 循环类型 = 配置.读配置项(配置文件, "交互信息", "循环类型", "");
            if (曲名 != "") { 设置类.日志.输出("虽然修改了曲名，但是当前框架版本基于显示信息定义文件将导致一些问题，曲名信息修复后才可展示"); }
            if (作者 != "") { 控件.详细信息.Text = 作者; }
            if (专辑 != "") { 控件.详细信息.Text += " - " + 专辑; }
            if (配置.读配置项(配置文件, "交互信息", "循环类型", "") == "") { 控件.循环类型.Content = 语言.音频类型.循环类型.永续; return; }
            控件.循环类型.Content = (语言.音频类型.循环类型)Enum.Parse(typeof(语言.音频类型.循环类型), 配置.读配置项(配置文件, "交互信息", "循环类型", 语言.音频类型.循环类型.永续.ToString()));

            #endregion
        }

    }
}
