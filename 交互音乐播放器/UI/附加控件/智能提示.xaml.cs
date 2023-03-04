using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 交互音乐播放器.UI.附加控件
{
    /// <summary>
    /// 智能提示.xaml 的交互逻辑
    /// </summary>
    public partial class 智能提示 : UserControl
    {
        public string 标题 { get; set; }
        public string 语法 { get; set; }
        public string 提示 { get; set; }
        public 智能提示()
        {
            InitializeComponent();
            DataContext = this;
          
        }
        public 智能提示(string 标题, string 语法, string 提示)
        {
            InitializeComponent();
            DataContext = this;
            this.标题 = 标题;
            this.语法 = 语法;
            this.提示 = 提示;
            
        }
    }
}
