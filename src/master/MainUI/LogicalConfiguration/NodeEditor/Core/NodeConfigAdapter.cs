using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.Parameter;
using MainUI.LogicalConfiguration.Forms;

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
        private readonly Dictionary<string, Type> _configFormTypes = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// StepName 到配置窗体工厂的映射
        /// </summary>
        private readonly Dictionary<string, Func<WorkflowNodeBase, Form>> _configFormFactories = new(StringComparer.OrdinalIgnoreCase);

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
            // 延时等待
            RegisterFormFactory("DelayWait", CreateFormWithParameter<Form_DelayTime, Parameter_DelayTime>);

            // 条件判断
            RegisterFormFactory("ConditionJudge", CreateFormWithParameter<Form_Detection, Parameter_Detection>);

            // 循环工具
            RegisterFormFactory("CycleBegins", CreateFormWithParameter<Form_Loop, Parameter_Loop>);

            // 等待稳定
            RegisterFormFactory("Waitingforstability", CreateFormWithParameter<Form_WaitForStable, Parameter_WaitForStable>);

            // 变量赋值
            RegisterFormFactory("VariableAssign", CreateFormWithParameter<Form_VariableAssignment, Parameter_VariableAssignment>);

            // 读取PLC
            RegisterFormFactory("PLCRead", CreateFormWithParameter<Form_ReadPLC, Parameter_ReadPLC>);

            // 写入PLC
            RegisterFormFactory("PLCWrite", CreateFormWithParameter<Form_WritePLC, Parameter_WritePLC>);

            // 消息通知
            RegisterFormFactory("MessageNotify", CreateFormWithParameter<Form_SystemPrompt, Parameter_SystemPrompt>);

            // 实时监控
            RegisterFormFactory("MonitorTool", CreateFormWithParameter<Form_RealtimeMonitorPromptConfig, Parameter_RealtimeMonitorPrompt>);

            
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
                            // 从窗体提取参数
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
            using var form = new GenericNodeConfigForm(node);
            var result = owner != null ? form.ShowDialog(owner) : form.ShowDialog();

            if (result != DialogResult.OK) return new ConfigResult { Success = false, Message = "用户取消" };
            node.Remark = form.Remark;
            node.RefreshDisplay();
            return new ConfigResult { Success = true, Message = "配置成功" };

        }

        #endregion

        #region 窗体创建辅助方法

        /// <summary>
        /// 泛型方法：创建窗体并设置参数
        /// </summary>
        /// <typeparam name="TForm">窗体类型</typeparam>
        /// <typeparam name="TParam">参数类型</typeparam>
        /// <param name="node">工作流节点</param>
        /// <returns>创建的窗体实例</returns>
        private Form CreateFormWithParameter<TForm, TParam>(WorkflowNodeBase node)
            where TForm : Form, new()
            where TParam : class, new()
        {
            var form = new TForm();

            try
            {
                // 通过反射获取 Parameter 属性
                var paramProp = typeof(TForm).GetProperty("Parameter");
                if (paramProp != null && paramProp.CanWrite)
                {
                    // 如果节点已有参数且类型匹配，使用现有参数
                    if (node.StepParameter is TParam existingParam)
                    {
                        paramProp.SetValue(form, existingParam);
                    }
                    else
                    {
                        // 创建新参数
                        var newParam = new TParam();
                        paramProp.SetValue(form, newParam);
                        node.StepParameter = newParam;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置参数失败: {ex.Message}");
            }

            return form;
        }

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
    /// 用于没有专门配置窗体的节点
    /// </summary>
    public class GenericNodeConfigForm : Form
    {
        private TextBox txtRemark;
        private PropertyGrid propertyGrid;
        private Button btnOk;
        private Button btnCancel;
        private Label lblRemark;
        private Label lblProperties;

        public string Remark { get; set; }
        public WorkflowNodeBase Node { get; }

        public GenericNodeConfigForm(WorkflowNodeBase node)
        {
            Node = node;
            Remark = node.Remark;
            InitializeComponent();
            LoadNodeData();
        }

        private void InitializeComponent()
        {
            this.Text = $"配置 - {Node.DisplayName}";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 备注标签
            lblRemark = new Label
            {
                Text = "节点备注:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            // 备注文本框
            txtRemark = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(440, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // 属性标签
            lblProperties = new Label
            {
                Text = "节点属性:",
                Location = new Point(20, 120),
                AutoSize = true
            };

            // 属性网格
            propertyGrid = new PropertyGrid
            {
                Location = new Point(20, 145),
                Size = new Size(440, 200),
                HelpVisible = false
            };

            // 确定按钮
            btnOk = new Button
            {
                Text = "确定",
                Location = new Point(290, 370),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += BtnOk_Click;

            // 取消按钮
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(380, 370),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                lblRemark, txtRemark,
                lblProperties, propertyGrid,
                btnOk, btnCancel
            });

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadNodeData()
        {
            txtRemark.Text = Remark;

            // 如果有参数对象，显示在属性网格中
            if (Node.StepParameter != null)
            {
                propertyGrid.SelectedObject = Node.StepParameter;
            }
            else
            {
                propertyGrid.SelectedObject = Node;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Remark = txtRemark.Text;
        }
    }

    #endregion
}