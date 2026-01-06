using MainUI.LogicalConfiguration.Engine;
using MainUI.LogicalConfiguration.NodeEditor.Nodes;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 节点配置面板基类 - 所有嵌入式配置控件的父类
    /// </summary>
    public class NodeConfigPanelBase : UserControl
    {
        #region 事件

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// 配置完成事件（用户确认保存）
        /// </summary>
        public event EventHandler<ConfigSavedEventArgs> ConfigurationSaved;

        #endregion

        #region 属性

        /// <summary>
        /// 关联的节点
        /// </summary>
        protected WorkflowNodeBase _node;

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool HasChanges { get; protected set; }

        /// <summary>
        /// 支持的节点类型名称
        /// </summary>
        protected virtual string SupportedStepName { get; }

        /// <summary>
        /// 面板标题
        /// </summary>
        public virtual string PanelTitle { get; }

        #endregion

        #region 构造函数

        protected NodeConfigPanelBase()
        {
            InitializeBaseUI();
        }

        private void InitializeBaseUI()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(8);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 绑定节点 - 从节点加载配置到界面
        /// </summary>
        public virtual void BindNode(WorkflowNodeBase node)
        {
            _node = node;
            HasChanges = false;
            LoadFromNode();
        }

        /// <summary>
        /// 解绑节点 - 清空界面
        /// </summary>
        public virtual void UnbindNode()
        {
            if (HasChanges)
            {
                // 可选：提示保存
            }
            _node = null;
            HasChanges = false;
            ClearUI();
        }

        /// <summary>
        /// 保存配置到节点
        /// </summary>
        public virtual bool SaveToNode()
        {
            if (_node == null) return false;

            try
            {
                ApplyToNode();
                _node.IsConfigured = true;
                _node.RefreshDisplay();
                HasChanges = false;

                ConfigurationSaved?.Invoke(this, new ConfigSavedEventArgs(_node, true));
                return true;
            }
            catch (Exception ex)
            {
                ConfigurationSaved?.Invoke(this, new ConfigSavedEventArgs(_node, false, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public virtual ValidationResult ValidateConfig()
        {
            return new ValidationResult { IsValid = true };
        }

        #endregion

        #region 抽象方法 - 子类必须实现

        /// <summary>
        /// 从节点加载数据到UI
        /// </summary>
        protected virtual void LoadFromNode()
        {
        }

        /// <summary>
        /// 将UI数据应用到节点
        /// </summary>
        protected virtual void ApplyToNode()
        {
        }

        /// <summary>
        /// 清空UI
        /// </summary>
        protected virtual void ClearUI()
        {
             
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 标记有更改
        /// </summary>
        protected void MarkAsChanged()
        {
            HasChanges = true;
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        protected Label CreateLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("微软雅黑", 9F),
                Location = new Point(8, top),
                AutoSize = true
            };
        }

        /// <summary>
        /// 创建文本框
        /// </summary>
        protected TextBox CreateTextBox(int top, int width = 200)
        {
            var txt = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, top),
                Width = width
            };
            txt.TextChanged += (s, e) => MarkAsChanged();
            return txt;
        }

        /// <summary>
        /// 创建数值输入框
        /// </summary>
        protected NumericUpDown CreateNumericInput(int top, decimal min = 0, decimal max = 100000, int decimalPlaces = 0)
        {
            var num = new NumericUpDown
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Location = new Point(8, top),
                Width = 120,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimalPlaces
            };
            num.ValueChanged += (s, e) => MarkAsChanged();
            return num;
        }

        /// <summary>
        /// 创建下拉框
        /// </summary>
        protected ComboBox CreateComboBox(int top, int width = 200)
        {
            var cmb = new ComboBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(8, top),
                Width = width
            };
            cmb.SelectedIndexChanged += (s, e) => MarkAsChanged();
            return cmb;
        }

        /// <summary>
        /// 创建复选框
        /// </summary>
        protected CheckBox CreateCheckBox(string text, int top)
        {
            var chk = new CheckBox
            {
                Text = text,
                ForeColor = Color.White,
                Location = new Point(8, top),
                AutoSize = true
            };
            chk.CheckedChanged += (s, e) => MarkAsChanged();
            return chk;
        }

        #endregion
    }

    #region 事件参数类

    /// <summary>
    /// 配置保存事件参数
    /// </summary>
    public class ConfigSavedEventArgs : EventArgs
    {
        public WorkflowNodeBase Node { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }

        public ConfigSavedEventArgs(WorkflowNodeBase node, bool success, string errorMessage = null)
        {
            Node = node;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }


    #endregion
}
