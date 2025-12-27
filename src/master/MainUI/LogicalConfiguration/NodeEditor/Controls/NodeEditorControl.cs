using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Services;
using System.Drawing.Drawing2D;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 节点选择事件参数
    /// </summary>
    public class NodeSelectedEventArgs : EventArgs
    {
        public WorkflowNodeBase Node { get; set; }
        public List<WorkflowNodeBase> SelectedNodes { get; set; }
    }

    /// <summary>
    /// 连接创建事件参数
    /// </summary>
    public class ConnectionCreatedEventArgs : EventArgs
    {
        public NodeConnection Connection { get; set; }
    }

    /// <summary>
    /// 节点双击事件参数
    /// </summary>
    public class NodeDoubleClickEventArgs : EventArgs
    {
        public WorkflowNodeBase Node { get; set; }
    }

    /// <summary>
    /// 节点编辑器控件 - 核心画布控件
    /// </summary>
    public class NodeEditorControl : Control
    {
        #region 事件

        /// <summary>
        /// 节点选择改变事件
        /// </summary>
        public event EventHandler<NodeSelectedEventArgs> NodeSelected;

        /// <summary>
        /// 连接创建事件
        /// </summary>
        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;

        /// <summary>
        /// 连接删除事件
        /// </summary>
        public event EventHandler<ConnectionCreatedEventArgs> ConnectionDeleted;

        /// <summary>
        /// 节点双击事件
        /// </summary>
        public event EventHandler<NodeDoubleClickEventArgs> NodeDoubleClick;

        /// <summary>
        /// 文档修改事件
        /// </summary>
        public event EventHandler DocumentChanged;

        #endregion

        #region 字段

        private WorkflowDocument _document;
        private List<WorkflowNodeBase> _selectedNodes = new();

        // 视图变换
        private PointF _viewOffset = PointF.Empty;
        private float _zoomLevel = 1.0f;

        // 交互状态
        private bool _isDraggingNodes = false;
        private bool _isDraggingView = false;
        private bool _isCreatingConnection = false;
        private bool _isSelecting = false;
        private Point _lastMousePos;
        private Point _dragStartPos;
        private Rectangle _selectionRect;

        // 连线临时状态
        private WorkflowNodeBase _connectionSourceNode;
        private NodeSocket _connectionSourceSocket;
        private Point _connectionEndPoint;

        // 悬停状态
        private WorkflowNodeBase _hoveredNode;
        private NodeSocket _hoveredSocket;
        private NodeConnection _hoveredConnection;

        // 视图设置
        private bool _showGrid = true;
        private int _gridSize = 20;
        private Color _gridColor = Color.FromArgb(45, 45, 48);
        private Color _backgroundColor = Color.FromArgb(30, 30, 30);

        #endregion

        #region 属性

        /// <summary>
        /// 工作流文档
        /// </summary>
        public WorkflowDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 选中的节点
        /// </summary>
        public IReadOnlyList<WorkflowNodeBase> SelectedNodes => _selectedNodes;

        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool ShowGrid
        {
            get => _showGrid;
            set { _showGrid = value; Invalidate(); }
        }

        /// <summary>
        /// 网格大小
        /// </summary>
        public int GridSize
        {
            get => _gridSize;
            set { _gridSize = Math.Max(10, value); Invalidate(); }
        }

        /// <summary>
        /// 网格颜色
        /// </summary>
        public Color GridColor
        {
            get => _gridColor;
            set { _gridColor = value; Invalidate(); }
        }

        /// <summary>
        /// 缩放级别
        /// </summary>
        public float ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = Math.Max(0.25f, Math.Min(2.0f, value));
                Invalidate();
            }
        }

        /// <summary>
        /// 最小缩放
        /// </summary>
        public float MinZoom { get; set; } = 0.25f;

        /// <summary>
        /// 最大缩放
        /// </summary>
        public float MaxZoom { get; set; } = 2.0f;

        #endregion

        #region 构造函数

        public NodeEditorControl()
        {
            // 双缓冲防止闪烁
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = _backgroundColor;
            AllowDrop = true;

            _document = new WorkflowDocument();
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 应用变换
            g.TranslateTransform(_viewOffset.X, _viewOffset.Y);
            g.ScaleTransform(_zoomLevel, _zoomLevel);

            // 绘制网格
            if (_showGrid)
            {
                DrawGrid(g);
            }

            // 绘制连接线
            foreach (var connection in _document.Connections)
            {
                connection.Draw(g);
            }

            // 绘制临时连接线
            if (_isCreatingConnection && _connectionSourceSocket != null)
            {
                DrawTempConnection(g);
            }

            // 绘制节点
            foreach (var node in _document.Nodes)
            {
                node.Draw(g);
            }

            // 重置变换，绘制UI元素
            g.ResetTransform();

            // 绘制选择框
            if (_isSelecting)
            {
                DrawSelectionRect(g);
            }

            // 绘制缩放指示器
            DrawZoomIndicator(g);
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawGrid(Graphics g)
        {
            var visibleRect = GetVisibleWorldRect();
            int startX = ((int)visibleRect.X / _gridSize) * _gridSize;
            int startY = ((int)visibleRect.Y / _gridSize) * _gridSize;

            using var pen = new Pen(_gridColor);

            // 绘制次网格线
            for (int x = startX; x < visibleRect.Right; x += _gridSize)
            {
                g.DrawLine(pen, x, visibleRect.Y, x, visibleRect.Bottom);
            }

            for (int y = startY; y < visibleRect.Bottom; y += _gridSize)
            {
                g.DrawLine(pen, visibleRect.X, y, visibleRect.Right, y);
            }

            // 绘制主网格线（每5格加粗）
            using var majorPen = new Pen(Color.FromArgb(60, 60, 65));
            int majorGridSize = _gridSize * 5;

            startX = ((int)visibleRect.X / majorGridSize) * majorGridSize;
            startY = ((int)visibleRect.Y / majorGridSize) * majorGridSize;

            for (int x = startX; x < visibleRect.Right; x += majorGridSize)
            {
                g.DrawLine(majorPen, x, visibleRect.Y, x, visibleRect.Bottom);
            }

            for (int y = startY; y < visibleRect.Bottom; y += majorGridSize)
            {
                g.DrawLine(majorPen, visibleRect.X, y, visibleRect.Right, y);
            }
        }

        /// <summary>
        /// 绘制临时连接线
        /// </summary>
        private void DrawTempConnection(Graphics g)
        {
            var startPoint = _connectionSourceSocket.GetConnectionPoint();
            var endPoint = ScreenToWorld(_connectionEndPoint);

            using var pen = new Pen(Color.FromArgb(100, 200, 200, 200), 2)
            {
                DashStyle = DashStyle.Dash
            };

            // 绘制贝塞尔曲线
            int dx = Math.Abs((int)(endPoint.X - startPoint.X));
            int offset = Math.Max(50, dx / 2);

            var cp1 = new Point(startPoint.X + offset, startPoint.Y);
            var cp2 = new Point((int)endPoint.X - offset, (int)endPoint.Y);

            g.DrawBezier(pen, startPoint,
                cp1, cp2,
                new Point((int)endPoint.X, (int)endPoint.Y));
        }

        /// <summary>
        /// 绘制选择框
        /// </summary>
        private void DrawSelectionRect(Graphics g)
        {
            using var brush = new SolidBrush(Color.FromArgb(30, 0, 122, 204));
            using var pen = new Pen(Color.FromArgb(0, 122, 204), 1);

            g.FillRectangle(brush, _selectionRect);
            g.DrawRectangle(pen, _selectionRect);
        }

        /// <summary>
        /// 绘制缩放指示器
        /// </summary>
        private void DrawZoomIndicator(Graphics g)
        {
            var text = $"{(int)(_zoomLevel * 100)}%";
            using var font = new Font("Segoe UI", 9);
            using var brush = new SolidBrush(Color.FromArgb(100, 200, 200, 200));

            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, Width - size.Width - 10, Height - size.Height - 10);
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            _lastMousePos = e.Location;
            _dragStartPos = e.Location;
            var worldPos = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                //TODO: 暂时使用Point.Round(worldPos)解决错误,原来的代码是：_document.GetSocketAt(worldPos)
                // 检查是否点击了端口
                var (node, socket) = _document.GetSocketAt(Point.Round(worldPos));
                if (socket != null && socket.Type == SocketType.Output)
                {
                    // 开始创建连接
                    _isCreatingConnection = true;
                    _connectionSourceNode = node;
                    _connectionSourceSocket = socket;
                    _connectionEndPoint = e.Location;
                    return;
                }

                // 检查是否点击了节点
                var clickedNode = _document.GetNodeAt(Point.Round(worldPos));
                if (clickedNode != null)
                {
                    // Ctrl+点击 多选
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        if (_selectedNodes.Contains(clickedNode))
                            _selectedNodes.Remove(clickedNode);
                        else
                            _selectedNodes.Add(clickedNode);
                    }
                    else if (!_selectedNodes.Contains(clickedNode))
                    {
                        ClearSelection();
                        SelectNode(clickedNode);
                    }

                    _isDraggingNodes = true;
                }
                else
                {
                    // 检查是否点击了连接线
                    var connection = _document.GetConnectionAt(Point.Round(worldPos));
                    if (connection != null)
                    {
                        // 选中连接线
                        foreach (var conn in _document.Connections)
                            conn.IsSelected = false;
                        connection.IsSelected = true;
                    }
                    else
                    {
                        // 开始框选
                        ClearSelection();
                        _isSelecting = true;
                        _selectionRect = new Rectangle(e.Location, Size.Empty);
                    }
                }

                Invalidate();
            }
            else if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                // 开始平移视图
                _isDraggingView = true;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var worldPos = ScreenToWorld(e.Location);
            int deltaX = e.X - _lastMousePos.X;
            int deltaY = e.Y - _lastMousePos.Y;

            if (_isDraggingNodes && _selectedNodes.Count > 0)
            {
                // 移动选中的节点
                int worldDeltaX = (int)(deltaX / _zoomLevel);
                int worldDeltaY = (int)(deltaY / _zoomLevel);

                foreach (var node in _selectedNodes)
                {
                    node.Move(worldDeltaX, worldDeltaY);
                }

                _document.UpdateConnectionPositions();
                Invalidate();
            }
            else if (_isDraggingView)
            {
                // 平移视图
                _viewOffset = new PointF(
                    _viewOffset.X + deltaX,
                    _viewOffset.Y + deltaY);
                Invalidate();
            }
            else if (_isCreatingConnection)
            {
                // 更新连接终点
                _connectionEndPoint = e.Location;

                // 检查是否悬停在有效端口上
                var (node, socket) = _document.GetSocketAt( Point.Round(worldPos));
                _hoveredSocket = (socket != null && socket.Type == SocketType.Input) ? socket : null;

                Invalidate();
            }
            else if (_isSelecting)
            {
                // 更新选择框
                _selectionRect = new Rectangle(
                    Math.Min(_dragStartPos.X, e.X),
                    Math.Min(_dragStartPos.Y, e.Y),
                    Math.Abs(e.X - _dragStartPos.X),
                    Math.Abs(e.Y - _dragStartPos.Y));

                Invalidate();
            }
            else
            {
                // 更新悬停状态
                UpdateHoverState(worldPos);
            }

            _lastMousePos = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            var worldPos = ScreenToWorld(e.Location);

            if (_isCreatingConnection && _connectionSourceSocket != null)
            {
                // 完成连接创建
                var (targetNode, targetSocket) = _document.GetSocketAt(Point.Round(worldPos));

                if (targetSocket != null && targetSocket.Type == SocketType.Input &&
                    targetNode != _connectionSourceNode)
                {
                    var connection = _document.CreateConnection(
                        _connectionSourceNode, _connectionSourceSocket,
                        targetNode, targetSocket);

                    if (connection != null)
                    {
                        ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs { Connection = connection });
                        DocumentChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            if (_isSelecting)
            {
                // 选择框选范围内的节点
                var worldRect = ScreenRectToWorld(_selectionRect);
                foreach (var node in _document.GetNodesInRect(worldRect))
                {
                    SelectNode(node);
                }
            }

            // 重置状态
            _isDraggingNodes = false;
            _isDraggingView = false;
            _isCreatingConnection = false;
            _isSelecting = false;
            _connectionSourceNode = null;
            _connectionSourceSocket = null;
            Cursor = Cursors.Default;

            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var worldPos = ScreenToWorld(e.Location);
            var node = _document.GetNodeAt(Point.Round(worldPos));

            if (node != null)
            {
                NodeDoubleClick?.Invoke(this, new NodeDoubleClickEventArgs { Node = node });
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // 缩放
            float zoomDelta = e.Delta > 0 ? 0.1f : -0.1f;
            float newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoomLevel + zoomDelta));

            if (newZoom != _zoomLevel)
            {
                // 以鼠标位置为中心缩放
                var mouseWorldBefore = ScreenToWorld(e.Location);

                _zoomLevel = newZoom;

                var mouseWorldAfter = ScreenToWorld(e.Location);

                _viewOffset.X += (mouseWorldAfter.X - mouseWorldBefore.X) * _zoomLevel;
                _viewOffset.Y += (mouseWorldAfter.Y - mouseWorldBefore.Y) * _zoomLevel;

                Invalidate();
            }
        }

        /// <summary>
        /// 更新悬停状态
        /// </summary>
        private void UpdateHoverState(PointF worldPos)
        {
            bool needsInvalidate = false;

            // 检查节点悬停
            var node = _document.GetNodeAt(new Point((int)worldPos.X, (int)worldPos.Y));
            if (node != _hoveredNode)
            {
                if (_hoveredNode != null)
                    _hoveredNode.IsHighlighted = false;
                if (node != null)
                    node.IsHighlighted = true;
                _hoveredNode = node;
                needsInvalidate = true;
            }

            // 检查连接线悬停
            var connection = _document.GetConnectionAt(new Point((int)worldPos.X, (int)worldPos.Y));
            if (connection != _hoveredConnection)
            {
                if (_hoveredConnection != null)
                    _hoveredConnection.IsHighlighted = false;
                if (connection != null)
                    connection.IsHighlighted = true;
                _hoveredConnection = connection;
                needsInvalidate = true;
            }

            if (needsInvalidate)
            {
                Invalidate();
            }
        }

        #endregion

        #region 键盘事件

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    DeleteSelected();
                    break;

                case Keys.A when e.Control:
                    SelectAll();
                    break;

                case Keys.C when e.Control:
                    CopySelected();
                    break;

                case Keys.V when e.Control:
                    Paste();
                    break;

                case Keys.Escape:
                    ClearSelection();
                    Invalidate();
                    break;
            }
        }

        #endregion

        #region 拖放支持

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(typeof(string)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            var nodeType = e.Data.GetData(typeof(string)) as string;
            if (string.IsNullOrEmpty(nodeType)) return;

            var clientPoint = PointToClient(new Point(e.X, e.Y));
            var worldPoint = ScreenToWorld(clientPoint);

            // 对齐到网格
            int snappedX = ((int)worldPoint.X / _gridSize) * _gridSize;
            int snappedY = ((int)worldPoint.Y / _gridSize) * _gridSize;

            if (NodeFactory.TryCreateNode(nodeType, out var node))
            {
                node.Location = new Point(snappedX, snappedY);
                _document.AddNode(node);

                ClearSelection();
                SelectNode(node);

                DocumentChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        #endregion

        #region 选择操作

        /// <summary>
        /// 选择节点
        /// </summary>
        public void SelectNode(WorkflowNodeBase node)
        {
            if (node == null) return;

            node.IsSelected = true;
            if (!_selectedNodes.Contains(node))
            {
                _selectedNodes.Add(node);
            }

            NodeSelected?.Invoke(this, new NodeSelectedEventArgs
            {
                Node = node,
                SelectedNodes = _selectedNodes.ToList()
            });
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            foreach (var node in _selectedNodes)
            {
                node.IsSelected = false;
            }
            _selectedNodes.Clear();

            foreach (var conn in _document.Connections)
            {
                conn.IsSelected = false;
            }

            NodeSelected?.Invoke(this, new NodeSelectedEventArgs
            {
                Node = null,
                SelectedNodes = new List<WorkflowNodeBase>()
            });
        }

        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            ClearSelection();
            foreach (var node in _document.Nodes)
            {
                SelectNode(node);
            }
            Invalidate();
        }

        /// <summary>
        /// 删除选中的元素
        /// </summary>
        public void DeleteSelected()
        {
            // 删除选中的连接
            var selectedConnections = _document.Connections.Where(c => c.IsSelected).ToList();
            foreach (var conn in selectedConnections)
            {
                _document.RemoveConnection(conn);
                ConnectionDeleted?.Invoke(this, new ConnectionCreatedEventArgs { Connection = conn });
            }

            // 删除选中的节点
            foreach (var node in _selectedNodes.ToList())
            {
                // 不允许删除开始和结束节点
                if (node.NodeType == "Start" || node.NodeType == "End")
                    continue;

                _document.RemoveNode(node);
            }

            _selectedNodes.Clear();
            DocumentChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        #endregion

        #region 复制粘贴

        private List<WorkflowNodeBase> _clipboard = new();

        /// <summary>
        /// 复制选中的节点
        /// </summary>
        public void CopySelected()
        {
            _clipboard.Clear();
            foreach (var node in _selectedNodes)
            {
                if (node.NodeType == "Start" || node.NodeType == "End")
                    continue;
                _clipboard.Add(node.Clone());
            }
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        public void Paste()
        {
            if (_clipboard.Count == 0) return;

            ClearSelection();

            foreach (var node in _clipboard)
            {
                var clone = node.Clone();
                clone.NodeId = Guid.NewGuid();
                clone.Location = new Point(node.Location.X + 50, node.Location.Y + 50);
                _document.AddNode(clone);
                SelectNode(clone);
            }

            DocumentChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        #endregion

        #region 视图操作

        /// <summary>
        /// 放大
        /// </summary>
        public void ZoomIn()
        {
            ZoomLevel = Math.Min(MaxZoom, _zoomLevel + 0.1f);
        }

        /// <summary>
        /// 缩小
        /// </summary>
        public void ZoomOut()
        {
            ZoomLevel = Math.Max(MinZoom, _zoomLevel - 0.1f);
        }

        /// <summary>
        /// 适应视图
        /// </summary>
        public void ZoomToFit()
        {
            var bounds = _document.GetBoundingRect();
            if (bounds.IsEmpty) return;

            // 添加边距
            bounds.Inflate(50, 50);

            // 计算缩放比例
            float scaleX = (float)Width / bounds.Width;
            float scaleY = (float)Height / bounds.Height;
            _zoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, Math.Min(scaleX, scaleY)));

            // 居中
            _viewOffset = new PointF(
                (Width - bounds.Width * _zoomLevel) / 2 - bounds.X * _zoomLevel,
                (Height - bounds.Height * _zoomLevel) / 2 - bounds.Y * _zoomLevel);

            Invalidate();
        }

        /// <summary>
        /// 重置视图
        /// </summary>
        public void ResetView()
        {
            _zoomLevel = 1.0f;
            _viewOffset = PointF.Empty;
            Invalidate();
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        public PointF ScreenToWorld(Point screenPoint)
        {
            return new PointF(
                (screenPoint.X - _viewOffset.X) / _zoomLevel,
                (screenPoint.Y - _viewOffset.Y) / _zoomLevel);
        }

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        public Point WorldToScreen(PointF worldPoint)
        {
            return new Point(
                (int)(worldPoint.X * _zoomLevel + _viewOffset.X),
                (int)(worldPoint.Y * _zoomLevel + _viewOffset.Y));
        }

        /// <summary>
        /// 屏幕矩形转世界矩形
        /// </summary>
        private Rectangle ScreenRectToWorld(Rectangle screenRect)
        {
            var topLeft = ScreenToWorld(screenRect.Location);
            var bottomRight = ScreenToWorld(new Point(screenRect.Right, screenRect.Bottom));

            return new Rectangle(
                (int)topLeft.X,
                (int)topLeft.Y,
                (int)(bottomRight.X - topLeft.X),
                (int)(bottomRight.Y - topLeft.Y));
        }

        /// <summary>
        /// 获取可见的世界区域
        /// </summary>
        private RectangleF GetVisibleWorldRect()
        {
            var topLeft = ScreenToWorld(Point.Empty);
            var bottomRight = ScreenToWorld(new Point(Width, Height));

            return new RectangleF(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(WorkflowNodeBase node)
        {
            _document.AddNode(node);
            Invalidate();
        }

        /// <summary>
        /// 添加连接
        /// </summary>
        public void AddConnection(NodeConnection connection)
        {
            _document.AddConnection(connection);
            Invalidate();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshView()
        {
            _document.UpdateConnectionPositions();
            Invalidate();
        }

        #endregion
    }
}
