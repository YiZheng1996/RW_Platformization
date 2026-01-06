using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using MainUI.LogicalConfiguration.Parameter;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    #region 通用配置面板

    /// <summary>
    /// 通用节点配置面板 - 用于没有专门配置面板的节点
    /// </summary>
    public class GenericNodeConfigPanel : NodeConfigPanelBase
    {
        private Label _lblNodeName;
        private Label _lblStepName;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "*";
        public override string PanelTitle => "基本配置";

        public GenericNodeConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var lblNameTitle = CreateLabel("节点名称:", 10);
            _lblNodeName = new Label
            {
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Location = new Point(8, 30),
                AutoSize = true
            };

            var lblStepTitle = CreateLabel("步骤类型:", 60);
            _lblStepName = new Label
            {
                ForeColor = Color.FromArgb(100, 180, 255),
                Font = new Font("Consolas", 9F),
                Location = new Point(8, 80),
                AutoSize = true
            };

            var lblRemarkTitle = CreateLabel("备注说明:", 110);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 130),
                Width = 160,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblNameTitle, _lblNodeName,
                lblStepTitle, _lblStepName,
                lblRemarkTitle, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;
            _lblNodeName.Text = _node.DisplayName;
            _lblStepName.Text = _node.StepName;
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;
            _node.Remark = _txtRemark.Text;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // GenericNodeConfigPanel
            // 
            Name = "GenericNodeConfigPanel";
            Size = new Size(1480, 623);
            ResumeLayout(false);

        }

        protected override void ClearUI()
        {
            _lblNodeName.Text = "";
            _lblStepName.Text = "";
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region 延时等待配置面板

    /// <summary>
    /// 延时等待节点配置面板
    /// </summary>
    public class DelayWaitConfigPanel : NodeConfigPanelBase
    {
        private NumericUpDown _numDelay;
        private ComboBox _cmbUnit;
        private TextBox _txtRemark;
        private Label _lblPreview;

        protected override string SupportedStepName => "DelayWait";
        public override string PanelTitle => "延时等待";

        public DelayWaitConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 延时时间
            var lblDelay = CreateLabel("延时时间:", 10);
            _numDelay = new NumericUpDown
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Location = new Point(8, 32),
                Width = 100,
                Minimum = 1,
                Maximum = 999999,
                Value = 1000,
                DecimalPlaces = 0
            };
            _numDelay.ValueChanged += (s, e) => { MarkAsChanged(); UpdatePreview(); };

            // 单位选择
            _cmbUnit = new ComboBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(115, 32),
                Width = 55
            };
            _cmbUnit.Items.AddRange(new object[] { "毫秒", "秒", "分钟" });
            _cmbUnit.SelectedIndex = 0;
            _cmbUnit.SelectedIndexChanged += (s, e) => { MarkAsChanged(); UpdatePreview(); };

            // 预览
            _lblPreview = new Label
            {
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("微软雅黑", 9F),
                Location = new Point(8, 62),
                AutoSize = true,
                Text = "= 1000 毫秒"
            };

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 90),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 100);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 122),
                Width = 160,
                Height = 50,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblDelay, _numDelay, _cmbUnit, _lblPreview,
                separator,
                lblRemark, _txtRemark
            });
        }

        private void UpdatePreview()
        {
            int ms = (int)_numDelay.Value;
            string unit = _cmbUnit.SelectedItem?.ToString() ?? "毫秒";
            
            switch (unit)
            {
                case "秒":
                    ms *= 1000;
                    break;
                case "分钟":
                    ms *= 60000;
                    break;
            }

            if (ms >= 60000)
                _lblPreview.Text = $"= {ms / 60000.0:F1} 分钟";
            else if (ms >= 1000)
                _lblPreview.Text = $"= {ms / 1000.0:F1} 秒";
            else
                _lblPreview.Text = $"= {ms} 毫秒";
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_DelayTime;
            if (param != null)
            {
                // 智能选择单位
                if (param.T >= 60000 && param.T % 60000 == 0)
                {
                    _numDelay.Value = (decimal)(param.T / 60000);
                    _cmbUnit.SelectedItem = "分钟";
                }
                else if (param.T >= 1000 && param.T % 1000 == 0)
                {
                    _numDelay.Value = (decimal)param.T / 1000;
                    _cmbUnit.SelectedItem = "秒";
                }
                else
                {
                    _numDelay.Value = (decimal)param.T;
                    _cmbUnit.SelectedItem = "毫秒";
                }
            }
            _txtRemark.Text = _node.Remark ?? "";
            UpdatePreview();
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            int ms = (int)_numDelay.Value;
            string unit = _cmbUnit.SelectedItem?.ToString() ?? "毫秒";
            
            switch (unit)
            {
                case "秒":
                    ms *= 1000;
                    break;
                case "分钟":
                    ms *= 60000;
                    break;
            }

            var param = _node.StepParameter as Parameter_DelayTime ?? new Parameter_DelayTime();
            param.T = ms;
            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _numDelay.Value = 1000;
            _cmbUnit.SelectedIndex = 0;
            _txtRemark.Text = "";
            _lblPreview.Text = "= 1000 毫秒";
        }
    }

    #endregion

    #region 条件判断配置面板

    /// <summary>
    /// 条件判断节点配置面板
    /// </summary>
    public class ConditionConfigPanel : NodeConfigPanelBase
    {
        private TextBox _txtConditionName;
        private TextBox _txtExpression;
        private Button _btnEditExpression;
        private TextBox _txtRemark;
        private Label _lblExpressionPreview;

        protected override string SupportedStepName => "ConditionJudge";
        public override string PanelTitle => "条件判断";

        public ConditionConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 条件名称
            var lblName = CreateLabel("条件名称:", 10);
            _txtConditionName = CreateTextBox(32, 160);

            // 条件表达式
            var lblExpr = CreateLabel("条件表达式:", 68);
            
            _txtExpression = new TextBox
            {
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.FromArgb(100, 200, 100),
                Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 90),
                Width = 130,
                ReadOnly = true
            };
            _txtExpression.TextChanged += (s, e) => MarkAsChanged();

            // 编辑按钮
            _btnEditExpression = new Button
            {
                Text = "...",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                Size = new Size(28, 23),
                Location = new Point(142, 89),
                Cursor = Cursors.Hand
            };
            _btnEditExpression.FlatAppearance.BorderSize = 1;
            _btnEditExpression.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _btnEditExpression.Click += BtnEditExpression_Click;

            // 表达式预览
            _lblExpressionPreview = new Label
            {
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("微软雅黑", 8F),
                Location = new Point(8, 118),
                Size = new Size(160, 30),
                Text = "点击 [...] 编辑条件"
            };

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 155),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 165);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 187),
                Width = 160,
                Height = 50,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblName, _txtConditionName,
                lblExpr, _txtExpression, _btnEditExpression, _lblExpressionPreview,
                separator,
                lblRemark, _txtRemark
            });
        }

        private void BtnEditExpression_Click(object sender, EventArgs e)
        {
            // TODO: 这里可以调用现有的 Form_Detection 窗体
            // 或者打开一个简化的表达式编辑对话框
            
            var inputForm = new Form
            {
                Text = "编辑条件表达式",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var txtInput = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(340, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10F),
                Text = _txtExpression.Text
            };

            var lblHint = new Label
            {
                Text = "示例: {变量名} >= 10  或  {PLC.D100} == true",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(180, 110),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White
            };

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(270, 110),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            inputForm.Controls.AddRange(new Control[] { lblHint, txtInput, btnOk, btnCancel });
            inputForm.AcceptButton = btnOk;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                _txtExpression.Text = txtInput.Text;
                UpdateExpressionPreview();
                MarkAsChanged();
            }
        }

        private void UpdateExpressionPreview()
        {
            if (string.IsNullOrEmpty(_txtExpression.Text))
            {
                _lblExpressionPreview.Text = "点击 [...] 编辑条件";
            }
            else
            {
                var expr = _txtExpression.Text;
                _lblExpressionPreview.Text = expr.Length > 30 ? expr.Substring(0, 27) + "..." : expr;
            }
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_Detection;
            if (param != null)
            {
                _txtConditionName.Text = param.DetectionName ?? "";
                _txtExpression.Text = param.ConditionExpression ?? "";
            }
            _txtRemark.Text = _node.Remark ?? "";
            UpdateExpressionPreview();
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_Detection ?? new Parameter_Detection();
            param.DetectionName = _txtConditionName.Text;
            param.ConditionExpression = _txtExpression.Text;
            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _txtConditionName.Text = "";
            _txtExpression.Text = "";
            _txtRemark.Text = "";
            _lblExpressionPreview.Text = "点击 [...] 编辑条件";
        }
    }

    #endregion

    #region 循环配置面板

    /// <summary>
    /// 循环节点配置面板
    /// </summary>
    public class LoopConfigPanel : NodeConfigPanelBase
    {
        private NumericUpDown _numLoopCount;
        private CheckBox _chkUseVariable;
        private TextBox _txtVariable;
        private CheckBox _chkEnableCounter;
        private TextBox _txtCounterVar;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "CycleBegins";
        public override string PanelTitle => "循环配置";

        public LoopConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 循环次数
            var lblCount = CreateLabel("循环次数:", 10);
            _numLoopCount = new NumericUpDown
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Location = new Point(8, 32),
                Width = 80,
                Minimum = 1,
                Maximum = 99999,
                Value = 10
            };
            _numLoopCount.ValueChanged += (s, e) => MarkAsChanged();

            // 使用变量
            _chkUseVariable = CreateCheckBox("使用变量作为次数", 62);
            _chkUseVariable.CheckedChanged += (s, e) =>
            {
                _numLoopCount.Enabled = !_chkUseVariable.Checked;
                _txtVariable.Enabled = _chkUseVariable.Checked;
                MarkAsChanged();
            };

            _txtVariable = CreateTextBox(88, 160);
            _txtVariable.Enabled = false;
            _txtVariable.PlaceholderText = "{变量名}";

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 120),
                Size = new Size(160, 1)
            };

            // 计数器变量
            _chkEnableCounter = CreateCheckBox("启用计数器变量", 130);
            
            var lblCounterVar = CreateLabel("计数器名:", 155);
            _txtCounterVar = CreateTextBox(175, 100);
            _txtCounterVar.Text = "LoopIndex";

            // 分隔线2
            var separator2 = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 208),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 218);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 240),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblCount, _numLoopCount,
                _chkUseVariable, _txtVariable,
                separator,
                _chkEnableCounter, lblCounterVar, _txtCounterVar,
                separator2,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_Loop;
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.LoopCountExpression) && 
                    param.LoopCountExpression.StartsWith("{"))
                {
                    _chkUseVariable.Checked = true;
                    _txtVariable.Text = param.LoopCountExpression;
                }
                else
                {
                    _chkUseVariable.Checked = false;
                    _numLoopCount.Value = Convert.ToDecimal(param.LoopCountExpression);
                }

                _chkEnableCounter.Checked = param.EnableCounter;
                _txtCounterVar.Text = param.LoopCountExpression ?? "LoopIndex";
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_Loop ?? new Parameter_Loop();
            
            if (_chkUseVariable.Checked)
            {
                param.CounterVariableName = _txtVariable.Text;
                param.CounterVariableName = "10";
            }
            else
            {
                param.LoopCountExpression = _numLoopCount.Value.ToString();
                param.LoopCountExpression = null;
            }

            param.EnableCounter = _chkEnableCounter.Checked;
            param.CounterVariableName = _txtCounterVar.Text;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _numLoopCount.Value = 10;
            _chkUseVariable.Checked = false;
            _txtVariable.Text = "";
            _chkEnableCounter.Checked = false;
            _txtCounterVar.Text = "LoopIndex";
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region PLC读取配置面板

    /// <summary>
    /// PLC读取节点配置面板
    /// </summary>
    public class PLCReadConfigPanel : NodeConfigPanelBase
    {
        private ComboBox _cmbModule;
        private TextBox _txtAddress;
        private ComboBox _cmbDataType;
        private TextBox _txtTargetVar;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "PLCRead";
        public override string PanelTitle => "读取PLC";

        public PLCReadConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 模块选择
            var lblModule = CreateLabel("PLC模块:", 10);
            _cmbModule = CreateComboBox(32, 160);
            _cmbModule.Items.AddRange(new object[] { "PLC1", "PLC2", "MainPLC" });
            _cmbModule.SelectedIndex = 0;

            // 地址
            var lblAddr = CreateLabel("读取地址:", 68);
            _txtAddress = CreateTextBox(90, 160);
            _txtAddress.PlaceholderText = "D100, M0, Y0...";

            // 数据类型
            var lblType = CreateLabel("数据类型:", 126);
            _cmbDataType = CreateComboBox(148, 100);
            _cmbDataType.Items.AddRange(new object[] { "Int16", "Int32", "Float", "Bool", "String" });
            _cmbDataType.SelectedIndex = 0;

            // 目标变量
            var lblTarget = CreateLabel("保存到变量:", 184);
            _txtTargetVar = CreateTextBox(206, 160);
            _txtTargetVar.PlaceholderText = "变量名";

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 240),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 250);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 272),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblModule, _cmbModule,
                lblAddr, _txtAddress,
                lblType, _cmbDataType,
                lblTarget, _txtTargetVar,
                separator,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_PLCReadWrite;
            if (param != null)
            {
                // 设置模块
                if (!string.IsNullOrEmpty(param.ModuleName))
                {
                    var idx = _cmbModule.Items.IndexOf(param.ModuleName);
                    _cmbModule.SelectedIndex = idx >= 0 ? idx : 0;
                }

                _txtAddress.Text = param.Address ?? "";
                
                // 设置数据类型
                if (!string.IsNullOrEmpty(param.DataType))
                {
                    var idx = _cmbDataType.Items.IndexOf(param.DataType);
                    _cmbDataType.SelectedIndex = idx >= 0 ? idx : 0;
                }

                _txtTargetVar.Text = param.TargetVariable ?? "";
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_PLCReadWrite ?? new Parameter_PLCReadWrite();
            param.ModuleName = _cmbModule.SelectedItem?.ToString();
            param.Address = _txtAddress.Text;
            param.DataType = _cmbDataType.SelectedItem?.ToString();
            param.TargetVariable = _txtTargetVar.Text;
            param.IsRead = true;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _cmbModule.SelectedIndex = 0;
            _txtAddress.Text = "";
            _cmbDataType.SelectedIndex = 0;
            _txtTargetVar.Text = "";
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region PLC写入配置面板

    /// <summary>
    /// PLC写入节点配置面板
    /// </summary>
    public class PLCWriteConfigPanel : NodeConfigPanelBase
    {
        private ComboBox _cmbModule;
        private TextBox _txtAddress;
        private TextBox _txtValue;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "PLCWrite";
        public override string PanelTitle => "写入PLC";

        public PLCWriteConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 模块选择
            var lblModule = CreateLabel("PLC模块:", 10);
            _cmbModule = CreateComboBox(32, 160);
            _cmbModule.Items.AddRange(new object[] { "PLC1", "PLC2", "MainPLC" });
            _cmbModule.SelectedIndex = 0;

            // 地址
            var lblAddr = CreateLabel("写入地址:", 68);
            _txtAddress = CreateTextBox(90, 160);
            _txtAddress.PlaceholderText = "D100, M0, Y0...";

            // 写入值
            var lblValue = CreateLabel("写入值/变量:", 126);
            _txtValue = CreateTextBox(148, 160);
            _txtValue.PlaceholderText = "123 或 {变量名}";

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 185),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 195);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 217),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblModule, _cmbModule,
                lblAddr, _txtAddress,
                lblValue, _txtValue,
                separator,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_PLCReadWrite;
            if (param != null)
            {
                if (!string.IsNullOrEmpty(param.ModuleName))
                {
                    var idx = _cmbModule.Items.IndexOf(param.ModuleName);
                    _cmbModule.SelectedIndex = idx >= 0 ? idx : 0;
                }

                _txtAddress.Text = param.Address ?? "";
                _txtValue.Text = param.WriteValue ?? "";
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_PLCReadWrite ?? new Parameter_PLCReadWrite();
            param.ModuleName = _cmbModule.SelectedItem?.ToString();
            param.Address = _txtAddress.Text;
            param.WriteValue = _txtValue.Text;
            param.IsRead = false;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _cmbModule.SelectedIndex = 0;
            _txtAddress.Text = "";
            _txtValue.Text = "";
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region 变量赋值配置面板

    /// <summary>
    /// 变量赋值节点配置面板
    /// </summary>
    public class VariableAssignConfigPanel : NodeConfigPanelBase
    {
        private TextBox _txtTargetVar;
        private ComboBox _cmbValueType;
        private TextBox _txtValue;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "VariableAssign";
        public override string PanelTitle => "变量赋值";

        public VariableAssignConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 目标变量
            var lblTarget = CreateLabel("目标变量:", 10);
            _txtTargetVar = CreateTextBox(32, 160);
            _txtTargetVar.PlaceholderText = "变量名";

            // 值类型
            var lblType = CreateLabel("值类型:", 68);
            _cmbValueType = CreateComboBox(90, 100);
            _cmbValueType.Items.AddRange(new object[] { "固定值", "变量", "表达式" });
            _cmbValueType.SelectedIndex = 0;

            // 赋值内容
            var lblValue = CreateLabel("赋值内容:", 126);
            _txtValue = CreateTextBox(148, 160);

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 185),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 195);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 217),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblTarget, _txtTargetVar,
                lblType, _cmbValueType,
                lblValue, _txtValue,
                separator,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_VariableAssignment;
            if (param != null)
            {
                _txtTargetVar.Text = param.TargetVarName ?? "";
                _txtValue.Text = param.Expression ?? param.Expression?.ToString() ?? "";
                
                // 判断值类型
                if (!string.IsNullOrEmpty(param.Expression))
                {
                    if (param.Expression.StartsWith("{") && param.Expression.EndsWith("}"))
                        _cmbValueType.SelectedItem = "变量";
                    else if (param.Expression.Contains("+") || param.Expression.Contains("-") ||
                             param.Expression.Contains("*") || param.Expression.Contains("/"))
                        _cmbValueType.SelectedItem = "表达式";
                    else
                        _cmbValueType.SelectedItem = "固定值";
                }
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_VariableAssignment ?? new Parameter_VariableAssignment();
            param.TargetVarName = _txtTargetVar.Text;
            param.Expression = _txtValue.Text;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _txtTargetVar.Text = "";
            _cmbValueType.SelectedIndex = 0;
            _txtValue.Text = "";
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region 等待稳定配置面板

    /// <summary>
    /// 等待稳定节点配置面板
    /// </summary>
    public class WaitStableConfigPanel : NodeConfigPanelBase
    {
        private TextBox _txtTargetVar;
        private NumericUpDown _numThreshold;
        private NumericUpDown _numDuration;
        private NumericUpDown _numTimeout;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "Waitingforstability";
        public override string PanelTitle => "等待稳定";

        public WaitStableConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 监控变量
            var lblTarget = CreateLabel("监控变量:", 10);
            _txtTargetVar = CreateTextBox(32, 160);
            _txtTargetVar.PlaceholderText = "{变量名}";

            // 波动阈值
            var lblThreshold = CreateLabel("波动阈值:", 68);
            _numThreshold = CreateNumericInput(90, 0, 10000, 2);
            _numThreshold.Value = 0.5m;
            _numThreshold.Width = 80;

            // 稳定持续时间
            var lblDuration = CreateLabel("稳定时间(ms):", 126);
            _numDuration = CreateNumericInput(148, 100, 60000, 0);
            _numDuration.Value = 2000;
            _numDuration.Width = 80;

            // 超时
            var lblTimeout = CreateLabel("超时时间(ms):", 184);
            _numTimeout = CreateNumericInput(206, 1000, 300000, 0);
            _numTimeout.Value = 30000;
            _numTimeout.Width = 80;

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 245),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 255);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 277),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblTarget, _txtTargetVar,
                lblThreshold, _numThreshold,
                lblDuration, _numDuration,
                lblTimeout, _numTimeout,
                separator,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_WaitStable;
            if (param != null)
            {
                _txtTargetVar.Text = param.VariableName ?? "";
                _numThreshold.Value = (decimal)param.Threshold;
                _numDuration.Value = param.StableDuration;
                _numTimeout.Value = param.Timeout;
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_WaitStable ?? new Parameter_WaitStable();
            param.VariableName = _txtTargetVar.Text;
            param.Threshold = (double)_numThreshold.Value;
            param.StableDuration = (int)_numDuration.Value;
            param.Timeout = (int)_numTimeout.Value;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _txtTargetVar.Text = "";
            _numThreshold.Value = 0.5m;
            _numDuration.Value = 2000;
            _numTimeout.Value = 30000;
            _txtRemark.Text = "";
        }
    }

    #endregion

    #region 消息通知配置面板

    /// <summary>
    /// 消息通知节点配置面板
    /// </summary>
    public class MessageNotifyConfigPanel : NodeConfigPanelBase
    {
        private ComboBox _cmbType;
        private TextBox _txtTitle;
        private TextBox _txtContent;
        private CheckBox _chkWaitConfirm;
        private TextBox _txtRemark;

        protected override string SupportedStepName => "MessageNotify";
        public override string PanelTitle => "消息通知";

        public MessageNotifyConfigPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 消息类型
            var lblType = CreateLabel("消息类型:", 10);
            _cmbType = CreateComboBox(32, 100);
            _cmbType.Items.AddRange(new object[] { "信息", "警告", "错误", "成功" });
            _cmbType.SelectedIndex = 0;

            // 标题
            var lblTitle = CreateLabel("标题:", 68);
            _txtTitle = CreateTextBox(90, 160);
            _txtTitle.PlaceholderText = "消息标题";

            // 内容
            var lblContent = CreateLabel("内容:", 126);
            _txtContent = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 148),
                Width = 160,
                Height = 60,
                Multiline = true
            };
            _txtContent.TextChanged += (s, e) => MarkAsChanged();

            // 等待确认
            _chkWaitConfirm = CreateCheckBox("等待用户确认", 218);

            // 分隔线
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(8, 248),
                Size = new Size(160, 1)
            };

            // 备注
            var lblRemark = CreateLabel("备注:", 258);
            _txtRemark = new TextBox
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 280),
                Width = 160,
                Height = 40,
                Multiline = true
            };
            _txtRemark.TextChanged += (s, e) => MarkAsChanged();

            this.Controls.AddRange(new Control[] {
                lblType, _cmbType,
                lblTitle, _txtTitle,
                lblContent, _txtContent,
                _chkWaitConfirm,
                separator,
                lblRemark, _txtRemark
            });
        }

        protected override void LoadFromNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_MessageNotify;
            if (param != null)
            {
                var idx = _cmbType.Items.IndexOf(param.MessageType);
                _cmbType.SelectedIndex = idx >= 0 ? idx : 0;
                _txtTitle.Text = param.Title ?? "";
                _txtContent.Text = param.Content ?? "";
                _chkWaitConfirm.Checked = param.WaitForConfirm;
            }
            _txtRemark.Text = _node.Remark ?? "";
        }

        protected override void ApplyToNode()
        {
            if (_node == null) return;

            var param = _node.StepParameter as Parameter_MessageNotify ?? new Parameter_MessageNotify();
            param.MessageType = _cmbType.SelectedItem?.ToString() ?? "信息";
            param.Title = _txtTitle.Text;
            param.Content = _txtContent.Text;
            param.WaitForConfirm = _chkWaitConfirm.Checked;

            _node.StepParameter = param;
            _node.Remark = _txtRemark.Text;
        }

        protected override void ClearUI()
        {
            _cmbType.SelectedIndex = 0;
            _txtTitle.Text = "";
            _txtContent.Text = "";
            _chkWaitConfirm.Checked = false;
            _txtRemark.Text = "";
        }
    }

    #endregion
}
