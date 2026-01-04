using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 工作流图转换器 - 在 STNodeEditor 图和 ChildModel 列表之间进行转换
    /// </summary>
    public class WorkflowGraphConverter(STNodeEditor editor)
    {
        #region 私有字段

        private readonly STNodeEditor _editor = editor ?? throw new ArgumentNullException(nameof(editor));

        #endregion

        #region 图 → ChildModel 列表

        /// <summary>
        /// 将节点图转换为 ChildModel 列表
        /// 使用拓扑排序确定执行顺序
        /// </summary>
        public List<ChildModel> ConvertToChildModels()
        {
            var result = new List<ChildModel>();

            // 获取所有工作流节点
            var allNodes = GetAllWorkflowNodes();
            if (allNodes.Count == 0)
                return result;

            // 查找开始节点
            var startNode = allNodes.FirstOrDefault(n => n is StartNode);
            if (startNode == null)
            {
                // 如果没有开始节点，使用拓扑排序
                return ConvertUsingTopologicalSort(allNodes);
            }

            // 从开始节点开始遍历
            var visited = new HashSet<Guid>();
            var orderedNodes = new List<WorkflowNodeBase>();
            TraverseFromNode(startNode, visited, orderedNodes, allNodes);

            // 转换为 ChildModel
            int stepNum = 1;
            foreach (var node in orderedNodes)
            {
                // 跳过开始和结束节点
                if (node is StartNode || node is EndNode || node is CommentNode)
                    continue;

                var model = node.ToChildModel(stepNum);

                // 处理条件节点的跳转设置
                if (node is ConditionNode conditionNode)
                {
                    ProcessConditionNodeJumps(conditionNode, orderedNodes, model);
                }

                // 处理循环节点
                if (node is LoopNode loopNode)
                {
                    ProcessLoopNodeJumps(loopNode, orderedNodes, model);
                }

                result.Add(model);
                stepNum++;
            }

            return result;
        }

        /// <summary>
        /// 使用拓扑排序转换节点
        /// </summary>
        private List<ChildModel> ConvertUsingTopologicalSort(List<WorkflowNodeBase> nodes)
        {
            var result = new List<ChildModel>();

            // 简单的拓扑排序实现
            var sorted = TopologicalSort(nodes);

            int stepNum = 1;
            foreach (var node in sorted)
            {
                if (node is StartNode || node is EndNode || node is CommentNode)
                    continue;

                result.Add(node.ToChildModel(stepNum));
                stepNum++;
            }

            return result;
        }

        /// <summary>
        /// 拓扑排序
        /// </summary>
        private List<WorkflowNodeBase> TopologicalSort(List<WorkflowNodeBase> nodes)
        {
            var inDegree = new Dictionary<WorkflowNodeBase, int>();
            var adjacency = new Dictionary<WorkflowNodeBase, List<WorkflowNodeBase>>();

            // 初始化
            foreach (var node in nodes)
            {
                inDegree[node] = 0;
                adjacency[node] = new List<WorkflowNodeBase>();
            }

            // 计算入度和邻接表
            foreach (var node in nodes)
            {
                var successors = GetSuccessorNodes(node);
                foreach (var successor in successors)
                {
                    if (nodes.Contains(successor))
                    {
                        adjacency[node].Add(successor);
                        inDegree[successor]++;
                    }
                }
            }

            // Kahn算法
            var queue = new Queue<WorkflowNodeBase>();
            foreach (var node in nodes.Where(n => inDegree[n] == 0))
            {
                queue.Enqueue(node);
            }

            var result = new List<WorkflowNodeBase>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);

                foreach (var successor in adjacency[node])
                {
                    inDegree[successor]--;
                    if (inDegree[successor] == 0)
                    {
                        queue.Enqueue(successor);
                    }
                }
            }

            // 如果结果数量不等于节点数量，说明有循环
            if (result.Count != nodes.Count)
            {
                // 返回原始顺序（按位置）
                return nodes.OrderBy(n => n.Top).ThenBy(n => n.Left).ToList();
            }

            return result;
        }

        /// <summary>
        /// 从节点开始遍历
        /// </summary>
        private void TraverseFromNode(WorkflowNodeBase node, HashSet<Guid> visited,
            List<WorkflowNodeBase> orderedNodes, List<WorkflowNodeBase> allNodes)
        {
            if (node == null || visited.Contains(node.NodeId))
                return;

            visited.Add(node.NodeId);
            orderedNodes.Add(node);

            // 获取后续节点
            var successors = GetSuccessorNodes(node);
            foreach (var successor in successors)
            {
                TraverseFromNode(successor, visited, orderedNodes, allNodes);
            }
        }

        /// <summary>
        /// 获取节点的后续节点
        /// </summary>
        private List<WorkflowNodeBase> GetSuccessorNodes(WorkflowNodeBase node)
        {
            // 使用节点提供的公共方法，而不是直接访问 OutputOptions
            return node.GetConnectedNodesFromOutput(0);
        }

        /// <summary>
        /// 处理条件节点的跳转设置
        /// </summary>
        private void ProcessConditionNodeJumps(ConditionNode conditionNode,
            List<WorkflowNodeBase> orderedNodes, ChildModel model)
        {
            // 获取 True 分支目标
            var trueTarget = conditionNode.GetTrueBranchNode();
            var falseTarget = conditionNode.GetFalseBranchNode();

            // 计算跳转步骤号
            if (model.StepParameter is Parameter.Parameter_Detection param)
            {
                if (param.ResultHandling == null)
                {
                    param.ResultHandling = new Parameter.ResultHandling();
                }

                // 计算 True 分支跳转
                if (trueTarget != null)
                {
                    int trueIndex = orderedNodes.IndexOf(trueTarget);
                    param.ResultHandling.SuccessJumpStep = trueIndex >= 0 ? trueIndex : -1;
                }

                // 计算 False 分支跳转
                if (falseTarget != null)
                {
                    int falseIndex = orderedNodes.IndexOf(falseTarget);
                    param.ResultHandling.FailureJumpStep = falseIndex >= 0 ? falseIndex : -1;
                    param.ResultHandling.OnFailure = Parameter.FailureAction.JumpToStep;
                }
            }
        }

        /// <summary>
        /// 处理循环节点的跳转设置
        /// </summary>
        private void ProcessLoopNodeJumps(LoopNode loopNode,
            List<WorkflowNodeBase> orderedNodes, ChildModel model)
        {
            // 循环节点的子步骤需要特殊处理
            // 在执行时动态处理
        }

        #endregion

        #region ChildModel 列表 → 图

        /// <summary>
        /// 从 ChildModel 列表创建节点图
        /// </summary>
        public void LoadFromChildModels(List<ChildModel> childModels)
        {
            if (childModels == null || childModels.Count == 0)
                return;

            // 清空当前画布
            _editor.Nodes.Clear();

            // 创建节点
            var nodeMap = new Dictionary<int, WorkflowNodeBase>();
            int x = 100;
            int y = 100;
            int yStep = 100;

            // 添加开始节点
            var startNode = new StartNode
            {
                Left = x,
                Top = y
            };
            _editor.Nodes.Add(startNode);

            y += yStep;

            // 创建工作流节点
            foreach (var model in childModels)
            {
                var node = WorkflowNodeFactory.CreateNodeFromChildModel(model);
                if (node != null)
                {
                    node.Left = x;
                    node.Top = y;
                    _editor.Nodes.Add(node);
                    nodeMap[model.StepNum] = node;
                    y += yStep;
                }
            }

            // 添加结束节点
            var endNode = new EndNode
            {
                Left = x,
                Top = y
            };
            _editor.Nodes.Add(endNode);

            // 创建连接
            CreateConnectionsFromModels(childModels, nodeMap, startNode, endNode);

            // 自动布局
            AutoLayoutNodes();
        }

        /// <summary>
        /// 根据模型创建连接
        /// </summary>
        private void CreateConnectionsFromModels(List<ChildModel> models,
            Dictionary<int, WorkflowNodeBase> nodeMap,
            StartNode startNode, EndNode endNode)
        {
            // 连接开始节点到第一个步骤
            if (models.Count > 0 && nodeMap.TryGetValue(models[0].StepNum, out var firstNode))
            {
                ConnectNodes(startNode, firstNode);
            }

            // 连接步骤节点
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                if (!nodeMap.TryGetValue(model.StepNum, out var currentNode))
                    continue;

                // 默认连接到下一个节点
                if (i < models.Count - 1)
                {
                    if (nodeMap.TryGetValue(models[i + 1].StepNum, out var nextNode))
                    {
                        // 特殊处理条件节点
                        if (currentNode is ConditionNode condNode)
                        {
                            // True 分支连接到下一个节点
                            ConnectConditionNode(condNode, true, nextNode);

                            // False 分支根据配置连接
                            // TODO: 从参数中读取跳转目标
                        }
                        else
                        {
                            ConnectNodes(currentNode, nextNode);
                        }
                    }
                }
                else
                {
                    // 最后一个节点连接到结束
                    ConnectNodes(currentNode, endNode);
                }
            }
        }

        /// <summary>
        /// 连接两个节点
        /// </summary>
        private void ConnectNodes(WorkflowNodeBase from, WorkflowNodeBase to)
        {
            from.ConnectTo(to);
        }

        /// <summary>
        /// 连接条件节点
        /// </summary>
        private void ConnectConditionNode(ConditionNode condNode, bool isTrueBranch, WorkflowNodeBase target)
        {
            if (target?.InputExecution == null) return;

            if (isTrueBranch)
            {
                // True 分支 - 使用第一个输出
                condNode.OutputTrue?.ConnectOption(target.InputExecution);
            }
            else
            {
                // False 分支 - 使用第二个输出
                condNode.OutputFalse?.ConnectOption(target.InputExecution);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取所有工作流节点
        /// </summary>
        public List<WorkflowNodeBase> GetAllWorkflowNodes()
        {
            var result = new List<WorkflowNodeBase>();

            foreach (STNode node in _editor.Nodes)
            {
                if (node is WorkflowNodeBase workflowNode)
                {
                    result.Add(workflowNode);
                }
            }

            return result;
        }

        /// <summary>
        /// 自动布局节点
        /// </summary>
        public void AutoLayoutNodes()
        {
            var nodes = GetAllWorkflowNodes();
            if (nodes.Count == 0) return;

            // 简单的垂直布局
            int x = 150;
            int y = 50;
            int yStep = 100;

            // 查找开始节点
            var startNode = nodes.FirstOrDefault(n => n is StartNode);
            if (startNode != null)
            {
                startNode.Left = x;
                startNode.Top = y;
                y += yStep;
            }

            // 其他节点
            foreach (var node in nodes)
            {
                if (node is StartNode || node is EndNode)
                    continue;

                // 条件节点需要特殊处理
                if (node is ConditionNode)
                {
                    node.Left = x;
                    node.Top = y;
                    y += yStep + 30; // 条件节点后留更多空间
                }
                else if (node is LoopNode)
                {
                    node.Left = x;
                    node.Top = y;
                    y += yStep + 30;
                }
                else
                {
                    node.Left = x;
                    node.Top = y;
                    y += yStep;
                }
            }

            // 结束节点放在最后
            var endNode = nodes.FirstOrDefault(n => n is EndNode);
            if (endNode != null)
            {
                endNode.Left = x;
                endNode.Top = y;
            }
        }

        /// <summary>
        /// 验证工作流图的有效性
        /// </summary>
        public ValidationResult ValidateGraph()
        {
            var result = new ValidationResult();
            var nodes = GetAllWorkflowNodes();

            // 检查是否有开始节点
            var startNodes = nodes.Where(n => n is StartNode).ToList();
            if (startNodes.Count == 0)
            {
                result.AddError("工作流缺少开始节点");
            }
            else if (startNodes.Count > 1)
            {
                result.AddWarning("工作流有多个开始节点，只有第一个会生效");
            }

            // 检查是否有结束节点
            var endNodes = nodes.Where(n => n is EndNode).ToList();
            if (endNodes.Count == 0)
            {
                result.AddWarning("工作流没有明确的结束节点");
            }

            // 检查孤立节点
            foreach (var node in nodes)
            {
                if (node is StartNode || node is CommentNode)
                    continue;

                bool hasInput = false;
                for (int i = 0; i < node.InputOptionsCount; i++)
                {
                    if (node.InputExecution.ConnectionCount > 0)
                    {
                        hasInput = true;
                        break;
                    }
                }

                if (!hasInput)
                {
                    result.AddWarning($"节点 [{node.DisplayName}] 没有输入连接");
                }
            }

            // 检查未配置的节点
            foreach (var node in nodes)
            {
                if (node is StartNode || node is EndNode || node is CommentNode)
                    continue;

                if (!node.IsConfigured)
                {
                    result.AddWarning($"节点 [{node.DisplayName}] 尚未配置");
                }
            }

            return result;
        }

        #endregion
    }

    #region 验证结果类

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public List<ValidationMessage> Messages { get; } = [];

        public bool HasErrors => Messages.Any(m => m.Level == ValidationLevel.Error);
        public bool HasWarnings => Messages.Any(m => m.Level == ValidationLevel.Warning);
        public bool IsValid => !HasErrors;

        public void AddError(string message)
        {
            Messages.Add(new ValidationMessage(ValidationLevel.Error, message));
        }

        public void AddWarning(string message)
        {
            Messages.Add(new ValidationMessage(ValidationLevel.Warning, message));
        }

        public void AddInfo(string message)
        {
            Messages.Add(new ValidationMessage(ValidationLevel.Info, message));
        }
    }

    /// <summary>
    /// 验证消息
    /// </summary>
    public class ValidationMessage(ValidationLevel level, string message)
    {
        public ValidationLevel Level { get; } = level;

        public string Message { get; } = message;
    }

    /// <summary>
    /// 验证级别
    /// </summary>
    public enum ValidationLevel
    {
        Info,
        Warning,
        Error
    }

    #endregion
}
