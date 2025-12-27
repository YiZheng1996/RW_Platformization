using System;
using System.Drawing;
using MainUI.LogicalConfiguration.NodeEditor.Core;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 开始节点 - 工作流入口点
    /// </summary>
    [Serializable]
    public class StartNode : WorkflowNodeBase
    {
        public override string NodeType => "Start";
        public override Color NodeColor => Color.FromArgb(46, 204, 113);  // 绿色
        public override string IconName => "icon_start.png";

        public StartNode()
        {
            DisplayName = "开始";
            Width = 100;
            Height = 50;

            // 只有输出端口
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output)
            {
                Color = Color.FromArgb(46, 204, 113),
                Label = ""
            });
        }

        public override string GetParameterPreview() => "工作流起点";

        public override WorkflowNodeBase Clone()
        {
            var clone = new StartNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }

    /// <summary>
    /// 结束节点 - 工作流终点
    /// </summary>
    [Serializable]
    public class EndNode : WorkflowNodeBase
    {
        public override string NodeType => "End";
        public override Color NodeColor => Color.FromArgb(231, 76, 60);  // 红色
        public override string IconName => "icon_end.png";

        public EndNode()
        {
            DisplayName = "结束";
            Width = 100;
            Height = 50;

            // 只有输入端口
            InputSockets.Add(new NodeSocket("In", SocketType.Input)
            {
                Color = Color.FromArgb(231, 76, 60),
                Label = ""
            });
        }

        public override string GetParameterPreview() => "工作流终点";

        public override WorkflowNodeBase Clone()
        {
            var clone = new EndNode();
            CopyBasicProperties(clone);
            return clone;
        }
    }
}
