using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Nodes
{
    #region 开始节点

    /// <summary>
    /// 开始节点 - 工作流的起点
    /// </summary>
    [STNode("工作流", "工作流设计器", "开始流程", "", "工作流的起始点")]
    public class StartNode : WorkflowNodeBase
    {
        public override string StepName => "Start";

        public override string DisplayName => "开始流程";

        public override string CategoryPath => "工作流";

        public override string Description => "工作流的起始点，每个工作流必须有且只有一个开始节点";

        public override string ConfigSummary => "";

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(100, 50);
            this.Title = "开始流程";
        }

        protected override void OnOwnerChanged()
        {
            base.OnOwnerChanged();
            if (this.Owner != null)
            {
                // 设置执行流类型的颜色 - 使用白色
                this.Owner?.SetTypeColor(ExecutionFlowType, Color.White);
            }
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 40, 167, 69); // 绿色
        }

        protected override void CreateDefaultPorts()
        {
            // 开始节点只有输出端口，放在底部
            OutputExecution = this.OutputOptions.Add("▶", ExecutionFlowType, false);
        }

        //protected override void OnDrawBody(DrawingTools dt)
        //{
        //    // 开始节点不需要绘制配置摘要
        //    Graphics g = dt.Graphics;

        //    // 绘制圆形背景
        //    int padding = 5;
        //    Rectangle rect = new(
        //        this.Left + padding,
        //        this.Top + this.TitleHeight + padding,
        //        this.Width - padding * 2,
        //        this.Height - this.TitleHeight - padding * 2
        //    );

        //    using var brush = new SolidBrush(Color.FromArgb(50, 40, 167, 69));
        //    g.FillEllipse(brush, rect);
        //}
    }

    #endregion

    #region 结束节点

    /// <summary>
    /// 结束节点 - 工作流的终点
    /// </summary>
    [STNode("工作流", "工作流设计器", "结束流程", "", "工作流的结束点")]
    public class EndNode : WorkflowNodeBase
    {
        public override string StepName => "End";
        public override string DisplayName => "结束流程";
        public override string CategoryPath => "工作流";
        public override string Description => "工作流的结束点，可以有多个结束节点";
        public override string ConfigSummary => "";

        /// <summary>
        /// 结束状态: 成功/失败/中止
        /// </summary>
        public EndStatus Status { get; set; } = EndStatus.Success;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(100, 50);
            UpdateTitle();
        }

        protected override Color GetTitleColor()
        {
            return Status switch
            {
                EndStatus.Success => Color.FromArgb(200, 40, 167, 69),  // 绿色
                EndStatus.Failure => Color.FromArgb(200, 220, 53, 69), // 红色
                EndStatus.Abort => Color.FromArgb(200, 255, 193, 7),   // 黄色
                _ => Color.FromArgb(200, 108, 117, 125)                // 灰色
            };
        }

        protected override void CreateDefaultPorts()
        {
            // 结束节点只有输入端口
            InputExecution = this.InputOptions.Add("▶", ExecutionFlowType, true);
        }

        private void UpdateTitle()
        {
            this.Title = Status switch
            {
                EndStatus.Success => "完成",
                EndStatus.Failure => "失败",
                EndStatus.Abort => "中止",
                _ => "结束"
            };
            this.TitleColor = GetTitleColor();
        }

        public void SetStatus(EndStatus status)
        {
            Status = status;
            UpdateTitle();
            this.Invalidate();
        }

        //protected override void OnDrawBody(DrawingTools dt)
        //{
        //    Graphics g = dt.Graphics;

        //    // 绘制圆形背景
        //    int padding = 5;
        //    Rectangle rect = new(
        //        this.Left + padding,
        //        this.Top + this.TitleHeight + padding,
        //        this.Width - padding * 2,
        //        this.Height - this.TitleHeight - padding * 2
        //    );

        //    Color bgColor = Status switch
        //    {
        //        EndStatus.Success => Color.FromArgb(50, 40, 167, 69),
        //        EndStatus.Failure => Color.FromArgb(50, 220, 53, 69),
        //        EndStatus.Abort => Color.FromArgb(50, 255, 193, 7),
        //        _ => Color.FromArgb(50, 108, 117, 125)
        //    };

        //    using var brush = new SolidBrush(bgColor);
        //    g.FillEllipse(brush, rect);
        //}

        protected override void OnSaveNodeData(System.Collections.Generic.Dictionary<string, byte[]> dic)
        {
            dic["EndStatus"] = BitConverter.GetBytes((int)Status);
        }

        protected override void OnLoadNodeData(System.Collections.Generic.Dictionary<string, byte[]> dic)
        {
            if (dic.ContainsKey("EndStatus"))
            {
                Status = (EndStatus)BitConverter.ToInt32(dic["EndStatus"], 0);
                UpdateTitle();
            }
        }
    }

    /// <summary>
    /// 结束状态枚举
    /// </summary>
    public enum EndStatus
    {
        /// <summary>
        /// 成功完成
        /// </summary>
        Success = 0,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failure = 1,

        /// <summary>
        /// 中止执行
        /// </summary>
        Abort = 2
    }

    #endregion

    #region 注释节点

    /// <summary>
    /// 注释节点 - 用于添加说明文字
    /// </summary>
    [STNode("工作流", "工作流设计器", "注释", "", "添加注释说明")]
    public class CommentNode : WorkflowNodeBase
    {
        public override string StepName => "Comment";
        public override string DisplayName => "注释";
        public override string CategoryPath => "工作流";
        public override string Description => "用于添加说明文字，不参与执行";
        public override string ConfigSummary => string.IsNullOrEmpty(CommentText) ? "双击编辑..." : CommentText;

        /// <summary>
        /// 注释文本
        /// </summary>
        public string CommentText { get; set; } = "";

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Size = new Size(200, 80);
            this.Title = "注释";
        }

        protected override Color GetTitleColor()
        {
            return Color.FromArgb(200, 108, 117, 125); // 灰色
        }

        protected override void CreateDefaultPorts()
        {
            // 注释节点没有端口
        }

        protected override void OnDrawBody(DrawingTools dt)
        {
            Graphics g = dt.Graphics;

            // 绘制注释文本
            int padding = 8;
            Rectangle textRect = new(
                this.Left + padding,
                this.Top + this.TitleHeight + padding,
                this.Width - padding * 2,
                this.Height - this.TitleHeight - padding * 2
            );

            string displayText = string.IsNullOrEmpty(CommentText) ? "双击编辑注释..." : CommentText;

            using var font = new Font("微软雅黑", 9f);
            using var brush = new SolidBrush(Color.FromArgb(200, 200, 200));
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisWord
            };
            g.DrawString(displayText, font, brush, textRect, format);
        }

        protected override void OnSaveNodeData(Dictionary<string, byte[]> dic)
        {
            dic["CommentText"] = System.Text.Encoding.UTF8.GetBytes(CommentText ?? "");
        }

        protected override void OnLoadNodeData(Dictionary<string, byte[]> dic)
        {
            if (dic.TryGetValue("CommentText", out byte[] value))
            {
                CommentText = System.Text.Encoding.UTF8.GetString(value);
            }
        }

        public override void OpenConfigDialog()
        {
            // 简单的输入对话框
            using var form = new System.Windows.Forms.Form();
            form.Text = "编辑注释";
            form.Size = new Size(400, 250);
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            var textBox = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                Text = CommentText,
                Location = new Point(10, 10),
                Size = new Size(365, 150),
                ScrollBars = ScrollBars.Vertical
            };

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(210, 170),
                Size = new Size(80, 30)
            };

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(295, 170),
                Size = new Size(80, 30)
            };

            form.Controls.AddRange([textBox, btnOk, btnCancel]);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK)
            {
                CommentText = textBox.Text;
                this.Invalidate();
            }
        }
    }

    #endregion
}
