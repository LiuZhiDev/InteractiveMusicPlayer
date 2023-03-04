using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using ICSharpCode.AvalonEdit.Editing;

namespace 交互音乐播放器.UI.附加控件
{
    public class 脚本编辑器控制
    {
        public static void 应用文档高亮(TextEditor 编辑器, string XML_XSHD文件路径)
        {
            Stream xshd_stream = File.OpenRead(XML_XSHD文件路径);
            XmlReader xshd_reader = new XmlTextReader(xshd_stream);
            编辑器.SyntaxHighlighting = HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            xshd_reader.Close();
            xshd_stream.Close();
        }

        public static (int X坐标, int Y坐标, int 总位置) 获取输入光标所在行列(TextEditor 编辑器)
        {
            return (编辑器.TextArea.Caret.Column, 编辑器.TextArea.Caret.Line, 编辑器.TextArea.Caret.Offset);
        }

        public static string 取当前行文本(TextEditor 编辑器, int 当前行)
        {
            if (当前行 > 编辑器.Document.Lines.Count )
            {
                return null;
            }
            var 内容信息 = 编辑器.Document.Lines[当前行-1];
            int 本行开始 = 0;
            if (内容信息.PreviousLine == null)
            {本行开始 = 0;} else { 本行开始 = 内容信息.PreviousLine.Offset + 内容信息.PreviousLine.Length + 2; }
            
            var 本行结束 = 本行开始 + 内容信息.Length ;
            var 总大小 = 本行结束 - 本行开始;
            var 文本 = 编辑器.Document.GetText(本行开始, 总大小);
            return 文本;
        }

        public static string 取当前方法名(string 段落文本)
        {
            if (string.IsNullOrEmpty(段落文本)) { return null; }
            var 方法名 = 段落文本.Split('|').FirstOrDefault();
            if (string.IsNullOrEmpty(方法名))
            {
                return null;
            }
            else
            {
                return 方法名;
            }
        }

        public static void 替换当前方法名(TextEditor 编辑器,string 选定方法名)
        {
            var 当前行 = 编辑器.TextArea.Caret.Line;
            if (当前行 > 编辑器.Document.Lines.Count)
            {
                return ;
            }
            var 内容信息 = 编辑器.Document.Lines[当前行 - 1];
            int 本行开始 = 0;
            if (内容信息.PreviousLine == null)
            { 本行开始 = 0; }
            else { 本行开始 = 内容信息.PreviousLine.Offset + 内容信息.PreviousLine.Length + 2; }

            var 本行结束 = 本行开始 + 内容信息.Length;
            var 总大小 = 本行结束 - 本行开始;
            var 段落文本 = 编辑器.Document.GetText(本行开始, 总大小);

            if (string.IsNullOrEmpty(段落文本)) { return ; }
            var 方法名 = 段落文本.Split('|').FirstOrDefault();
            if (string.IsNullOrEmpty(方法名))
            {
                return ;
            }
            编辑器.Document.Replace(本行开始, 方法名.Length, "");
            编辑器.Document.Insert(本行开始, 选定方法名+"|");
        }

        public static int 当前选择块(string 段落文本,int 光标的横向位置)
        {
          
            if (string.IsNullOrEmpty(段落文本)) { return 0; }
            var 变量组 = 段落文本.Split('|');
            if (变量组.Length <=0) { return 0; }
            int 已遍历数量 = 1;
            int 当前段开头坐标 = 1;
            int 当前段结尾坐标 = 1;
            for (int i = 0; i < 变量组.Length; i++)
            {
                当前段结尾坐标 = 当前段开头坐标+ 变量组[i].Length + 1;
                if (光标的横向位置<当前段结尾坐标&&光标的横向位置>=当前段开头坐标) { return i; }
                else { 当前段开头坐标 += 变量组[i].Length +1; }
            }
            return 变量组.Length ;
        }

    }
}
