using MainUI.LogicalConfiguration.NodeEditor.Nodes;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 节点配置宿主面板 - 承载动态配置控件
    /// 根据选中的节点类型自动加载对应的配置面板
    /// </summary>
    public class NodeConfigHostPanel : UserControl
    {
        #region 私有字段

        private Panel _headerPanel;
        private Label _titleLabel;
        private Button _btnSave;
        private Button _btnReset;
        private Panel _contentPanel;
        private Label _emptyLabel;

        private NodeConfigPanelBase _currentPanel;
        private WorkflowNodeBase _currentNode;

        /// <summary>
        /// 配置面板注册表
        /// </summary>
        private readonly Dictionary<string, Func<NodeConfigPanelBase>> _panelFactories 
            = new(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region 事件

        /// <summary>
        /// 配置保存事件
        /// </summary>
        public event EventHandler<ConfigSavedEventArgs> ConfigurationSaved;

        /// <summary>
        /// 配置变更事件（有未保存的更改）
        /// </summary>
        public event EventHandler ConfigurationChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 当前配置面板
        /// </summary>
        public NodeConfigPanelBase CurrentPanel => _currentPanel;

        /// <summary>
        /// 当前节点
        /// </summary>
        public WorkflowNodeBase CurrentNode => _currentNode;

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool HasChanges => _currentPanel?.HasChanges ?? false;

        #endregion

        #region 构造函数

        public NodeConfigHostPanel()
        {
            InitializeUi();
            RegisterDefaultPanels();
        }

        private void InitializeUi()
        {
            this.BackColor = Color.FromArgb(35, 35, 35);
            this.Dock = DockStyle.Fill;
            this.MinimumSize = new Size(180, 200);

            // 头部面板
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(8, 8, 8, 8)
            };

            // 标题标签
            _titleLabel = new Label
            {
                Text = "节点配置",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Location = new Point(8, 12),
                AutoSize = true
            };

            // 保存按钮
            _btnSave = new Button
            {
                Text = "✓ 保存",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Size = new Size(60, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            // 重置按钮
            _btnReset = new Button
            {
                Text = "↺",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Size = new Size(30, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += BtnReset_Click;

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_btnReset);
            _headerPanel.Controls.Add(_btnSave);

            // 调整按钮位置
            _headerPanel.Resize += (s, e) =>
            {
                _btnSave.Location = new Point(_headerPanel.Width - _btnSave.Width - 8, 8);
                _btnReset.Location = new Point(_btnSave.Left - _btnReset.Width - 5, 8);
            };

            // 内容面板
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                AutoScroll = true,
                Padding = new Padding(4)
            };

            // 空状态提示
            _emptyLabel = new Label
            {
                Text = "请选择一个节点\n进行配置",
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("微软雅黑", 10F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            _contentPanel.Controls.Add(_emptyLabel);

            this.Controls.Add(_contentPanel);
            this.Controls.Add(_headerPanel);
        }

        #endregion

        #region 面板注册

        /// <summary>
        /// 注册默认的配置面板
        /// </summary>
        private void RegisterDefaultPanels()
        {
            // 延时等待
            RegisterPanel("DelayWait", () => new DelayWaitConfigPanel());
            
            // 条件判断
            RegisterPanel("ConditionJudge", () => new ConditionConfigPanel());
            
            // 循环
            RegisterPanel("CycleBegins", () => new LoopConfigPanel());
            
            // PLC读取
            RegisterPanel("PLCRead", () => new PLCReadConfigPanel());
            
            // PLC写入
            RegisterPanel("PLCWrite", () => new PLCWriteConfigPanel());
            
            // 变量赋值
            RegisterPanel("VariableAssign", () => new VariableAssignConfigPanel());
            
            // 等待稳定
            RegisterPanel("Waitingforstability", () => new WaitStableConfigPanel());
            
            // 消息通知
            RegisterPanel("MessageNotify", () => new MessageNotifyConfigPanel());
        }

        /// <summary>
        /// 注册配置面板
        /// </summary>
        public void RegisterPanel(string stepName, Func<NodeConfigPanelBase> factory)
        {
            _panelFactories[stepName] = factory;
        }

        /// <summary>
        /// 注册配置面板（泛型版本）
        /// </summary>
        public void RegisterPanel<T>(string stepName) where T : NodeConfigPanelBase, new()
        {
            _panelFactories[stepName] = () => new T();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置当前节点 - 自动加载对应的配置面板
        /// </summary>
        public void SetNode(WorkflowNodeBase node)
        {
            // 如果是同一个节点，不重复加载
            if (_currentNode == node) return;

            // 清理当前面板
            ClearCurrentPanel();

            _currentNode = node;

            if (node == null)
            {
                ShowEmptyState();
                return;
            }

            // 尝试创建对应的配置面板
            if (_panelFactories.TryGetValue(node.StepName, out var factory))
            {
                try
                {
                    _currentPanel = factory();
                    _currentPanel.Dock = DockStyle.Fill;
                    
                    // 订阅事件
                    _currentPanel.ConfigurationChanged += Panel_ConfigurationChanged;
                    _currentPanel.ConfigurationSaved += Panel_ConfigurationSaved;
                    
                    // 绑定节点
                    _currentPanel.BindNode(node);

                    // 显示面板
                    _contentPanel.Controls.Clear();
                    _contentPanel.Controls.Add(_currentPanel);
                    
                    // 更新标题
                    _titleLabel.Text = $"⚙ {_currentPanel.PanelTitle}";
                    _btnSave.Enabled = false;
                    _btnReset.Enabled = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"创建配置面板失败: {ex.Message}");
                    ShowGenericPanel(node);
                }
            }
            else
            {
                // 没有专门的配置面板，显示通用面板
                ShowGenericPanel(node);
            }
        }

        /// <summary>
        /// 保存当前配置
        /// </summary>
        public bool SaveCurrentConfig()
        {
            return _currentPanel?.SaveToNode() ?? false;
        }

        /// <summary>
        /// 重置配置（重新从节点加载）
        /// </summary>
        public void ResetConfig()
        {
            if (_currentNode != null && _currentPanel != null)
            {
                _currentPanel.BindNode(_currentNode);
                _btnSave.Enabled = false;
            }
        }

        /// <summary>
        /// 清空面板
        /// </summary>
        public void Clear()
        {
            ClearCurrentPanel();
            _currentNode = null;
            ShowEmptyState();
        }

        #endregion

        #region 私有方法

        private void ClearCurrentPanel()
        {
            if (_currentPanel != null)
            {
                _currentPanel.ConfigurationChanged -= Panel_ConfigurationChanged;
                _currentPanel.ConfigurationSaved -= Panel_ConfigurationSaved;
                _currentPanel.UnbindNode();
                _currentPanel.Dispose();
                _currentPanel = null;
            }
        }

        private void ShowEmptyState()
        {
            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(_emptyLabel);
            _titleLabel.Text = "节点配置";
            _btnSave.Enabled = false;
            _btnReset.Enabled = false;
        }

        private void ShowGenericPanel(WorkflowNodeBase node)
        {
            // 显示通用配置面板（备注+基本属性）
            var genericPanel = new GenericNodeConfigPanel();
            genericPanel.Dock = DockStyle.Fill;
            genericPanel.ConfigurationChanged += Panel_ConfigurationChanged;
            genericPanel.ConfigurationSaved += Panel_ConfigurationSaved;
            genericPanel.BindNode(node);

            _currentPanel = genericPanel;
            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(genericPanel);

            _titleLabel.Text = $"⚙ {node.DisplayName}";
            _btnSave.Enabled = false;
            _btnReset.Enabled = true;
        }

        #endregion

        #region 事件处理

        private void Panel_ConfigurationChanged(object sender, EventArgs e)
        {
            _btnSave.Enabled = true;
            ConfigurationChanged?.Invoke(this, e);
        }

        private void Panel_ConfigurationSaved(object sender, ConfigSavedEventArgs e)
        {
            _btnSave.Enabled = false;
            ConfigurationSaved?.Invoke(this, e);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveCurrentConfig();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (HasChanges)
            {
                var result = MessageBox.Show(
                    "确定要放弃当前更改吗？",
                    "确认重置",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            ResetConfig();
        }

        #endregion
    }
}
