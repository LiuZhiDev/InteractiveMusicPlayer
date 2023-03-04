using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 交互音乐播放器.UI.附加控件
{
    #region 节拍数据类
    public class 节拍
    {
        public int 小节 { get; set; } = 0;
        public int 拍 { get; set; } = 0;
        public int 总节拍 { get; set; } = -1;
        public float BPM { get; set; } = 0;
        public float 实时BPM { get; set; } = 0;
        public int 平均延时 { get; set; } = 0;
        public int 最大拍分量 { get; set; } = 4;
    }
    #endregion
    /// <summary>
    /// 迷你节拍器.xaml 的交互逻辑
    /// </summary>
    public partial class 节拍器 : UserControl
    {
        Queue<float> 延迟堆 = new Queue<float>(8);
        Queue<float> BPM堆 = new Queue<float>(8);
        TimeSpan 上次按下 = TimeSpan.FromSeconds(9999);
        TimeSpan 本次按下 = TimeSpan.FromSeconds(9999);
        SoundPlayer 强拍;
        SoundPlayer 弱拍;
        int 精度 = 0;
        int 全局延迟 = 0;
        int 已按键次数 = 0;

        节拍 节拍 = new 节拍();
        Stopwatch 秒表 = new Stopwatch();
        public 节拍器()
        {
            InitializeComponent();
            强拍 = new SoundPlayer(@"音频\底鼓.wav");
            弱拍 = new SoundPlayer(@"音频\闭嚓.wav");
        }
        public void 模拟按钮点击(Button 按钮)
        {
            按钮.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, 按钮));
        }

        public void 添加节拍并运算()
        {
            秒表.Start();
            已按键次数 += 1;
            运算节拍();
            if (上次按下 == TimeSpan.FromSeconds(9999))  //如果是第一次按
            {
                上次按下 = 取当前时间();
                return;
            }
            本次按下 = 取当前时间();
            float 间隔 = (float)本次按下.Subtract(上次按下).TotalMilliseconds;
            上次按下 = 本次按下;
            if (间隔 < 0) { 无效数据处理(); return; }//小于零是无效数据
            延迟堆.Enqueue(间隔);
            var 平均延迟 = 计算平均值(延迟堆);
            var BPM = 计算BPM(平均延迟);
            节拍.实时BPM = BPM;
            //栈输出(延迟堆);
        }

        public void 无效数据处理()
        {

        }

        public void 运算节拍()
        {
            节拍.总节拍 += 1;
            节拍.小节 = 节拍.总节拍 / 节拍.最大拍分量;
            节拍.拍 = 节拍.总节拍 % 节拍.最大拍分量 + 1;
            var 故事板 = (Storyboard)Resources[$"亮起第{节拍.拍}拍"];
            if (节拍.总节拍 <= 8) { 信息.Text = $"再按{9 - 节拍.总节拍}次"; }
            else
            {
                信息.Text = $"{Math.Round(节拍.BPM, 精度)} BPM | 实时 {Math.Round(节拍.实时BPM, 精度)} BPM";
            }
            if (节拍.拍 == 1) { 强拍.Play(); }
            if (节拍.拍 == 2) { 弱拍.Play(); }
            if (节拍.拍 == 3) { 强拍.Play(); }
            if (节拍.拍 == 4) { 弱拍.Play(); }

            调整精度();
            故事板.Begin();
        }

        public void 调整精度()
        {
            if (精度 == 2) { return; }
            if ((节拍.总节拍 + 1) % 60 == 0) { 精度 += 1; }
        }

        public void 清理节拍数据()
        {
            延迟堆 = new Queue<float>(8);
            BPM堆 = new Queue<float>(8);
            上次按下 = TimeSpan.FromSeconds(9999);
            本次按下 = TimeSpan.FromSeconds(9999);
            精度 = 0;
            全局延迟 = 0;
            已按键次数 = 0;
            节拍 = new 节拍();
        }

        public float 计算BPM(float 平均值)
        {
            var 输出数值 = (float)60000 / 平均值;
            if (输出数值 < 0) { return -1; }
            BPM堆.Enqueue(输出数值);
            if (BPM堆.Count >= 8) { BPM堆.Dequeue(); }
            节拍.BPM = BPM堆.Average();
            return (float)输出数值;
        }

        public float 计算平均值(Queue<float> 延迟堆)
        {
            if (延迟堆.Count < 8) { return -1; }
            延迟堆.Dequeue();
            return 延迟堆.Average();
        }


        public void 栈输出(Queue<float> 延迟堆)
        {
            string 数组输出 = "";
            foreach (int i in 延迟堆)
            {
                数组输出 += $" [{i}] ";
            }
            Debug.Print(数组输出);
        }

        public TimeSpan 取当前时间() { return TimeSpan.Parse(DateTime.Now.ToString("00:00:ss.fffffff")); }

        private void 键盘事件(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                模拟按钮点击(击拍按钮);
            }
            if (e.Key == Key.R)
            {
                模拟按钮点击(重置按钮);
            }

        }

        private void 击拍按钮_Click(object sender, RoutedEventArgs e)
        {
            添加节拍并运算();
        }

        private void 重置按钮_Click(object sender, RoutedEventArgs e)
        {
            清理节拍数据();
        }


    }
}
