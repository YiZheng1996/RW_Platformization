using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using ST.Library.UI.NodeEditor;
using System.ComponentModel;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 工作流设计器面板 - 完整的节点编辑器控件
    /// 包含：节点树、节点编辑器、属性面板
    /// </summary>
    public partial class WorkflowDesignerPanel : UserControl
    {
        #region 私有字段

        private WorkflowGraphConverter _converter;
        private string _currentFilePath;
        private bool _isDirty = false;

        private TextBox _searchBox;
        private Panel _searchPanel;
        private PictureBox _miniMap;
        private STNode _copiedNode;
        private Point _lastRightClickPosition;

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

        /// <summary>
        /// 配置面板
        /// </summary>
        [Browsable(false)]
        public NodeConfigHostPanel ConfigPanel => _configHostPanel;

        /// <summary>
        /// 是否有未保存的配置更改
        /// </summary>
        public bool HasUnsavedConfig => _configHostPanel?.HasChanges ?? false;

        #endregion

        #region 构造函数

        public WorkflowDesignerPanel()
        {
            InitializeComponent();
            _nodeEditor.SetTypeColor(typeof(ExecutionFlow), Color.White);

            InitializeNodeEditor();
            RegisterNodeTypes();
            BindEvents();

            InitializeEnhancements();
            InitializeConfigPanel();
        }

        private void InitializeEnhancements()
        {
            // 2. 使用增强的右键菜单
            _nodeEditor.ContextMenuStrip = CreateEnhancedContextMenu();

            // 3. 初始化缩放控制
            InitializeZoomControls();

            // 4. 初始化小地图（可选）
            // InitializeMiniMap();

            // 5. 绑定增强的键盘事件
            _nodeEditor.KeyDown -= OnEditorKeyDown;  // 移除旧的
            _nodeEditor.KeyDown += OnEditorKeyDownEnhanced;  // 使用新的
        }

        #endregion

        #region 新增方法

        /// <summary>
        /// 初始化配置面板
        /// </summary>
        private void InitializeConfigPanel()
        {
            if (_configHostPanel == null) return;

            // 监听配置保存事件
            _configHostPanel.ConfigurationSaved += (s, e) =>
            {
                if (e.Success)
                {
                    // 标记为已修改
                    IsDirty = true;

                    // 触发工作流变化事件
                    WorkflowChanged?.Invoke(this, EventArgs.Empty);

                    // 刷新节点显示
                    e.Node?.RefreshDisplay();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"配置保存失败: {e.ErrorMessage}");
                }
            };

            // 监听配置变更事件（有未保存的更改）
            _configHostPanel.ConfigurationChanged += (s, e) =>
            {
                // 可以在标题显示未保存标记
                UpdateTitle();
            };

            // 注册额外的自定义配置面板（如果有的话）
            // _configHostPanel.RegisterPanel("CustomStep", () => new CustomConfigPanel());
        }

        /// <summary>
        /// 注册自定义配置面板
        /// </summary>
        public void RegisterConfigPanel(string stepName, Func<NodeConfigPanelBase> factory)
        {
            _configHostPanel?.RegisterPanel(stepName, factory);
        }

        /// <summary>
        /// 注册自定义配置面板（泛型版本）
        /// </summary>
        public void RegisterConfigPanel<T>(string stepName) where T : NodeConfigPanelBase, new()
        {
            _configHostPanel?.RegisterPanel<T>(stepName);
        }

        #endregion

        #region 初始化

        private void InitializeNodeEditor()
        {
            _converter = new WorkflowGraphConverter(_nodeEditor);

            // ★ 修改：同时更新属性网格和配置面板 ★
            _nodeEditor.ActiveChanged += (s, e) =>
            {
                // 更新属性网格
                _propertyGrid.SetNode(_nodeEditor.ActiveNode);

                // ★ 新增：更新配置面板 ★
                if (_nodeEditor.ActiveNode is WorkflowNodeBase workflowNode)
                {
                    _configHostPanel?.SetNode(workflowNode);
                }
                else
                {
                    _configHostPanel?.Clear();
                }
            };
        }

        private void RegisterNodeTypes()
        {
            // 注册节点类型到 TreeView
            var nodesByCategory = WorkflowNodeFactory.GetNodesByCategory();

            foreach (var category in nodesByCategory)
            {
                Debug.WriteLine($"Category: {category.Key}");
                foreach (var nodeInfo in category.Value)
                {
                    Debug.WriteLine($"  Node: {nodeInfo.DisplayName}");
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

            // 记录右键点击位置（用于粘贴）
            _nodeEditor.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    //_lastRightClickPosition = _nodeEditor.PointToCanvas(e.Location);
                }
            };
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
            if (_nodeEditor.ActiveNode is not WorkflowNodeBase node) return;

            // 触发双击事件
            var args = new NodeDoubleClickEventArgs(node);
            NodeDoubleClick?.Invoke(this, args);

            if (args.Handled) return;

            // ========================================
            // 方案A: 完全使用嵌入式配置面板（推荐）
            // ========================================

            // 确保配置面板显示当前节点
            _configHostPanel?.SetNode(node);

            // 标记事件已处理
            args.Handled = true;

            // ========================================
            // 方案B: 混合模式 - 某些节点使用弹窗
            // ========================================
            // 取消注释以下代码启用混合模式
            /*
            // 判断是否需要使用弹窗（复杂配置的节点）
            if (ShouldUsePopupConfig(node.StepName))
            {
                // 使用原有的弹窗方式
                var result = NodeConfigAdapter.Instance.OpenConfigForm(node, this.FindForm());
                if (result.Success)
                {
                    node.RefreshDisplay();
                    IsDirty = true;
                    WorkflowChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                // 使用嵌入式配置面板
                _configHostPanel?.SetNode(node);
            }
            args.Handled = true;
            */
        }

        /// <summary>
        /// 判断是否应该使用弹窗配置
        /// 对于特别复杂的节点，可能弹窗更合适
        /// </summary>
        private bool ShouldUsePopupConfig(string stepName)
        {
            // 这些节点配置比较复杂，建议使用弹窗
            // 如果你觉得嵌入式面板足够用，可以返回 false
            return stepName switch
            {
                "CycleBegins" => true,     // 循环有子步骤配置，可能需要更大空间
                // "ConditionJudge" => true, // 条件表达式编辑器
                _ => false
            };
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            // Delete 删除选中节点
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedNodes();
                e.Handled = true;
            }
            else switch (e.Control)
            {
                // Ctrl+A 全选
                case true when e.KeyCode == Keys.A:
                    SelectAllNodes();
                    e.Handled = true;
                    break;
                // Ctrl+S 保存
                case true when e.KeyCode == Keys.S:
                    OnSaveClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                //  新增：复制
                case true when e.KeyCode == Keys.C:
                    OnCopyNode(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                //  新增：粘贴
                case true when e.KeyCode == Keys.V:
                    OnPasteNode(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                //  新增：缩放快捷键
                case true when e.KeyCode == Keys.Add:
                    OnZoomInClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case true when e.KeyCode == Keys.Subtract:
                    OnZoomOutClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case true when e.KeyCode == Keys.D0:
                    OnZoomResetClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
            }
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
            // 先检查并保存当前配置
            if (!CheckAndSaveConfig()) return;

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

        /// <summary>
        /// 保存前检查是否有未保存的配置
        /// </summary>
        private bool CheckAndSaveConfig()
        {
            if (_configHostPanel?.HasChanges != true) return true;
            var result = MessageBox.Show(
                "当前节点配置有未保存的更改，是否保存？",
                "保存确认",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    return _configHostPanel.SaveCurrentConfig();
                case DialogResult.No:
                    return true; // 不保存，继续
                case DialogResult.Cancel:
                    return false; // 取消操作
                case DialogResult.None:
                case DialogResult.OK:
                case DialogResult.Abort:
                case DialogResult.Retry:
                case DialogResult.Ignore:
                case DialogResult.TryAgain:
                case DialogResult.Continue:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
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

            // 连接开始和结束 - 使用公共方法
            startNode.ConnectTo(endNode);

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
                var ext = System.IO.Path.GetExtension(filePath).ToLower();

                switch (ext)
                {
                    case ".stn":
                        // 使用 STNodeEditor 原生保存
                        _nodeEditor.SaveCanvas(filePath);
                        break;
                    case ".json":
                    {
                        // 转换为 ChildModel 并保存为 JSON
                        var models = _converter.ConvertToChildModels();
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(models, Newtonsoft.Json.Formatting.Indented);
                        System.IO.File.WriteAllText(filePath, json);
                        break;
                    }
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
                var ext = System.IO.Path.GetExtension(filePath).ToLower();

                switch (ext)
                {
                    case ".stn":
                        // 使用 STNodeEditor 原生加载
                        _nodeEditor.Nodes.Clear();
                        _nodeEditor.LoadCanvas(filePath);
                        break;
                    case ".json":
                    {
                        // 从 JSON 加载 ChildModel 列表
                        var json = System.IO.File.ReadAllText(filePath);
                        var models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChildModel>>(json);
                        _converter.LoadFromChildModels(models);
                        break;
                    }
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
            if (selectedNodes == null || selectedNodes.Length <= 0) return;
            foreach (var node in selectedNodes)
            {
                _nodeEditor.Nodes.Remove(node);
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
            var dirtyMark = (IsDirty || HasUnsavedConfig) ? " *" : "";
        }

        #endregion

        #region 功能2: 增强的右键菜单

        /// <summary>
        /// 创建增强的右键菜单
        /// </summary>
        private ContextMenuStrip CreateEnhancedContextMenu()
        {
            var menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            // 节点操作
            var copyItem = new ToolStripMenuItem("复制节点 (Ctrl+C)", null, OnCopyNode)
            {
                ShortcutKeys = Keys.Control | Keys.C
            };
            var pasteItem = new ToolStripMenuItem("粘贴节点 (Ctrl+V)", null, OnPasteNode)
            {
                ShortcutKeys = Keys.Control | Keys.V
            };
            var deleteItem = new ToolStripMenuItem("删除节点 (Delete)", null, OnDeleteNode)
            {
                ShortcutKeys = Keys.Delete
            };

            menu.Items.Add(copyItem);
            menu.Items.Add(pasteItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(deleteItem);
            menu.Items.Add(new ToolStripSeparator());

            // 对齐工具
            var alignMenu = new ToolStripMenuItem("对齐节点");
            alignMenu.DropDownItems.Add("左对齐", null, (s, e) => AlignNodes(AlignmentType.Left));
            alignMenu.DropDownItems.Add("右对齐", null, (s, e) => AlignNodes(AlignmentType.Right));
            alignMenu.DropDownItems.Add("顶部对齐", null, (s, e) => AlignNodes(AlignmentType.Top));
            alignMenu.DropDownItems.Add("底部对齐", null, (s, e) => AlignNodes(AlignmentType.Bottom));
            alignMenu.DropDownItems.Add(new ToolStripSeparator());
            alignMenu.DropDownItems.Add("水平居中", null, (s, e) => AlignNodes(AlignmentType.HorizontalCenter));
            alignMenu.DropDownItems.Add("垂直居中", null, (s, e) => AlignNodes(AlignmentType.VerticalCenter));

            menu.Items.Add(alignMenu);
            menu.Items.Add(new ToolStripSeparator());

            // 分布工具
            var distributeMenu = new ToolStripMenuItem("分布节点");
            distributeMenu.DropDownItems.Add("水平分布", null, (s, e) => DistributeNodes(true));
            distributeMenu.DropDownItems.Add("垂直分布", null, (s, e) => DistributeNodes(false));

            menu.Items.Add(distributeMenu);
            menu.Items.Add(new ToolStripSeparator());

            // 配置
            var configItem = new ToolStripMenuItem("配置节点 (双击)", null, OnConfigNode);
            menu.Items.Add(configItem);

            // 右键菜单打开前检查
            menu.Opening += (s, e) =>
            {
                var selectedNodes = _nodeEditor.GetSelectedNode();
                bool hasSelection = selectedNodes != null && selectedNodes.Length > 0;
                bool hasCopied = _copiedNode != null;

                copyItem.Enabled = hasSelection;
                pasteItem.Enabled = hasCopied;
                deleteItem.Enabled = hasSelection;
                alignMenu.Enabled = selectedNodes != null && selectedNodes.Length >= 2;
                distributeMenu.Enabled = selectedNodes != null && selectedNodes.Length >= 3;
                configItem.Enabled = _nodeEditor.ActiveNode != null;
            };

            return menu;
        }

        private void OnCopyNode(object sender, EventArgs e)
        {
            _copiedNode = _nodeEditor.ActiveNode;
            if (_copiedNode != null)
            {
                MessageBox.Show("节点已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnPasteNode(object sender, EventArgs e)
        {
            if (_copiedNode == null) return;

            try
            {
                // 克隆节点
                var newNode = CloneNode(_copiedNode);
                if (newNode != null)
                {
                    // 偏移位置
                    newNode.Left = _copiedNode.Left + 30;
                    newNode.Top = _copiedNode.Top + 30;

                    _nodeEditor.Nodes.Add(newNode);
                    IsDirty = true;
                    WorkflowChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"粘贴失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDeleteNode(object sender, EventArgs e)
        {
            DeleteSelectedNodes();
        }

        private void OnConfigNode(object sender, EventArgs e)
        {
            if (_nodeEditor.ActiveNode is WorkflowNodeBase node)
            {
                var args = new NodeDoubleClickEventArgs(node);
                NodeDoubleClick?.Invoke(this, args);

                if (!args.Handled)
                {
                    node.OpenConfigDialog();
                }
            }
        }

        #endregion

        #region 功能3: 节点对齐

        private enum AlignmentType
        {
            Left, Right, Top, Bottom, HorizontalCenter, VerticalCenter
        }

        /// <summary>
        /// 对齐选中的节点
        /// </summary>
        private void AlignNodes(AlignmentType alignType)
        {
            var selectedNodes = _nodeEditor.GetSelectedNode();
            if (selectedNodes == null || selectedNodes.Length < 2) return;

            switch (alignType)
            {
                case AlignmentType.Left:
                    int minLeft = selectedNodes.Min(n => n.Left);
                    foreach (var node in selectedNodes)
                        node.Left = minLeft;
                    break;

                case AlignmentType.Right:
                    int maxRight = selectedNodes.Max(n => n.Left + n.Width);
                    foreach (var node in selectedNodes)
                        node.Left = maxRight - node.Width;
                    break;

                case AlignmentType.Top:
                    int minTop = selectedNodes.Min(n => n.Top);
                    foreach (var node in selectedNodes)
                        node.Top = minTop;
                    break;

                case AlignmentType.Bottom:
                    int maxBottom = selectedNodes.Max(n => n.Top + n.Height);
                    foreach (var node in selectedNodes)
                        node.Top = maxBottom - node.Height;
                    break;

                case AlignmentType.HorizontalCenter:
                    int avgLeft = (int)selectedNodes.Average(n => n.Left + n.Width / 2);
                    foreach (var node in selectedNodes)
                        node.Left = avgLeft - node.Width / 2;
                    break;

                case AlignmentType.VerticalCenter:
                    int avgTop = (int)selectedNodes.Average(n => n.Top + n.Height / 2);
                    foreach (var node in selectedNodes)
                        node.Top = avgTop - node.Height / 2;
                    break;
            }

            _nodeEditor.Invalidate();
            IsDirty = true;
        }

        /// <summary>
        /// 分布节点
        /// </summary>
        private void DistributeNodes(bool horizontal)
        {
            var selectedNodes = _nodeEditor.GetSelectedNode();
            if (selectedNodes == null || selectedNodes.Length < 3) return;

            var sortedNodes = horizontal
                ? selectedNodes.OrderBy(n => n.Left).ToArray()
                : selectedNodes.OrderBy(n => n.Top).ToArray();

            if (horizontal)
            {
                int totalWidth = sortedNodes.Sum(n => n.Width);
                int span = sortedNodes.Last().Left - sortedNodes.First().Left;
                int gap = (span - totalWidth) / (sortedNodes.Length - 1);

                int currentX = sortedNodes.First().Left;
                foreach (var node in sortedNodes)
                {
                    node.Left = currentX;
                    currentX += node.Width + gap;
                }
            }
            else
            {
                int totalHeight = sortedNodes.Sum(n => n.Height);
                int span = sortedNodes.Last().Top - sortedNodes.First().Top;
                int gap = (span - totalHeight) / (sortedNodes.Length - 1);

                int currentY = sortedNodes.First().Top;
                foreach (var node in sortedNodes)
                {
                    node.Top = currentY;
                    currentY += node.Height + gap;
                }
            }

            _nodeEditor.Invalidate();
            IsDirty = true;
        }

        #endregion

        #region 功能4: 缩放控制

        /// <summary>
        /// 初始化缩放显示 - 添加到工具栏
        /// </summary>
        private void InitializeZoomControls()
        {
            // 在工具栏添加缩放控制
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 缩放显示标签
            var zoomLabelItem = new ToolStripLabel("缩放: 100%")
            {
                Name = "_zoomLabelItem",
                AutoSize = false,
                Width = 230,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White
            };
            _toolStrip.Items.Add(zoomLabelItem);

            // 放大按钮
            var btnZoomIn = new ToolStripButton("🔍 放大", null, OnZoomInClick)
            {
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "放大 (Ctrl + 加号)"
            };

            // 缩小按钮
            var btnZoomOut = new ToolStripButton("🔍 缩小", null, OnZoomOutClick)
            {
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "缩小 (Ctrl + 减号)"
            };

            // 重置按钮
            var btnZoomReset = new ToolStripButton("🔍 重置", null, OnZoomResetClick)
            {
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "重置缩放 (Ctrl+0)"
            };

            _toolStrip.Items.Add(btnZoomIn);
            _toolStrip.Items.Add(btnZoomOut);
            _toolStrip.Items.Add(btnZoomReset);

            // 监听缩放变化
            //_nodeEditor.CanvasScaleChanged += (s, e) => UpdateZoomLabel();
            _nodeEditor.CanvasScaled += (s, e) => UpdateZoomLabel();
        }

        private void UpdateZoomLabel()
        {
            if (_toolStrip.Items["_zoomLabelItem"] is ToolStripLabel zoomLabelItem)
            {
                int zoomPercent = (int)(_nodeEditor.CanvasScale * 100);
                zoomLabelItem.Text = $"缩放: {zoomPercent}%";
            }
        }

        #endregion

        #region 功能5: 小地图/缩略图

        /// <summary>
        /// 初始化小地图
        /// </summary>
        private void InitializeMiniMap()
        {
            _miniMap = new PictureBox
            {
                Size = new Size(200, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(25, 25, 25),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // 定位到右上角
            _miniMap.Location = new Point(
                _nodeEditor.Width - _miniMap.Width - 10,
                10
            );

            // 添加到节点编辑器
            _nodeEditor.Controls.Add(_miniMap);
            _miniMap.BringToFront();

            // 鼠标点击小地图跳转
            _miniMap.MouseClick += OnMiniMapClick;

            // 定时更新小地图
            //var miniMapTimer = new Timer { Interval = 1000 };
            //miniMapTimer.Tick += (s, e) => UpdateMiniMap();
            //miniMapTimer.Start();
        }

        //private void UpdateMiniMap()
        //{
        //    if (_nodeEditor.Nodes.Count == 0)
        //    {
        //        _miniMap.Image = null;
        //        return;
        //    }

        //    try
        //    {
        //        // 计算所有节点的边界
        //        int minX = _nodeEditor.Nodes.Min(n => n.Left);
        //        int minY = _nodeEditor.Nodes.Min(n => n.Top);
        //        int maxX = _nodeEditor.Nodes.Max(n => n.Left + n.Width);
        //        int maxY = _nodeEditor.Nodes.Max(n => n.Top + n.Height);

        //        int width = maxX - minX + 40;
        //        int height = maxY - minY + 40;

        //        if (width <= 0 || height <= 0) return;

        //        // 创建缩略图
        //        var bitmap = new Bitmap(_miniMap.Width, _miniMap.Height);
        //        using (var g = Graphics.FromImage(bitmap))
        //        {
        //            g.Clear(_miniMap.BackColor);

        //            float scaleX = (float)_miniMap.Width / width;
        //            float scaleY = (float)_miniMap.Height / height;
        //            float scale = Math.Min(scaleX, scaleY);

        //            // 绘制节点
        //            foreach (var node in _nodeEditor.Nodes)
        //            {
        //                int x = (int)((node.Left - minX + 20) * scale);
        //                int y = (int)((node.Top - minY + 20) * scale);
        //                int w = Math.Max(2, (int)(node.Width * scale));
        //                int h = Math.Max(2, (int)(node.Height * scale));

        //                var color = node == _nodeEditor.ActiveNode
        //                    ? Color.Yellow
        //                    : Color.FromArgb(100, 150, 200);

        //                g.FillRectangle(new SolidBrush(color), x, y, w, h);
        //                g.DrawRectangle(Pens.White, x, y, w, h);
        //            }
        //        }

        //        _miniMap.Image?.Dispose();
        //        _miniMap.Image = bitmap;
        //    }
        //    catch
        //    {
        //        // 忽略错误
        //    }
        //}

        private void OnMiniMapClick(object sender, MouseEventArgs e)
        {
            // 根据点击位置调整视图
            // 这里需要根据实际情况实现
        }

        #endregion

        #region 功能6: 辅助方法

        /// <summary>
        /// 克隆节点
        /// </summary>
        internal static STNode CloneNode(STNode source)
        {
            if (source is not WorkflowNodeBase workflowNode) return null;
            // 使用工厂创建相同类型的节点
            var newNode = WorkflowNodeFactory.CreateNode(workflowNode.StepName);
            if (newNode == null || workflowNode.StepParameter == null) return newNode;
            // 深度复制参数 (需要参数类实现 ICloneable 或序列化)
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(workflowNode.StepParameter);
                var paramType = workflowNode.StepParameter.GetType();
                newNode.StepParameter = Newtonsoft.Json.JsonConvert.DeserializeObject(json, paramType);
            }
            catch
            {
                // 复制失败，使用默认参数
            }
            return newNode;
        }

        /// <summary>
        /// 增强的键盘快捷键处理
        /// </summary>
        private void OnEditorKeyDownEnhanced(object sender, KeyEventArgs e)
        {
            switch (e.Control)
            {
                // 复制
                case true when e.KeyCode == Keys.C:
                    OnCopyNode(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                // 粘贴
                case true when e.KeyCode == Keys.V:
                    OnPasteNode(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                // 缩放
                case true when e.KeyCode == Keys.Add:
                    OnZoomInClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case true when e.KeyCode == Keys.Subtract:
                    OnZoomOutClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case true when e.KeyCode == Keys.D0:
                    OnZoomResetClick(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
            }
        }

        #endregion
    }

    #region 事件参数类

    /// <summary>
    /// 节点选中事件参数
    /// </summary>
    public class NodeSelectedEventArgs(WorkflowNodeBase node) : EventArgs
    {
        public WorkflowNodeBase Node { get; } = node;
    }

    /// <summary>
    /// 节点双击事件参数
    /// </summary>
    public class NodeDoubleClickEventArgs(WorkflowNodeBase node) : EventArgs
    {
        public WorkflowNodeBase Node { get; } = node;
        public bool Handled { get; set; } = false;
    }

    /// <summary>
    /// 验证结果事件参数
    /// </summary>
    public class ValidationResultEventArgs(ValidationResult result) : EventArgs
    {
        public ValidationResult Result { get; } = result;
    }

    #endregion
}