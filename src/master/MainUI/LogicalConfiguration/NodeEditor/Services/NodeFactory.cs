using System;
using System.Collections.Generic;
using System.Linq;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;

namespace MainUI.LogicalConfiguration.NodeEditor.Services
{
    /// <summary>
    /// 节点分类信息
    /// </summary>
    public class NodeCategoryInfo
    {
        public string CategoryName { get; set; }
        public string CategoryKey { get; set; }
        public string IconName { get; set; }
        public List<NodeTypeInfo> NodeTypes { get; set; } = new();
    }

    /// <summary>
    /// 节点类型信息
    /// </summary>
    public class NodeTypeInfo
    {
        public string DisplayName { get; set; }
        public string TypeKey { get; set; }
        public string IconName { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 节点工厂 - 根据类型创建节点实例
    /// </summary>
    public static class NodeFactory
    {
        #region 节点类型映射

        /// <summary>
        /// 节点类型映射表
        /// </summary>
        private static readonly Dictionary<string, Func<WorkflowNodeBase>> NodeCreators = new()
        {
            // 控制节点
            { "Start", () => new StartNode() },
            { "End", () => new EndNode() },

            // 逻辑节点
            { "DelayWait", () => new DelayNode() },
            { "ConditionJudge", () => new ConditionNode() },
            { "CycleBegins", () => new LoopNode() },
            { "Waitingforstability", () => new WaitStableNode() },
            { "MonitorTool", () => new MonitorNode() },
            { "LoopControlBreak", () => new BreakNode() },
            { "LoopControlContinue", () => new ContinueNode() },

            // 数据节点
            { "VariableAssign", () => new VariableAssignNode() },
            { "DefineVar", () => new DefineVarNode() },
            { "MessageNotify", () => new MessageNode() },
            { "Report", () => new ReportNode() },

            // 通信节点
            { "ReadPLC", () => new ReadPLCNode() },
            { "WritePLC", () => new WritePLCNode() },
        };

        #endregion

        #region 节点分类定义

        /// <summary>
        /// 获取节点分类信息（用于工具箱显示）
        /// </summary>
        public static List<NodeCategoryInfo> GetNodeCategories()
        {
            return new List<NodeCategoryInfo>
            {
                new NodeCategoryInfo
                {
                    CategoryName = "控制节点",
                    CategoryKey = "Control",
                    IconName = "folder.png",
                    NodeTypes = new List<NodeTypeInfo>
                    {
                        new NodeTypeInfo { DisplayName = "开始", TypeKey = "Start", IconName = "icon_start.png", Description = "工作流起点" },
                        new NodeTypeInfo { DisplayName = "结束", TypeKey = "End", IconName = "icon_end.png", Description = "工作流终点" }
                    }
                },
                new NodeCategoryInfo
                {
                    CategoryName = "逻辑控制",
                    CategoryKey = "Logic",
                    IconName = "folder.png",
                    NodeTypes = new List<NodeTypeInfo>
                    {
                        new NodeTypeInfo { DisplayName = "延时等待", TypeKey = "DelayWait", IconName = "icon_delay.png", Description = "等待指定时间" },
                        new NodeTypeInfo { DisplayName = "条件判断", TypeKey = "ConditionJudge", IconName = "icon_condition.png", Description = "条件分支判断" },
                        new NodeTypeInfo { DisplayName = "循环", TypeKey = "CycleBegins", IconName = "icon_loop.png", Description = "循环执行子步骤" },
                        new NodeTypeInfo { DisplayName = "等待稳定", TypeKey = "Waitingforstability", IconName = "icon_stable.png", Description = "等待数值稳定" },
                        new NodeTypeInfo { DisplayName = "实时监控", TypeKey = "MonitorTool", IconName = "icon_monitor.png", Description = "实时数据监控" },
                        new NodeTypeInfo { DisplayName = "跳出循环", TypeKey = "LoopControlBreak", IconName = "icon_break.png", Description = "立即跳出当前循环" },
                        new NodeTypeInfo { DisplayName = "继续循环", TypeKey = "LoopControlContinue", IconName = "icon_continue.png", Description = "跳过本次循环" }
                    }
                },
                new NodeCategoryInfo
                {
                    CategoryName = "数据操作",
                    CategoryKey = "Data",
                    IconName = "folder.png",
                    NodeTypes = new List<NodeTypeInfo>
                    {
                        new NodeTypeInfo { DisplayName = "变量赋值", TypeKey = "VariableAssign", IconName = "icon_variable.png", Description = "设置变量值" },
                        new NodeTypeInfo { DisplayName = "变量定义", TypeKey = "DefineVar", IconName = "icon_define.png", Description = "定义新变量" },
                        new NodeTypeInfo { DisplayName = "消息通知", TypeKey = "MessageNotify", IconName = "icon_message.png", Description = "显示消息提示" },
                        new NodeTypeInfo { DisplayName = "生成报表", TypeKey = "Report", IconName = "icon_report.png", Description = "生成测试报表" }
                    }
                },
                new NodeCategoryInfo
                {
                    CategoryName = "通信操作",
                    CategoryKey = "PLC",
                    IconName = "folder.png",
                    NodeTypes = new List<NodeTypeInfo>
                    {
                        new NodeTypeInfo { DisplayName = "读取PLC", TypeKey = "ReadPLC", IconName = "icon_plc_read.png", Description = "从PLC读取数据" },
                        new NodeTypeInfo { DisplayName = "写入PLC", TypeKey = "WritePLC", IconName = "icon_plc_write.png", Description = "向PLC写入数据" }
                    }
                }
            };
        }

        #endregion

        #region 创建方法

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="nodeType">节点类型标识</param>
        /// <returns>节点实例</returns>
        public static WorkflowNodeBase CreateNode(string nodeType)
        {
            if (string.IsNullOrEmpty(nodeType))
            {
                throw new ArgumentNullException(nameof(nodeType));
            }

            if (NodeCreators.TryGetValue(nodeType, out var creator))
            {
                return creator();
            }

            throw new ArgumentException($"未知的节点类型: {nodeType}", nameof(nodeType));
        }

        /// <summary>
        /// 尝试创建节点
        /// </summary>
        /// <param name="nodeType">节点类型标识</param>
        /// <param name="node">创建的节点</param>
        /// <returns>是否成功</returns>
        public static bool TryCreateNode(string nodeType, out WorkflowNodeBase node)
        {
            node = null;

            if (string.IsNullOrEmpty(nodeType))
                return false;

            if (NodeCreators.TryGetValue(nodeType, out var creator))
            {
                node = creator();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从 ChildModel 创建节点
        /// </summary>
        /// <param name="step">ChildModel 步骤</param>
        /// <returns>节点实例</returns>
        public static WorkflowNodeBase CreateFromChildModel(ChildModel step)
        {
            if (step == null)
                throw new ArgumentNullException(nameof(step));

            var node = CreateNode(step.StepName);
            node.DisplayName = step.StepName;
            node.Remark = step.Remark;
            node.Parameter = step.StepParameter;
            node.ErrorMessage = step.ErrorMessage;

            // 根据状态设置执行状态
            node.ExecutionStatus = step.Status switch
            {
                1 => NodeExecutionStatus.Completed,
                2 => NodeExecutionStatus.Running,
                -1 => NodeExecutionStatus.Failed,
                _ => NodeExecutionStatus.Pending
            };

            return node;
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 获取所有已注册的节点类型
        /// </summary>
        public static IEnumerable<string> GetAllNodeTypes() => NodeCreators.Keys;

        /// <summary>
        /// 检查节点类型是否存在
        /// </summary>
        public static bool IsNodeTypeRegistered(string nodeType)
        {
            return !string.IsNullOrEmpty(nodeType) && NodeCreators.ContainsKey(nodeType);
        }

        /// <summary>
        /// 获取节点类型信息
        /// </summary>
        public static NodeTypeInfo GetNodeTypeInfo(string nodeType)
        {
            var categories = GetNodeCategories();
            foreach (var category in categories)
            {
                var typeInfo = category.NodeTypes.FirstOrDefault(t => t.TypeKey == nodeType);
                if (typeInfo != null)
                    return typeInfo;
            }
            return null;
        }

        #endregion

        #region 注册方法

        /// <summary>
        /// 注册自定义节点类型
        /// </summary>
        /// <param name="nodeType">节点类型标识</param>
        /// <param name="creator">创建函数</param>
        public static void RegisterNodeType(string nodeType, Func<WorkflowNodeBase> creator)
        {
            if (string.IsNullOrEmpty(nodeType))
                throw new ArgumentNullException(nameof(nodeType));

            if (creator == null)
                throw new ArgumentNullException(nameof(creator));

            NodeCreators[nodeType] = creator;
        }

        /// <summary>
        /// 注销节点类型
        /// </summary>
        /// <param name="nodeType">节点类型标识</param>
        public static bool UnregisterNodeType(string nodeType)
        {
            if (string.IsNullOrEmpty(nodeType))
                return false;

            return NodeCreators.Remove(nodeType);
        }

        #endregion
    }
}
