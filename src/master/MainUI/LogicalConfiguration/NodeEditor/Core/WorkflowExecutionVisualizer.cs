using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 工作流执行状态可视化器
    /// 在节点编辑器上实时显示执行状态
    /// </summary>
    public class WorkflowExecutionVisualizer : IDisposable
    {
        #region 私有字段

        private readonly STNodeEditor _editor;
        private readonly Dictionary<Guid, NodeExecutionState> _nodeStates;
        private readonly System.Windows.Forms.Timer _animationTimer;
        private int _animationFrame = 0;
        private bool _isDisposed = false;

        #endregion

        #region 属性

        /// <summary>
        /// 当前执行的节点
        /// </summary>
        public WorkflowNodeBase CurrentNode { get; private set; }

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsExecuting { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 执行状态改变事件
        /// </summary>
        public event EventHandler<ExecutionStateChangedEventArgs> ExecutionStateChanged;

        #endregion

        #region 构造函数

        public WorkflowExecutionVisualizer(STNodeEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _nodeStates = new Dictionary<Guid, NodeExecutionState>();

            // 动画定时器
            _animationTimer = new System.Windows.Forms.Timer
            {
                Interval = 100 // 100ms 刷新一次
            };
            _animationTimer.Tick += OnAnimationTick;

            // 订阅编辑器绘制事件
            _editor.Paint += OnEditorPaint;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 开始执行可视化
        /// </summary>
        public void StartExecution()
        {
            IsExecuting = true;
            _animationFrame = 0;
            _animationTimer.Start();

            // 重置所有节点状态
            foreach (var node in GetAllWorkflowNodes())
            {
                SetNodeState(node.NodeId, ExecutionStatus.Pending);
            }

            _editor.Invalidate();
        }

        /// <summary>
        /// 停止执行可视化
        /// </summary>
        public void StopExecution()
        {
            IsExecuting = false;
            _animationTimer.Stop();
            CurrentNode = null;
            _editor.Invalidate();
        }

        /// <summary>
        /// 重置所有状态
        /// </summary>
        public void Reset()
        {
            _nodeStates.Clear();
            CurrentNode = null;
            IsExecuting = false;
            _animationTimer.Stop();
            _editor.Invalidate();
        }

        /// <summary>
        /// 设置节点开始执行
        /// </summary>
        public void SetNodeExecuting(Guid nodeId)
        {
            SetNodeState(nodeId, ExecutionStatus.Executing);
            CurrentNode = FindNodeById(nodeId);

            // 滚动到当前节点
            if (CurrentNode != null)
            {
                ScrollToNode(CurrentNode);
            }

            _editor.Invalidate();
            ExecutionStateChanged?.Invoke(this, new ExecutionStateChangedEventArgs(nodeId, ExecutionStatus.Executing));
        }

        /// <summary>
        /// 设置节点执行完成
        /// </summary>
        public void SetNodeCompleted(Guid nodeId, bool success = true)
        {
            var status = success ? ExecutionStatus.Completed : ExecutionStatus.Failed;
            SetNodeState(nodeId, status);
            _editor.Invalidate();
            ExecutionStateChanged?.Invoke(this, new ExecutionStateChangedEventArgs(nodeId, status));
        }

        /// <summary>
        /// 设置节点被跳过
        /// </summary>
        public void SetNodeSkipped(Guid nodeId)
        {
            SetNodeState(nodeId, ExecutionStatus.Skipped);
            _editor.Invalidate();
            ExecutionStateChanged?.Invoke(this, new ExecutionStateChangedEventArgs(nodeId, ExecutionStatus.Skipped));
        }

        /// <summary>
        /// 高亮显示节点
        /// </summary>
        public void HighlightNode(Guid nodeId, Color color)
        {
            var state = GetOrCreateState(nodeId);
            state.HighlightColor = color;
            state.IsHighlighted = true;
            _editor.Invalidate();
        }

        /// <summary>
        /// 取消高亮
        /// </summary>
        public void ClearHighlight(Guid nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out var state))
            {
                state.IsHighlighted = false;
                _editor.Invalidate();
            }
        }

        /// <summary>
        /// 清除所有高亮
        /// </summary>
        public void ClearAllHighlights()
        {
            foreach (var state in _nodeStates.Values)
            {
                state.IsHighlighted = false;
            }
            _editor.Invalidate();
        }

        #endregion

        #region 私有方法

        private void SetNodeState(Guid nodeId, ExecutionStatus status)
        {
            var state = GetOrCreateState(nodeId);
            state.Status = status;
            state.LastUpdateTime = DateTime.Now;
        }

        private NodeExecutionState GetOrCreateState(Guid nodeId)
        {
            if (!_nodeStates.TryGetValue(nodeId, out var state))
            {
                state = new NodeExecutionState { NodeId = nodeId };
                _nodeStates[nodeId] = state;
            }
            return state;
        }

        private WorkflowNodeBase FindNodeById(Guid nodeId)
        {
            foreach (STNode node in _editor.Nodes)
            {
                if (node is WorkflowNodeBase workflowNode && workflowNode.NodeId == nodeId)
                {
                    return workflowNode;
                }
            }
            return null;
        }

        private IEnumerable<WorkflowNodeBase> GetAllWorkflowNodes()
        {
            foreach (STNode node in _editor.Nodes)
            {
                if (node is WorkflowNodeBase workflowNode)
                {
                    yield return workflowNode;
                }
            }
        }

        private void ScrollToNode(WorkflowNodeBase node)
        {
            // 将节点滚动到视图中心
            var nodeCenter = new Point(node.Left + node.Width / 2, node.Top + node.Height / 2);
            var viewCenter = new Point(_editor.Width / 2, _editor.Height / 2);

            // STNodeEditor 的 MoveCanvas 方法
            float offsetX = viewCenter.X - nodeCenter.X * _editor.CanvasScale - _editor.CanvasOffsetX;
            float offsetY = viewCenter.Y - nodeCenter.Y * _editor.CanvasScale - _editor.CanvasOffsetY;

            _editor.MoveCanvas(offsetX, offsetY, false, CanvasMoveArgs.All);
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _animationFrame++;
            if (_animationFrame > 100) _animationFrame = 0;
            _editor.Invalidate();
        }

        private void OnEditorPaint(object sender, PaintEventArgs e)
        {
            if (!IsExecuting && _nodeStates.Count == 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (STNode node in _editor.Nodes)
            {
                if (node is WorkflowNodeBase workflowNode)
                {
                    if (_nodeStates.TryGetValue(workflowNode.NodeId, out var state))
                    {
                        DrawNodeExecutionState(g, workflowNode, state);
                    }
                }
            }
        }

        private void DrawNodeExecutionState(Graphics g, WorkflowNodeBase node, NodeExecutionState state)
        {
            // 计算节点在屏幕上的位置
            float x = node.Left * _editor.CanvasScale + _editor.CanvasOffsetX;
            float y = node.Top * _editor.CanvasScale + _editor.CanvasOffsetY;
            float width = node.Width * _editor.CanvasScale;
            float height = node.Height * _editor.CanvasScale;

            RectangleF nodeRect = new RectangleF(x, y, width, height);

            // 绘制状态边框
            Color borderColor = GetStatusColor(state.Status);
            float borderWidth = state.Status == ExecutionStatus.Executing ? 3 : 2;

            using (var pen = new Pen(borderColor, borderWidth))
            {
                // 执行中时添加动画效果
                if (state.Status == ExecutionStatus.Executing)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pen.DashOffset = _animationFrame % 10;
                }

                g.DrawRectangle(pen, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
            }

            // 绘制状态图标
            DrawStatusIcon(g, nodeRect, state.Status);

            // 绘制高亮
            if (state.IsHighlighted)
            {
                using (var brush = new SolidBrush(Color.FromArgb(50, state.HighlightColor)))
                {
                    g.FillRectangle(brush, nodeRect);
                }
            }
        }

        private void DrawStatusIcon(Graphics g, RectangleF nodeRect, ExecutionStatus status)
        {
            if (status == ExecutionStatus.Pending) return;

            float iconSize = 16;
            float iconX = nodeRect.Right - iconSize - 5;
            float iconY = nodeRect.Top + 5;

            string iconText = status switch
            {
                ExecutionStatus.Executing => "▶",
                ExecutionStatus.Completed => "✓",
                ExecutionStatus.Failed => "✗",
                ExecutionStatus.Skipped => "⏭",
                _ => ""
            };

            Color iconColor = GetStatusColor(status);

            using (var font = new Font("Segoe UI Symbol", 10f, FontStyle.Bold))
            using (var brush = new SolidBrush(iconColor))
            {
                g.DrawString(iconText, font, brush, iconX, iconY);
            }
        }

        private Color GetStatusColor(ExecutionStatus status)
        {
            return status switch
            {
                ExecutionStatus.Pending => Color.Gray,
                ExecutionStatus.Executing => Color.DodgerBlue,
                ExecutionStatus.Completed => Color.LimeGreen,
                ExecutionStatus.Failed => Color.Red,
                ExecutionStatus.Skipped => Color.Orange,
                _ => Color.Gray
            };
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            _animationTimer.Stop();
            _animationTimer.Dispose();
            _editor.Paint -= OnEditorPaint;
            _nodeStates.Clear();

            _isDisposed = true;
        }

        #endregion
    }

    #region 辅助类型

    /// <summary>
    /// 节点执行状态
    /// </summary>
    public class NodeExecutionState
    {
        public Guid NodeId { get; set; }
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
        public DateTime LastUpdateTime { get; set; }
        public bool IsHighlighted { get; set; }
        public Color HighlightColor { get; set; } = Color.Yellow;
        public string Message { get; set; }
    }

    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// 待执行
        /// </summary>
        Pending,

        /// <summary>
        /// 执行中
        /// </summary>
        Executing,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已跳过
        /// </summary>
        Skipped
    }

    /// <summary>
    /// 执行状态改变事件参数
    /// </summary>
    public class ExecutionStateChangedEventArgs : EventArgs
    {
        public Guid NodeId { get; }
        public ExecutionStatus Status { get; }

        public ExecutionStateChangedEventArgs(Guid nodeId, ExecutionStatus status)
        {
            NodeId = nodeId;
            Status = status;
        }
    }

    #endregion

    #region 工作流执行追踪器

    /// <summary>
    /// 工作流执行追踪器 - 与 StepExecutionManager 集成
    /// </summary>
    public class WorkflowExecutionTracker : IDisposable
    {
        private readonly WorkflowExecutionVisualizer _visualizer;
        private readonly Dictionary<int, Guid> _stepToNodeMap;
        private bool _isDisposed;

        public WorkflowExecutionTracker(WorkflowExecutionVisualizer visualizer)
        {
            _visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
            _stepToNodeMap = new Dictionary<int, Guid>();
        }

        /// <summary>
        /// 构建步骤号到节点ID的映射
        /// </summary>
        public void BuildStepMapping(List<ChildModel> steps, List<WorkflowNodeBase> nodes)
        {
            _stepToNodeMap.Clear();

            // 简单的按顺序映射
            // 更复杂的场景可能需要根据 StepName 匹配
            int nodeIndex = 0;
            foreach (var step in steps)
            {
                if (nodeIndex < nodes.Count)
                {
                    var node = nodes[nodeIndex];
                    if (node.StepName == step.StepName)
                    {
                        _stepToNodeMap[step.StepNum] = node.NodeId;
                    }
                    nodeIndex++;
                }
            }
        }

        /// <summary>
        /// 通知步骤开始执行
        /// </summary>
        public void OnStepStarted(int stepNum)
        {
            if (_stepToNodeMap.TryGetValue(stepNum, out var nodeId))
            {
                _visualizer.SetNodeExecuting(nodeId);
            }
        }

        /// <summary>
        /// 通知步骤执行完成
        /// </summary>
        public void OnStepCompleted(int stepNum, bool success)
        {
            if (_stepToNodeMap.TryGetValue(stepNum, out var nodeId))
            {
                _visualizer.SetNodeCompleted(nodeId, success);
            }
        }

        /// <summary>
        /// 通知步骤被跳过
        /// </summary>
        public void OnStepSkipped(int stepNum)
        {
            if (_stepToNodeMap.TryGetValue(stepNum, out var nodeId))
            {
                _visualizer.SetNodeSkipped(nodeId);
            }
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        public void Start()
        {
            _visualizer.StartExecution();
        }

        /// <summary>
        /// 停止执行
        /// </summary>
        public void Stop()
        {
            _visualizer.StopExecution();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            _visualizer.Reset();
            _stepToNodeMap.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _stepToNodeMap.Clear();
            _isDisposed = true;
        }
    }

    #endregion
}
