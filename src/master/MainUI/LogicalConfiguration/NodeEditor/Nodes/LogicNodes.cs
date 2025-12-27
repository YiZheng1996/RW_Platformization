using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.Parameter;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 延时等待节点
    /// </summary>
    [Serializable]
    public class DelayNode : WorkflowNodeBase
    {
        public override string NodeType => "DelayWait";
        public override Color NodeColor => Color.FromArgb(149, 165, 166);  // 灰色
        public override string IconName => "icon_delay.png";

        /// <summary>
        /// 延时参数
        /// </summary>
        public new Parameter_DelayTime Parameter
        {
            get => base.Parameter as Parameter_DelayTime ?? new Parameter_DelayTime();
            set => base.Parameter = value;
        }

        public DelayNode()
        {
            DisplayName = "延时等待";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            return $"等待 {p.T}ms";
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new DelayNode();
            CopyBasicProperties(clone);
            clone.Parameter = new Parameter_DelayTime { T = Parameter.T };
            return clone;
        }
    }

    /// <summary>
    /// 条件判断节点 - 支持True/False两个分支
    /// </summary>
    [Serializable]
    public class ConditionNode : WorkflowNodeBase
    {
        public override string NodeType => "ConditionJudge";
        public override Color NodeColor => Color.FromArgb(243, 156, 18);  // 橙色
        public override string IconName => "icon_condition.png";

        /// <summary>
        /// 条件判断参数
        /// </summary>
        public new Parameter_Detection Parameter
        {
            get => base.Parameter as Parameter_Detection ?? new Parameter_Detection();
            set => base.Parameter = value;
        }

        public ConditionNode()
        {
            DisplayName = "条件判断";
            Height = 80;  // 增加高度以容纳两个输出端口

            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });

            // 两个输出端口：True 和 False
            OutputSockets.Add(new NodeSocket("True", SocketType.Output)
            {
                Color = Color.FromArgb(46, 204, 113),  // 绿色
                Label = "✓ True"
            });
            OutputSockets.Add(new NodeSocket("False", SocketType.Output)
            {
                Color = Color.FromArgb(231, 76, 60),  // 红色
                Label = "✗ False"
            });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            if (string.IsNullOrEmpty(p.ConditionExpression))
                return "未配置条件";
            return p.ConditionExpression.Length > 25 
                ? p.ConditionExpression.Substring(0, 22) + "..." 
                : p.ConditionExpression;
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new ConditionNode();
            CopyBasicProperties(clone);
            // 深拷贝参数
            if (Parameter != null)
            {
                clone.Parameter = new Parameter_Detection
                {
                    DetectionName = Parameter.DetectionName,
                    ConditionExpression = Parameter.ConditionExpression
                };
            }
            return clone;
        }
    }

    /// <summary>
    /// 循环节点 - 支持内嵌子步骤
    /// </summary>
    [Serializable]
    public class LoopNode : WorkflowNodeBase
    {
        public override string NodeType => "CycleBegins";
        public override Color NodeColor => Color.FromArgb(142, 68, 173);  // 深紫色
        public override string IconName => "icon_loop.png";

        /// <summary>
        /// 循环参数
        /// </summary>
        public new Parameter_Loop Parameter
        {
            get => base.Parameter as Parameter_Loop ?? new Parameter_Loop();
            set => base.Parameter = value;
        }

        /// <summary>
        /// 子步骤节点集合（循环体内的节点）
        /// </summary>
        public List<WorkflowNodeBase> ChildNodes { get; set; } = new();

        /// <summary>
        /// 子步骤连接集合
        /// </summary>
        public List<NodeConnection> ChildConnections { get; set; } = new();

        /// <summary>
        /// 是否展开显示子步骤
        /// </summary>
        public bool IsExpanded { get; set; } = false;

        public LoopNode()
        {
            DisplayName = "循环";
            Height = 80;

            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White, Label = "完成" });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            var loopCount = p.LoopCountExpression ?? "10";
            var childCount = p.ChildSteps?.Count ?? ChildNodes.Count;

            var preview = $"循环 {loopCount} 次";
            if (childCount > 0)
            {
                preview += $" ({childCount}步)";
            }
            return preview;
        }

        /// <summary>
        /// 重写绘制 - 显示循环图标
        /// </summary>
        protected override void DrawContent(Graphics g)
        {
            base.DrawContent(g);

            // 绘制循环图标
            using var font = new Font("Segoe UI Symbol", 14);
            using var brush = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
            g.DrawString("🔄", font, brush, Location.X + Width - 30, Location.Y + TitleHeight + 2);
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new LoopNode();
            CopyBasicProperties(clone);
            clone.Parameter = new Parameter_Loop
            {
                LoopCountExpression = Parameter.LoopCountExpression,
                CounterVariableName = Parameter.CounterVariableName,
                EnableCounter = Parameter.EnableCounter,
                EnableEarlyExit = Parameter.EnableEarlyExit,
                ExitConditionExpression = Parameter.ExitConditionExpression
            };
            return clone;
        }
    }

    /// <summary>
    /// 等待稳定节点
    /// </summary>
    [Serializable]
    public class WaitStableNode : WorkflowNodeBase
    {
        public override string NodeType => "Waitingforstability";
        public override Color NodeColor => Color.FromArgb(52, 73, 94);  // 深蓝灰
        public override string IconName => "icon_stable.png";

        /// <summary>
        /// 等待稳定参数
        /// </summary>
        public new Parameter_WaitForStable Parameter
        {
            get => base.Parameter as Parameter_WaitForStable ?? new Parameter_WaitForStable();
            set => base.Parameter = value;
        }

        public WaitStableNode()
        {
            DisplayName = "等待稳定";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            return $"阈值≤{p.StabilityThreshold:F2}, {p.StableCount}次";
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new WaitStableNode();
            CopyBasicProperties(clone);
            clone.Parameter = new Parameter_WaitForStable
            {
                StabilityThreshold = Parameter.StabilityThreshold,
                StableCount = Parameter.StableCount,
                SamplingInterval = Parameter.SamplingInterval,
                TimeoutSeconds = Parameter.TimeoutSeconds
            };
            return clone;
        }
    }

    /// <summary>
    /// 实时监控节点
    /// </summary>
    [Serializable]
    public class MonitorNode : WorkflowNodeBase
    {
        public override string NodeType => "MonitorTool";
        public override Color NodeColor => Color.FromArgb(230, 126, 34);  // 橙色
        public override string IconName => "icon_monitor.png";

        public MonitorNode()
        {
            DisplayName = "实时监控";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview() => "监控运行中...";

        public override WorkflowNodeBase Clone()
        {
            var clone = new MonitorNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }

    /// <summary>
    /// 循环控制 - Break
    /// </summary>
    [Serializable]
    public class BreakNode : WorkflowNodeBase
    {
        public override string NodeType => "LoopControlBreak";
        public override Color NodeColor => Color.FromArgb(192, 57, 43);  // 深红色
        public override string IconName => "icon_break.png";

        public BreakNode()
        {
            DisplayName = "跳出循环";
            Width = 120;
            Height = 50;

            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            // Break没有输出端口，因为它终止循环
        }

        public override string GetParameterPreview() => "Break";

        public override WorkflowNodeBase Clone()
        {
            var clone = new BreakNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }

    /// <summary>
    /// 循环控制 - Continue
    /// </summary>
    [Serializable]
    public class ContinueNode : WorkflowNodeBase
    {
        public override string NodeType => "LoopControlContinue";
        public override Color NodeColor => Color.FromArgb(39, 174, 96);  // 深绿色
        public override string IconName => "icon_continue.png";

        public ContinueNode()
        {
            DisplayName = "继续循环";
            Width = 120;
            Height = 50;

            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            // Continue没有输出端口，因为它跳到循环开始
        }

        public override string GetParameterPreview() => "Continue";

        public override WorkflowNodeBase Clone()
        {
            var clone = new ContinueNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }
}
