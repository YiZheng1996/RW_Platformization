using System;
using System.Drawing;
using System.Linq;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.Parameter;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 读取PLC节点
    /// </summary>
    [Serializable]
    public class ReadPLCNode : WorkflowNodeBase
    {
        public override string NodeType => "ReadPLC";
        public override Color NodeColor => Color.FromArgb(52, 152, 219);  // 蓝色
        public override string IconName => "icon_plc_read.png";

        /// <summary>
        /// 读取PLC参数
        /// </summary>
        public new Parameter_ReadPLC Parameter
        {
            get => base.Parameter as Parameter_ReadPLC ?? new Parameter_ReadPLC();
            set => base.Parameter = value;
        }

        public ReadPLCNode()
        {
            DisplayName = "读取PLC";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            if (p.Items == null || p.Items.Count == 0)
                return "未配置";

            var firstItem = p.Items.First();
            var preview = $"{firstItem.PlcModuleName}.{firstItem.PlcKeyName}";

            if (!string.IsNullOrEmpty(firstItem.TargetVarName))
                preview += $" → @{firstItem.TargetVarName}";

            if (p.Items.Count > 1)
                preview += $" (+{p.Items.Count - 1})";

            return preview.Length > 30 ? preview.Substring(0, 27) + "..." : preview;
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new ReadPLCNode();
            CopyBasicProperties(clone);

            if (Parameter?.Items != null)
            {
                clone.Parameter = new Parameter_ReadPLC
                {
                    Items = Parameter.Items.Select(item => new PlcReadItem
                    {
                        PlcModuleName = item.PlcModuleName,
                        PlcKeyName = item.PlcKeyName,
                        TargetVarName = item.TargetVarName
                    }).ToList()
                };
            }

            return clone;
        }
    }

    /// <summary>
    /// 写入PLC节点
    /// </summary>
    [Serializable]
    public class WritePLCNode : WorkflowNodeBase
    {
        public override string NodeType => "WritePLC";
        public override Color NodeColor => Color.FromArgb(155, 89, 182);  // 紫色
        public override string IconName => "icon_plc_write.png";

        /// <summary>
        /// 写入PLC参数
        /// </summary>
        public new Parameter_WritePLC Parameter
        {
            get => base.Parameter as Parameter_WritePLC ?? new Parameter_WritePLC();
            set => base.Parameter = value;
        }

        public WritePLCNode()
        {
            DisplayName = "写入PLC";
            InputSockets.Add(new NodeSocket("In", SocketType.Input) { Color = Color.White });
            OutputSockets.Add(new NodeSocket("Out", SocketType.Output) { Color = Color.White });
        }

        public override string GetParameterPreview()
        {
            var p = Parameter;
            if (p.Items == null || p.Items.Count == 0)
                return "未配置";

            var firstItem = p.Items.First();
            var value = firstItem.PlcValue?.Length > 10
                ? firstItem.PlcValue.Substring(0, 7) + "..."
                : firstItem.PlcValue;

            var preview = $"{firstItem.PlcModuleName}.{firstItem.PlcKeyName} ← {value}";

            if (p.Items.Count > 1)
                preview += $" (+{p.Items.Count - 1})";

            return preview.Length > 30 ? preview.Substring(0, 27) + "..." : preview;
        }

        public override WorkflowNodeBase Clone()
        {
            var clone = new WritePLCNode();
            CopyBasicProperties(clone);

            if (Parameter?.Items != null)
            {
                clone.Parameter = new Parameter_WritePLC
                {
                    Description = Parameter.Description,
                    Items = Parameter.Items.Select(item => item.Clone()).ToList()
                };
            }

            return clone;
        }
    }
}
