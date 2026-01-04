using MainUI.LogicalConfiguration.Parameter;
using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 条件判断节点 - 根据条件表达式判断执行分支
    /// 有两个输出端口：True 和 False
    /// </summary>
    [STNode("逻辑控制", "工作流设计器", "条件判断", "", "根据条件表达式判断执行分支")]
    public class ConditionNode : WorkflowNodeBase
    {
        #region 属性

        public override string StepName => "ConditionJudge";
        public override string DisplayName => "条件判断";
        public override string CategoryPath => "逻辑控制";
        public override string Description => "根据条件表达式判断执行分支，条件为真执行True分支，否则执行False分支";

        /// <summary>
        /// 条件参数
        /// </summary>
        public Parameter_Detection Parameter
        {
            get => StepParameter as Parameter_Detection;
            set => StepParameter = value;
        }

        /// <summary>
        /// 配置摘要
        /// </summary>
        public override string ConfigSummary
        {
            get
            {
                if (Parameter == null || string.IsNullOrEmpty(Parameter.ConditionExpression))
                    return "未配置条件";

                string expr = Parameter.ConditionExpression;
                return expr.Length > 25 ? expr.Substring(0, 22) + "..." : expr;
            }
        }

        #endregion

        #region 输出端口

        /// <summary>
        /// True 分支输出
        /// </summary>
        public STNodeOption OutputTrue;

        /// <summary>
        /// False 分支输出
        /// </summary>
        public STNodeOption OutputFalse;

        #endregion

        #region 重写方法

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(180, 80);
            this.Title = "条件判断";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 255, 193, 7); // 黄色
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // True 分支 (绿色)
            OutputTrue = this.OutputOptions.Add("✓ True", ExecutionFlowType, false);

            // False 分支 (红色)
            OutputFalse = this.OutputOptions.Add("✗ False", ExecutionFlowType, false);
        }

        protected override void OnOwnerChanged()
        {
            base.OnOwnerChanged();

            if (this.Owner == null) return;

            // 可以为不同分支设置不同颜色（通过自定义绘制实现）
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_Detection
            {
                DetectionName = "条件判断",
                ConditionExpression = "{value} >= 0"
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_Detection>(json);
            }
            catch
            {
                Parameter = new Parameter_Detection();
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_Detection);
        }

        protected override void OnDrawBody(DrawingTools dt)
        {
            base.OnDrawBody(dt);

            Graphics g = dt.Graphics;

            // 绘制分支标识
            DrawBranchIndicators(g);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 绘制分支指示器
        /// </summary>
        private void DrawBranchIndicators(Graphics g)
        {
            int rightX = this.Left + this.Width - 35;
            int y1 = this.Top + this.TitleHeight + 15;
            int y2 = y1 + 25;

            // True 标识 (绿色小圆点)
            using (var brush = new SolidBrush(Color.FromArgb(40, 167, 69)))
            {
                g.FillEllipse(brush, rightX, y1, 8, 8);
            }

            // False 标识 (红色小圆点)
            using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
            {
                g.FillEllipse(brush, rightX, y2, 8, 8);
            }
        }

        /// <summary>
        /// 获取 True 分支连接的节点
        /// </summary>
        public WorkflowNodeBase GetTrueBranchNode()
        {
            if (OutputTrue?.ConnectionCount > 0)
            {
                var connections = OutputTrue.GetConnectedOption();
                if (connections.Count > 0)
                {
                    return connections[0].Owner as WorkflowNodeBase;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取 False 分支连接的节点
        /// </summary>
        public WorkflowNodeBase GetFalseBranchNode()
        {
            if (OutputFalse?.ConnectionCount > 0)
            {
                var connections = OutputFalse.GetConnectedOption();
                if (connections.Count > 0)
                {
                    return connections[0].Owner as WorkflowNodeBase;
                }
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 循环节点 - 支持循环执行一组步骤
    /// 有两个输出：循环体 和 完成后
    /// </summary>
    [STNode("逻辑控制", "工作流设计器", "循环工具", "", "循环执行一组步骤")]
    public class LoopNode : WorkflowNodeBase
    {
        #region 属性

        public override string StepName => "CycleBegins";
        public override string DisplayName => "循环";
        public override string CategoryPath => "逻辑控制";
        public override string Description => "循环执行一组步骤，支持固定次数循环和条件循环";

        /// <summary>
        /// 循环参数
        /// </summary>
        public Parameter_Loop Parameter
        {
            get => StepParameter as Parameter_Loop;
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

                string countExpr = Parameter.LoopCountExpression ?? "10";
                string summary = $"循环 {countExpr} 次";

                if (Parameter.EnableEarlyExit && !string.IsNullOrEmpty(Parameter.ExitConditionExpression))
                {
                    summary += " [可提前退出]";
                }

                return summary;
            }
        }

        #endregion

        #region 输出端口

        /// <summary>
        /// 循环体输出 - 每次循环执行的分支
        /// </summary>
        protected STNodeOption OutputLoopBody;

        /// <summary>
        /// 完成输出 - 循环结束后执行的分支
        /// </summary>
        protected STNodeOption OutputComplete;

        /// <summary>
        /// 循环返回输入 - 从循环体返回的入口
        /// </summary>
        protected STNodeOption InputLoopBack;

        #endregion

        #region 重写方法

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(180, 100);
            this.Title = "循环";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 138, 43, 226); // 紫色
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入
            InputExecution = this.InputOptions.Add("▶ 进入", ExecutionFlowType, true);

            // 循环返回输入
            InputLoopBack = this.InputOptions.Add("↩ 返回", ExecutionFlowType, true);

            // 循环体输出
            OutputLoopBody = this.OutputOptions.Add("↻ 循环体", ExecutionFlowType, false);

            // 完成输出
            OutputComplete = this.OutputOptions.Add("✓ 完成", ExecutionFlowType, false);
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_Loop
            {
                LoopCountExpression = "10",
                CounterVariableName = "LoopIndex",
                EnableCounter = true,
                EnableEarlyExit = false,
                Description = "循环步骤"
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_Loop>(json);
            }
            catch
            {
                Parameter = new Parameter_Loop();
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_Loop);
        }

        protected override void OnDrawBody(DrawingTools dt)
        {
            base.OnDrawBody(dt);

            Graphics g = dt.Graphics;

            // 绘制循环指示
            DrawLoopIndicator(g);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 绘制循环指示器
        /// </summary>
        private void DrawLoopIndicator(Graphics g)
        {
            // 绘制一个小的循环箭头图标
            int centerX = this.Left + this.Width / 2;
            int centerY = this.Top + this.TitleHeight + (this.Height - this.TitleHeight) / 2 + 10;
            int radius = 12;

            using var pen = new Pen(Color.FromArgb(100, 138, 43, 226), 2);
            // 绘制圆弧
            g.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, -30, 300);

            // 绘制箭头
            Point[] arrow =
            [
                new Point(centerX + radius - 2, centerY - 5),
                    new Point(centerX + radius + 5, centerY),
                    new Point(centerX + radius - 2, centerY + 5)
            ];
            g.DrawLines(pen, arrow);
        }

        /// <summary>
        /// 获取循环体分支连接的第一个节点
        /// </summary>
        public WorkflowNodeBase GetLoopBodyNode()
        {
            if (OutputLoopBody?.ConnectionCount > 0)
            {
                var connections = OutputLoopBody.GetConnectedOption();
                if (connections.Count > 0)
                {
                    return connections[0].Owner as WorkflowNodeBase;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取完成后分支连接的节点
        /// </summary>
        public WorkflowNodeBase GetCompleteNode()
        {
            if (OutputComplete?.ConnectionCount > 0)
            {
                var connections = OutputComplete.GetConnectedOption();
                if (connections.Count > 0)
                {
                    return connections[0].Owner as WorkflowNodeBase;
                }
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 等待稳定节点 - 等待变量值稳定
    /// </summary>
    [STNode("逻辑控制", "工作流设计器", "等待稳定", "", "等待变量值稳定后继续执行")]
    public class WaitForStableNode : WorkflowNodeBase
    {
        #region 属性

        public override string StepName => "Waitingforstability";
        public override string DisplayName => "等待稳定";
        public override string CategoryPath => "逻辑控制";
        public override string Description => "监测变量或PLC地址的值，当变化率小于阈值时认为稳定";

        /// <summary>
        /// 参数
        /// </summary>
        public Parameter_WaitForStable Parameter
        {
            get => StepParameter as Parameter_WaitForStable;
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

                string source = Parameter.MonitorSourceType == MonitorSourceType.Variable
                    ? $"@{Parameter.MonitorVariable}"
                    : $"PLC[{Parameter.PlcModuleName}]";

                return $"监测 {source}, 阈值≤{Parameter.StabilityThreshold:F2}";
            }
        }

        #endregion

        #region 输出端口

        /// <summary>
        /// 超时输出
        /// </summary>
        public STNodeOption OutputTimeout;

        #endregion

        #region 重写方法

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(180, 80);
            this.Title = "等待稳定";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 23, 162, 184); // 青色
        }

        protected override void CreateDefaultPorts()
        {
            // 执行输入
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 稳定后输出
            OutputExecution = this.OutputOptions.Add("✓ 稳定", ExecutionFlowType, false);

            // 超时输出 (可选)
            OutputTimeout = this.OutputOptions.Add("⏱ 超时", ExecutionFlowType, false);
        }

        protected override void InitializeParameter()
        {
            Parameter = new Parameter_WaitForStable
            {
                Description = "等待稳定",
                MonitorSourceType = MonitorSourceType.Variable,
                StabilityThreshold = 0.1,
                SamplingInterval = 1,
                StableCount = 3,
                TimeoutSeconds = 60
            };
        }

        protected override void LoadParameterFromJson(string json)
        {
            try
            {
                Parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter_WaitForStable>(json);
            }
            catch
            {
                Parameter = new Parameter_WaitForStable();
            }
        }

        public override Type GetParameterType()
        {
            return typeof(Parameter_WaitForStable);
        }

        #endregion
    }
}
