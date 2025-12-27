using MainUI.LogicalConfiguration.NodeEditor.Converters;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using Microsoft.Extensions.Logging;

namespace MainUI.LogicalConfiguration.NodeEditor.Services
{
    /// <summary>
    /// 节点执行适配器 - 将节点模型适配到执行引擎
    /// </summary>
    public class NodeExecutionAdapter
    {
        #region 事件

        /// <summary>
        /// 节点状态变化事件
        /// </summary>
        public event EventHandler<NodeStatusChangedEventArgs> NodeStatusChanged;

        /// <summary>
        /// 执行进度事件
        /// </summary>
        public event EventHandler<ExecutionProgressEventArgs> ExecutionProgress;

        #endregion

        #region 字段

        private readonly IStepExecutionService _executionService;
        private readonly WorkflowConverter _converter;
        private readonly ILogger<NodeExecutionAdapter> _logger;

        private readonly Dictionary<Guid, WorkflowNodeBase> _nodeMap = new();
        private readonly Dictionary<int, Guid> _stepToNodeMap = new();

        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        #endregion

        #region 属性

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRunning => _isRunning;

        #endregion

        #region 构造函数

        public NodeExecutionAdapter(
            IStepExecutionService executionService,
            ILogger<NodeExecutionAdapter> logger = null)
        {
            _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
            _converter = new WorkflowConverter();
            _logger = logger;

            // 订阅执行服务的状态变化事件
            _executionService.StepStatusChanged += OnStepStatusChanged;
        }

        #endregion

        #region 执行方法

        /// <summary>
        /// 执行节点工作流
        /// </summary>
        public async Task<ExecutionResult> ExecuteAsync(
            List<WorkflowNodeBase> nodes,
            List<NodeConnection> connections,
            CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                return ExecutionResult.Failed("工作流正在执行中");
            }

            try
            {
                _isRunning = true;
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // 重置所有节点状态
                ResetNodeStatus(nodes);

                // 构建节点映射
                BuildNodeMaps(nodes);

                // 转换为 ChildModel
                _logger?.LogInformation("开始转换节点到步骤...");
                var childModels = _converter.ConvertNodesToChildModels(nodes, connections);

                if (childModels.Count == 0)
                {
                    return ExecutionResult.Failed("没有可执行的步骤");
                }

                // 构建步骤到节点的映射
                BuildStepToNodeMap(childModels, nodes);

                // 执行工作流
                _logger?.LogInformation("开始执行工作流，共 {Count} 个步骤", childModels.Count);
                var result = await _executionService.ExecuteWorkflowAsync(childModels, _cts.Token);

                return new ExecutionResult
                {
                    Success = result.Success,
                    ErrorMessage = result.ErrorMessage,
                    ExecutedSteps = result.ExecutedSteps,
                    TotalSteps = result.TotalSteps,
                    TotalExecutionTime = result.TotalExecutionTime
                };
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("工作流执行被取消");
                return ExecutionResult.Cancelleds();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "工作流执行失败");
                return ExecutionResult.Failed(ex.Message);
            }
            finally
            {
                _isRunning = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// 取消执行
        /// </summary>
        public void Cancel()
        {
            _cts?.Cancel();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 重置节点状态
        /// </summary>
        private void ResetNodeStatus(List<WorkflowNodeBase> nodes)
        {
            foreach (var node in nodes)
            {
                node.ExecutionStatus = NodeExecutionStatus.Pending;
                node.ErrorMessage = null;
            }
        }

        /// <summary>
        /// 构建节点映射
        /// </summary>
        private void BuildNodeMaps(List<WorkflowNodeBase> nodes)
        {
            _nodeMap.Clear();
            foreach (var node in nodes)
            {
                _nodeMap[node.NodeId] = node;
            }
        }

        /// <summary>
        /// 构建步骤到节点的映射
        /// </summary>
        private void BuildStepToNodeMap(List<ChildModel> steps, List<WorkflowNodeBase> nodes)
        {
            _stepToNodeMap.Clear();

            // 简单映射：按顺序对应（跳过开始/结束节点）
            int stepIndex = 0;
            foreach (var node in nodes)
            {
                if (node.NodeType == "Start" || node.NodeType == "End")
                    continue;

                if (stepIndex < steps.Count)
                {
                    _stepToNodeMap[steps[stepIndex].StepNum] = node.NodeId;
                    stepIndex++;
                }
            }
        }

        /// <summary>
        /// 处理步骤状态变化
        /// </summary>
        private void OnStepStatusChanged(object sender, StepStatusChangedEventArgs e)
        {
            // 查找对应的节点
            if (_stepToNodeMap.TryGetValue(e.StepNum, out var nodeId) &&
                _nodeMap.TryGetValue(nodeId, out var node))
            {
                // 更新节点状态
                node.ExecutionStatus = e.Status switch
                {
                    StepStatus.Running => NodeExecutionStatus.Running,
                    StepStatus.Completed => NodeExecutionStatus.Completed,
                    StepStatus.Failed => NodeExecutionStatus.Failed,
                    StepStatus.Skipped => NodeExecutionStatus.Skipped,
                    _ => NodeExecutionStatus.Pending
                };

                if (e.Status == StepStatus.Failed)
                {
                    node.ErrorMessage = e.Message;
                }

                // 触发节点状态变化事件
                NodeStatusChanged?.Invoke(this, new NodeStatusChangedEventArgs
                {
                    NodeId = nodeId,
                    Node = node,
                    Status = node.ExecutionStatus,
                    Message = e.Message
                });
            }

            // 触发进度事件
            ExecutionProgress?.Invoke(this, new ExecutionProgressEventArgs
            {
                CurrentStep = e.StepNum,
                StepName = e.StepName,
                Status = e.Status,
                Message = e.Message
            });
        }

        #endregion
    }

    /// <summary>
    /// 执行结果
    /// </summary>
    public class ExecutionResult
    {
        public bool Success { get; set; }
        public bool Cancelled { get; set; }
        public string ErrorMessage { get; set; }
        public int ExecutedSteps { get; set; }
        public int TotalSteps { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }

        public static ExecutionResult Successful(int executedSteps = 0, int totalSteps = 0)
        {
            return new ExecutionResult
            {
                Success = true,
                ExecutedSteps = executedSteps,
                TotalSteps = totalSteps
            };
        }

        public static ExecutionResult Failed(string message)
        {
            return new ExecutionResult
            {
                Success = false,
                ErrorMessage = message
            };
        }

        public static ExecutionResult Cancelleds()
        {
            return new ExecutionResult
            {
                Success = false,
                Cancelled = true,
                ErrorMessage = "执行已取消"
            };
        }
    }

    /// <summary>
    /// 节点状态变化事件参数
    /// </summary>
    public class NodeStatusChangedEventArgs : EventArgs
    {
        public Guid NodeId { get; set; }
        public WorkflowNodeBase Node { get; set; }
        public NodeExecutionStatus Status { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 执行进度事件参数
    /// </summary>
    public class ExecutionProgressEventArgs : EventArgs
    {
        public int CurrentStep { get; set; }
        public string StepName { get; set; }
        public StepStatus Status { get; set; }
        public string Message { get; set; }
        public double ProgressPercent { get; set; }
    }
}
