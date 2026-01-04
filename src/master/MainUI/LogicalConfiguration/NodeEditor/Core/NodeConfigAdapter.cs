using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.Parameter;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 节点配置适配器 - 负责打开节点对应的配置窗体
    /// 将现有的参数配置窗体与节点编辑器集成
    /// </summary>
    public class NodeConfigAdapter
    {
        #region 单例

        private static NodeConfigAdapter _instance;
        private static readonly object _lock = new object();

        public static NodeConfigAdapter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new NodeConfigAdapter();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region 私有字段

        /// <summary>
        /// StepName 到配置窗体类型的映射
        /// </summary>
        private readonly Dictionary<string, Type> _configFormTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// StepName 到配置窗体工厂的映射
        /// </summary>
        private readonly Dictionary<string, Func<WorkflowNodeBase, Form>> _configFormFactories = new Dictionary<string, Func<WorkflowNodeBase, Form>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 服务提供者（用于依赖注入）
        /// </summary>
        private IServiceProvider _serviceProvider;

        #endregion

        #region 初始化

        private NodeConfigAdapter()
        {
            RegisterDefaultForms();
        }

        /// <summary>
        /// 设置服务提供者
        /// </summary>
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 注册默认的配置窗体
        /// </summary>
        private void RegisterDefaultForms()
        {
            // 注册内置的配置窗体工厂
            // 使用工厂方法而不是类型，以便更灵活地创建窗体

            // 延时等待 - 使用现有的 Form_DelayTime
            RegisterFormFactory("DelayWait", node =>
            {
                return CreateDelayTimeForm(node);
            });

            // 条件判断 - 使用现有的 Form_Detection
            RegisterFormFactory("ConditionJudge", node =>
            {
                return CreateDetectionForm(node);
            });

            // 循环 - 使用现有的 Form_Loop
            RegisterFormFactory("CycleBegins", node =>
            {
                return CreateLoopForm(node);
            });

            // 等待稳定 - 使用现有的 Form_WaitForStable
            RegisterFormFactory("Waitingforstability", node =>
            {
                return CreateWaitForStableForm(node);
            });

            // 变量赋值 - 使用现有的 Form_VariableAssignment
            RegisterFormFactory("VariableAssign", node =>
            {
                return CreateVariableAssignForm(node);
            });

            // 读取PLC - 使用现有的 Form_ReadPLC
            RegisterFormFactory("PLCRead", node =>
            {
                return CreateReadPLCForm(node);
            });

            // 写入PLC - 使用现有的 Form_WritePLC
            RegisterFormFactory("PLCWrite", node =>
            {
                return CreateWritePLCForm(node);
            });

            // 消息通知
            RegisterFormFactory("MessageNotify", node =>
            {
                return CreateMessageNotifyForm(node);
            });

            // 实时监控
            RegisterFormFactory("MonitorTool", node =>
            {
                return CreateRealtimeMonitorForm(node);
            });
        }

        #endregion

        #region 注册方法

        /// <summary>
        /// 注册配置窗体类型
        /// </summary>
        public void RegisterFormType<TForm>(string stepName) where TForm : Form
        {
            _configFormTypes[stepName] = typeof(TForm);
        }

        /// <summary>
        /// 注册配置窗体工厂
        /// </summary>
        public void RegisterFormFactory(string stepName, Func<WorkflowNodeBase, Form> factory)
        {
            _configFormFactories[stepName] = factory;
        }

        /// <summary>
        /// 取消注册
        /// </summary>
        public void UnregisterForm(string stepName)
        {
            _configFormTypes.Remove(stepName);
            _configFormFactories.Remove(stepName);
        }

        #endregion

        #region 打开配置窗体

        /// <summary>
        /// 打开节点的配置窗体
        /// </summary>
        /// <param name="node">要配置的节点</param>
        /// <param name="owner">父窗口</param>
        /// <returns>配置结果</returns>
        public ConfigResult OpenConfigForm(WorkflowNodeBase node, IWin32Window owner = null)
        {
            if (node == null)
                return new ConfigResult { Success = false, Message = "节点为空" };

            try
            {
                // 优先使用工厂方法
                if (_configFormFactories.TryGetValue(node.StepName, out var factory))
                {
                    using (var form = factory(node))
                    {
                        if (form == null)
                        {
                            return new ConfigResult { Success = false, Message = "无法创建配置窗体" };
                        }

                        var result = owner != null ? form.ShowDialog(owner) : form.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            // 从窗体获取参数
                            ExtractParameterFromForm(form, node);
                            node.IsConfigured = true;
                            node.RefreshDisplay();

                            return new ConfigResult { Success = true, Message = "配置成功" };
                        }

                        return new ConfigResult { Success = false, Message = "用户取消" };
                    }
                }

                // 尝试使用类型创建
                if (_configFormTypes.TryGetValue(node.StepName, out var formType))
                {
                    using (var form = (Form)Activator.CreateInstance(formType))
                    {
                        // 尝试设置参数
                        SetParameterToForm(form, node);

                        var result = owner != null ? form.ShowDialog(owner) : form.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            ExtractParameterFromForm(form, node);
                            node.IsConfigured = true;
                            node.RefreshDisplay();

                            return new ConfigResult { Success = true, Message = "配置成功" };
                        }

                        return new ConfigResult { Success = false, Message = "用户取消" };
                    }
                }

                // 没有注册的窗体，使用默认的通用配置
                return OpenGenericConfigForm(node, owner);
            }
            catch (Exception ex)
            {
                return new ConfigResult { Success = false, Message = $"打开配置失败: {ex.Message}" };
            }
        }

        /// <summary>
        /// 打开通用配置窗体
        /// </summary>
        private ConfigResult OpenGenericConfigForm(WorkflowNodeBase node, IWin32Window owner)
        {
            using (var form = new GenericNodeConfigForm(node))
            {
                var result = owner != null ? form.ShowDialog(owner) : form.ShowDialog();

                if (result == DialogResult.OK)
                {
                    node.Remark = form.Remark;
                    node.RefreshDisplay();
                    return new ConfigResult { Success = true, Message = "配置成功" };
                }

                return new ConfigResult { Success = false, Message = "用户取消" };
            }
        }

        #endregion

        #region 窗体创建工厂方法

        /// <summary>
        /// 创建延时配置窗体
        /// </summary>
        private Form CreateDelayTimeForm(WorkflowNodeBase node)
        {
            // 这里返回你项目中实际的 Form_DelayTime
            // 示例:
            // var form = new Form_DelayTime();
            // form.Parameter = node.StepParameter as Parameter_DelayTime ?? new Parameter_DelayTime();
            // return form;

            // 临时使用通用窗体
            return new DelayTimeConfigForm(node);
        }

        /// <summary>
        /// 创建条件判断配置窗体
        /// </summary>
        private Form CreateDetectionForm(WorkflowNodeBase node)
        {
            // 返回你项目中实际的 Form_Detection
            return new ConditionConfigForm(node);
        }

        /// <summary>
        /// 创建循环配置窗体
        /// </summary>
        private Form CreateLoopForm(WorkflowNodeBase node)
        {
            return new LoopConfigForm(node);
        }

        /// <summary>
        /// 创建等待稳定配置窗体
        /// </summary>
        private Form CreateWaitForStableForm(WorkflowNodeBase node)
        {
            return new WaitForStableConfigForm(node);
        }

        /// <summary>
        /// 创建变量赋值配置窗体
        /// </summary>
        private Form CreateVariableAssignForm(WorkflowNodeBase node)
        {
            return new GenericNodeConfigForm(node);
        }

        /// <summary>
        /// 创建PLC读取配置窗体
        /// </summary>
        private Form CreateReadPLCForm(WorkflowNodeBase node)
        {
            return new GenericNodeConfigForm(node);
        }

        /// <summary>
        /// 创建PLC写入配置窗体
        /// </summary>
        private Form CreateWritePLCForm(WorkflowNodeBase node)
        {
            return new GenericNodeConfigForm(node);
        }

        /// <summary>
        /// 创建消息通知配置窗体
        /// </summary>
        private Form CreateMessageNotifyForm(WorkflowNodeBase node)
        {
            return new MessageNotifyConfigForm(node);
        }

        /// <summary>
        /// 创建实时监控配置窗体
        /// </summary>
        private Form CreateRealtimeMonitorForm(WorkflowNodeBase node)
        {
            return new GenericNodeConfigForm(node);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 设置参数到窗体
        /// </summary>
        private void SetParameterToForm(Form form, WorkflowNodeBase node)
        {
            // 尝试查找 Parameter 属性
            var paramProp = form.GetType().GetProperty("Parameter");
            if (paramProp != null && paramProp.CanWrite)
            {
                paramProp.SetValue(form, node.StepParameter);
            }
        }

        /// <summary>
        /// 从窗体提取参数
        /// </summary>
        private void ExtractParameterFromForm(Form form, WorkflowNodeBase node)
        {
            // 尝试查找 Parameter 属性
            var paramProp = form.GetType().GetProperty("Parameter");
            if (paramProp != null && paramProp.CanRead)
            {
                node.StepParameter = paramProp.GetValue(form);
            }
        }

        #endregion
    }

    #region 配置结果

    /// <summary>
    /// 配置结果
    /// </summary>
    public class ConfigResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region 内置配置窗体

    /// <summary>
    /// 通用节点配置窗体
    /// </summary>
    public class GenericNodeConfigForm : Form
    {
        private TextBox txtRemark;
        private PropertyGrid propertyGrid;
        private Button btnOk;
        private Button btnCancel;

        public string Remark { get; set; }
        public WorkflowNodeBase Node { get; }

        public GenericNodeConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Remark = node.Remark;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"配置 - {Node.DisplayName}";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 属性网格
            propertyGrid = new PropertyGrid
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(465, 250),
                SelectedObject = Node.StepParameter
            };

            // 备注标签
            var lblRemark = new Label
            {
                Text = "备注:",
                Location = new System.Drawing.Point(10, 270),
                AutoSize = true
            };

            // 备注文本框
            txtRemark = new TextBox
            {
                Text = Remark,
                Location = new System.Drawing.Point(10, 290),
                Size = new System.Drawing.Size(465, 25)
            };

            // 确定按钮
            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(315, 330),
                Size = new System.Drawing.Size(75, 25)
            };
            btnOk.Click += (s, e) => { Remark = txtRemark.Text; };

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(400, 330),
                Size = new System.Drawing.Size(75, 25)
            };

            this.Controls.AddRange(new Control[] { propertyGrid, lblRemark, txtRemark, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }

    /// <summary>
    /// 延时配置窗体
    /// </summary>
    public class DelayTimeConfigForm : Form
    {
        private NumericUpDown numDelay;
        private ComboBox cmbUnit;
        private Button btnOk;
        private Button btnCancel;

        public WorkflowNodeBase Node { get; }
        public Parameter_DelayTime Parameter { get; private set; }

        public DelayTimeConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Parameter = node.StepParameter as Parameter_DelayTime ?? new Parameter_DelayTime();
            InitializeComponent();
            LoadParameter();
        }

        private void InitializeComponent()
        {
            this.Text = "延时等待配置";
            this.Size = new System.Drawing.Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblDelay = new Label
            {
                Text = "延时时间:",
                Location = new System.Drawing.Point(20, 30),
                AutoSize = true
            };

            numDelay = new NumericUpDown
            {
                Location = new System.Drawing.Point(100, 28),
                Size = new System.Drawing.Size(120, 25),
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0
            };

            cmbUnit = new ComboBox
            {
                Location = new System.Drawing.Point(230, 28),
                Size = new System.Drawing.Size(80, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUnit.Items.AddRange(new[] { "毫秒", "秒", "分钟" });
            cmbUnit.SelectedIndex = 0;

            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(75, 28)
            };
            btnOk.Click += OnOkClick;

            btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(235, 100),
                Size = new System.Drawing.Size(75, 28)
            };

            this.Controls.AddRange(new Control[] { lblDelay, numDelay, cmbUnit, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadParameter()
        {
            double ms = Parameter.T;
            if (ms >= 60000)
            {
                numDelay.Value = (decimal)(ms / 60000);
                cmbUnit.SelectedIndex = 2; // 分钟
            }
            else if (ms >= 1000)
            {
                numDelay.Value = (decimal)(ms / 1000);
                cmbUnit.SelectedIndex = 1; // 秒
            }
            else
            {
                numDelay.Value = (decimal)ms;
                cmbUnit.SelectedIndex = 0; // 毫秒
            }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            double value = (double)numDelay.Value;
            switch (cmbUnit.SelectedIndex)
            {
                case 1: value *= 1000; break;    // 秒 -> 毫秒
                case 2: value *= 60000; break;   // 分钟 -> 毫秒
            }
            Parameter.T = value;
            Node.StepParameter = Parameter;
        }
    }

    /// <summary>
    /// 条件判断配置窗体
    /// </summary>
    public class ConditionConfigForm : Form
    {
        private TextBox txtCondition;
        private TextBox txtName;
        private Button btnOk;
        private Button btnCancel;

        public WorkflowNodeBase Node { get; }
        public Parameter_Detection Parameter { get; private set; }

        public ConditionConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Parameter = node.StepParameter as Parameter_Detection ?? new Parameter_Detection();
            InitializeComponent();
            LoadParameter();
        }

        private void InitializeComponent()
        {
            this.Text = "条件判断配置";
            this.Size = new System.Drawing.Size(450, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label
            {
                Text = "检测项名称:",
                Location = new System.Drawing.Point(20, 25),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Location = new System.Drawing.Point(110, 22),
                Size = new System.Drawing.Size(300, 25)
            };

            var lblCondition = new Label
            {
                Text = "条件表达式:",
                Location = new System.Drawing.Point(20, 65),
                AutoSize = true
            };

            txtCondition = new TextBox
            {
                Location = new System.Drawing.Point(110, 62),
                Size = new System.Drawing.Size(300, 25)
            };

            var lblHint = new Label
            {
                Text = "提示: 使用 {value} 代表数据源的值\n例如: {value} >= 100 && {value} <= 200",
                Location = new System.Drawing.Point(110, 95),
                Size = new System.Drawing.Size(300, 40),
                ForeColor = System.Drawing.Color.Gray
            };

            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(255, 145),
                Size = new System.Drawing.Size(75, 28)
            };
            btnOk.Click += OnOkClick;

            btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(335, 145),
                Size = new System.Drawing.Size(75, 28)
            };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblCondition, txtCondition, lblHint, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadParameter()
        {
            txtName.Text = Parameter.DetectionName;
            txtCondition.Text = Parameter.ConditionExpression;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            Parameter.DetectionName = txtName.Text;
            Parameter.ConditionExpression = txtCondition.Text;
            Node.StepParameter = Parameter;
        }
    }

    /// <summary>
    /// 循环配置窗体
    /// </summary>
    public class LoopConfigForm : Form
    {
        private TextBox txtLoopCount;
        private TextBox txtCounterVar;
        private CheckBox chkEnableCounter;
        private CheckBox chkEnableEarlyExit;
        private TextBox txtExitCondition;
        private Button btnOk;
        private Button btnCancel;

        public WorkflowNodeBase Node { get; }
        public Parameter_Loop Parameter { get; private set; }

        public LoopConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Parameter = node.StepParameter as Parameter_Loop ?? new Parameter_Loop();
            InitializeComponent();
            LoadParameter();
        }

        private void InitializeComponent()
        {
            this.Text = "循环配置";
            this.Size = new System.Drawing.Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblCount = new Label
            {
                Text = "循环次数:",
                Location = new System.Drawing.Point(20, 25),
                AutoSize = true
            };

            txtLoopCount = new TextBox
            {
                Location = new System.Drawing.Point(120, 22),
                Size = new System.Drawing.Size(290, 25)
            };

            chkEnableCounter = new CheckBox
            {
                Text = "启用计数器变量",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };
            chkEnableCounter.CheckedChanged += (s, e) => { txtCounterVar.Enabled = chkEnableCounter.Checked; };

            var lblCounterVar = new Label
            {
                Text = "计数器变量:",
                Location = new System.Drawing.Point(40, 90),
                AutoSize = true
            };

            txtCounterVar = new TextBox
            {
                Location = new System.Drawing.Point(120, 87),
                Size = new System.Drawing.Size(290, 25)
            };

            chkEnableEarlyExit = new CheckBox
            {
                Text = "启用提前退出",
                Location = new System.Drawing.Point(20, 125),
                AutoSize = true
            };
            chkEnableEarlyExit.CheckedChanged += (s, e) => { txtExitCondition.Enabled = chkEnableEarlyExit.Checked; };

            var lblExit = new Label
            {
                Text = "退出条件:",
                Location = new System.Drawing.Point(40, 155),
                AutoSize = true
            };

            txtExitCondition = new TextBox
            {
                Location = new System.Drawing.Point(120, 152),
                Size = new System.Drawing.Size(290, 25),
                Enabled = false
            };

            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(255, 220),
                Size = new System.Drawing.Size(75, 28)
            };
            btnOk.Click += OnOkClick;

            btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(335, 220),
                Size = new System.Drawing.Size(75, 28)
            };

            this.Controls.AddRange(new Control[] {
                lblCount, txtLoopCount,
                chkEnableCounter, lblCounterVar, txtCounterVar,
                chkEnableEarlyExit, lblExit, txtExitCondition,
                btnOk, btnCancel
            });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadParameter()
        {
            txtLoopCount.Text = Parameter.LoopCountExpression;
            chkEnableCounter.Checked = Parameter.EnableCounter;
            txtCounterVar.Text = Parameter.CounterVariableName;
            txtCounterVar.Enabled = Parameter.EnableCounter;
            chkEnableEarlyExit.Checked = Parameter.EnableEarlyExit;
            txtExitCondition.Text = Parameter.ExitConditionExpression;
            txtExitCondition.Enabled = Parameter.EnableEarlyExit;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            Parameter.LoopCountExpression = txtLoopCount.Text;
            Parameter.EnableCounter = chkEnableCounter.Checked;
            Parameter.CounterVariableName = txtCounterVar.Text;
            Parameter.EnableEarlyExit = chkEnableEarlyExit.Checked;
            Parameter.ExitConditionExpression = txtExitCondition.Text;
            Node.StepParameter = Parameter;
        }
    }

    /// <summary>
    /// 等待稳定配置窗体
    /// </summary>
    public class WaitForStableConfigForm : Form
    {
        private TextBox txtVariable;
        private NumericUpDown numThreshold;
        private NumericUpDown numInterval;
        private NumericUpDown numStableCount;
        private NumericUpDown numTimeout;
        private Button btnOk;
        private Button btnCancel;

        public WorkflowNodeBase Node { get; }
        public Parameter_WaitForStable Parameter { get; private set; }

        public WaitForStableConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Parameter = node.StepParameter as Parameter_WaitForStable ?? new Parameter_WaitForStable();
            InitializeComponent();
            LoadParameter();
        }

        private void InitializeComponent()
        {
            this.Text = "等待稳定配置";
            this.Size = new System.Drawing.Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;
            int labelX = 20;
            int inputX = 130;

            var lblVar = new Label { Text = "监测变量:", Location = new System.Drawing.Point(labelX, y), AutoSize = true };
            txtVariable = new TextBox { Location = new System.Drawing.Point(inputX, y - 3), Size = new System.Drawing.Size(230, 25) };
            y += 35;

            var lblThreshold = new Label { Text = "稳定阈值:", Location = new System.Drawing.Point(labelX, y), AutoSize = true };
            numThreshold = new NumericUpDown { Location = new System.Drawing.Point(inputX, y - 3), Size = new System.Drawing.Size(100, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 1000, Value = 0.1m };
            y += 35;

            var lblInterval = new Label { Text = "采样间隔(秒):", Location = new System.Drawing.Point(labelX, y), AutoSize = true };
            numInterval = new NumericUpDown { Location = new System.Drawing.Point(inputX, y - 3), Size = new System.Drawing.Size(100, 25), Minimum = 1, Maximum = 3600, Value = 1 };
            y += 35;

            var lblCount = new Label { Text = "连续稳定次数:", Location = new System.Drawing.Point(labelX, y), AutoSize = true };
            numStableCount = new NumericUpDown { Location = new System.Drawing.Point(inputX, y - 3), Size = new System.Drawing.Size(100, 25), Minimum = 1, Maximum = 100, Value = 3 };
            y += 35;

            var lblTimeout = new Label { Text = "超时时间(秒):", Location = new System.Drawing.Point(labelX, y), AutoSize = true };
            numTimeout = new NumericUpDown { Location = new System.Drawing.Point(inputX, y - 3), Size = new System.Drawing.Size(100, 25), Minimum = 0, Maximum = 36000, Value = 60 };
            y += 45;

            btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(200, y), Size = new System.Drawing.Size(75, 28) };
            btnOk.Click += OnOkClick;

            btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(285, y), Size = new System.Drawing.Size(75, 28) };

            this.Controls.AddRange(new Control[] {
                lblVar, txtVariable, lblThreshold, numThreshold,
                lblInterval, numInterval, lblCount, numStableCount,
                lblTimeout, numTimeout, btnOk, btnCancel
            });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadParameter()
        {
            txtVariable.Text = Parameter.MonitorVariable;
            numThreshold.Value = (decimal)Parameter.StabilityThreshold;
            numInterval.Value = Parameter.SamplingInterval;
            numStableCount.Value = Parameter.StableCount;
            numTimeout.Value = Parameter.TimeoutSeconds;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            Parameter.MonitorVariable = txtVariable.Text;
            Parameter.StabilityThreshold = (double)numThreshold.Value;
            Parameter.SamplingInterval = (int)numInterval.Value;
            Parameter.StableCount = (int)numStableCount.Value;
            Parameter.TimeoutSeconds = (int)numTimeout.Value;
            Node.StepParameter = Parameter;
        }
    }

    /// <summary>
    /// 消息通知配置窗体
    /// </summary>
    public class MessageNotifyConfigForm : Form
    {
        private TextBox txtMessage;
        private ComboBox cmbType;
        private Button btnOk;
        private Button btnCancel;

        public WorkflowNodeBase Node { get; }

        public MessageNotifyConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            InitializeComponent();
            LoadParameter();
        }

        private void InitializeComponent()
        {
            this.Text = "消息通知配置";
            this.Size = new System.Drawing.Size(450, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblType = new Label { Text = "消息类型:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            cmbType = new ComboBox
            {
                Location = new System.Drawing.Point(100, 22),
                Size = new System.Drawing.Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new[] { "信息", "警告", "错误", "成功" });
            cmbType.SelectedIndex = 0;

            var lblMsg = new Label { Text = "消息内容:", Location = new System.Drawing.Point(20, 65), AutoSize = true };
            txtMessage = new TextBox
            {
                Location = new System.Drawing.Point(100, 62),
                Size = new System.Drawing.Size(320, 50),
                Multiline = true
            };

            btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(260, 125), Size = new System.Drawing.Size(75, 28) };
            btnOk.Click += OnOkClick;

            btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(345, 125), Size = new System.Drawing.Size(75, 28) };

            this.Controls.AddRange(new Control[] { lblType, cmbType, lblMsg, txtMessage, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadParameter()
        {
            if (Node is MessageNotifyNode msgNode)
            {
                txtMessage.Text = msgNode.Message;
                cmbType.SelectedIndex = (int)msgNode.MessageType;
            }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (Node is MessageNotifyNode msgNode)
            {
                msgNode.Message = txtMessage.Text;
                msgNode.MessageType = (MessageType)cmbType.SelectedIndex;
            }
        }
    }

    #endregion
}
