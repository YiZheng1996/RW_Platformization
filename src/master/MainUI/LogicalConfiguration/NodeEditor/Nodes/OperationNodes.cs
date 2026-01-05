using MainUI.LogicalConfiguration.Parameter;
using ST.Library.UI.NodeEditor;
using static MainUI.LogicalConfiguration.Parameter.Parameter_WritePLC;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    #region 延时等待节点

    /// <summary>
    /// 延时等待节点 - 暂停执行指定时间
    /// </summary>
    //[STNode("逻辑控制/延时等待", "工作流设计器", "", "", "暂停执行指定时间")]
    [STNode("逻辑控制", "工作流设计器", "延时等待", "", "暂停执行指定时间")]
    public class DelayNode : WorkflowNodeBase
    {
        public override string StepName => "DelayWait";
        public override string DisplayName => "延时等待";
        public override string CategoryPath => "逻辑控制";
        public override string Description => "暂停工作流执行指定的时间";

        /// <summary>
        /// 延时参数
        /// </summary>
        public Parameter_DelayTime Parameter
        {
            get => StepParameter as Parameter_DelayTime;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter == null)
                    return "未配置";

                var ms = Parameter.T;
                return ms switch
                {
                    < 1000 => $"等待 {ms:F0} 毫秒",
                    < 60000 => $"等待 {ms / 1000:F1} 秒",
                    _ => $"等待 {ms / 60000:F1} 分钟"
                };
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "延时等待";
        }

        protected override void CreateDefaultPorts()
        {
            // 输入在顶部
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 输出在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 108, 117, 125); // 灰色
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_DelayTime { T = 1000 };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_DelayTime>(json);
            }
            catch
            {
                Parameter = new Parameter_DelayTime { T = 1000 };
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_DelayTime);
        }
    }

    #endregion

    #region 变量赋值节点

    /// <summary>
    /// 变量赋值节点 - 为变量设置值
    /// </summary>
    //[STNode("数据操作/变量赋值", "工作流设计器", "", "", "为变量设置值")]
    [STNode("数据操作", "工作流设计器", "变量赋值", "", "为变量设置值")]
    public class VariableAssignNode : WorkflowNodeBase
    {
        public override string StepName => "VariableAssign";
        public override string DisplayName => "变量赋值";
        public override string CategoryPath => "数据操作";
        public override string Description => "为变量设置固定值、表达式结果或其他变量的值";

        /// <summary>
        /// 赋值参数
        /// </summary>
        public Parameter_VariableAssignment Parameter
        {
            get => StepParameter as Parameter_VariableAssignment;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter == null)
                    return "未配置";

                string target = Parameter.TargetVarName ?? "?";
                return $"@{target} = ...";
            }
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);
            //SetPortAlignment(InputExecution, PortAlignment.Top);

            // 执行输出
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
            //SetPortAlignment(OutputExecution, PortAlignment.Bottom);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "变量赋值";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 40, 167, 69); // 绿色
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_VariableAssignment();
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_VariableAssignment>(json);
            }
            catch
            {
                Parameter = new Parameter_VariableAssignment();
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_VariableAssignment);
        }
    }

    #endregion

    #region 读取PLC节点

    /// <summary>
    /// 读取PLC节点 - 从PLC读取数据到变量
    /// </summary>
    //[STNode("通信操作/读取PLC", "工作流设计器", "", "", "从PLC读取数据到变量")]
    [STNode("通信操作", "工作流设计器", "读取PLC", "", "从PLC读取数据到变量")]
    public class ReadPLCNode : WorkflowNodeBase
    {
        public override string StepName => "PLCRead";
        public override string DisplayName => "读取PLC";
        public override string CategoryPath => "通信操作";
        public override string Description => "从PLC模块读取数据并存储到变量中";

        /// <summary>
        /// 读取参数
        /// </summary>
        public Parameter_ReadPLC Parameter
        {
            get => StepParameter as Parameter_ReadPLC;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter?.Items == null || Parameter.Items.Count == 0)
                    return "未配置点位";

                return $"读取 {Parameter.Items.Count} 个点位";
            }
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入 - 在顶部
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 执行输出 - 在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "读取PLC";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 13, 110, 253); // 蓝色
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_ReadPLC
            {
                Items = new List<PlcReadItem>()
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_ReadPLC>(json);
            }
            catch
            {
                Parameter = new Parameter_ReadPLC { Items = new List<PlcReadItem>() };
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_ReadPLC);
        }
    }

    #endregion

    #region 写入PLC节点

    /// <summary>
    /// 写入PLC节点 - 向PLC写入数据
    /// </summary>
    //[STNode("通信操作/写入PLC", "工作流设计器", "", "", "向PLC写入数据")]
    [STNode("通信操作", "工作流设计器", "写入PLC", "", "向PLC写入数据")]
    public class WritePLCNode : WorkflowNodeBase
    {
        public override string StepName => "PLCWrite";
        public override string DisplayName => "写入PLC";
        public override string CategoryPath => "通信操作";
        public override string Description => "向PLC模块写入数据";

        /// <summary>
        /// 写入参数
        /// </summary>
        public Parameter_WritePLC Parameter
        {
            get => StepParameter as Parameter_WritePLC;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter?.Items == null || Parameter.Items.Count == 0)
                    return "未配置点位";

                return $"写入 {Parameter.Items.Count} 个点位";
            }
        }
        protected override void CreateDefaultPorts()
        {
            // 执行输入 - 在顶部
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 执行输出 - 在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "写入PLC";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 220, 53, 69); // 红色
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_WritePLC
            {
                Items = []
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_WritePLC>(json);
            }
            catch
            {
                Parameter = new Parameter_WritePLC { Items = new List<PLCWriteItem>() };
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_WritePLC);
        }
    }

    #endregion

    #region 消息通知节点

    /// <summary>
    /// 消息通知节点 - 发送消息提示
    /// </summary>
    //[STNode("数据操作/消息通知", "工作流设计器", "", "", "发送消息通知")]
    [STNode("数据操作", "工作流设计器", "消息通知", "", "发送消息通知")]
    public class MessageNotifyNode : WorkflowNodeBase
    {
        public override string StepName => "MessageNotify";
        public override string DisplayName => "消息通知";
        public override string CategoryPath => "数据操作";
        public override string Description => "发送消息通知或弹出提示框";

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType { get; set; } = MessageType.Info;

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return "未配置消息";

                string preview = Message.Length > 20 ? Message.Substring(0, 17) + "..." : Message;
                return $"[{MessageType}] {preview}";
            }
        }
        protected override void CreateDefaultPorts()
        {
            // 执行输入 - 在顶部
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 执行输出 - 在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "消息通知";
        }

        protected override Color GetTitleColor()
        {
            return MessageType switch
            {
                MessageType.Info => Color.FromArgb(200, 23, 162, 184),
                MessageType.Warning => Color.FromArgb(200, 255, 193, 7),
                MessageType.Error => Color.FromArgb(200, 220, 53, 69),
                MessageType.Question => Color.FromArgb(200, 40, 167, 69),
                _ => Color.FromArgb(200, 108, 117, 125)
            };
        }

        protected override void OnSaveNodeData(Dictionary<string, byte[]> dic)
        {
            dic["Message"] = System.Text.Encoding.UTF8.GetBytes(Message ?? "");
            dic["MessageType"] = BitConverter.GetBytes((int)MessageType);
        }

        protected override void OnLoadNodeData(Dictionary<string, byte[]> dic)
        {
            if (dic.ContainsKey("Message"))
            {
                Message = System.Text.Encoding.UTF8.GetString(dic["Message"]);
            }
            if (dic.TryGetValue("MessageType", out byte[] value))
            {
                MessageType = (MessageType)BitConverter.ToInt32(value, 0);
            }
        }
    }
    #endregion

    #region 实时监控节点

    /// <summary>
    /// 实时监控节点 - 显示实时监控窗口
    /// </summary>
    //[STNode("逻辑控制/实时监控", "工作流设计器", "", "", "显示实时监控窗口")]
    [STNode("逻辑控制", "工作流设计器", "实时监控", "", "显示实时监控窗口")]
    public class RealtimeMonitorNode : WorkflowNodeBase
    {
        public override string StepName => "MonitorTool";
        public override string DisplayName => "实时监控";
        public override string CategoryPath => "逻辑控制";
        public override string Description => "显示实时监控窗口，监测变量或PLC值的变化";

        /// <summary>
        /// 监控参数
        /// </summary>
        public Parameter_RealtimeMonitorPrompt Parameter
        {
            get => StepParameter as Parameter_RealtimeMonitorPrompt;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter == null)
                    return "未配置";

                return Parameter.Title ?? "实时监控";
            }
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入 - 在顶部
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 执行输出 - 在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.Title = "实时监控";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 111, 66, 193); // 紫色
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_RealtimeMonitorPrompt
            {
                Title = "实时监控"
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_RealtimeMonitorPrompt>(json);
            }
            catch
            {
                Parameter = new Parameter_RealtimeMonitorPrompt();
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_RealtimeMonitorPrompt);
        }
    }

    #endregion
}
