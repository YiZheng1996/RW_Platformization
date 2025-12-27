using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ST.Library.UI.NodeEditor;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.NodeEditor.Core;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 工作流设计器面板 - 完整的节点编辑器控件
    /// 包含：节点树、节点编辑器、属性面板
    /// </summary>
    public class WorkflowDesignerPanel : UserControl
    {
        #region 控件

        private SplitContainer _mainSplitContainer;
        private SplitContainer _rightSplitContainer;
        private STNodeEditor _nodeEditor;
        private STNodeTreeView _nodeTreeView;
        private STNodePropertyGrid _propertyGrid;
        private Panel _toolbarPanel;
        private ToolStrip _toolStrip;

        #endregion

        #region 私有字段

        private WorkflowGraphConverter _converter;
        private string _currentFilePath;
        private bool _isDirty = false;

        #endregion

        #region 事件

        /// <summary>
        /// 节点选中事件
        /// </summary>
        public event EventHandler<NodeSelectedEventArgs> NodeSelected;

        /// <summary>
        /// 节点双击事件 (打开配置)
        /// </summary>
        public event EventHandler<NodeDoubleClickEventArgs> NodeDoubleClick;

        /// <summary>
        /// 工作流改变事件
        /// </summary>
        public event EventHandler WorkflowChanged;

        /// <summary>
        /// 验证结果事件
        /// </summary>
        public event EventHandler<ValidationResultEventArgs> ValidationCompleted;

        #endregion

        #region 属性

        /// <summary>
        /// 节点编辑器
        /// </summary>
        [Browsable(false)]
        public STNodeEditor NodeEditor => _nodeEditor;

        /// <summary>
        /// 节点树视图
        /// </summary>
        [Browsable(false)]
        public STNodeTreeView NodeTreeView => _nodeTreeView;

        /// <summary>
        /// 属性网格
        /// </summary>
        [Browsable(false)]
        public STNodePropertyGrid PropertyGrid => _propertyGrid;

        /// <summary>
        /// 图转换器
        /// </summary>
        [Browsable(false)]
        public WorkflowGraphConverter Converter => _converter;

        /// <summary>
        /// 当前文件路径
        /// </summary>
        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => _currentFilePath = value;
        }

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    UpdateTitle();
                }
            }
        }

        /// <summary>
        /// 当前选中的节点
        /// </summary>
        public WorkflowNodeBase SelectedNode
        {
            get => _nodeEditor?.ActiveNode as WorkflowNodeBase;
        }

        #endregion

        #region 构造函数

        public WorkflowDesignerPanel()
        {
            InitializeComponent();
            InitializeNodeEditor();
            RegisterNodeTypes();
            BindEvents();
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            _toolbarPanel = new Panel();
            _toolStrip = new ToolStrip();
            _mainSplitContainer = new SplitContainer();
            _nodeTreeView = new STNodeTreeView();
            _rightSplitContainer = new SplitContainer();
            _nodeEditor = new STNodeEditor();
            _propertyGrid = new STNodePropertyGrid();
            _toolbarPanel.SuspendLayout();
            ((ISupportInitialize)_mainSplitContainer).BeginInit();
            _mainSplitContainer.Panel1.SuspendLayout();
            _mainSplitContainer.Panel2.SuspendLayout();
            _mainSplitContainer.SuspendLayout();
            ((ISupportInitialize)_rightSplitContainer).BeginInit();
            _rightSplitContainer.Panel1.SuspendLayout();
            _rightSplitContainer.Panel2.SuspendLayout();
            _rightSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // _toolbarPanel
            // 
            _toolbarPanel.Controls.Add(_toolStrip);
            _toolbarPanel.Location = new Point(0, 0);
            _toolbarPanel.Name = "_toolbarPanel";
            _toolbarPanel.Size = new Size(200, 100);
            _toolbarPanel.TabIndex = 1;
            // 
            // _toolStrip
            // 
            _toolStrip.Location = new Point(0, 0);
            _toolStrip.Name = "_toolStrip";
            _toolStrip.Size = new Size(200, 25);
            _toolStrip.TabIndex = 0;
            // 
            // _mainSplitContainer
            // 
            _mainSplitContainer.Location = new Point(0, 0);
            _mainSplitContainer.Name = "_mainSplitContainer";
            // 
            // _mainSplitContainer.Panel1
            // 
            _mainSplitContainer.Panel1.Controls.Add(_nodeTreeView);
            // 
            // _mainSplitContainer.Panel2
            // 
            _mainSplitContainer.Panel2.Controls.Add(_rightSplitContainer);
            _mainSplitContainer.Size = new Size(150, 100);
            _mainSplitContainer.TabIndex = 0;
            // 
            // _nodeTreeView
            // 
            _nodeTreeView.AllowDrop = true;
            _nodeTreeView.BackColor = Color.FromArgb(35, 35, 35);
            _nodeTreeView.FolderCountColor = Color.FromArgb(40, 255, 255, 255);
            _nodeTreeView.ForeColor = Color.FromArgb(220, 220, 220);
            _nodeTreeView.ItemBackColor = Color.FromArgb(45, 45, 45);
            _nodeTreeView.ItemHoverColor = Color.FromArgb(50, 125, 125, 125);
            _nodeTreeView.Location = new Point(0, 0);
            _nodeTreeView.MinimumSize = new Size(100, 60);
            _nodeTreeView.Name = "_nodeTreeView";
            _nodeTreeView.ShowFolderCount = true;
            _nodeTreeView.Size = new Size(200, 150);
            _nodeTreeView.TabIndex = 0;
            _nodeTreeView.TextBoxColor = Color.FromArgb(30, 30, 30);
            _nodeTreeView.TitleColor = Color.FromArgb(60, 60, 60);
            // 
            // _rightSplitContainer
            // 
            _rightSplitContainer.Location = new Point(0, 0);
            _rightSplitContainer.Name = "_rightSplitContainer";
            // 
            // _rightSplitContainer.Panel1
            // 
            _rightSplitContainer.Panel1.Controls.Add(_nodeEditor);
            // 
            // _rightSplitContainer.Panel2
            // 
            _rightSplitContainer.Panel2.Controls.Add(_propertyGrid);
            _rightSplitContainer.Size = new Size(150, 100);
            _rightSplitContainer.TabIndex = 0;
            // 
            // _nodeEditor
            // 
            _nodeEditor.AllowDrop = true;
            _nodeEditor.BackColor = Color.FromArgb(34, 34, 34);
            _nodeEditor.Curvature = 0.3F;
            _nodeEditor.Location = new Point(0, 0);
            _nodeEditor.LocationBackColor = Color.FromArgb(120, 0, 0, 0);
            _nodeEditor.MarkBackColor = Color.FromArgb(180, 0, 0, 0);
            _nodeEditor.MarkForeColor = Color.FromArgb(180, 0, 0, 0);
            _nodeEditor.MinimumSize = new Size(100, 100);
            _nodeEditor.Name = "_nodeEditor";
            _nodeEditor.Size = new Size(200, 200);
            _nodeEditor.TabIndex = 0;
            // 
            // _propertyGrid
            // 
            _propertyGrid.BackColor = Color.FromArgb(35, 35, 35);
            _propertyGrid.DescriptionColor = Color.FromArgb(200, 184, 134, 11);
            _propertyGrid.ErrorColor = Color.FromArgb(200, 165, 42, 42);
            _propertyGrid.ForeColor = Color.White;
            _propertyGrid.ItemHoverColor = Color.FromArgb(50, 125, 125, 125);
            _propertyGrid.ItemValueBackColor = Color.FromArgb(80, 80, 80);
            _propertyGrid.Location = new Point(0, 0);
            _propertyGrid.MinimumSize = new Size(120, 50);
            _propertyGrid.Name = "_propertyGrid";
            _propertyGrid.ShowTitle = true;
            _propertyGrid.Size = new Size(200, 150);
            _propertyGrid.TabIndex = 0;
            _propertyGrid.TitleColor = Color.FromArgb(127, 0, 0, 0);
            // 
            // WorkflowDesignerPanel
            // 
            Controls.Add(_mainSplitContainer);
            Controls.Add(_toolbarPanel);
            Name = "WorkflowDesignerPanel";
            _toolbarPanel.ResumeLayout(false);
            _toolbarPanel.PerformLayout();
            _mainSplitContainer.Panel1.ResumeLayout(false);
            _mainSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)_mainSplitContainer).EndInit();
            _mainSplitContainer.ResumeLayout(false);
            _rightSplitContainer.Panel1.ResumeLayout(false);
            _rightSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)_rightSplitContainer).EndInit();
            _rightSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void AddToolbarButtons()
        {
            // 新建
            var btnNew = new ToolStripButton("新建", null, OnNewClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "新建工作流 (Ctrl+N)"
            };

            // 打开
            var btnOpen = new ToolStripButton("打开", null, OnOpenClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "打开工作流 (Ctrl+O)"
            };

            // 保存
            var btnSave = new ToolStripButton("保存", null, OnSaveClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "保存工作流 (Ctrl+S)"
            };

            _toolStrip.Items.Add(btnNew);
            _toolStrip.Items.Add(btnOpen);
            _toolStrip.Items.Add(btnSave);
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 验证
            var btnValidate = new ToolStripButton("验证", null, OnValidateClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "验证工作流"
            };

            // 自动布局
            var btnAutoLayout = new ToolStripButton("自动布局", null, OnAutoLayoutClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "自动排列节点"
            };

            _toolStrip.Items.Add(btnValidate);
            _toolStrip.Items.Add(btnAutoLayout);
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 缩放控制
            var btnZoomIn = new ToolStripButton("放大", null, OnZoomInClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            var btnZoomOut = new ToolStripButton("缩小", null, OnZoomOutClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            var btnZoomReset = new ToolStripButton("重置缩放", null, OnZoomResetClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            _toolStrip.Items.Add(btnZoomIn);
            _toolStrip.Items.Add(btnZoomOut);
            _toolStrip.Items.Add(btnZoomReset);
        }

        private void InitializeNodeEditor()
        {
            _converter = new WorkflowGraphConverter(_nodeEditor);

            // 关联属性网格
            _nodeEditor.ActiveChanged += (s, e) =>
            {
                _propertyGrid.SetNode(_nodeEditor.ActiveNode);
            };
        }

        private void RegisterNodeTypes()
        {
            // 注册节点类型到 TreeView
            var nodesByCategory = WorkflowNodeFactory.GetNodesByCategory();

            foreach (var category in nodesByCategory)
            {
                foreach (var nodeInfo in category.Value)
                {
                    try
                    {
                        // 加载节点类型的程序集
                        _nodeTreeView.AddNode(nodeInfo.NodeType);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"注册节点类型失败: {nodeInfo.StepName}, {ex.Message}");
                    }
                }
            }

            // 刷新树视图
            _nodeTreeView.Refresh();
        }

        private void BindEvents()
        {
            // 节点选中
            _nodeEditor.ActiveChanged += OnActiveNodeChanged;

            // 节点添加/删除
            _nodeEditor.NodeAdded += OnNodeAdded;
            _nodeEditor.NodeRemoved += OnNodeRemoved;

            // 连接变化
            _nodeEditor.OptionConnected += OnOptionConnected;
            _nodeEditor.OptionDisConnected += OnOptionDisconnected;

            // 双击节点
            _nodeEditor.MouseDoubleClick += OnEditorDoubleClick;

            // 键盘快捷键
            _nodeEditor.KeyDown += OnEditorKeyDown;
        }

        #endregion

        #region 事件处理

        private void OnActiveNodeChanged(object sender, EventArgs e)
        {
            var activeNode = _nodeEditor.ActiveNode as WorkflowNodeBase;
            NodeSelected?.Invoke(this, new NodeSelectedEventArgs(activeNode));
        }

        private void OnNodeAdded(object sender, STNodeEditorEventArgs e)
        {
            IsDirty = true;
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnNodeRemoved(object sender, STNodeEditorEventArgs e)
        {
            IsDirty = true;
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnOptionConnected(object sender, STNodeEditorOptionEventArgs e)
        {
            IsDirty = true;
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnOptionDisconnected(object sender, STNodeEditorOptionEventArgs e)
        {
            IsDirty = true;
            WorkflowChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnEditorDoubleClick(object sender, MouseEventArgs e)
        {
            var node = _nodeEditor.ActiveNode as WorkflowNodeBase;
            if (node != null)
            {
                // 触发双击事件
                var args = new NodeDoubleClickEventArgs(node);
                NodeDoubleClick?.Invoke(this, args);

                if (!args.Handled)
                {
                    // 默认打开配置对话框
                    node.OpenConfigDialog();
                }
            }
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            // Delete 删除选中节点
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedNodes();
                e.Handled = true;
            }
            // Ctrl+A 全选
            else if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAllNodes();
                e.Handled = true;
            }
            // Ctrl+S 保存
            else if (e.Control && e.KeyCode == Keys.S)
            {
                OnSaveClick(sender, EventArgs.Empty);
                e.Handled = true;
            }
            // Ctrl+Z 撤销 (暂不实现)
            // Ctrl+Y 重做 (暂不实现)
        }

        #endregion

        #region 工具栏事件

        private void OnNewClick(object sender, EventArgs e)
        {
            if (IsDirty)
            {
                var result = MessageBox.Show("当前工作流有未保存的更改，是否保存？",
                    "确认", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    OnSaveClick(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            NewWorkflow();
        }

        private void OnOpenClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "工作流文件 (*.stn)|*.stn|JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                dialog.Title = "打开工作流";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadWorkflow(dialog.FileName);
                }
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "工作流文件 (*.stn)|*.stn|JSON文件 (*.json)|*.json";
                    dialog.Title = "保存工作流";
                    dialog.DefaultExt = "stn";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _currentFilePath = dialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            SaveWorkflow(_currentFilePath);
        }

        private void OnValidateClick(object sender, EventArgs e)
        {
            var result = ValidateWorkflow();
            ValidationCompleted?.Invoke(this, new ValidationResultEventArgs(result));

            // 显示验证结果
            if (result.HasErrors)
            {
                MessageBox.Show(
                    string.Join("\n", result.Messages.Where(m => m.Level == ValidationLevel.Error).Select(m => m.Message)),
                    "验证错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (result.HasWarnings)
            {
                MessageBox.Show(
                    string.Join("\n", result.Messages.Where(m => m.Level == ValidationLevel.Warning).Select(m => m.Message)),
                    "验证警告",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("工作流验证通过！", "验证成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnAutoLayoutClick(object sender, EventArgs e)
        {
            _converter.AutoLayoutNodes();
            _nodeEditor.Invalidate();
        }

        private void OnZoomInClick(object sender, EventArgs e)
        {
            _nodeEditor.ScaleCanvas(1.2f, _nodeEditor.Width / 2, _nodeEditor.Height / 2);
        }

        private void OnZoomOutClick(object sender, EventArgs e)
        {
            _nodeEditor.ScaleCanvas(0.8f, _nodeEditor.Width / 2, _nodeEditor.Height / 2);
        }

        private void OnZoomResetClick(object sender, EventArgs e)
        {
            _nodeEditor.ScaleCanvas(1f / _nodeEditor.CanvasScale, _nodeEditor.Width / 2, _nodeEditor.Height / 2);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 新建工作流
        /// </summary>
        public void NewWorkflow()
        {
            _nodeEditor.Nodes.Clear();

            // 添加默认的开始和结束节点
            var startNode = new StartNode
            {
                Left = 100,
                Top = 100
            };
            _nodeEditor.Nodes.Add(startNode);

            var endNode = new EndNode
            {
                Left = 100,
                Top = 300
            };
            _nodeEditor.Nodes.Add(endNode);

            // 连接开始和结束
            if (startNode.OutputOptions.Count > 0 && endNode.InputOptions.Count > 0)
            {
                startNode.OutputOptions[0].ConnectOption(endNode.InputOptions[0]);
            }

            _currentFilePath = null;
            IsDirty = false;
            UpdateTitle();
        }

        /// <summary>
        /// 保存工作流到文件
        /// </summary>
        public void SaveWorkflow(string filePath)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(filePath).ToLower();

                if (ext == ".stn")
                {
                    // 使用 STNodeEditor 原生保存
                    _nodeEditor.SaveCanvas(filePath);
                }
                else if (ext == ".json")
                {
                    // 转换为 ChildModel 并保存为 JSON
                    var models = _converter.ConvertToChildModels();
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(models, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(filePath, json);
                }

                _currentFilePath = filePath;
                IsDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从文件加载工作流
        /// </summary>
        public void LoadWorkflow(string filePath)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(filePath).ToLower();

                if (ext == ".stn")
                {
                    // 使用 STNodeEditor 原生加载
                    _nodeEditor.Nodes.Clear();
                    _nodeEditor.LoadCanvas(filePath);
                }
                else if (ext == ".json")
                {
                    // 从 JSON 加载 ChildModel 列表
                    string json = System.IO.File.ReadAllText(filePath);
                    var models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChildModel>>(json);
                    _converter.LoadFromChildModels(models);
                }

                _currentFilePath = filePath;
                IsDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从 ChildModel 列表加载
        /// </summary>
        public void LoadFromChildModels(List<ChildModel> models)
        {
            _converter.LoadFromChildModels(models);
            IsDirty = false;
        }

        /// <summary>
        /// 导出为 ChildModel 列表
        /// </summary>
        public List<ChildModel> ExportToChildModels()
        {
            return _converter.ConvertToChildModels();
        }

        /// <summary>
        /// 验证工作流
        /// </summary>
        public ValidationResult ValidateWorkflow()
        {
            return _converter.ValidateGraph();
        }

        /// <summary>
        /// 删除选中的节点
        /// </summary>
        public void DeleteSelectedNodes()
        {
            var selectedNodes = _nodeEditor.GetSelectedNode();
            if (selectedNodes != null && selectedNodes.Length > 0)
            {
                foreach (var node in selectedNodes)
                {
                    _nodeEditor.Nodes.Remove(node);
                }
            }
        }

        /// <summary>
        /// 全选节点
        /// </summary>
        public void SelectAllNodes()
        {
            // STNodeEditor 可能没有直接的全选方法
            // 需要遍历设置选中状态
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public WorkflowNodeBase AddNode(string stepName, int x = 100, int y = 100)
        {
            var node = WorkflowNodeFactory.CreateNode(stepName);
            if (node != null)
            {
                node.Left = x;
                node.Top = y;
                _nodeEditor.Nodes.Add(node);
            }
            return node;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(WorkflowNodeBase node)
        {
            if (node != null)
            {
                _nodeEditor.Nodes.Add(node);
            }
        }

        #endregion

        #region 私有方法

        private void UpdateTitle()
        {
            // 可以触发标题更新事件
            // 父窗体监听此事件来更新窗口标题
        }

        #endregion
    }

    #region 事件参数类

    /// <summary>
    /// 节点选中事件参数
    /// </summary>
    public class NodeSelectedEventArgs : EventArgs
    {
        public WorkflowNodeBase Node { get; }

        public NodeSelectedEventArgs(WorkflowNodeBase node)
        {
            Node = node;
        }
    }

    /// <summary>
    /// 节点双击事件参数
    /// </summary>
    public class NodeDoubleClickEventArgs : EventArgs
    {
        public WorkflowNodeBase Node { get; }
        public bool Handled { get; set; }

        public NodeDoubleClickEventArgs(WorkflowNodeBase node)
        {
            Node = node;
            Handled = false;
        }
    }

    /// <summary>
    /// 验证结果事件参数
    /// </summary>
    public class ValidationResultEventArgs : EventArgs
    {
        public ValidationResult Result { get; }

        public ValidationResultEventArgs(ValidationResult result)
        {
            Result = result;
        }
    }

    #endregion
}
