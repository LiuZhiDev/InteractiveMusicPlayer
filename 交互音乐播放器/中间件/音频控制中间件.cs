﻿using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using 交互音乐播放器.中间件;
using 交互音乐播放器.数据;
using 交互音乐播放器.效果器;
using 交互音乐播放器.数据;
using 交互音乐播放器.数据.播放中数据;
using 交互音乐播放器.音频逻辑;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using 交互音乐播放器.UI.附加控件;

namespace 交互音乐播放器.中间件
{


    public class 音频控制中间件
    {
        public static 音频控制中间件? 当前中间件;
        public static 音乐播放数据? 播放数据 = new 音乐播放数据();
        public static 脚本文件数据? 脚本数据;
        public static 音频控制器 当前音频控制器;
        public Dictionary<string, int> 音频播放流组 = new Dictionary<string, int>();
        public Dictionary<string, int> 音频播放器组 = new Dictionary<string, int>();
        public 音频控制中间件(脚本文件数据 脚本)
        {
            if (当前中间件 != null) { 当前中间件.命令_停止所有(); }
            当前中间件 = this;
            脚本数据 = 脚本;
            重置临时配置();
            播放数据 = 脚本读到内存播放数据(脚本);
            if (播放数据 == null) { MessageBox.Show("播放失败，因为具有同样的段落名称 段落名称不可以相同","无法播放");return; }
            新建播放器(脚本);
            执行脚本文件(脚本);
            更新歌词数据();
        }

        private void 更新歌词数据()
        {
            if (歌词显示窗.当前显示窗 != null) { 歌词显示窗.加载基础信息(播放数据!.当前文件!.路径); }
        }

        private void 新建播放器(脚本文件数据 脚本)
        {
            音频控制器 控制器 = new 音频控制器(脚本, 播放数据);
            当前音频控制器 = 控制器;
        }

        public 音乐播放数据? 脚本读到内存播放数据(脚本文件数据 脚本)
        {
            音乐播放数据 播放数据 = null;
            if (脚本.段落名称 == null || 脚本.段落名称.Count == 0 || 脚本.拥有配置文件 == false)
            {
                播放数据 = 生成配置到内存(脚本); //无脚本文件时生成逻辑
            }
            else
            {
                播放数据 = 配置文件读取到内存(脚本); //有脚本文件时生成逻辑
                if (播放数据 == null) { return null; }
            }
            return 播放数据;
        }
        public 音乐播放数据 生成配置到内存(脚本文件数据 脚本)
        {
            音乐播放数据 主配置 = new 音乐播放数据();
            for (int 段落编号 = 0; 段落编号 < 脚本.文件组.Count; 段落编号++)
            {
                if (脚本.文件组.Count > 1 && 脚本.拥有配置文件 == false)
                {
                    var 文件别名 = Path.GetFileNameWithoutExtension(脚本.文件组[段落编号]).Split('_')[1];
                    主配置.文件组.Add(文件别名, new 数据.播放中数据.文件信息()
                    {
                        别名 = 文件别名,
                        编号 = 段落编号,
                        路径 = 脚本.文件组[段落编号]
                    });
                    主配置.段落组.Add(文件别名, new 数据.播放中数据.段落信息()
                    {
                        别名 = 文件别名,
                        绑定文件 = 主配置.文件组[文件别名],//最终是转换为了别名
                        编号 = 段落编号,
                        状态 = 数据.播放中数据.段落信息.播放状态.停止,
                        播放位置 = new 数据.播放中数据.位置()
                    });
                    if (脚本.段落名称 == null) { 脚本.段落名称 = new List<string>(); }
                    脚本.段落名称.Add(文件别名);
                }
                else
                {
                    主配置.文件组.Add(脚本.名称, new 数据.播放中数据.文件信息()
                    {
                        别名 = 脚本.名称,
                        编号 = 段落编号,
                        路径 = 脚本.文件组[段落编号]
                    });
                    主配置.段落组.Add(脚本.名称, new 数据.播放中数据.段落信息()
                    {
                        别名 = 脚本.名称,
                        绑定文件 = 主配置.文件组[脚本.名称],//最终是转换为了别名
                        编号 = 段落编号,
                        状态 = 数据.播放中数据.段落信息.播放状态.停止,
                        播放位置 = new 数据.播放中数据.位置()
                    });
                }
              
            }
            return 主配置;
        }

        public 音乐播放数据 配置文件读取到内存(脚本文件数据 脚本)
        {
            音乐播放数据 主配置 = new 音乐播放数据();
            for (int 段落编号 = 0; 段落编号 < 脚本.文件组.Count; 段落编号++)
            {
                if (脚本.段落名称.Count <= 段落编号) { continue; } //如果已经加载过了就不加载了
                if (主配置.文件组.ContainsKey(脚本.段落名称[段落编号])) { return null; }
                if (!File.Exists(脚本.文件组[段落编号])) 
                {
                    MessageBox.Show("脚本中的待播放文件并不存在，将替换为当前目录文件名");
                    var 文件名 = Path.GetFileNameWithoutExtension(脚本.文件组[段落编号]);
                    var 扩展名 = Path.GetExtension(脚本.文件组[段落编号]);
                    var 当前目录 = $"{UI界面数据.当前浏览文件夹}\\{文件名}{扩展名}";
                    脚本.文件组[段落编号] = 当前目录;
                    if (!File.Exists(脚本.文件组[段落编号])) { MessageBox.Show("当前目录亦未找到文件，请检查配置文件。"); continue; }
                }
                主配置.文件组.Add(脚本.段落名称[段落编号], new 数据.播放中数据.文件信息()
                {
                    别名 = 脚本.段落名称[段落编号],
                    编号 = 段落编号,
                    路径 = 脚本.文件组[段落编号]
                });
                主配置.段落组.Add(脚本.段落名称[段落编号], new 数据.播放中数据.段落信息()
                {
                    别名 = 脚本.段落名称[段落编号],
                    绑定文件 = 主配置.文件组[脚本.段落名称[段落编号]],//最终是转换为了别名
                    Offset = 脚本.Offset组[段落编号],
                    启用循环 = 脚本.循环下标组[段落编号],
                    编号 = 段落编号,
                    状态 = 数据.播放中数据.段落信息.播放状态.停止,
                    节拍信息_读取 = new 数据.播放中数据.节拍(0, 0, 0, 0)
                    {
                        BPM = 脚本.BPM组[段落编号],
                        每小节拍数 = 脚本.小节节拍总数组[段落编号]
                    },
                    节拍信息_显示 = new 数据.播放中数据.节拍(0, 0, 0, 0)
                    {
                        BPM = 脚本.BPM组[段落编号],
                        每小节拍数 = 脚本.小节节拍总数组[段落编号]
                    },
                    播放位置 = new 数据.播放中数据.位置()
                });
            }
            return 主配置;
        }

        public void 重置临时配置()
        {
            播放数据 = new 音乐播放数据();
            计划.全局事件.Clear();
            计划.全局按钮集.Clear();
            计划.全局等待.Clear();
            计划.全局默认按钮 = null;
            UI界面数据.默认按钮状态 = true;
            淡入淡出.重置所有挂载();
        }

        private void 执行脚本文件(脚本文件数据 脚本)
        {
            //如果没有脚本文件执行的命令
            if (脚本.脚本文档 == null || 脚本.脚本文档.Length == 0)
            {
                无脚本文档逻辑(脚本);
                return;
            }
            //如果有脚本文件执行的命令
            脚本解析器 解析器 = new 脚本解析器(脚本);

        }

        private void 无脚本文档逻辑(脚本文件数据 脚本)
        {
            //读取所有文件
            int 文件序列 = 0;
            foreach (var 文件 in 播放数据.文件组)
            {
                命令_加载文件(文件.Value.路径, 文件.Value.别名);
                文件序列++;
            }
            //播放第一个文件
            //初始化播放脚本(0, 0);
            if (播放数据.文件组.Values.Count != 0)
            {
                播放数据.当前文件 = 播放数据.文件组.Values.FirstOrDefault();
            }
            string 播放流名称 = "默认播放流";
            var 新流 = 命令_新建播放流(播放流名称);
            新流.当前段落 = 播放数据.段落组.Values.FirstOrDefault();
            计划.全局按钮集!.Add("无脚本", new 计划.按钮数据()
            {
                允许显示 = true,
                可用段落 = 播放数据.段落组.Values.FirstOrDefault().别名,
                按钮类型 = 计划.按钮数据._按钮类型.正常进行,
                按钮显示名称 = "无脚本",
                绑定的脚本 = null
            });
            新流.当前段落.绑定文件 = 播放数据.文件组.Values.FirstOrDefault();
            播放数据.重定向当前流引用(新流.流别名);
            if (播放数据.空值或空引用(音乐播放数据.播放数据组.当前文件)) { Debug.Print("无脚本逻辑未能加载文件，文件引用是空的"); return; }
            命令_播放文件(播放流名称, 播放数据.当前文件!.别名);
        }

        [Obsolete("该方法已弃用，名称有未知的意义")]
        private void 初始化播放脚本(int 当前播放位置, int 当前播放段落)
        {
            throw new System.Exception("正在调用无法预知行为的方法");
            //当前脚本.状态_当前位置 = 当前播放位置;

        }

        public bool 命令_加载文件(string 文件路径, string 别名)
        {
            return 当前音频控制器.命令_加载文件(文件路径, 别名);
        }

        public 流信息? 命令_新建播放流(string 播放流名称)
        {
            if (!音频播放流组.TryAdd(播放流名称, 当前音频控制器.命令_新建播放流(播放流名称, 音频播放流组.Count - 1)))
            {
                return null;
            }
            if (播放数据.空值或空引用(音乐播放数据.播放数据组.播放流组, 播放流名称))
            {
                播放数据.播放流组.Add(播放流名称, new 数据.播放中数据.流信息() { 流别名 = 播放流名称, 流编号 = 播放数据.播放流组.Count - 1 });
            }
            else
            {
                播放数据.播放流组[播放流名称] = new 数据.播放中数据.流信息() { 流别名 = 播放流名称, 流编号 = 播放数据.播放流组.Count - 1 };
            }
            return 播放数据.播放流组[播放流名称];
        }

        public void 命令_播放文件(string 播放流名称, string 别名)
        {
            当前音频控制器.命令_播放音乐(播放流名称, 别名);
        }

        public void 命令_暂停()
        {
            当前音频控制器.命令_暂停所有();
        }

        public void 命令_暂停(string 播放流别名)
        {
            当前音频控制器.命令_暂停(播放流别名);
        }

        public void 命令_继续所有()
        {
            if (当前音频控制器 == null) { return; }
            当前音频控制器.命令_继续所有();
        }

        public void 命令_停止所有()
        {
            if (当前音频控制器 == null) { return; }
            当前音频控制器.命令_停止所有();
        }


        public void 命令_停止(string 播放流别名)
        {
            当前音频控制器.命令_停止(播放流别名);
        }

        public void 命令_淡入(string 播放流名称, int 段落编号)
        {

        }

        public void 命令_淡出(string 播放流名称, int 段落编号)
        {

        }

        public void 命令_设定总音量(bool 是否为偏移值, int 音量大小)
        {
            当前音频控制器.命令_设置总音量(false, (float)音量大小 / 100);
        }

        public void 命令_添加切换按钮(string 播放流名称)
        {

        }

        public void 命令_设置循环等待(string 播放流名称, int 段落编号, string 提示信息)
        {

        }

        public void 命令_单流设置切换点(string 播放流名称, int 前文件编号, int 前切换点, int 后文件编号, int 后切换点)
        {

        }

        public void 命令_设置下个段落(string 播放流名称, int 前文件编号, int 前切换点, int 后文件编号, int 后切换点)
        {

        }

        public void 命令_指定UI目标(string 切换按钮名称, string 脚本段名称)
        {

        }

        public void 命令_添加切换按钮(string 播放流名称, string 脚本段名称)
        {

        }

        public void 命令_添加数值器(string 播放流名称, string 脚本段名称)
        {

        }

        public void 命令_切换到下段落(string 播放流名称)
        {

        }

        public void 命令_全部重定位(float 大致位置)
        {
            if (当前音频控制器 == null) { return; }
            当前音频控制器.命令_全部重定位(大致位置);
        }

        public void 命令_重定位(long 字节数, string 文件别名)
        {
            当前音频控制器.命令_重定位(字节数, 文件别名);
        }

        public void 补例_更新播放内容()
        {

            if (播放数据!.空值或空引用(音乐播放数据.播放数据组.当前流) || 播放数据.当前流!.当前段落 == null) { return; }

            if (播放数据.当前流.当前段落.节拍信息_读取.BPM == -1) { return; }
            var 流别名 = 播放数据.当前流!.流别名;
            var 小节偏移 = 播放数据.当前流.当前段落.Offset;
            if (音频控制器.当前控制器 == null || 播放数据.当前流.当前段落.绑定文件 == null) { return; }
            var 当前文件 = 音频控制器.当前控制器.已载文件组[播放数据.当前流.当前段落.绑定文件.别名];
            var 当前位置 = 音频控制器.当前控制器.音频播放器组[流别名].GetPosition();
            var 当前播放总拍数 = (int)((当前位置 +
                数据转换.数字时间转目标字节(小节偏移, 当前文件) - 播放数据.当前流.播放器偏移) / 播放数据.当前流.当前段落.节拍信息_显示.每拍字节); //需要增加小节偏移量才能更准确
            播放数据.当前流.当前段落.节拍信息_显示.拍 = 当前播放总拍数 % 播放数据.当前流.当前段落.节拍信息_显示.每小节拍数 + 1;
            播放数据.当前流.当前段落.节拍信息_显示.小节 = 当前播放总拍数 / 播放数据.当前流.当前段落.节拍信息_显示.每小节拍数 + 1;

            if (计划.全局事件.Contains(计划.事件.播放至指定CUE点并执行脚本))
            {
                if (当前位置 - 播放数据.当前流.播放器偏移 + 数据转换.数字时间转目标字节(((double)UI界面数据.UI动态更新时间 / 1000), (WaveStream)播放数据.当前流.当前段落.绑定文件.文件实例) >= 播放数据.当前流.当前段落.下个Cue点!.出点位置)
                {
                    Debug.Print($"!!!!!!!!!!在{当前位置 - 播放数据.当前流.播放器偏移} - {播放数据.当前流.当前段落.下个Cue点!.出点位置}={当前位置 - 播放数据.当前流.播放器偏移 - 播放数据.当前流.当前段落.下个Cue点!.出点位置}  => {音频控制器.当前控制器.音频播放器组[流别名].GetPositionTimeSpan()}| {播放数据.当前流.当前段落.节拍信息_显示.小节}.{播放数据.当前流.当前段落.节拍信息_显示.拍} 节拍处切换");
                    if (播放数据.当前流.当前段落!.下个Cue点!.绑定的脚本 != null)
                    {

                        脚本解析器.当前解析.运行脚本(播放数据.当前流.当前段落!.下个Cue点!.绑定的脚本);
                        脚本命令.回执_启用默认按钮();
                    }
                    播放数据.当前流.当前段落!.下个Cue点 = null;
                    计划.全局事件.Remove(计划.事件.播放至指定CUE点并执行脚本);
                }
            }
        }
        /// <summary>
        /// 将文件名转换成文件序号，不支持从别名转换
        /// 转换成功返回文件名，否则返回-1
        /// </summary>
        /// <param name="文件名">文件名</param>
        public int 文件名转文件序号(string 文件名)
        {
            return 脚本数据.段落名称.IndexOf(文件名);
        }
    }
}
