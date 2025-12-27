using System;
using System.Drawing;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.Parameter;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 变量赋值节点
    /// </summary>
    [Serializable]
    public class VariableAssignNode : WorkflowNodeBase
    {
        public override string NodeType => "VariableAssign";
        public override Color NodeColor => Color.FromArgb(26, 188, 156);  // 青色
        public override string IconName => "icon_variable.png";

        /// <summary>
        /// 变量赋值参数
        /// </summary>
        public new Parameter_VariableAssignment Parameter
        {
            get => base.Parameter as Parameter_VariableAssignment ?? new Parameter_VariableAssignment();
            set => base.Parameter = value;
        }

        public VariableAssignNode()
        {
            DisplayName = "变量赋值";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            if (string.IsNullOrEmpty(p.TargetVarName))
                return "未配置";

            var value = p.Expression?.Length > 15
                ? p.Expression.Substring(0, 12) + "..."
                : p.Expression;

            return $"@{p.TargetVarName} = {value}";
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new VariableAssignNode();
            CopyBasicProperties(clone);
            clone.Parameter = new Parameter_VariableAssignment
            {
                TargetVarName = Parameter.TargetVarName,
                Expression = Parameter.Expression
            };
            return clone;
        }
    }

    /// <summary>
    /// 变量定义节点
    /// </summary>
    [Serializable]
    public class DefineVarNode : WorkflowNodeBase
    {
        public override string NodeType => "DefineVar";
        public override Color NodeColor => Color.FromArgb(22, 160, 133);  // 深青色
        public override string IconName => "icon_define.png";

        /// <summary>
        /// 变量定义参数
        /// </summary>
        public new Parameter_DefineVar Parameter
        {
            get => base.Parameter as Parameter_DefineVar ?? new Parameter_DefineVar();
            set => base.Parameter = value;
        }

        public DefineVarNode()
        {
            DisplayName = "变量定义";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            if (string.IsNullOrEmpty(p.VarName))
                return "未配置";

            return $"定义: @{p.VarName}";
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new DefineVarNode();
            CopyBasicProperties(clone);
            clone.Parameter = new Parameter_DefineVar
            {
                VarName = Parameter.VarName
            };
            return clone;
        }
    }

    /// <summary>
    /// 消息通知节点
    /// </summary>
    [Serializable]
    public class MessageNode : WorkflowNodeBase
    {
        public override string NodeType => "MessageNotify";
        public override Color NodeColor => Color.FromArgb(241, 196, 15);  // 黄色
        public override string IconName => "icon_message.png";

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; } = MessageType.Info;

        public MessageNode()
        {
            DisplayName = "消息通知";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            if (string.IsNullOrEmpty(Message))
                return "未配置消息";

            return Message.Length > 25 ? Message.Substring(0, 22) + "..." : Message;
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new MessageNode();
            CopyBasicProperties(clone);
            clone.Message = Message;
            clone.Type = Type;
            return clone;
        }
    }

    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success
    }

    /// <summary>
    /// 报表节点
    /// </summary>
    [Serializable]
    public class ReportNode : WorkflowNodeBase
    {
        public override string NodeType => "Report";
        public override Color NodeColor => Color.FromArgb(52, 73, 94);  // 深蓝灰
        public override string IconName => "icon_report.png";

        public ReportNode()
        {
            DisplayName = "生成报表";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview() => "生成测试报表";

        public override WorkflowNodeBase Clone()
        {
            var clone = new ReportNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }
}
