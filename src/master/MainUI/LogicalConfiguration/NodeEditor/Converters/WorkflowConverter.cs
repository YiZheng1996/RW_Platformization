using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.Parameter;
using Microsoft.Extensions.Logging;

namespace MainUI.LogicalConfiguration.NodeEditor.Converters
{
    /// <summary>
    /// 工作流转换器 - 实现节点模型与 ChildModel 的双向转换
    /// </summary>
    public class WorkflowConverter
    {
        private readonly ILogger<WorkflowConverter> _logger;

        public WorkflowConverter(ILogger<WorkflowConverter> logger = null)
        {
            _logger = logger;
        }

        #region Node → ChildModel 转换

        /// <summary>
        /// 将节点列表转换为 ChildModel 列表
        /// </summary>
        public List<ChildModel> ConvertNodesToChildModels(
            List<WorkflowNodeBase> nodes,
            List<NodeConnection> connections)
        {
            var result = new List<ChildModel>();

            try
            {
                // 拓扑排序，确定执行顺序
                var orderedNodes = TopologicalSort(nodes, connections);

                int stepNum = 1;
                var nodeToStepMap = new Dictionary<Guid, int>();

                // 第一轮：分配步骤号
                foreach (var node in orderedNodes)
                {
                    // 跳过开始/结束节点
                    if (node is StartNode || node is EndNode)
                        continue;

                    nodeToStepMap[node.NodeId] = stepNum++;
                }

                // 第二轮：创建 ChildModel
                stepNum = 1;
                foreach (var node in orderedNodes)
                {
                    if (node is StartNode || node is EndNode)
                        continue;

                    var childModel = new ChildModel
                    {
                        StepNum = stepNum++,
                        StepName = node.NodeType,
                        Remark = node.Remark,
                        StepParameter = CloneParameter(node.Parameter),
                        Status = 0
                    };

                    // 处理条件判断的跳转逻辑
                    if (node is ConditionNode conditionNode)
                    {
                        SetConditionJumpTargets(conditionNode, connections, nodeToStepMap, childModel);
                    }

                    // 处理循环节点的子步骤
                    if (node is LoopNode loopNode && loopNode.ChildNodes.Any())
                    {
                        if (childModel.StepParameter is Parameter_Loop loopParam)
                        {
                            loopParam.ChildSteps = ConvertNodesToChildModels(
                                loopNode.ChildNodes,
                                loopNode.ChildConnections);
                        }
                    }

                    result.Add(childModel);
                }

                _logger?.LogInformation("节点转换完成，共 {Count} 个步骤", result.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "节点转换为 ChildModel 失败");
            }

            return result;
        }

        /// <summary>
        /// 设置条件判断的跳转目标
        /// </summary>
        private void SetConditionJumpTargets(
            ConditionNode conditionNode,
            List<NodeConnection> connections,
            Dictionary<Guid, int> nodeToStepMap,
            ChildModel childModel)
        {
            if (!(childModel.StepParameter is Parameter_Detection param))
                return;

            if (param.ResultHandling == null)
                param.ResultHandling = new ResultHandling();

            // 查找 True 分支连接
            var trueSocket = conditionNode.OutputSockets.FirstOrDefault(s => s.Name == "True");
            if (trueSocket != null)
            {
                var trueConnection = connections.FirstOrDefault(c =>
                    c.SourceNodeId == conditionNode.NodeId &&
                    c.SourceSocketId == trueSocket.Id);

                if (trueConnection != null && nodeToStepMap.TryGetValue(trueConnection.TargetNodeId, out int trueStep))
                {
                    param.ResultHandling.SuccessJumpStep = trueStep;
                }
            }

            // 查找 False 分支连接
            var falseSocket = conditionNode.OutputSockets.FirstOrDefault(s => s.Name == "False");
            if (falseSocket != null)
            {
                var falseConnection = connections.FirstOrDefault(c =>
                    c.SourceNodeId == conditionNode.NodeId &&
                    c.SourceSocketId == falseSocket.Id);

                if (falseConnection != null && nodeToStepMap.TryGetValue(falseConnection.TargetNodeId, out int falseStep))
                {
                    param.ResultHandling.FailureJumpStep = falseStep;
                    param.ResultHandling.OnFailure = FailureAction.JumpToStep;
                }
            }
        }

        #endregion

        #region ChildModel → Node 转换

        /// <summary>
        /// 将 ChildModel 列表转换为节点和连接
        /// </summary>
        public (List<WorkflowNodeBase> Nodes, List<NodeConnection> Connections)
            ConvertChildModelsToNodes(List<ChildModel> childModels)
        {
            var nodes = new List<WorkflowNodeBase>();
            var connections = new List<NodeConnection>();

            try
            {
                if (childModels == null || childModels.Count == 0)
                {
                    return AddDefaultNodes(nodes, connections);
                }

                // 添加开始节点
                var startNode = new StartNode
                {
                    Location = new Point(50, 200)
                };
                nodes.Add(startNode);

                // 转换步骤为节点
                int xOffset = 200;
                int yOffset = 50;
                int maxX = 800;
                var stepToNodeMap = new Dictionary<int, WorkflowNodeBase>();

                foreach (var step in childModels)
                {
                    var node = Services.NodeFactory.CreateFromChildModel(step);
                    node.Location = new Point(xOffset, yOffset);
                    nodes.Add(node);

                    stepToNodeMap[step.StepNum] = node;

                    xOffset += 200;
                    if (xOffset > maxX)
                    {
                        xOffset = 200;
                        yOffset += 120;
                    }
                }

                // 添加结束节点
                var endNode = new EndNode
                {
                    Location = new Point(xOffset, yOffset)
                };
                nodes.Add(endNode);

                // 创建连接
                CreateConnections(nodes, connections, childModels, stepToNodeMap, startNode, endNode);

                _logger?.LogInformation("ChildModel 转换完成，{NodeCount} 个节点，{ConnCount} 个连接",
                    nodes.Count, connections.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ChildModel 转换为节点失败");
                return AddDefaultNodes(nodes, connections);
            }

            return (nodes, connections);
        }

        /// <summary>
        /// 创建节点间的连接
        /// </summary>
        private void CreateConnections(
            List<WorkflowNodeBase> nodes,
            List<NodeConnection> connections,
            List<ChildModel> childModels,
            Dictionary<int, WorkflowNodeBase> stepToNodeMap,
            WorkflowNodeBase startNode,
            WorkflowNodeBase endNode)
        {
            WorkflowNodeBase previousNode = startNode;

            for (int i = 0; i < childModels.Count; i++)
            {
                var step = childModels[i];
                if (!stepToNodeMap.TryGetValue(step.StepNum, out var currentNode))
                    continue;

                // 从前一个节点连接到当前节点
                if (previousNode != null)
                {
                    // 条件节点需要特殊处理
                    if (previousNode is ConditionNode prevCondition)
                    {
                        // 条件节点的连接由参数决定，这里不自动创建
                    }
                    else
                    {
                        var outSocket = previousNode.OutputSockets.FirstOrDefault();
                        var inSocket = currentNode.InputSockets.FirstOrDefault();

                        if (outSocket != null && inSocket != null)
                        {
                            connections.Add(new NodeConnection
                            {
                                SourceNodeId = previousNode.NodeId,
                                SourceSocketId = outSocket.Id,
                                TargetNodeId = currentNode.NodeId,
                                TargetSocketId = inSocket.Id,
                                LineColor = Color.FromArgb(150, 150, 150)
                            });
                        }
                    }
                }

                // 处理条件节点的跳转连接
                if (currentNode is ConditionNode conditionNode && step.StepParameter is Parameter_Detection param)
                {
                    CreateConditionConnections(conditionNode, param, stepToNodeMap, connections, endNode);
                }

                previousNode = currentNode;
            }

            // 连接最后一个非条件节点到结束节点
            if (previousNode != null && !(previousNode is ConditionNode))
            {
                var outSocket = previousNode.OutputSockets.FirstOrDefault();
                var inSocket = endNode.InputSockets.FirstOrDefault();

                if (outSocket != null && inSocket != null)
                {
                    connections.Add(new NodeConnection
                    {
                        SourceNodeId = previousNode.NodeId,
                        SourceSocketId = outSocket.Id,
                        TargetNodeId = endNode.NodeId,
                        TargetSocketId = inSocket.Id,
                        LineColor = Color.FromArgb(150, 150, 150)
                    });
                }
            }
        }

        /// <summary>
        /// 创建条件节点的跳转连接
        /// </summary>
        private void CreateConditionConnections(
            ConditionNode conditionNode,
            Parameter_Detection param,
            Dictionary<int, WorkflowNodeBase> stepToNodeMap,
            List<NodeConnection> connections,
            WorkflowNodeBase endNode)
        {
            if (param.ResultHandling == null) return;

            // True 分支
            var trueSocket = conditionNode.OutputSockets.FirstOrDefault(s => s.Name == "True");
            if (trueSocket != null && param.ResultHandling.SuccessJumpStep > 0)
            {
                if (stepToNodeMap.TryGetValue(param.ResultHandling.SuccessJumpStep, out var trueTarget))
                {
                    var targetInSocket = trueTarget.InputSockets.FirstOrDefault();
                    if (targetInSocket != null)
                    {
                        connections.Add(new NodeConnection
                        {
                            SourceNodeId = conditionNode.NodeId,
                            SourceSocketId = trueSocket.Id,
                            TargetNodeId = trueTarget.NodeId,
                            TargetSocketId = targetInSocket.Id,
                            LineColor = Color.FromArgb(46, 204, 113),
                            Label = "True"
                        });
                    }
                }
            }

            // False 分支
            var falseSocket = conditionNode.OutputSockets.FirstOrDefault(s => s.Name == "False");
            if (falseSocket != null)
            {
                WorkflowNodeBase falseTarget = null;

                if (param.ResultHandling.OnFailure == FailureAction.JumpToStep &&
                    param.ResultHandling.FailureJumpStep > 0)
                {
                    stepToNodeMap.TryGetValue(param.ResultHandling.FailureJumpStep, out falseTarget);
                }
                else if (param.ResultHandling.OnFailure == FailureAction.Stop)
                {
                    falseTarget = endNode;
                }

                if (falseTarget != null)
                {
                    var targetInSocket = falseTarget.InputSockets.FirstOrDefault();
                    if (targetInSocket != null)
                    {
                        connections.Add(new NodeConnection
                        {
                            SourceNodeId = conditionNode.NodeId,
                            SourceSocketId = falseSocket.Id,
                            TargetNodeId = falseTarget.NodeId,
                            TargetSocketId = targetInSocket.Id,
                            LineColor = Color.FromArgb(231, 76, 60),
                            Label = "False"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 添加默认节点（开始和结束）
        /// </summary>
        private (List<WorkflowNodeBase>, List<NodeConnection>) AddDefaultNodes(
            List<WorkflowNodeBase> nodes, List<NodeConnection> connections)
        {
            var startNode = new StartNode { Location = new Point(100, 200) };
            var endNode = new EndNode { Location = new Point(400, 200) };

            nodes.Add(startNode);
            nodes.Add(endNode);

            return (nodes, connections);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 拓扑排序 - 确定节点执行顺序
        /// </summary>
        private List<WorkflowNodeBase> TopologicalSort(
            List<WorkflowNodeBase> nodes,
            List<NodeConnection> connections)
        {
            var result = new List<WorkflowNodeBase>();
            var visited = new HashSet<Guid>();

            // 从开始节点开始遍历
            var startNode = nodes.FirstOrDefault(n => n is StartNode);
            if (startNode != null)
            {
                DFS(startNode.NodeId, visited, result, nodes, connections);
            }

            // 添加未访问的节点（孤立节点）
            foreach (var node in nodes.Where(n => !visited.Contains(n.NodeId)))
            {
                result.Add(node);
            }

            return result;
        }

        /// <summary>
        /// 深度优先搜索
        /// </summary>
        private void DFS(
            Guid nodeId,
            HashSet<Guid> visited,
            List<WorkflowNodeBase> result,
            List<WorkflowNodeBase> allNodes,
            List<NodeConnection> connections)
        {
            if (visited.Contains(nodeId)) return;

            var node = allNodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node == null) return;

            visited.Add(nodeId);
            result.Add(node);

            // 获取所有从当前节点出发的连接，True 分支优先
            var outConnections = connections
                .Where(c => c.SourceNodeId == nodeId)
                .OrderBy(c =>
                {
                    var socket = node.OutputSockets.FirstOrDefault(s => s.Id == c.SourceSocketId);
                    return socket?.Name == "True" ? 0 : 1;
                });

            foreach (var conn in outConnections)
            {
                DFS(conn.TargetNodeId, visited, result, allNodes, connections);
            }
        }

        /// <summary>
        /// 深拷贝参数对象
        /// </summary>
        private object CloneParameter(object parameter)
        {
            if (parameter == null) return null;

            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameter);
                return Newtonsoft.Json.JsonConvert.DeserializeObject(json, parameter.GetType());
            }
            catch
            {
                return parameter;
            }
        }

        #endregion
    }
}
