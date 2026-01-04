using MainUI.LogicalConfiguration.NodeEditor.Controls;
using MainUI.LogicalConfiguration.NodeEditor.Core;

namespace MainUI.LogicalConfiguration.NodeEditor.Forms
{
    /// <summary>
    /// 工作流设计器窗体 - 独立的可视化工作流编辑窗口
    /// </summary>
    public partial class WorkflowDesignerForm : Form
    {
        #region 私有字段

        private List<ChildModel> _originalModels;
        private Action<List<ChildModel>> _onSaveCallback;

        #endregion

        #region 属性

        /// <summary>
        /// 设计器面板
        /// </summary>
        public WorkflowDesignerPanel DesignerPanel => _designerPanel;

        /// <summary>
        /// 对话框结果数据
        /// </summary>
        public List<ChildModel> ResultModels { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public WorkflowDesignerForm()
        {
            InitializeComponent();
            InitializeDesigner();

            // 添加菜单栏
            CreateMenuStrip();
        }

        /// <summary>
        /// 带初始数据的构造函数
        /// </summary>
        /// <param name="initialModels">初始的步骤模型列表</param>
        /// <param name="onSave">保存回调</param>
        public WorkflowDesignerForm(List<ChildModel> initialModels, Action<List<ChildModel>> onSave = null)
            : this()
        {
            _originalModels = initialModels;
            _onSaveCallback = onSave;

            if (initialModels != null && initialModels.Count > 0)
            {
                _designerPanel.LoadFromChildModels(initialModels);
            }
        }

        #endregion

        #region 初始化

        private void InitializeDesigner()
        {
            // 绑定设计器事件
            _designerPanel.NodeSelected += OnNodeSelected;
            _designerPanel.NodeDoubleClick += OnNodeDoubleClick;
            _designerPanel.WorkflowChanged += OnWorkflowChanged;
            _designerPanel.ValidationCompleted += OnValidationCompleted;

            // 绑定编辑器缩放事件
            _designerPanel.NodeEditor.CanvasScaled += OnCanvasScaled;

            // 初始化新工作流
            _designerPanel.NewWorkflow();
            UpdateStatusBar();
        }

        #endregion

        #region 菜单事件

        private void OnMenuNew(object sender, EventArgs e)
        {
            if (_designerPanel.IsDirty)
            {
                var result = MessageBox.Show("当前工作流有未保存的更改，是否保存？",
                    "确认", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    OnMenuSave(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            _designerPanel.NewWorkflow();
            UpdateStatusBar();
        }

        private void OnMenuOpen(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "工作流文件 (*.stn)|*.stn|JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                dialog.Title = "打开工作流";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _designerPanel.LoadWorkflow(dialog.FileName);
                    UpdateStatusBar();
                }
            }
        }

        private void OnMenuSave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_designerPanel.CurrentFilePath))
            {
                OnMenuSaveAs(sender, e);
            }
            else
            {
                _designerPanel.SaveWorkflow(_designerPanel.CurrentFilePath);
            }
        }

        private void OnMenuSaveAs(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "工作流文件 (*.stn)|*.stn|JSON文件 (*.json)|*.json";
                dialog.Title = "保存工作流";
                dialog.DefaultExt = "stn";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _designerPanel.SaveWorkflow(dialog.FileName);
                }
            }
        }

        private void OnMenuExport(object sender, EventArgs e)
        {
            var models = _designerPanel.ExportToChildModels();

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON文件 (*.json)|*.json";
                dialog.Title = "导出步骤列表";
                dialog.DefaultExt = "json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(models, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show($"已导出 {models.Count} 个步骤", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnMenuImport(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON文件 (*.json)|*.json";
                dialog.Title = "导入步骤列表";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = System.IO.File.ReadAllText(dialog.FileName);
                        var models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChildModel>>(json);
                        _designerPanel.LoadFromChildModels(models);
                        MessageBox.Show($"已导入 {models.Count} 个步骤", "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateStatusBar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnMenuExit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnMenuDelete(object sender, EventArgs e)
        {
            _designerPanel.DeleteSelectedNodes();
            UpdateStatusBar();
        }

        private void OnMenuSelectAll(object sender, EventArgs e)
        {
            _designerPanel.SelectAllNodes();
        }

        private void OnMenuZoomIn(object sender, EventArgs e)
        {
            _designerPanel.NodeEditor.ScaleCanvas(1.2f,
                _designerPanel.NodeEditor.Width / 2,
                _designerPanel.NodeEditor.Height / 2);
        }

        private void OnMenuZoomOut(object sender, EventArgs e)
        {
            _designerPanel.NodeEditor.ScaleCanvas(0.8f,
                _designerPanel.NodeEditor.Width / 2,
                _designerPanel.NodeEditor.Height / 2);
        }

        private void OnMenuZoomReset(object sender, EventArgs e)
        {
            var scale = _designerPanel.NodeEditor.CanvasScale;
            _designerPanel.NodeEditor.ScaleCanvas(1f / scale,
                _designerPanel.NodeEditor.Width / 2,
                _designerPanel.NodeEditor.Height / 2);
        }

        private void OnMenuAutoLayout(object sender, EventArgs e)
        {
            _designerPanel.Converter.AutoLayoutNodes();
            _designerPanel.NodeEditor.Invalidate();
        }

        private void OnMenuValidate(object sender, EventArgs e)
        {
            var result = _designerPanel.ValidateWorkflow();

            if (result.HasErrors)
            {
                MessageBox.Show(
                    string.Join("\n", result.Messages.FindAll(m => m.Level == ValidationLevel.Error).ConvertAll(m => m.Message)),
                    "验证错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (result.HasWarnings)
            {
                MessageBox.Show(
                    string.Join("\n", result.Messages.FindAll(m => m.Level == ValidationLevel.Warning).ConvertAll(m => m.Message)),
                    "验证警告",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("工作流验证通过！", "验证成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnMenuHelp(object sender, EventArgs e)
        {
            var helpText = @"工作流设计器使用说明

1. 基本操作:
   - 从左侧节点树拖拽节点到画布
   - 拖动节点的连接点创建连接
   - 双击节点打开配置对话框
   - Delete 键删除选中的节点

2. 画布操作:
   - 鼠标滚轮缩放画布
   - 按住中键或空格+左键拖动画布
   - Ctrl+A 全选节点

3. 节点类型:
   - 开始/结束: 工作流的起点和终点
   - 条件判断: 根据条件执行不同分支
   - 循环: 重复执行一组步骤
   - 延时等待: 暂停执行
   - 读取/写入PLC: PLC通信操作
   - 变量赋值: 设置变量值

4. 保存和加载:
   - .stn 格式: 保存完整的节点图
   - .json 格式: 保存为步骤列表";

            MessageBox.Show(helpText, "使用说明", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnMenuAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "工作流设计器 v1.0\n\n基于 STNodeEditor 构建\n\n用于可视化配置工作流程",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        #region 设计器事件

        private void OnNodeSelected(object sender, NodeSelectedEventArgs e)
        {
            if (e.Node != null)
            {
                _statusLabel.Text = $"选中: {e.Node.DisplayName}";
            }
            else
            {
                _statusLabel.Text = "就绪";
            }
        }

        private void OnNodeDoubleClick(object sender, NodeDoubleClickEventArgs e)
        {
            // 可以在这里自定义双击行为
            // 例如打开自定义的配置窗体

            // 默认行为由 WorkflowDesignerPanel 处理
        }

        private void OnWorkflowChanged(object sender, EventArgs e)
        {
            UpdateStatusBar();
            UpdateTitle();
        }

        private void OnValidationCompleted(object sender, ValidationResultEventArgs e)
        {
            // 可以在这里更新状态栏显示验证结果
        }

        private void OnCanvasScaled(object sender, EventArgs e)
        {
            float scale = _designerPanel.NodeEditor.CanvasScale;
            _zoomLabel.Text = $"缩放: {scale * 100:F0}%";
        }

        #endregion

        #region 辅助方法

        private void UpdateStatusBar()
        {
            int nodeCount = _designerPanel.NodeEditor.Nodes.Count;
            _nodeCountLabel.Text = $"节点: {nodeCount}";
        }

        private void UpdateTitle()
        {
            string title = "工作流设计器";

            if (!string.IsNullOrEmpty(_designerPanel.CurrentFilePath))
            {
                title += $" - {System.IO.Path.GetFileName(_designerPanel.CurrentFilePath)}";
            }

            if (_designerPanel.IsDirty)
            {
                title += " *";
            }

            this.Text = title;
        }

        private void AddNodeAtCenter(string stepName)
        {
            int x = _designerPanel.NodeEditor.Width / 2 - 90;
            int y = _designerPanel.NodeEditor.Height / 2 - 30;

            // 转换为画布坐标
            var canvasPoint = _designerPanel.NodeEditor.ControlToCanvas(new Point(x, y));

            _designerPanel.AddNode(stepName, canvasPoint.X, canvasPoint.Y);
        }

        #endregion

        #region 重写方法

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_designerPanel.IsDirty)
            {
                var result = MessageBox.Show("当前工作流有未保存的更改，是否保存？",
                    "确认", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    OnMenuSave(this, EventArgs.Empty);

                    // 如果保存后仍有未保存状态（用户取消了保存对话框）
                    if (_designerPanel.IsDirty)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // 设置结果数据
            ResultModels = _designerPanel.ExportToChildModels();

            // 调用保存回调
            if (_onSaveCallback != null && this.DialogResult == DialogResult.OK)
            {
                _onSaveCallback(ResultModels);
            }

            base.OnFormClosing(e);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 以对话框方式显示
        /// </summary>
        /// <returns>导出的步骤列表（如果用户点击确定）</returns>
        public List<ChildModel> ShowDialogAndGetResult()
        {
            // 添加确定/取消按钮
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var btnOk = new Button
            {
                Text = "确定",
                Size = new Size(100, 30),
                Location = new Point(this.Width - 230, 10),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 30),
                Location = new Point(this.Width - 120, 10),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };

            buttonPanel.Controls.AddRange(new Control[] { btnOk, btnCancel });
            this.Controls.Add(buttonPanel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            if (this.ShowDialog() == DialogResult.OK)
            {
                return _designerPanel.ExportToChildModels();
            }

            return null;
        }

        #endregion
    }
}