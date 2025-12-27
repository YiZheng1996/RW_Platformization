using MainUI.LogicalConfiguration.NodeEditor.Core;
using Newtonsoft.Json;

namespace MainUI.LogicalConfiguration.NodeEditor.Services
{
    /// <summary>
    /// 工作流文档 - 管理节点和连接的集合
    /// </summary>
    [Serializable]
    public class WorkflowDocument
    {
        #region 属性

        /// <summary>
        /// 文档唯一标识
        /// </summary>
        public Guid DocumentId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 文档名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        public string ModelTypeName { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 节点集合
        /// </summary>
        public List<WorkflowNodeBase> Nodes { get; set; } = [];

        /// <summary>
        /// 连接集合
        /// </summary>
        public List<NodeConnection> Connections { get; set; } = [];

        /// <summary>
        /// 文档版本
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 视图偏移量（用于保存/恢复视图位置）
        /// </summary>
        public Point ViewOffset { get; set; }

        /// <summary>
        /// 视图缩放比例
        /// </summary>
        public float ZoomLevel { get; set; } = 1.0f;

        #endregion

        #region 节点操作

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(WorkflowNodeBase node)
        {
            if (node == null) return;

            // 确保节点ID唯一
            if (Nodes.Any(n => n.NodeId == node.NodeId))
            {
                node.NodeId = Guid.NewGuid();
            }

            Nodes.Add(node);
            LastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// 移除节点及其相关连接
        /// </summary>
        public bool RemoveNode(WorkflowNodeBase node)
        {
            if (node == null) return false;

            // 移除相关连接
            Connections.RemoveAll(c =>
                c.SourceNodeId == node.NodeId || c.TargetNodeId == node.NodeId);

            var result = Nodes.Remove(node);

            if (result)
            {
                LastModifiedTime = DateTime.Now;
            }

            return result;
        }

        /// <summary>
        /// 根据ID移除节点
        /// </summary>
        public bool RemoveNodeById(Guid nodeId)
        {
            var node = GetNodeById(nodeId);
            return node != null && RemoveNode(node);
        }

        /// <summary>
        /// 根据ID获取节点
        /// </summary>
        public WorkflowNodeBase GetNodeById(Guid nodeId)
        {
            return Nodes.FirstOrDefault(n => n.NodeId == nodeId);
        }

        /// <summary>
        /// 获取开始节点
        /// </summary>
        public WorkflowNodeBase GetStartNode()
        {
            return Nodes.FirstOrDefault(n => n.NodeType == "Start");
        }

        /// <summary>
        /// 获取结束节点
        /// </summary>
        public WorkflowNodeBase GetEndNode()
        {
            return Nodes.FirstOrDefault(n => n.NodeType == "End");
        }

        /// <summary>
        /// 清除所有节点和连接
        /// </summary>
        public void Clear()
        {
            Nodes.Clear();
            Connections.Clear();
            LastModifiedTime = DateTime.Now;
        }

        #endregion

        #region 连接操作

        /// <summary>
        /// 添加连接
        /// </summary>
        public bool AddConnection(NodeConnection connection)
        {
            if (connection == null) return false;

            // 验证连接的源和目标节点是否存在
            var sourceNode = GetNodeById(connection.SourceNodeId);
            var targetNode = GetNodeById(connection.TargetNodeId);

            if (sourceNode == null || targetNode == null)
                return false;

            // 检查是否已存在相同连接
            if (Connections.Any(c =>
                c.SourceNodeId == connection.SourceNodeId &&
                c.SourceSocketId == connection.SourceSocketId &&
                c.TargetNodeId == connection.TargetNodeId &&
                c.TargetSocketId == connection.TargetSocketId))
            {
                return false;
            }

            // 检查目标端口是否已有连接（如果不允许多连接）
            var targetSocket = targetNode.GetSocketById(connection.TargetSocketId);
            if (targetSocket != null && !targetSocket.AllowMultipleConnections)
            {
                // 移除现有连接
                Connections.RemoveAll(c =>
                    c.TargetNodeId == connection.TargetNodeId &&
                    c.TargetSocketId == connection.TargetSocketId);
            }

            Connections.Add(connection);
            LastModifiedTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 创建并添加连接
        /// </summary>
        public NodeConnection CreateConnection(
            WorkflowNodeBase sourceNode, NodeSocket sourceSocket,
            WorkflowNodeBase targetNode, NodeSocket targetSocket)
        {
            if (sourceNode == null || targetNode == null ||
                sourceSocket == null || targetSocket == null)
                return null;

            // 验证端口类型
            if (sourceSocket.Type != SocketType.Output || targetSocket.Type != SocketType.Input)
                return null;

            // 不允许自连接
            if (sourceNode.NodeId == targetNode.NodeId)
                return null;

            var connection = new NodeConnection
            {
                SourceNodeId = sourceNode.NodeId,
                SourceSocketId = sourceSocket.Id,
                TargetNodeId = targetNode.NodeId,
                TargetSocketId = targetSocket.Id,
                LineColor = sourceSocket.Color
            };

            if (AddConnection(connection))
            {
                return connection;
            }

            return null;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        public bool RemoveConnection(NodeConnection connection)
        {
            if (connection == null) return false;

            var result = Connections.Remove(connection);

            if (result)
            {
                LastModifiedTime = DateTime.Now;
            }

            return result;
        }

        /// <summary>
        /// 根据ID移除连接
        /// </summary>
        public bool RemoveConnectionById(Guid connectionId)
        {
            var connection = Connections.FirstOrDefault(c => c.Id == connectionId);
            return connection != null && RemoveConnection(connection);
        }

        /// <summary>
        /// 获取节点的所有输入连接
        /// </summary>
        public IEnumerable<NodeConnection> GetInputConnections(Guid nodeId)
        {
            return Connections.Where(c => c.TargetNodeId == nodeId);
        }

        /// <summary>
        /// 获取节点的所有输出连接
        /// </summary>
        public IEnumerable<NodeConnection> GetOutputConnections(Guid nodeId)
        {
            return Connections.Where(c => c.SourceNodeId == nodeId);
        }

        /// <summary>
        /// 获取指定端口的连接
        /// </summary>
        public IEnumerable<NodeConnection> GetSocketConnections(Guid socketId)
        {
            return Connections.Where(c =>
                c.SourceSocketId == socketId || c.TargetSocketId == socketId);
        }

        #endregion

        #region 更新连接位置

        /// <summary>
        /// 更新所有连接的位置（在节点移动后调用）
        /// </summary>
        public void UpdateConnectionPositions()
        {
            foreach (var connection in Connections)
            {
                UpdateConnectionPosition(connection);
            }
        }

        /// <summary>
        /// 更新单个连接的位置
        /// </summary>
        private void UpdateConnectionPosition(NodeConnection connection)
        {
            var sourceNode = GetNodeById(connection.SourceNodeId);
            var targetNode = GetNodeById(connection.TargetNodeId);

            if (sourceNode == null || targetNode == null) return;

            var sourceSocket = sourceNode.GetSocketById(connection.SourceSocketId);
            var targetSocket = targetNode.GetSocketById(connection.TargetSocketId);

            if (sourceSocket == null || targetSocket == null) return;

            connection.StartPoint = sourceSocket.GetConnectionPoint();
            connection.EndPoint = targetSocket.GetConnectionPoint();
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 获取指定位置的节点
        /// </summary>
        public WorkflowNodeBase GetNodeAt(Point point)
        {
            // 逆序遍历，优先返回上层节点
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                if (Nodes[i].ContainsPoint(point))
                    return Nodes[i];
            }
            return null;
        }

        /// <summary>
        /// 获取指定位置的端口
        /// </summary>
        public (WorkflowNodeBase Node, NodeSocket Socket) GetSocketAt(Point point)
        {
            foreach (var node in Nodes)
            {
                var socket = node.GetSocketAt(point);
                if (socket != null)
                    return (node, socket);
            }
            return (null, null);
        }

        /// <summary>
        /// 获取指定位置的连接
        /// </summary>
        public NodeConnection GetConnectionAt(Point point)
        {
            return Connections.FirstOrDefault(c => c.HitTest(point));
        }

        /// <summary>
        /// 获取矩形区域内的所有节点
        /// </summary>
        public IEnumerable<WorkflowNodeBase> GetNodesInRect(Rectangle rect)
        {
            return Nodes.Where(n => rect.IntersectsWith(n.Bounds));
        }

        /// <summary>
        /// 获取所有节点的边界矩形
        /// </summary>
        public Rectangle GetBoundingRect()
        {
            if (Nodes.Count == 0)
                return Rectangle.Empty;

            int minX = Nodes.Min(n => n.Location.X);
            int minY = Nodes.Min(n => n.Location.Y);
            int maxX = Nodes.Max(n => n.Location.X + n.Width);
            int maxY = Nodes.Max(n => n.Location.Y + n.Height);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        #endregion

        #region 序列化

        /// <summary>
        /// 序列化为JSON
        /// </summary>
        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// 从JSON反序列化
        /// </summary>
        public static WorkflowDocument FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new WorkflowDocument();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonConvert.DeserializeObject<WorkflowDocument>(json, settings);
        }

        #endregion

        #region 验证

        /// <summary>
        /// 验证文档是否有效
        /// </summary>
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            // 检查是否有节点
            if (Nodes.Count == 0)
            {
                errors.Add("工作流没有任何节点");
                return false;
            }

            // 检查是否有开始节点
            var startNodes = Nodes.Count(n => n.NodeType == "Start");
            if (startNodes == 0)
                errors.Add("缺少开始节点");
            else if (startNodes > 1)
                errors.Add("存在多个开始节点");

            // 检查是否有结束节点
            var endNodes = Nodes.Count(n => n.NodeType == "End");
            if (endNodes == 0)
                errors.Add("缺少结束节点");

            // 检查孤立节点
            foreach (var node in Nodes)
            {
                if (node.NodeType == "Start") continue;

                var hasInput = Connections.Any(c => c.TargetNodeId == node.NodeId);
                if (!hasInput)
                {
                    errors.Add($"节点 [{node.DisplayName}] 没有输入连接");
                }
            }

            return errors.Count == 0;
        }

        #endregion
    }
}
