using MainUI.LogicalConfiguration.LogicalManager;
using MainUI.LogicalConfiguration.NodeEditor.Controls;
using MainUI.LogicalConfiguration.NodeEditor.Converters;
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.Services;
using Microsoft.Extensions.Logging;

namespace MainUI.LogicalConfiguration.NodeEditor.Forms
{
    /// <summary>
    /// 节点工作流设计器窗体
    /// </summary>
    public partial class FrmNodeWorkflowDesigner : Form
    {
        #region 字段

        private readonly IWorkflowStateService _workflowState;
        private readonly ILogger<FrmNodeWorkflowDesigner> _logger;
        private readonly WorkflowConverter _converter;
        private readonly IFormService _formService;

        // UI控件
        private NodeEditorControl _nodeEditor;
        private NodeToolboxControl _toolbox;
        private NodePropertyPanel _propertyPanel;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private SplitContainer _mainSplitter;
        private SplitContainer _rightSplitter;

        // 状态
        private bool _isModified = false;

        #endregion

        #region 构造函数

        public FrmNodeWorkflowDesigner(
            IWorkflowStateService workflowState,
            ILogger<FrmNodeWorkflowDesigner> logger = null,
            IFormService formService = null)
        {
            _workflowState = workflowState ?? throw new ArgumentNullException(nameof(workflowState));
            _logger = logger;
            _formService = formService;
            _converter = new WorkflowConverter(logger as ILogger<WorkflowConverter>);

            InitializeComponent();
            InitializeCustomUI();
            RegisterEventHandlers();
            LoadExistingWorkflow();
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            SuspendLayout();

            // 窗体基本设置
            Text = "工作流设计器 - 节点编辑模式";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            Font = new Font("微软雅黑", 9);

            ResumeLayout(false);
        }

        private void InitializeCustomUI()
        {
            // 创建工具栏
            CreateToolStrip();

            // 创建状态栏
            CreateStatusStrip();

            // 创建主分割容器
            _mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                //SplitterDistance = 200,
                SplitterWidth = 3,
                BackColor = Color.FromArgb(45, 45, 48),
                //Panel1MinSize = 150,
                //Panel2MinSize = 400
            };

            // 左侧：工具箱
            _toolbox = new NodeToolboxControl
            {
                Dock = DockStyle.Fill,
                Title = "节点工具箱"
            };
            _mainSplitter.Panel1.Controls.Add(_toolbox);

            // 右侧再分割
            _rightSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 800,
                SplitterWidth = 3,
                BackColor = Color.FromArgb(45, 45, 48),
                //Panel2MinSize = 250
            };

            // 中间：节点编辑器
            _nodeEditor = new NodeEditorControl
            {
                Dock = DockStyle.Fill,
                ShowGrid = true
            };
            _rightSplitter.Panel1.Controls.Add(_nodeEditor);

            // 右侧：属性面板
            _propertyPanel = new NodePropertyPanel
            {
                Dock = DockStyle.Fill
            };
            _rightSplitter.Panel2.Controls.Add(_propertyPanel);

            _mainSplitter.Panel2.Controls.Add(_rightSplitter);

            // 添加到窗体
            Controls.Add(_mainSplitter);
            Controls.Add(_toolStrip);
            Controls.Add(_statusStrip);
        }

        /// <summary>
        /// 创建工具栏
        /// </summary>
        private void CreateToolStrip()
        {
            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Renderer = new DarkToolStripRenderer()
            };

            // 文件操作
            AddToolStripButton("保存", "💾", BtnSave_Click, "保存工作流 (Ctrl+S)");
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 编辑操作
            AddToolStripButton("撤销", "↶", BtnUndo_Click, "撤销 (Ctrl+Z)");
            AddToolStripButton("重做", "↷", BtnRedo_Click, "重做 (Ctrl+Y)");
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 视图操作
            AddToolStripButton("放大", "🔍+", BtnZoomIn_Click, "放大视图");
            AddToolStripButton("缩小", "🔍-", BtnZoomOut_Click, "缩小视图");
            AddToolStripButton("适应", "⊞", BtnZoomFit_Click, "适应窗口");
            AddToolStripButton("重置", "🏠", BtnResetView_Click, "重置视图");
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 工作流操作
            AddToolStripButton("验证", "✓", BtnValidate_Click, "验证工作流");
            AddToolStripButton("运行", "▶", BtnRun_Click, "运行工作流");
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 布局操作
            AddToolStripButton("自动布局", "⋮⋮", BtnAutoLayout_Click, "自动排列节点");

            Controls.Add(_toolStrip);
        }

        /// <summary>
        /// 添加工具栏按钮
        /// </summary>
        private void AddToolStripButton(string text, string icon, EventHandler clickHandler, string tooltip = null)
        {
            var btn = new ToolStripButton
            {
                Text = $"{icon} {text}",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ForeColor = Color.White,
                ToolTipText = tooltip ?? text
            };
            btn.Click += clickHandler;
            _toolStrip.Items.Add(btn);
        }

        /// <summary>
        /// 创建状态栏
        /// </summary>
        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(0, 122, 204),
                SizingGrip = false
            };

            _statusLabel = new ToolStripStatusLabel
            {
                Text = "就绪",
                ForeColor = Color.White
            };

            var zoomLabel = new ToolStripStatusLabel
            {
                Text = "100%",
                ForeColor = Color.White,
                Alignment = ToolStripItemAlignment.Right
            };

            var nodeCountLabel = new ToolStripStatusLabel
            {
                Text = "节点: 0",
                ForeColor = Color.White,
                Alignment = ToolStripItemAlignment.Right
            };

            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
            _statusStrip.Items.Add(nodeCountLabel);
            _statusStrip.Items.Add(new ToolStripSeparator());
            _statusStrip.Items.Add(zoomLabel);

            Controls.Add(_statusStrip);
        }

        /// <summary>
        /// 注册事件处理
        /// </summary>
        private void RegisterEventHandlers()
        {
            // 节点编辑器事件
            _nodeEditor.NodeSelected += NodeEditor_NodeSelected;
            _nodeEditor.NodeDoubleClick += NodeEditor_NodeDoubleClick;
            _nodeEditor.ConnectionCreated += NodeEditor_ConnectionCreated;
            _nodeEditor.ConnectionDeleted += NodeEditor_ConnectionDeleted;
            _nodeEditor.DocumentChanged += NodeEditor_DocumentChanged;

            // 工具箱事件
            _toolbox.NodeDragStart += Toolbox_NodeDragStart;

            // 属性面板事件
            _propertyPanel.ParameterChanged += PropertyPanel_ParameterChanged;
            _propertyPanel.EditParameterRequested += PropertyPanel_EditParameterRequested;

            // 键盘快捷键
            KeyPreview = true;
            KeyDown += Form_KeyDown;

            // 窗体关闭
            FormClosing += Form_FormClosing;
        }

        #endregion

        #region 事件处理

        private void NodeEditor_NodeSelected(object sender, NodeSelectedEventArgs e)
        {
            _propertyPanel.SelectedNode = e.Node;
            UpdateStatusBar();
        }

        private void NodeEditor_NodeDoubleClick(object sender, NodeDoubleClickEventArgs e)
        {
            OpenParameterForm(e.Node);
        }

        private void NodeEditor_ConnectionCreated(object sender, ConnectionCreatedEventArgs e)
        {
            _isModified = true;
            UpdateStatusBar();
            _logger?.LogDebug("连接已创建: {ConnectionId}", e.Connection.Id);
        }

        private void NodeEditor_ConnectionDeleted(object sender, ConnectionCreatedEventArgs e)
        {
            _isModified = true;
            UpdateStatusBar();
            _logger?.LogDebug("连接已删除: {ConnectionId}", e.Connection.Id);
        }

        private void NodeEditor_DocumentChanged(object sender, EventArgs e)
        {
            _isModified = true;
            UpdateStatusBar();
        }

        private void Toolbox_NodeDragStart(object sender, string nodeType)
        {
            _statusLabel.Text = $"拖动节点: {nodeType}";
        }

        private void PropertyPanel_ParameterChanged(object sender, EventArgs e)
        {
            _isModified = true;
            _nodeEditor.RefreshView();
        }

        private void PropertyPanel_EditParameterRequested(object sender, WorkflowNodeBase node)
        {
            OpenParameterForm(node);
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        BtnSave_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Keys.Z:
                        BtnUndo_Click(sender, e);
                        e.Handled = true;
                        break;
                    case Keys.Y:
                        BtnRedo_Click(sender, e);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isModified)
            {
                var result = MessageBox.Show(
                    "工作流已修改，是否保存？",
                    "保存确认",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveWorkflow();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region 工具栏事件

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            await SaveWorkflowAsync();
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            // TODO: 实现撤销功能
            _statusLabel.Text = "撤销功能待实现";
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            // TODO: 实现重做功能
            _statusLabel.Text = "重做功能待实现";
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            _nodeEditor.ZoomIn();
            UpdateStatusBar();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            _nodeEditor.ZoomOut();
            UpdateStatusBar();
        }

        private void BtnZoomFit_Click(object sender, EventArgs e)
        {
            _nodeEditor.ZoomToFit();
            UpdateStatusBar();
        }

        private void BtnResetView_Click(object sender, EventArgs e)
        {
            _nodeEditor.ResetView();
            UpdateStatusBar();
        }

        private void BtnValidate_Click(object sender, EventArgs e)
        {
            ValidateWorkflow();
        }

        private async void BtnRun_Click(object sender, EventArgs e)
        {
            await RunWorkflowAsync();
        }

        private void BtnAutoLayout_Click(object sender, EventArgs e)
        {
            AutoLayoutNodes();
        }

        #endregion

        #region 工作流操作

        /// <summary>
        /// 加载现有的工作流配置
        /// </summary>
        private void LoadExistingWorkflow()
        {
            try
            {
                _statusLabel.Text = "正在加载工作流...";

                var steps = _workflowState.GetSteps();
                if (steps == null || steps.Count == 0)
                {
                    AddDefaultNodes();
                    _statusLabel.Text = "已创建新工作流";
                    return;
                }

                // 转换为节点
                var (nodes, connections) = _converter.ConvertChildModelsToNodes(steps);

                // 设置文档
                _nodeEditor.Document.Nodes.Clear();
                _nodeEditor.Document.Connections.Clear();

                foreach (var node in nodes)
                {
                    _nodeEditor.Document.AddNode(node);
                }

                foreach (var conn in connections)
                {
                    _nodeEditor.Document.AddConnection(conn);
                }

                // 更新连接位置
                _nodeEditor.Document.UpdateConnectionPositions();

                // 自动布局
                AutoLayoutNodes();

                _nodeEditor.RefreshView();
                _statusLabel.Text = $"已加载 {nodes.Count} 个节点";
                _logger?.LogInformation("工作流加载完成，共 {Count} 个节点", nodes.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "加载工作流失败");
                AddDefaultNodes();
                _statusLabel.Text = "加载失败，已创建新工作流";
            }
        }

        /// <summary>
        /// 添加默认节点
        /// </summary>
        private void AddDefaultNodes()
        {
            var startNode = new StartNode { Location = new Point(100, 200) };
            var endNode = new EndNode { Location = new Point(500, 200) };

            _nodeEditor.Document.AddNode(startNode);
            _nodeEditor.Document.AddNode(endNode);
            _nodeEditor.RefreshView();
        }

        /// <summary>
        /// 保存工作流
        /// </summary>
        private void SaveWorkflow()
        {
            SaveWorkflowAsync().Wait();
        }

        /// <summary>
        /// 异步保存工作流
        /// </summary>
        private async Task SaveWorkflowAsync()
        {
            try
            {
                _statusLabel.Text = "正在保存...";
                _logger?.LogInformation("开始保存工作流...");

                // 转换为 ChildModel 列表
                var childModels = _converter.ConvertNodesToChildModels(
                    _nodeEditor.Document.Nodes,
                    _nodeEditor.Document.Connections);

                // 更新工作流状态
                _workflowState.ClearSteps();
                foreach (var step in childModels)
                {
                    _workflowState.AddStep(step);
                }

                // 保存到 JSON
                await JsonManager.UpdateConfigAsync(config =>
                {
                    var parent = config.Form.Find(p =>
                        p.ModelTypeName == _workflowState.ModelTypeName &&
                        p.ModelName == _workflowState.ModelName &&
                        p.ItemName == _workflowState.ItemName);

                    if (parent != null)
                    {
                        parent.ChildSteps = childModels;
                    }

                    return Task.CompletedTask;
                });

                _isModified = false;
                _statusLabel.Text = $"保存成功 ({childModels.Count} 个步骤)";

                MessageBox.Show(
                    $"工作流保存成功！\n共 {childModels.Count} 个步骤",
                    "保存成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _logger?.LogInformation("工作流保存完成，共 {Count} 个步骤", childModels.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "保存工作流失败");
                _statusLabel.Text = "保存失败";

                MessageBox.Show(
                    $"保存失败: {ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 验证工作流
        /// </summary>
        private void ValidateWorkflow()
        {
            var isValid = _nodeEditor.Document.IsValid(out var errors);

            if (isValid)
            {
                _statusLabel.Text = "验证通过 ✓";
                MessageBox.Show(
                    "工作流验证通过！",
                    "验证结果",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                _statusLabel.Text = $"验证失败 ({errors.Count} 个问题)";

                var message = "工作流存在以下问题：\n\n" + string.Join("\n", errors);
                MessageBox.Show(
                    message,
                    "验证结果",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 运行工作流
        /// </summary>
        private async Task RunWorkflowAsync()
        {
            // 先保存
            if (_isModified)
            {
                var result = MessageBox.Show(
                    "运行前需要保存工作流，是否保存？",
                    "保存确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await SaveWorkflowAsync();
                }
                else
                {
                    return;
                }
            }

            // 验证
            if (!_nodeEditor.Document.IsValid(out var errors))
            {
                MessageBox.Show(
                    "工作流验证失败，请先修复问题：\n\n" + string.Join("\n", errors),
                    "无法运行",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _statusLabel.Text = "正在运行工作流...";

            // TODO: 调用执行服务
            MessageBox.Show(
                "工作流执行功能请在主界面中使用",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _statusLabel.Text = "就绪";
        }

        /// <summary>
        /// 自动布局节点
        /// </summary>
        private void AutoLayoutNodes()
        {
            var nodes = _nodeEditor.Document.Nodes;
            if (nodes.Count == 0) return;

            // 简单的层级布局
            int x = 100;
            int y = 100;
            int xSpacing = 200;
            int ySpacing = 120;
            int maxX = 900;
            int col = 0;

            // 按顺序排列
            foreach (var node in nodes)
            {
                node.Location = new Point(x, y);
                x += xSpacing;
                col++;

                if (x > maxX)
                {
                    x = 100;
                    y += ySpacing;
                    col = 0;
                }
            }

            _nodeEditor.Document.UpdateConnectionPositions();
            _nodeEditor.RefreshView();
            _nodeEditor.ZoomToFit();
        }

        /// <summary>
        /// 打开节点参数配置窗体
        /// </summary>
        private void OpenParameterForm(WorkflowNodeBase node)
        {
            if (node == null) return;

            // 使用表单服务打开对应的参数配置窗体
            Form paramForm = null;

            try
            {
                // 根据节点类型创建对应的参数配置窗体
                //paramForm = _formService?.CreateParameterForm(node.NodeType, _workflowState);
                _formService?.OpenFormByName(this, node.DisplayName, this);

                if (paramForm != null)
                {
                    // 设置参数
                    var paramProperty = paramForm.GetType().GetProperty("Parameter");
                    paramProperty?.SetValue(paramForm, node.Parameter);

                    if (paramForm.ShowDialog() == DialogResult.OK)
                    {
                        // 获取更新后的参数
                        if (paramProperty != null)
                        {
                            node.Parameter = paramProperty.GetValue(paramForm);
                        }

                        _isModified = true;
                        _nodeEditor.RefreshView();
                        _propertyPanel.RefreshDisplay();

                        _logger?.LogInformation("节点参数已更新: {NodeType}", node.NodeType);
                    }
                }
                else
                {
                    // 没有专用窗体，使用属性网格
                    _statusLabel.Text = "请在右侧属性面板中编辑参数";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "打开参数配置窗体失败");
                MessageBox.Show(
                    $"打开参数配置失败: {ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                paramForm?.Dispose();
            }
        }

        /// <summary>
        /// 更新状态栏
        /// </summary>
        private void UpdateStatusBar()
        {
            var nodeCount = _nodeEditor.Document.Nodes.Count;
            var connectionCount = _nodeEditor.Document.Connections.Count;
            var selectedCount = _nodeEditor.SelectedNodes.Count;

            var modifiedMark = _isModified ? " *" : "";
            Text = $"工作流设计器 - {_workflowState.ItemName}{modifiedMark}";

            // 更新节点计数
            foreach (ToolStripItem item in _statusStrip.Items)
            {
                if (item is ToolStripStatusLabel label)
                {
                    if (label.Text.StartsWith("节点"))
                    {
                        label.Text = $"节点: {nodeCount} | 连接: {connectionCount} | 选中: {selectedCount}";
                    }
                    else if (label.Alignment == ToolStripItemAlignment.Right && label.Text.EndsWith("%"))
                    {
                        label.Text = $"{(int)(_nodeEditor.ZoomLevel * 100)}%";
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 深色主题工具栏渲染器
    /// </summary>
    public class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(45, 45, 48));
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var btn = e.Item as ToolStripButton;
            if (btn != null)
            {
                if (btn.Selected || btn.Pressed)
                {
                    using var brush = new SolidBrush(Color.FromArgb(62, 62, 66));
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(70, 70, 74));
            int x = e.Item.Width / 2;
            e.Graphics.DrawLine(pen, x, 4, x, e.Item.Height - 4);
        }
    }
}
