using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using 交互音乐播放器.数据;

namespace 交互音乐播放器.数据
{
    /// <summary>
    /// 执行有关MP3音频格式文件的配置文档（ID3）的读取操作
    /// </summary>
    internal class MP3信息读取
    {
        public string[] 读取MP3元数据(string 文件路径, 脚本文件数据 脚本项)
        {
            int mp3TagID = 0;
            string[] MP3标签集 = new string[6];
            FileStream 文件流 = new FileStream(文件路径, FileMode.Open, FileAccess.Read);
            byte[] 缓冲流 = new byte[10];
            // fs.Read(buffer, 0, 128);
            string mp3ID = "";

            文件流.Seek(0, SeekOrigin.Begin);
            文件流.Read(缓冲流, 0, 10);
            int size = (缓冲流[6] & 0x7F) * 0x200000 + (缓冲流[7] & 0x7F) * 0x400 + (缓冲流[8] & 0x7F) * 0x80 + (缓冲流[9] & 0x7F);
            //int size = (buffer[6] & 0x7F) * 0X200000 * (buffer[7] & 0x7f) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9]);
            mp3ID = Encoding.Default.GetString(缓冲流, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                mp3TagID = 1;
                //如果有扩展标签头就跨过 10个字节
                if ((缓冲流[5] & 0x40) == 0x40)
                {
                    文件流.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }

                MP3标签集 = ReadFrame(文件流, size, 脚本项);

                文件流.Dispose();
                return MP3标签集;

            }
            文件流.Dispose();
            return MP3标签集;
        }
        public string[] ReadFrame(FileStream fs, int size, 脚本文件数据 脚本数据)
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
                        fs.Dispose();
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        脚本数据.专辑图片.ImageSource = bitmap; //正在访问主线程的脚本项
                    });
                    


                    
                }
                else
                {

                }

              
            }
            fs.Dispose();
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
}
