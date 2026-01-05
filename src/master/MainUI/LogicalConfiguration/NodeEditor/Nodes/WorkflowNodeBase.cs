using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    /// <summary>
    /// 工作流节点基类 - 所有工作流节点的父类
    /// 提供通用属性、绘制方法和序列化支持
    /// </summary>
    public abstract class WorkflowNodeBase : STNode
    {
        #region 静态常量

        /// <summary>
        /// 执行流数据类型 - 用于节点之间的连接
        /// </summary>
        public static readonly Type ExecutionFlowType = typeof(ExecutionFlow);

        /// <summary>
        /// 节点默认宽度
        /// </summary>
        protected const int DEFAULT_WIDTH = 180;

        /// <summary>
        /// 节点默认高度
        /// </summary>
        protected const int DEFAULT_HEIGHT = 60;

        #endregion

        #region 属性

        /// <summary>
        /// 节点唯一标识符
        /// </summary>
        public Guid NodeId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 对应的步骤名称 (StepName)
        /// </summary>
        public abstract string StepName { get; }

        /// <summary>
        /// 节点显示名称
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// 节点分类路径 (用于TreeView)
        /// </summary>
        public abstract string CategoryPath { get; }

        /// <summary>
        /// 节点描述
        /// </summary>
        public virtual string Description => "";

        /// <summary>
        /// 节点图标 (可选)
        /// </summary>
        public virtual Image NodeIcon => null;

        /// <summary>
        /// 步骤参数对象
        /// </summary>
        public object StepParameter { get; set; }

        /// <summary>
        /// 步骤备注
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 是否已配置
        /// </summary>
        public bool IsConfigured { get; set; } = false;

        /// <summary>
        /// 配置摘要 (显示在节点上)
        /// </summary>
        public virtual string ConfigSummary => IsConfigured ? "已配置" : "未配置";

        #endregion

        #region 输入输出选项

        /// <summary>
        /// 执行输入端口
        /// </summary>
        public STNodeOption InputExecution;

        /// <summary>
        /// 执行输出端口 (普通流程)
        /// </summary>
        public STNodeOption OutputExecution;

        #endregion

        #region 构造函数

        protected WorkflowNodeBase()
        {
            // 基本设置在 OnCreate 中完成
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 节点创建时调用
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // 设置标题
            this.Title = DisplayName;

            // 设置默认大小
            this.AutoSize = false;
            this.Size = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);

            // 设置标题颜色 (子类可覆盖)
            this.TitleColor = GetTitleColor();

            // 添加默认执行端口
            CreateDefaultPorts();

            // 初始化参数
            InitializeParameter();
        }

        /// <summary>
        /// 所有者改变时设置类型颜色
        /// </summary>
        protected override void OnOwnerChanged()
        {
            base.OnOwnerChanged();

            if (this.Owner == null) return;

            // 设置执行流类型的颜色
            this.Owner.SetTypeColor(ExecutionFlowType, Color.White);
        }

        /// <summary>
        /// 绘制节点主体
        /// </summary>
        protected override void OnDrawBody(DrawingTools dt)
        {
            base.OnDrawBody(dt);

            Graphics g = dt.Graphics;

            // 绘制配置摘要
            DrawConfigSummary(g);

            // 绘制图标 (如果有)
            if (NodeIcon != null)
            {
                DrawNodeIcon(g);
            }
        }

        /// <summary>
        /// 保存节点数据
        /// </summary>
        protected override void OnSaveNode(Dictionary<string, byte[]> dic)
        {
            // 保存节点ID
            dic["NodeId"] = System.Text.Encoding.UTF8.GetBytes(NodeId.ToString());

            // 保存备注
            dic["Remark"] = System.Text.Encoding.UTF8.GetBytes(Remark ?? "");

            // 保存配置状态
            dic["IsConfigured"] = BitConverter.GetBytes(IsConfigured);

            // 保存参数 (JSON序列化)
            if (StepParameter != null)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(StepParameter);
                dic["StepParameter"] = System.Text.Encoding.UTF8.GetBytes(json);
            }

            // 子类可以继续添加保存数据
            OnSaveNodeData(dic);
        }

        /// <summary>
        /// 加载节点数据
        /// </summary>
        protected override void OnLoadNode(Dictionary<string, byte[]> dic)
        {
            // 加载节点ID
            if (dic.TryGetValue("NodeId", out byte[] value))
            {
                string idStr = System.Text.Encoding.UTF8.GetString(value);
                if (Guid.TryParse(idStr, out Guid id))
                {
                    NodeId = id;
                }
            }

            // 加载备注
            if (dic.ContainsKey("Remark"))
            {
                Remark = System.Text.Encoding.UTF8.GetString(dic["Remark"]);
            }

            // 加载配置状态
            if (dic.ContainsKey("IsConfigured"))
            {
                IsConfigured = BitConverter.ToBoolean(dic["IsConfigured"], 0);
            }

            // 加载参数
            if (dic.ContainsKey("StepParameter"))
            {
                string json = System.Text.Encoding.UTF8.GetString(dic["StepParameter"]);
                LoadParameterFromJson(json);
            }

            // 子类可以继续加载数据
            OnLoadNodeData(dic);
        }

        #endregion

        #region 虚方法 - 子类可重写

        /// <summary>
        /// 获取标题栏颜色
        /// </summary>
        protected virtual Color GetTitleColor()
        {
            return Color.FromArgb(200, 74, 144, 226); // 默认蓝色
        }

        /// <summary>
        /// 创建默认端口 - 左右方向
        /// </summary>
        protected virtual void CreateDefaultPorts()
        {
            // 执行输入 - 在左侧
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);

            // 执行输出 - 在右侧
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        /// <summary>
        /// 初始化参数对象
        /// </summary>
        protected virtual void InitializeParameter()
        {
            // 子类实现
        }

        /// <summary>
        /// 从JSON加载参数
        /// </summary>
        protected virtual void LoadParameterFromJson(string json)
        {
            // 子类实现具体的反序列化逻辑
        }

        /// <summary>
        /// 子类保存额外数据
        /// </summary>
        protected virtual void OnSaveNodeData(Dictionary<string, byte[]> dic)
        {
            // 子类实现
        }

        /// <summary>
        /// 子类加载额外数据
        /// </summary>
        protected virtual void OnLoadNodeData(Dictionary<string, byte[]> dic)
        {
            // 子类实现
        }

        /// <summary>
        /// 打开配置对话框
        /// </summary>
        public virtual void OpenConfigDialog()
        {
            // 子类实现具体的配置对话框
        }

        /// <summary>
        /// 获取参数的类型
        /// </summary>
        public virtual Type GetParameterType()
        {
            return null;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 绘制配置摘要
        /// </summary>
        private void DrawConfigSummary(Graphics g)
        {
            string summary = ConfigSummary;
            if (string.IsNullOrEmpty(summary)) return;

            // 计算位置
            int y = this.Top + this.TitleHeight + 5;
            int x = this.Left + 10;
            int width = this.Width - 20;

            // 设置字体和颜色
            using var font = new Font("微软雅黑", 8f);
            using var brush = new SolidBrush(IsConfigured ? Color.LightGreen : Color.Orange);
            // 截断过长的文本
            string displayText = summary;
            SizeF textSize = g.MeasureString(displayText, font);
            if (textSize.Width > width)
            {
                displayText = TruncateText(displayText, font, width, g);
            }

            g.DrawString(displayText, font, brush, x, y);
        }

        /// <summary>
        /// 绘制节点图标
        /// </summary>
        private void DrawNodeIcon(Graphics g)
        {
            if (NodeIcon == null) return;

            int iconSize = 16;
            int x = this.Left + this.Width - iconSize - 5;
            int y = this.Top + 5;

            g.DrawImage(NodeIcon, new Rectangle(x, y, iconSize, iconSize));
        }

        /// <summary>
        /// 截断文本
        /// </summary>
        private string TruncateText(string text, Font font, int maxWidth, Graphics g)
        {
            if (string.IsNullOrEmpty(text)) return "";

            string ellipsis = "...";
            SizeF ellipsisSize = g.MeasureString(ellipsis, font);
            float availableWidth = maxWidth - ellipsisSize.Width;

            for (int i = text.Length - 1; i > 0; i--)
            {
                string truncated = text.Substring(0, i);
                SizeF size = g.MeasureString(truncated, font);
                if (size.Width <= availableWidth)
                {
                    return truncated + ellipsis;
                }
            }

            return ellipsis;
        }

        /// <summary>
        /// 刷新节点显示
        /// </summary>
        public void RefreshDisplay()
        {
            this.Invalidate();
        }

        #endregion

        #region 转换方法

        /// <summary>
        /// 转换为 ChildModel
        /// </summary>
        public ChildModel ToChildModel(int stepNum)
        {
            return new ChildModel
            {
                StepNum = stepNum,
                StepName = this.StepName,
                StepParameter = this.StepParameter,
                Remark = this.Remark,
                Status = 0,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// 从 ChildModel 加载数据
        /// </summary>
        public virtual void LoadFromChildModel(ChildModel model)
        {
            if (model == null) return;

            this.StepParameter = model.StepParameter;
            this.Remark = model.Remark ?? "";
            this.IsConfigured = model.StepParameter != null &&
                               !(model.StepParameter is int) &&
                               !(model.StepParameter is long);
        }

        #endregion

        #region 公共方法 - 获取连接信息

        /// <summary>
        /// 获取输出端口连接的节点列表
        /// </summary>
        public List<WorkflowNodeBase> GetConnectedNodesFromOutput(int outputIndex = 0)
        {
            var result = new List<WorkflowNodeBase>();

            STNodeOption option = (outputIndex == 0) ? OutputExecution : null;
            if (option != null && option.ConnectionCount > 0)
            {
                var connectedOptions = option.GetConnectedOption();
                foreach (var connOpt in connectedOptions)
                {
                    if (connOpt.Owner is WorkflowNodeBase node)
                    {
                        result.Add(node);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取下一个连接的节点
        /// </summary>
        public WorkflowNodeBase GetNextNode()
        {
            var nodes = GetConnectedNodesFromOutput(0);
            return nodes.Count > 0 ? nodes[0] : null;
        }

        /// <summary>
        /// 检查是否有输入连接
        /// </summary>
        public bool HasInputConnection()
        {
            return InputExecution != null && InputExecution.ConnectionCount > 0;
        }

        /// <summary>
        /// 检查是否有输出连接
        /// </summary>
        public bool HasOutputConnection()
        {
            return OutputExecution != null && OutputExecution.ConnectionCount > 0;
        }

        /// <summary>
        /// 连接到另一个节点
        /// </summary>
        public bool ConnectTo(WorkflowNodeBase targetNode)
        {
            if (OutputExecution == null || targetNode?.InputExecution == null)
                return false;

            return OutputExecution.ConnectOption(targetNode.InputExecution) == ConnectionStatus.Connected;
        }

        #endregion
    }

    #region 辅助类型

    /// <summary>
    /// 执行流类型 - 用于标识节点间的执行连接
    /// </summary>
    public class ExecutionFlow
    {
        public static readonly ExecutionFlow Instance = new();
    }


    #endregion
}
