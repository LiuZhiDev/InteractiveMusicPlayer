using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using 交互音乐播放器.数据;
using 交互音乐播放器.中间件;
using 交互音乐播放器.效果器;
using 交互音乐播放器.数据;
using 交互音乐播放器.数据.播放中数据;
using 交互音乐播放器.音频逻辑;
using static 交互音乐播放器.数据.音乐播放数据;
using System.Windows;

namespace 交互音乐播放器.中间件
{

    public class 脚本解析器
    {
        public static 脚本解析器 当前解析;
        public static 音乐播放数据 数据;
        public static Dictionary<string, List<string>> 脚本字典 = new();
        public int 当前行 = 0;
        public static object 全局事件锁 = new object();
        public 脚本解析器(脚本文件数据 脚本)
        {
            if (当前解析 != null) { 脚本解析器多例初始化(脚本); return; }
            当前解析 = this;
            数据 = 音频控制中间件.播放数据;
            脚本命令.命名集.Clear();
            脚本字典.Clear();
            if (脚本命令.脚本指令集 == null) { 脚本命令.初始化指令集(); }
            预处理脚本文档(脚本.脚本文档);
            //段落名称预解析();
            运行脚本(脚本字典.FirstOrDefault().Key);
            开始监听事件();
        }
        public void 脚本解析器多例初始化(脚本文件数据 脚本)
        {
            预处理脚本文档(脚本.脚本文档);
            数据 = 音频控制中间件.播放数据;
            //段落名称预解析();
            运行脚本(脚本字典.FirstOrDefault().Key);
        }

        public void 热重载(string 脚本文件)
        {
            预处理脚本文档(脚本文件);
            Debug.Print("热重载完成");
        }

        /// <summary>
        /// 将脚本文档预先处理并加入字典，便于之后读取
        /// </summary>
        /// <param name="脚本文件"></param>
        private void 预处理脚本文档(string 脚本文件)
        {
            脚本字典.Clear();
            Regex 脚本名称匹配器 = new("-(.*)-");
            var 脚本名称匹配集 = 脚本名称匹配器.Matches(脚本文件);
            if (脚本名称匹配集.Count == 0) { return; }
            List<string> 脚本名称 = new();
            for (int i = 0; i < 脚本名称匹配集.Count; i++)
            {
                脚本名称.Add(脚本名称匹配集[i].Groups[1].Value.Trim());
            }
            foreach (var 脚本名 in 脚本名称)
            {
                Regex 脚本内容匹配器 = new($"{脚本名}-[\\s\\S]([\\s\\S]+?)-");
                var 脚本内容匹配集 = 脚本内容匹配器.Matches(脚本文件);
                if (脚本内容匹配集.Count == 0)
                {
                    Regex 结尾脚本匹配器 = new($"{脚本名}-[\\s\\S]([\\s\\S]+)");
                    脚本内容匹配集 = 结尾脚本匹配器.Matches(脚本文件);
                    if (脚本内容匹配集.Count == 0) { continue; }
                }
                string 脚本内容 = 脚本内容匹配集[0].Groups[1].Value.Trim();
                var 脚本列 = 脚本内容.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList<string>();
                脚本字典.Add(脚本名, 脚本列);
            }
        }
        [Obsolete("现在的数据结构已不再需要执行该方法")]
        private void 段落名称预解析()
        {
            /*
            int 序号 = 0;
            音频控制中间件.脚本数据.状态_段落名至序号.Clear();
            foreach (var 段落名 in 音频控制中间件.当前脚本.段落名称)
            {
                if (音频控制中间件.当前脚本.状态_段落名至序号.TryAdd(段落名, 序号))
                {
                    序号++;
                }

            }
            */
        }

        private bool 执行脚本行(string 脚本名称, int 行数)
        {
            var 脚本行 = 脚本字典[脚本名称][行数];
            if (脚本行.Contains("/*") || 脚本行.Contains("*/")) { return true; }
            if (脚本行.Contains("&"))
            {
                脚本行 = 临时命名替换(脚本行);
            }
            var 信息集 = 脚本行.Split('|');
            Func<string[], bool> 执行方法;
            var 脚本行有效 = 脚本命令.脚本指令集.TryGetValue(信息集[0], out 执行方法);
            if (!脚本行有效 || 执行方法 == null) { Debug.Print($"错误，{脚本名称}中第{行数}行无法执行，找不到函数[{信息集[0]}]"); return false; }
            var 执行结果 = 执行方法(信息集);
            return 执行结果;
        }

        private string 临时命名替换(string 脚本文本)
        {
            Regex 正则替换 = new(@"(\&.*)");
            var 替换变量文本 = 正则替换.Matches(脚本文本);
            if (替换变量文本.Count == 0)
            { Debug.Print("没有找到可以替换的变量信息"); return 脚本文本; }
            var 匹配的变量组 = 替换变量文本[0].Groups[0].Value.Split('|');
            foreach (var 匹配的变量名 in 匹配的变量组)
            {
                if (!匹配的变量名.Contains("&")) { continue; }
                if (!脚本命令.命名集.ContainsKey(匹配的变量名)) { Debug.Print($"没有找到变量 {匹配的变量名}"); return 脚本文本; }
                else
                {
                    脚本文本 = 脚本文本.Replace(匹配的变量名, 脚本命令.命名集[匹配的变量名]);
                }
            }


            return 脚本文本;
        }

        public void 运行脚本(string? 脚本名称)
        {
            if (脚本名称 == null) { MessageBox.Show("无法找到要运行的脚本，如果在开始出现此错误，请检查你是否建立了初始的脚本名"); return; }
            var 总行数 = 脚本字典[脚本名称].Count;
            Thread 脚本线程 = new(() =>
            {
                for (int 当前行 = 0; 当前行 < 总行数; 当前行++)
                {
                    var 结果 = 执行脚本行(脚本名称, 当前行);
                    if (!结果) { MessageBox.Show($"脚本执行失败 ，{脚本名称} 在 {当前行+1}/{总行数} 行出现问题（不含注释行），已停止执行"); return; }
                }
            });

            脚本线程.Start();
        }

        private void 开始监听事件()
        {
            Task task = new(() =>
                        {
                            while (UI界面数据.程序执行)
                            {
                                事件监听();
                                Thread.Sleep(100);
                            }
                        });
            Task 事件监听线程 = task;
            Task 等待监听线程 = new(() =>
            {
                while (UI界面数据.程序执行)
                {
                    等待状态监听();
                    Thread.Sleep(100);
                }
            });
            事件监听线程.Start();
            等待监听线程.Start();
            系统控制.线程.Add(事件监听线程);
            系统控制.线程.Add(等待监听线程);
        }

        private void 事件监听()
        {
            lock (全局事件锁)
            {
                if (音频控制中间件.播放数据 != null) 
                {
                    if (音频控制中间件.播放数据.空值或空引用(播放数据组.当前段落)) { Debug.Print("事件监听 - 当前段落引用为空"); }
                }

                if (计划.全局事件.Contains(计划.事件.切换至循环等待段落))
                {
                    脚本命令.事件_进入循环等待(数据.当前段落!);
                    计划.全局事件.Remove(计划.事件.切换至循环等待段落);
                }

                if (计划.全局事件.Contains(计划.事件.切换至可控制段落))
                {
                    脚本命令.事件_进入可控段落(数据.当前流.当前段落!);
                    计划.全局事件.Remove(计划.事件.切换至可控制段落);
                }

                if (计划.全局事件.Contains(计划.事件.按下默认按钮))
                {
                    var 是否运行 = 脚本命令.事件_按下默认按钮(); if (!是否运行) 
                    {
                        UI界面数据.默认按钮状态 = true; 
                        计划.全局事件.Remove(计划.事件.按下默认按钮);
                        return;
                    }
                    if (计划.全局事件.Contains(计划.事件.按下默认按钮))
                    {
                        计划.全局事件.Remove(计划.事件.按下默认按钮);
                    }
                }
                Debug.Print("执行完毕，将释放锁");
            }


        }

        private void 等待状态监听()
        {
            if (计划.全局等待.Contains(计划.等待.切换至循环等待段落))
            { 脚本命令.等待_完成循环等待(); 计划.全局等待.Remove(计划.等待.切换至循环等待段落); }
        }

        ~脚本解析器()
        {
            Debug.Print("脚本已停止解析");
        }
    }

    public static class 脚本命令
    {
        public static Dictionary<string, Func<string[], bool>> 脚本指令集;
        public static Dictionary<string, string> 命名集 = new();
        public enum 指令
        {
            初始化文件,
            动态初始化文件,
            重定位文件时间,
            新建播放流,
            播放文件,
            设置切换点,
            设置切换时间,
            设置循环等待按键,
            结束循环等待,
            设置默认按钮,
            强制刷新默认按钮,
            按钮可见,
            移除按钮,
            新建按钮,
            指定UI目标,
            新建流链路,
            删除流链路,
            修改流链路,
            新建段落链路,
            删除段落链路,
            修改段落链路,
            取得当前段落,
            下一段落链路,
            取得当前流,
            下一流链路,
            执行淡入,
            执行淡出,
            执行段落静音,
            解除挂载淡入淡出,
            执行混流,
            执行交叉叠化,
            解除挂载混流器,
            在下个切换点执行脚本,
            播放至下个切换点执行脚本,
            在下个切换点切换,
            在指定切换点切换,
            在指定切换点执行脚本,
            建立固定拍号脚本Cue,
            设置切换节拍,
            设置两点循环,
            删除两点循环,
            启用自动Cue,
            关闭自动Cue,
            清空所有Cue,
            重置下个Cue,
            禁用默认文件切换逻辑,
            禁用默认赋值当前段落,
            输出信息
        }

        public static void 初始化指令集()
        {
            脚本指令集 = new Dictionary<string, Func<string[], bool>>();
            脚本指令集.Add(指令.初始化文件.ToString(), 初始化文件);
            脚本指令集.Add(指令.动态初始化文件.ToString(), 动态初始化文件);
            脚本指令集.Add(指令.重定位文件时间.ToString(), 重定位文件时间);
            脚本指令集.Add(指令.新建播放流.ToString(), 新建播放流);
            脚本指令集.Add(指令.播放文件.ToString(), 播放文件);
            脚本指令集.Add(指令.设置切换点.ToString(), 设置切换点);
            脚本指令集.Add(指令.设置切换时间.ToString(), 设置切换时间);
            脚本指令集.Add(指令.设置循环等待按键.ToString(), 设置循环等待按键);
            脚本指令集.Add(指令.结束循环等待.ToString(), 结束循环等待);
            脚本指令集.Add(指令.设置默认按钮.ToString(), 设置默认按钮);
            脚本指令集.Add(指令.强制刷新默认按钮.ToString(), 强制刷新默认按钮);
            脚本指令集.Add(指令.移除按钮.ToString(), 移除按钮);
            脚本指令集.Add(指令.新建按钮.ToString(), 新建按钮);
            脚本指令集.Add(指令.按钮可见.ToString(), 按钮可见);
            脚本指令集.Add(指令.指定UI目标.ToString(), 指定UI目标);
            脚本指令集.Add(指令.新建流链路.ToString(), 新建流链路);
            脚本指令集.Add(指令.删除流链路.ToString(), 删除流链路);
            脚本指令集.Add(指令.修改流链路.ToString(), 修改流链路);
            脚本指令集.Add(指令.取得当前流.ToString(), 取得当前流);
            脚本指令集.Add(指令.下一流链路.ToString(), 下一流链路);
            脚本指令集.Add(指令.新建段落链路.ToString(), 新建段落链路);
            脚本指令集.Add(指令.删除段落链路.ToString(), 删除段落链路);
            脚本指令集.Add(指令.修改段落链路.ToString(), 修改段落链路);
            脚本指令集.Add(指令.取得当前段落.ToString(), 取得当前段落);
            脚本指令集.Add(指令.下一段落链路.ToString(), 下一段落链路);
            脚本指令集.Add(指令.执行淡入.ToString(), 执行淡入);
            脚本指令集.Add(指令.执行淡出.ToString(), 执行淡出);
            脚本指令集.Add(指令.执行混流.ToString(), 执行混流);
            脚本指令集.Add(指令.执行交叉叠化.ToString(), 执行交叉叠化);
            脚本指令集.Add(指令.解除挂载混流器.ToString(), 解除挂载混流器);
            脚本指令集.Add(指令.执行段落静音.ToString(), 执行段落静音);
            脚本指令集.Add(指令.解除挂载淡入淡出.ToString(), 解除挂载淡入淡出);
            脚本指令集.Add(指令.在下个切换点执行脚本.ToString(), 在下个切换点执行脚本);
            脚本指令集.Add(指令.播放至下个切换点执行脚本.ToString(), 播放至下个切换点执行脚本);
            脚本指令集.Add(指令.在下个切换点切换.ToString(), 在下个切换点切换);
            脚本指令集.Add(指令.在指定切换点切换.ToString(), 在指定切换点切换);
            脚本指令集.Add(指令.在指定切换点执行脚本.ToString(), 在指定切换点执行脚本);
            脚本指令集.Add(指令.建立固定拍号脚本Cue.ToString(), 建立固定拍号脚本Cue);
            脚本指令集.Add(指令.设置切换节拍.ToString(), 设置切换节拍);
            脚本指令集.Add(指令.设置两点循环.ToString(), 设置两点循环);
            脚本指令集.Add(指令.删除两点循环.ToString(), 删除两点循环);
            脚本指令集.Add(指令.启用自动Cue.ToString(), 启用自动Cue);
            脚本指令集.Add(指令.关闭自动Cue.ToString(), 关闭自动Cue);
            脚本指令集.Add(指令.重置下个Cue.ToString(), 重置下个Cue);
            脚本指令集.Add(指令.清空所有Cue.ToString(), 清空所有Cue);
            脚本指令集.Add(指令.禁用默认文件切换逻辑.ToString(), 禁用默认文件切换逻辑);
            脚本指令集.Add(指令.禁用默认赋值当前段落.ToString(), 禁用默认赋值当前段落);
            脚本指令集.Add(指令.输出信息.ToString(), 输出信息);
        }

        public static bool 初始化文件(string[] 参数表)
        {
            string 段落 = 参数表[1]; //参数1 - 段落名称
            string 别名 = ""; //参数1 - 别名
            if (参数表.Length - 1 == 2) { 别名 = 参数表[2]; }
            else { 别名 = 段落; }
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!配置.文件组.ContainsKey(段落)) { return false; }
            if (音频控制中间件.当前中间件!.命令_加载文件(配置.文件组[段落].路径, 别名) == false)
            {
                Debug.Print("初始化文件失败");
            }
            List<string> a = new();

            return true;
        }

        public static bool 动态初始化文件(string[] 参数表)
        {
            string 段落 = 参数表[1]; //参数1 - 段落名称
            string 别名 = ""; //参数1 - 别名
            if (参数表.Length - 1 == 2) { 别名 = 参数表[2]; } //如果填了别名就用别名
            else { 别名 = 段落; }
            音乐播放数据 配置 = 脚本解析器.数据;
            脚本文件数据 脚本 = 音频控制中间件.脚本数据!;
            var 文件名序号 = 音频控制中间件.当前中间件.文件名转文件序号(段落);
            //加载文件到文件组中，使用别名
            音频控制中间件.当前中间件.命令_加载文件(配置.文件组[段落].路径, 别名);
            配置.段落组[别名].节拍信息_读取.每小节拍数 = 音频控制中间件.脚本数据!.默认小节节拍总数;
            音频控制器.当前控制器!.命令_添加文件节拍信息(别名, 脚本.默认BPM,
            音频控制器.当前控制器!.已载文件组.LastOrDefault()!.Value.Length, 音频控制器.当前控制器!.已载文件组.LastOrDefault()!.Value.TotalTime);
            int 真实编号 = 配置.段落组[段落].文件编号; string 真实名称 = 脚本.段落名称[真实编号];
            配置.段落组[别名].别名 = 别名;
            配置.段落组[别名].Offset = 脚本.Offset组[文件名序号];
            配置.段落组[别名].启用循环 = (脚本.循环下标组[文件名序号]);
            配置.段落组[别名].缓冲区大小 = 配置.段落组[真实名称].缓冲区大小;
            配置.段落组[别名].缓冲区时间 = 配置.段落组[真实名称].缓冲区时间;
            return true;
        }

        public static bool 重定位文件时间(string[] 参数表)
        {
            string 别名 = 参数表[1]; ///参数1 - 别名
            string 时间 = 参数表[2]; ///参数1 - 时间
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!音频控制器.当前控制器!.已载文件组.ContainsKey(别名)) { Debug.Print("找不到该别名或段落，无法设置文件进度"); return false; }
            var 字节数 = (long)数据转换.文本时间转目标字节(时间, 音频控制器.当前控制器!.已载文件组[别名]);
            音频控制中间件.当前中间件.命令_重定位(字节数, 别名);
            return true;
        }

        public static bool 新建播放流(string[] 参数表)
        {
            string 流 = 参数表[1]; //参数1 - 播放流的名称

            音频控制中间件.当前中间件!.命令_新建播放流(流);
            return true;
        }

        public static bool 播放文件(string[] 参数表)
        {
            string 流 = 参数表[1]; //参数1 - 播放流的名称
            string 段落 = 参数表[2]; //参数2 - 段落名称或别名
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 流)) { Debug.Print("无法找到别名所对应的播放流，播放失败"); return false; }
            new Dictionary<int, List<段落信息.Cue数据>>();
            if (配置.空值或空引用(播放数据组.段落组, 段落)) { Debug.Print("无法找到别名所对应的文件段落，播放失败"); return false; }
            音频控制中间件.当前中间件!.命令_播放文件(流, 段落);
            return true;
        }

        public static bool 设置切换点(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 段落名称
            string 出点位置 = 参数表[2]; //参数2 - CUE出点位置
            string 目标段落 = 参数表[3]; //参数3 - 目标段落名称
            string 入点位置 = 参数表[4]; //参数4 - CUE入点位置
            string 切换点名称 = 参数表[5]; //参数5 - CUE名称
            //判空逻辑
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[起始段落]!.Cue点集 == null) { 配置.段落组[起始段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            if (配置.段落组[目标段落]!.Cue点集 == null) { 配置.段落组[目标段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            if (音频控制器.当前控制器!.已载文件组 == null || 音频控制器.当前控制器!.已载文件组.Count == 0) { return false; }
            //数据转换逻辑
            if (出点位置.Contains("段落末尾") || 出点位置.Contains("章节末尾")) { 出点位置 = 音频控制器.当前控制器!.已载文件组[起始段落].Length.ToString(); }
            if (入点位置.Contains("段落开始") || 入点位置.Contains("章节开始")) { 入点位置 = 0.ToString(); }
            //数据导入
            if (配置.段落组[起始段落]!.Cue点集!.ContainsKey(切换点名称) == false) { 配置.段落组[起始段落]!.Cue点集!.Add(切换点名称, new 段落信息.Cue数据()); }
            配置.段落组[起始段落]!.Cue点集![切换点名称] = (new 段落信息.Cue数据
            {
                出点位置 = long.Parse(出点位置),
                入点段落 = 目标段落,
                入点位置 = long.Parse(入点位置),
                Cue点名称 = 切换点名称
            });
            return true;
        }

        public static bool 设置切换时间(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 段落名称
            string 出点位置 = 参数表[2]; //参数2 - CUE出点时间
            string 目标段落 = 参数表[3]; //参数3 - 目标段落名称
            string 入点位置 = 参数表[4]; //参数4 - CUE入点时间
            string 切换点名称 = 参数表[5]; //参数4 - 切换点名称
            //判空逻辑
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[起始段落]!.Cue点集 == null) { 配置.段落组[起始段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            if (配置.段落组[目标段落]!.Cue点集 == null) { 配置.段落组[目标段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            //数据转换逻辑
            var 出点字节 = 数据转换.文本时间转目标字节(出点位置, 音频控制器.当前控制器!.已载文件组[起始段落]);
            var 入点字节 = 数据转换.文本时间转目标字节(入点位置, 音频控制器.当前控制器.已载文件组[目标段落]);
            //数据导入
            if (配置.段落组[起始段落]!.Cue点集!.ContainsKey(切换点名称) == false)
            { 配置.段落组[起始段落]!.Cue点集!.Add(切换点名称, new 段落信息.Cue数据()); }
            配置.段落组[起始段落]!.Cue点集![切换点名称] = (new 段落信息.Cue数据
            { 出点位置 = (long)出点字节, 入点段落 = 目标段落, 入点位置 = (long)入点字节, Cue点名称 = 切换点名称 });
            return true;
        }

        public static bool 设置切换节拍(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 段落名称
            string 出点节拍 = 参数表[2]; //参数2 - 出点节拍
            string 目标段落 = 参数表[3]; //参数3 - 目标段落名称
            string 入点节拍 = 参数表[4]; //参数4 - 入点节拍
            string 切换点名称 = 参数表[5]; //参数4 - 切换点名称
            //判空逻辑
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[起始段落]!.Cue点集 == null) { 配置.段落组[起始段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            if (配置.段落组[目标段落]!.Cue点集 == null) { 配置.段落组[目标段落]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            //数据转换逻辑
            var 出点字节 = 数据转换.文本节拍数转字节(出点节拍, 起始段落, 配置);
            var 入点字节 = 数据转换.文本节拍数转字节(入点节拍, 目标段落, 配置);
            //数据导入
            if (配置.段落组[起始段落]!.Cue点集!.ContainsKey(切换点名称) == false) { 配置.段落组[起始段落]!.Cue点集!.Add(切换点名称, new 段落信息.Cue数据()); }
            配置.当前段落!.Cue点集![切换点名称] = (new 段落信息.Cue数据
            { 出点位置 = (long)出点字节, 入点段落 = 目标段落, 入点位置 = (long)入点字节, Cue点名称 = 切换点名称 });
            return true;
        }

        public static bool 建立固定拍号脚本Cue(string[] 参数表)
        {
            string 段落名称 = 参数表[1]; //参数1 - 段落名称
            string 拍号 = 参数表[2]; //参数1 - 拍号
            string 默认入点段落 = 参数表[3]; //参数1 - 默认入点节拍
            string 默认入点节拍 = 参数表[4]; //参数1 - 默认入点节拍
            string 脚本名称 = 参数表[5]; //参数1 - 脚本名称
            int 拍 = -1;
            if (!int.TryParse(拍号, out 拍)) { Debug.Print("拍号无法被转换成正确的数值，请检查输入"); return false; }
            拍 -= 1;//实际计算时拍号是以0开始计算的，所以需要将其-1对齐
            //判空逻辑
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.段落组, 段落名称)) { Debug.Print("不存在该段落，请确认段落名称"); return false; }
            if (配置.段落组[段落名称]!.Cue点集 == null) { 配置.段落组[段落名称]!.Cue点集 = new Dictionary<string, 段落信息.Cue数据>(); }
            //为所有拍建立Cue点
            var 总节拍信息 = 数据转换.读取段落总节拍信息(段落名称, 配置);
            var 默认入点字节 = 数据转换.文本节拍数转字节(默认入点节拍, 段落名称, 配置);

            for (int 小节 = 1; 小节 <= 总节拍信息.小节; 小节++)
            {
                var 待添加小节 = 小节; var 待添加拍 = 拍;
                if ((待添加小节 - 1) * 总节拍信息.小节最大拍号 + 拍 > 总节拍信息.总拍数) { continue; }
                var 实际字节 = 数据转换.节拍数转字节(待添加小节, 待添加拍, 总节拍信息.小节最大拍号, 总节拍信息.每拍字节, -总节拍信息.偏移);
                var Cue点名称 = $"{段落名称}.{待添加小节}.{待添加拍}";
                if (配置.段落组[段落名称]!.Cue点集!.ContainsKey(Cue点名称) == false) { 配置.段落组[段落名称]!.Cue点集!.Add(Cue点名称, new 段落信息.Cue数据()); }
                配置.段落组[段落名称]!.Cue点集![$"{段落名称}.{待添加小节}.{待添加拍}"] = (new 段落信息.Cue数据
                {
                    出点位置 = (long)实际字节,
                    入点段落 = 默认入点段落,
                    入点位置 = (long)默认入点字节,
                    Cue点名称 = Cue点名称,
                    绑定的脚本 = 脚本名称
                });
            }
            return true;
        }

        public static bool 清空所有Cue(string[] 参数表)
        {
            string 段落名称 = 参数表[1]; //参数1 - 段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            配置.段落组[段落名称]!.Cue点集!.Clear();
            return true;
        }

        public static bool 在下个切换点切换(string[] 参数表)
        {
            string 流名称 = 参数表[1]; //参数1 - 流名称
            string 段落名称 = 参数表[2]; //参数1 - 段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[段落名称]!.Cue点集 == null || 配置.段落组[段落名称]!.Cue点集!.Count == 0)
            { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            if (流名称 == "当前流")
            {
                流名称 = 配置.当前流!.流别名;
            }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }

            var CUE点集 = 配置.段落组[段落名称].Cue点集!.Values.ToList();
            配置.段落组[段落名称].下个Cue点 = 数据转换.取最近Cue点(CUE点集, 配置);
            计划.全局事件.Add(计划.事件.到达指定CUE点并切换);
            UI界面数据.默认按钮状态 = false;
            return true;
        }

        public static bool 在下个切换点执行脚本(string[] 参数表)
        {
            string 流名称 = 参数表[1]; //参数1 - 流名称
            string 段落名称 = 参数表[2]; //参数1 - 段落名称
            string 脚本名称 = 参数表[3]; //参数1 - 脚本名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[段落名称]!.Cue点集 == null || 配置.段落组[段落名称]!.Cue点集!.Count == 0)
            { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            if (流名称 == "当前流")
            {
                流名称 = 配置.当前流!.流别名;
            }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }

            if (配置.段落组[段落名称].Cue点集 == null) { Debug.Print($"段落 {段落名称}，并没有可用的CUE点"); }
            var CUE点集 = 配置.段落组[段落名称].Cue点集!.Values.ToList();
            配置.段落组[段落名称].下个Cue点 = 数据转换.取最近Cue点(CUE点集, 配置);
            配置.段落组[段落名称].下个Cue点!.绑定的脚本 = 脚本名称;
            计划.全局事件.Add(计划.事件.到达指定CUE点并执行脚本);
            UI界面数据.默认按钮状态 = false;
            return true;
        }
        public static bool 播放至下个切换点执行脚本(string[] 参数表)
        {
            string 流名称 = 参数表[1]; //参数1 - 流名称
            string 段落名称 = 参数表[2]; //参数1 - 段落名称
            string 脚本名称 = 参数表[3]; //参数1 - 脚本名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[段落名称]!.Cue点集 == null || 配置.段落组[段落名称]!.Cue点集!.Count == 0)
            { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            if (流名称 == "当前流")
            {
                流名称 = 配置.当前流!.流别名;
            }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }

            if (配置.段落组[段落名称].Cue点集 == null) { Debug.Print($"段落 {段落名称}，并没有可用的CUE点"); }
            var CUE点集 = 配置.段落组[段落名称].Cue点集!.Values.ToList();
            配置.段落组[段落名称].下个Cue点 = 数据转换.取最近Cue点(CUE点集, 配置);
            配置.段落组[段落名称].下个Cue点!.绑定的脚本 = 脚本名称;
            计划.全局事件.Add(计划.事件.播放至指定CUE点并执行脚本);
            UI界面数据.默认按钮状态 = false;
            return true;
        }
        public static bool 在指定切换点切换(string[] 参数表)
        {
            string 流名称 = 参数表[1]; //参数1 - 流名称
            string 段落名称 = 参数表[2]; //参数1 - 段落名称
            string 切换点名称 = 参数表[3]; //参数1 - 切换点名称

            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[段落名称]!.Cue点集 == null || 配置.段落组[段落名称]!.Cue点集!.Count == 0)
            { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }

            var CUE点集 = 配置.段落组[段落名称].Cue点集!.Values.ToList();
            配置.段落组[段落名称].下个Cue点 = CUE点集.FirstOrDefault(x => x.Cue点名称 == 切换点名称);
            计划.全局事件.Add(计划.事件.到达指定CUE点并切换);
            UI界面数据.默认按钮状态 = false;
            return true;
        }

        public static bool 在指定切换点执行脚本(string[] 参数表)
        {
            string 流名称 = 参数表[1]; //参数1 - 流名称
            string 段落名称 = 参数表[2]; //参数1 - 段落名称
            string 切换点名称 = 参数表[3]; //参数1 - 切换点名称
            string 脚本名称 = 参数表[4]; //参数1 - 脚本名称

            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[段落名称].Cue点集 == null) { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.段落组[段落名称].别名;
            }

            var 当前播放位置 = 配置.段落组[段落名称].播放位置.已播放字节;

            var CUE点集 = 配置.段落组[段落名称].Cue点集!.Values.ToList();
            配置.段落组[段落名称].下个Cue点 = CUE点集.FirstOrDefault(x => x.Cue点名称 == 切换点名称);
            配置.段落组[段落名称].下个Cue点!.绑定的脚本 = 脚本名称;
            计划.全局事件.Add(计划.事件.到达指定CUE点并切换);
            UI界面数据.默认按钮状态 = false;
            return true;
        }

        public static bool 设置两点循环(string[] 参数表)
        {
            string 段落名称 = 参数表[1]; //参数1 - 段落名称
            string 循环Cue点名称 = 参数表[2]; //参数2 - Cue点名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (段落名称 != "当前段落" && 配置.空值或空引用(播放数据组.段落组, 段落名称))
            {
                Debug.Print("未找到段落，请检查名称是否正确");
            }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }

            if (配置.段落组[段落名称].Cue点集 == null) { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return false; }
            var 匹配的CUE信息 = 配置.段落组[段落名称].Cue点集!.Values.Where(Cue点信息 => Cue点信息.Cue点名称 == 循环Cue点名称).FirstOrDefault();
            if (匹配的CUE信息 == null) { Debug.Print("未找到Cue点，请检查名称是否正确"); return false; }
            配置.段落组[段落名称].两点循环Cue = 匹配的CUE信息;
            计划.全局事件.Add(计划.事件.处理两点循环);
            Debug.Print("已添加两点循环");
            return true;
        }

        public static bool 删除两点循环(string[] 参数表)
        {
            string 段落名称 = 参数表[1]; //参数1 - 段落名称
            string 循环名称 = 参数表[2]; //参数1 - 循环名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (段落名称 != "当前段落" && 配置.空值或空引用(播放数据组.段落组, 段落名称))
            {
                Debug.Print("未找到段落，请检查名称是否正确");
            }
            if (段落名称 == "当前段落")
            {
                段落名称 = 配置.当前段落!.别名;
            }
            配置.段落组[段落名称].两点循环Cue = null;
            计划.全局事件.Remove(计划.事件.处理两点循环);
            return true;
        }

        public static bool 启用自动Cue(string[] 参数表)
        {
            计划.全局事件.Add(计划.事件.自动处理所有Cue点);
            回执_取下个Cue点();
            return true;
        }

        public static bool 关闭自动Cue(string[] 参数表)
        {
            计划.全局事件.Remove(计划.事件.自动处理所有Cue点);
            return true;
        }

        public static bool 重置下个Cue(string[] 参数表)
        {
            音乐播放数据 配置 = 脚本解析器.数据;
            配置.当前流!.当前段落.下个Cue点 = null;
            return true;
        }

        public static bool 输出信息(string[] 参数表)
        {
            Debug.Print(参数表[1]); return true;
        }

        public static bool 设置循环等待按键(string[] 参数表)
        {
            string 等待段落名称 = 参数表[1]; //参数1 - 段落名称
            string 执行脚本名称 = 参数表[2]; //参数2 - 执行脚本名称
            string 按钮显示名称 = 参数表[3]; //参数3 - 文本显示/标识
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.段落组[等待段落名称].循环等待数据 == null) { 配置.段落组[等待段落名称].循环等待数据 = new 段落信息._循环等待数据(); }
            配置.段落组[等待段落名称].循环等待 = true;
            配置.段落组[等待段落名称].循环等待数据 = new 段落信息._循环等待数据
            { 当前等待状态 = 段落信息._循环等待数据.等待状态.未开始等待, 绑定的脚本 = 执行脚本名称, 按钮显示名称 = 按钮显示名称 };
            if (计划.全局按钮集 == null) { 计划.全局按钮集 = new Dictionary<string, 计划.按钮数据>(); }
            计划.全局按钮集!.Add(按钮显示名称, new 计划.按钮数据
            {
                按钮显示名称 = 按钮显示名称,
                按钮类型 = 计划.按钮数据._按钮类型.等待操作,
                绑定的脚本 = 执行脚本名称,
                可用段落 = 等待段落名称,
                允许显示 = false
            });
            return true;
        }

        public static bool 结束循环等待(string[] 参数表)
        {
            string 等待段落名称 = 参数表[1]; //参数1 - 段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (等待段落名称 == "当前段落")
            {
                等待段落名称 = 配置.当前流.当前段落!.别名;
            }
            配置.段落组[等待段落名称].循环等待数据!.当前等待状态 = 段落信息._循环等待数据.等待状态.结束;
            return true;
        }

        public static bool 设置默认按钮(string[] 参数表)
        {
            string 按钮名称 = 参数表[1]; //参数1 - 按钮名称
            音乐播放数据 配置 = 脚本解析器.数据;
            计划.全局默认按钮 = 计划.全局按钮集![按钮名称];
            return true;
        }
        public static bool 按钮可见(string[] 参数表)
        {
            string 按钮名称 = 参数表[1]; //参数1 - 按钮名称

            音乐播放数据 配置 = 脚本解析器.数据;
            if (!计划.全局按钮集!.ContainsKey(按钮名称)) { Debug.Print("设置按钮状态失败，可能是因为没有该按钮"); return false; }
            计划.全局按钮集![按钮名称].允许显示 = true;
            return true;
        }
        public static bool 新建按钮(string[] 参数表)
        {
            string 等待段落名称 = 参数表[1]; //参数1 - 段落名称
            string 执行脚本名称 = 参数表[2]; //参数2 - 执行脚本名称
            string 按钮显示名称 = 参数表[3]; //参数3 - 文本显示/标识
            string 按钮显示类型 = 参数表[4]; //参数4 - 按钮类型 正常/循环/等待/结束
            音乐播放数据 配置 = 脚本解析器.数据;
            List<string> 类型 = new() { "正常", "循环", "等待", "结束" };
            List<计划.按钮数据._按钮类型> 实际类型 = new List<计划.按钮数据._按钮类型>()
            { 计划.按钮数据._按钮类型.正常进行, 计划.按钮数据._按钮类型.无限循环,
              计划.按钮数据._按钮类型.等待操作, 计划.按钮数据._按钮类型.结束 };

            计划.按钮数据._按钮类型 按钮类型 = 计划.按钮数据._按钮类型.正常进行;
            if (!类型.Contains(按钮显示类型)) { Debug.Print("新建按钮失败，未找到对应的显示类型重载"); return false; }
            if (计划.全局按钮集 == null) { 计划.全局按钮集 = new Dictionary<string, 计划.按钮数据>(); }
            计划.按钮数据 新按钮 = new()
            {
                按钮显示名称 = 按钮显示名称,
                按钮类型 = 实际类型[类型.IndexOf(按钮显示类型)],
                绑定的脚本 = 执行脚本名称,
                可用段落 = 等待段落名称,
                允许显示 = false
            };
            计划.全局按钮集.Add(按钮显示名称, 新按钮);
            /* 未知功效的代码，若有用时再还原
            if (配置.状态_段内按钮 == null) { 配置.状态_段内按钮 = new Dictionary<int, List<计划.按钮数据>>(); }
            if (!配置.状态_段内按钮.ContainsKey(配置.状态_段落名至序号[等待段落名称])) { 配置.状态_段内按钮[配置.状态_段落名至序号[等待段落名称]] = new List<音乐播放数据.状态_按钮数据>(); }
            配置.状态_段内按钮[配置.状态_段落名至序号[等待段落名称]].Add(新按钮);
            */
            return true;
        }

        public static bool 移除按钮(string[] 参数表)
        {
            string 按钮名称 = 参数表[1]; //参数1 - 段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (按钮名称 == "默认按钮") { 按钮名称 = 计划.全局默认按钮!.按钮显示名称!; } //宏定义
            if (计划.全局默认按钮!.按钮显示名称 == 按钮名称) { 计划.全局默认按钮 = null; }
            后台移除按钮(按钮名称);
            return true;
        }

        private static void 后台移除按钮(string 按钮名称)
        {
            Thread 移除线程 = new(() =>
            {
                bool 是否移除成功 = false;
                音乐播放数据 配置 = 脚本解析器.数据;
                int 重试次数 = 0;
                while (!是否移除成功)
                {
                    if (UI界面数据.默认按钮名称 != 按钮名称)
                    {
                        try { 计划.全局按钮集!.Remove(按钮名称); }
                        catch { Debug.Print("已移除"); }
                        是否移除成功 = true;
                    }
                    重试次数++;
                    if (重试次数 > 10) //经常未知问题无法移除，可能需要排查问题，这只是保留措施
                    {
                        UI界面数据.默认按钮名称 = "";
                        计划.全局按钮集!.Remove(按钮名称);
                        是否移除成功 = true;
                        强制刷新默认按钮(Array.Empty<string>());
                        回执_启用默认按钮();
                    }
                    Thread.Sleep(100);
                }
            });
            移除线程.Start();
        }

        public static bool 新建流链路(string[] 参数表)
        {
            string 起始流 = 参数表[1]; //参数1 - 起始的流名称
            string 目标流 = 参数表[2]; //参数1 - 目标的流名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 起始流)) { Debug.Print($"链路失败，没有找到流{起始流}"); return false; }
            if (配置.空值或空引用(播放数据组.播放流组, 目标流)) { Debug.Print($"链路失败，没有找到流{目标流}"); return false; }
            配置.播放流组[起始流].下一流信息 = 配置.播放流组[目标流];
            return true;
        }

        public static bool 删除流链路(string[] 参数表)
        {
            string 起始流 = 参数表[1]; //参数1 - 起始的流名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 起始流)) { Debug.Print($"链路删除失败，没有找到流{起始流}"); return false; }
            配置.播放流组[起始流].下一流信息 = null;
            return true;
        }

        public static bool 修改流链路(string[] 参数表)
        {
            string 起始流 = 参数表[1]; //参数1 - 起始的流名称
            string 目标流 = 参数表[2]; //参数1 - 目标的流名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 起始流)) { Debug.Print($"链路删除失败，没有找到流{起始流}"); return false; }
            配置.播放流组[起始流].下一流信息 = 配置.播放流组[目标流];
            return true;
        }

        public static bool 取得当前流(string[] 参数表)
        {
            string 储存名 = 参数表[1]; //参数1 - 储存的变量别名
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.当前流)) { Debug.Print($"取得当前流失败，发生内部错误"); return false; }
            var 当前流 = 配置.当前流.流别名;
            if (!储存名.Contains('$')) { Debug.Print($"[取得当前流] 无法命名 {储存名} 每一个命名前都需要带有$符号，命名失败"); return false; }
            储存名 = 储存名.Replace('$', '&');
            var 命名结果 = 命名集.TryAdd(储存名, 当前流);
            if (!命名结果)
            {
                命名集[储存名] = 当前流;
                Debug.Print($"已将之前的命名替换成了现在的命名，将可以使用{储存名}访问{当前流}");
                return true;
            }
            Debug.Print($"将可以使用{储存名}访问{当前流}");
            return true;
        }

        public static bool 下一流链路(string[] 参数表)
        {
            string 起始流 = 参数表[1]; //参数1 - 起始的流名称 
            string 储存名 = 参数表[2]; //参数1 - 储存的变量别名
            音乐播放数据 配置 = 脚本解析器.数据;
            if (起始流 == "当前流")
            {
                if (配置.空值或空引用(播放数据组.当前流)) { Debug.Print($"取得当前流失败，发生内部错误"); return false; }
                起始流 = 配置.当前流!.流别名;
            }
            if (!配置.空值或空引用(播放数据组.播放流组, 起始流))
            {
                if (!储存名.Contains('$')) { Debug.Print($"[下一流链路] 无法命名 {储存名} 每一个命名前都需要带有$符号，命名失败"); return false; }
                储存名 = 储存名.Replace('$', '&');
                var 命名结果 = 命名集.TryAdd(储存名, 配置.播放流组[起始流].下一流信息!.流别名);
                if (!命名结果)
                {
                    命名集[储存名] = 配置.播放流组[起始流].下一流信息!.流别名;
                    Debug.Print($"已将之前的命名替换成了现在的命名，将可以使用 {储存名} 访问 {配置.播放流组[起始流].下一流信息!.流别名} ");
                    return true;
                }
                Debug.Print($"将可以使用{储存名}访问{配置.播放流组[起始流].下一流信息!.流别名}");
            }
            else
            {
                Debug.Print($"[下一流链路] 找不到名为 {起始流} 的链路，请确定已经设置"); return false;
            }
            return true;
        }

        public static bool 新建段落链路(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 起始的段落名称
            string 目标段落 = 参数表[2]; //参数1 - 目标的段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!配置.空值或空引用(播放数据组.段落组, 起始段落) && !配置.空值或空引用(播放数据组.段落组, 目标段落))
            {
                配置.段落组[起始段落].下一段落链路 = 配置.段落组[目标段落];
            }
            else
            {
                Debug.Print($"新建段落链路失败，因为{起始段落}或{目标段落}为空");
                return false;
            }
            return true;
        }
        public static bool 删除段落链路(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 起始的段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!配置.空值或空引用(播放数据组.段落组, 起始段落))
            {
                配置.段落组[起始段落].下一段落链路 = null;
            }
            else
            {
                Debug.Print($"删除段落链路失败，因为{起始段落}为空");
                return false;
            }
            return true;
        }
        public static bool 修改段落链路(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 起始的段落名称
            string 目标段落 = 参数表[2]; //参数1 - 目标的段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!配置.空值或空引用(播放数据组.段落组, 起始段落) && !配置.空值或空引用(播放数据组.段落组, 目标段落))
            {
                配置.段落组[起始段落].下一段落链路 = 配置.段落组[目标段落];
            }
            else
            {
                Debug.Print($"修改段落链路失败，因为{起始段落}或{目标段落}为空");
                return false;
            }

            return true;
        }
        public static bool 取得当前段落(string[] 参数表)
        {
            string 储存名 = 参数表[1]; //参数1 - 储存的变量别名
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.当前段落))
            {
                Debug.Print("当前段落状态为空，请检查引用情况");
            }
            var 当前段落 = 配置.当前段落!.别名;
            if (!储存名.Contains('$')) { Debug.Print($"[取得当前段落] 无法命名 {储存名} 每一个命名前都需要带有$符号，命名失败"); return false; }
            储存名 = 储存名.Replace('$', '&');
            var 命名结果 = 命名集.TryAdd(储存名, 当前段落);
            if (!命名结果)
            {
                命名集[储存名] = 当前段落;
                Debug.Print($"已将之前的命名替换成了现在的命名，将可以使用{储存名}访问{当前段落}");
                return true;
            }
            Debug.Print($"将可以使用{储存名}访问{当前段落}");
            return true;
        }
        public static bool 下一段落链路(string[] 参数表)
        {
            string 起始段落 = 参数表[1]; //参数1 - 起始的段落名称 
            string 储存名 = 参数表[2]; //参数1 - 储存的变量别名
            音乐播放数据 配置 = 脚本解析器.数据;
            if (起始段落 == "当前段落")
            {
                起始段落 = 配置.当前段落!.别名;
            }
            if (!配置.空值或空引用(播放数据组.段落组, 起始段落))
            {
                if (!储存名.Contains('$')) { Debug.Print($"[下一段落链路] 无法命名 {储存名} 每一个命名前都需要带有$符号，命名失败"); return false; }
                储存名 = 储存名.Replace('$', '&');
                var 命名结果 = 命名集.TryAdd(储存名, 配置.段落组[起始段落].下一段落链路.别名);
                if (!命名结果)
                {
                    命名集[储存名] = 配置.段落组[起始段落].下一段落链路.别名;
                    Debug.Print($"已将之前的命名替换成了现在的命名，将可以使用{储存名}访问{配置.段落组[起始段落].下一段落链路.别名}");
                    return true;
                }
                Debug.Print($"将可以使用{储存名}访问{配置.段落组[配置.段落组[起始段落].下一段落链路.别名].别名}了");
            }
            else
            {
                Debug.Print($"[下一段落链路] 找不到名为 {起始段落} 的链路，请确定已经设置"); return false;
            }
            return true;
        }

        public static bool 指定UI目标(string[] 参数表)
        {
            string 播放流名称 = 参数表[1]; //参数1 - 播放流名称
            音乐播放数据 配置 = 脚本解析器.数据;
            if (!!配置.空值或空引用(播放数据组.播放流组, 播放流名称))
            { Debug.Print($"指定UI目标失败，找不到播放流{播放流名称}"); return false; }
            配置.当前流 = 配置.播放流组[播放流名称];
            配置.当前段落 = 配置.播放流组[播放流名称].当前段落;
            配置.当前文件 = 配置.播放流组[播放流名称].当前段落.绑定文件;
            return true;
        }

        public static bool 执行淡入(string[] 参数表)
        {
            string 效果别名 = 参数表[1]; //参数1 - 效果别名
            string 流别名 = 参数表[2]; //参数1 - 播放流名称
            string 段落别名 = 参数表[3]; //参数1 - 播放段落名称
            string 总处理时间 = 参数表[4]; //参数1 - 要处理多少字节

            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 流别名))
            { Debug.Print($"无法找到{流别名}无法应用淡入效果"); return false; }
            if (配置.空值或空引用(播放数据组.段落组, 段落别名))
            { Debug.Print($"无法找到{段落别名}无法应用淡入效果"); return false; }
            var 流序号 = 配置.播放流组[流别名].流编号;
            var 段落序号 = 配置.段落组[段落别名].编号;
            double 处理字节数 = -1;
            if (!double.TryParse(总处理时间, out 处理字节数))
            {
                Debug.Print("转换失败 - 总处理字节不是正确的数字");
                return false;
            }
            处理字节数 *= 数据转换.计算每秒字节(配置.段落组[段落别名].缓冲区时间, 配置.段落组[段落别名].缓冲区大小);
            var 返回值 = 淡入淡出.添加挂载(效果别名, 淡入淡出.段落信息.效果模式.淡入, 流序号, 段落序号, (int)处理字节数);
            if (!返回值.HasValue) { Debug.Print("淡入效果器应用失败"); return false; }
            return true;
        }

        public static bool 执行淡出(string[] 参数表)
        {
            string 效果别名 = 参数表[1]; //参数1 - 效果别名
            string 流别名 = 参数表[2]; //参数1 - 播放流名称
            string 段落别名 = 参数表[3]; //参数1 - 播放段落名称
            string 总处理时间 = 参数表[4]; //参数1 - 要处理多少字节

            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 流别名))
            { Debug.Print($"无法找到{流别名}无法应用淡入效果"); return false; }
            if (配置.空值或空引用(播放数据组.段落组, 段落别名))
            { Debug.Print($"无法找到{段落别名}无法应用淡入效果"); return false; }
            var 流序号 = 配置.播放流组[流别名].流编号;
            var 段落序号 = 配置.段落组[段落别名].编号;
            double 处理字节数 = -1;
            if (!double.TryParse(总处理时间, out 处理字节数))
            {
                Debug.Print("转换失败 - 总处理字节不是正确的数字");
                return false;
            }
            处理字节数 *= 数据转换.计算每秒字节(配置.段落组[段落别名].缓冲区时间, 配置.段落组[段落别名].缓冲区大小);
            var 返回值 = 淡入淡出.添加挂载(效果别名, 淡入淡出.段落信息.效果模式.淡出, 流序号, 段落序号, (int)处理字节数);
            if (!返回值.HasValue) { Debug.Print("淡入效果器应用失败"); return false; }
            return true;
        }

        public static bool 执行段落静音(string[] 参数表)
        {
            string 效果别名 = 参数表[1]; //参数1 - 效果别名
            string 流别名 = 参数表[2]; //参数1 - 播放流名称
            string 段落别名 = 参数表[3]; //参数1 - 播放段落名称
            string 总处理时间 = 参数表[4]; //参数1 - 要处理多少字节

            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.空值或空引用(播放数据组.播放流组, 流别名))
            { Debug.Print($"无法找到{流别名}无法应用淡入效果"); return false; }
            if (配置.空值或空引用(播放数据组.段落组, 段落别名))
            { Debug.Print($"无法找到{段落别名}无法应用淡入效果"); return false; }
            var 流序号 = 配置.播放流组[流别名].流编号;
            var 段落序号 = 配置.段落组[段落别名].编号;
            int 处理字节数 = -1;
            if (!int.TryParse(总处理时间, out 处理字节数))
            {
                Debug.Print("转换失败 - 总处理字节不是正确的数字");
                return false;
            }
            处理字节数 *= 数据转换.计算每秒字节(配置.段落组[段落别名].缓冲区时间, 配置.段落组[段落别名].缓冲区大小);
            var 返回值 = 淡入淡出.添加挂载(效果别名, 淡入淡出.段落信息.效果模式.完全静音, 流序号, 段落序号, 处理字节数);
            if (!返回值.HasValue) { Debug.Print("淡入效果器应用失败"); return false; }
            return true;
        }

        public static bool 解除挂载淡入淡出(string[] 参数表)
        {
            string 效果别名 = 参数表[1]; //参数1 - 播放流名称
            var 处理结果 = 淡入淡出.解除挂载(效果别名);
            if (处理结果) { return true; }
            else { return true; }
        }

        public static bool 禁用默认文件切换逻辑(string[] 参数表)
        {
            string 段落别名 = 参数表[1]; //参数1 - 播放段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            配置.段落组[段落别名].禁用默认切换逻辑 = true;
            return true;
        }

        public static bool 禁用默认赋值当前段落(string[] 参数表)
        {
            string 段落别名 = 参数表[1]; //参数1 - 播放段落名称
            音乐播放数据 配置 = 脚本解析器.数据;
            配置.段落组[段落别名].禁用默认赋值当前段落 = true;
            return true;
        }

        public static bool 执行混流(string[] 参数表)
        {
            return false;
        }

        public static bool 执行交叉叠化(string[] 参数表)
        {
            return false;
        }


        public static bool 解除挂载混流器(string[] 参数表)
        {
            return false;
        }

        public static bool 事件_进入循环等待(段落信息 段落)
        {
            if (段落.循环等待数据 == null) { return false; }
            var 按钮名称 = 段落.循环等待数据!.按钮显示名称;
            计划.全局按钮集![按钮名称].允许显示 = true;
            return true;
        }

        public static bool 事件_进入可控段落(段落信息 段落)
        {
            音乐播放数据 配置 = 脚本解析器.数据;
            var 对应按钮 = 计划.全局按钮集!.Values.Where(x => x.可用段落 == 段落.别名);
            if (对应按钮 == null) { 计划.全局事件.Remove(计划.事件.切换至可控制段落); return false; }
            var 按钮集 = 对应按钮;
            if (按钮集 == null) { 计划.全局事件.Remove(计划.事件.切换至可控制段落); return false; }
            foreach (var 按钮 in 按钮集)
            {
                按钮.允许显示 = true;
            }
            return true;
        }

        public static bool 等待_完成循环等待()
        {
            UI界面数据.默认按钮状态 = true;
            return true;
        }

        public static bool 事件_按下默认按钮()
        {
            音乐播放数据 配置 = 脚本解析器.数据;
            if (计划.全局默认按钮 == null) { return false; }
            var 默认按钮名称 = 计划.全局默认按钮.按钮显示名称;
            if (默认按钮名称 == null) { return false; }
            if (!计划.全局按钮集.ContainsKey(默认按钮名称)) { return false; }
            var 绑定脚本 = 计划.全局按钮集![默认按钮名称].绑定的脚本;
            if (!计划.全局按钮集[默认按钮名称].允许显示) { return false; }
            脚本解析器.当前解析.运行脚本(绑定脚本);
            if (计划.全局按钮集[默认按钮名称].按钮类型 != 计划.按钮数据._按钮类型.等待操作)
            {
                UI界面数据.默认按钮状态 = true;
            }
            return true;
        }

        public static void 回执_取下个Cue点()
        {
            音乐播放数据 配置 = 脚本解析器.数据;
            if (配置.当前段落!.Cue点集 == null || 配置.当前段落!.Cue点集!.Count == 0)
            { Debug.Print("没有可供使用的切换点，请添加切换点后再使用"); return; }

            if (配置.当前流.当前段落.Cue点集 == null || 配置.当前流.当前段落.Cue点集.Count == 0)
            { Debug.Print("这个段落没有切换点"); }
            var CUE点集 = 配置.当前流.当前段落.Cue点集.Values.ToList();
            配置.当前流.当前段落.下个Cue点 = 数据转换.取最近Cue点(CUE点集, 配置);
            计划.全局事件.Add(计划.事件.到达指定CUE点并切换);
            UI界面数据.默认按钮状态 = false;
            return;
        }
        public static bool 强制刷新默认按钮(string[] 参数表)
        {
            UI界面数据.默认按钮状态 = true;
            return true;
        }
        public static void 回执_启用默认按钮()
        {
            UI界面数据.默认按钮状态 = true;
        }
    }
}
