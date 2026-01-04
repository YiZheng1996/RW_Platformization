namespace MainUI.LogicalConfiguration.NodeEditor.Forms
{
    partial class WorkflowDesignerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowDesignerForm));
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel();
            _zoomLabel = new ToolStripStatusLabel();
            _nodeCountLabel = new ToolStripStatusLabel();
            _designerPanel = new MainUI.LogicalConfiguration.NodeEditor.Controls.WorkflowDesignerPanel();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // _statusStrip
            // 
            _statusStrip.BackColor = Color.FromArgb(45, 45, 48);
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _zoomLabel, _nodeCountLabel });
            _statusStrip.Location = new Point(0, 835);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.Size = new Size(1384, 26);
            _statusStrip.TabIndex = 0;
            // 
            // _statusLabel
            // 
            _statusLabel.ForeColor = Color.White;
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(1244, 21);
            _statusLabel.Spring = true;
            _statusLabel.Text = "就绪";
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _zoomLabel
            // 
            _zoomLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _zoomLabel.BorderStyle = Border3DStyle.Etched;
            _zoomLabel.ForeColor = Color.LightGray;
            _zoomLabel.Name = "_zoomLabel";
            _zoomLabel.Size = new Size(75, 21);
            _zoomLabel.Text = "缩放: 100%";
            // 
            // _nodeCountLabel
            // 
            _nodeCountLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _nodeCountLabel.BorderStyle = Border3DStyle.Etched;
            _nodeCountLabel.ForeColor = Color.LightGray;
            _nodeCountLabel.Name = "_nodeCountLabel";
            _nodeCountLabel.Size = new Size(50, 21);
            _nodeCountLabel.Text = "节点: 0";
            // 
            // _designerPanel
            // 
            _designerPanel.CurrentFilePath = null;
            _designerPanel.Dock = DockStyle.Fill;
            _designerPanel.Location = new Point(0, 0);
            _designerPanel.Name = "_designerPanel";
            _designerPanel.Size = new Size(1384, 835);
            _designerPanel.TabIndex = 1;
            // 
            // WorkflowDesignerForm
            // 
            ClientSize = new Size(1384, 861);
            Controls.Add(_designerPanel);
            Controls.Add(_statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1000, 600);
            Name = "WorkflowDesignerForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "工作流设计器";
            WindowState = FormWindowState.Maximized;
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// 创建菜单栏
        /// </summary>
        private void CreateMenuStrip()
        {
            var menuStrip = new System.Windows.Forms.MenuStrip
            {
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                ForeColor = System.Drawing.Color.White
            };

            // 文件菜单
            var fileMenu = new System.Windows.Forms.ToolStripMenuItem("文件(&F)");
            fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                new System.Windows.Forms.ToolStripMenuItem("新建(&N)", null, OnMenuNew, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N),
                new System.Windows.Forms.ToolStripMenuItem("打开(&O)...", null, OnMenuOpen, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O),
                new System.Windows.Forms.ToolStripMenuItem("保存(&S)", null, OnMenuSave, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S),
                new System.Windows.Forms.ToolStripMenuItem("另存为(&A)...", null, OnMenuSaveAs),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("导出为步骤列表(&E)...", null, OnMenuExport),
                new System.Windows.Forms.ToolStripMenuItem("从步骤列表导入(&I)...", null, OnMenuImport),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("退出(&X)", null, OnMenuExit, System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)
            });

            // 编辑菜单
            var editMenu = new System.Windows.Forms.ToolStripMenuItem("编辑(&E)");
            editMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                new System.Windows.Forms.ToolStripMenuItem("撤销(&U)", null, null, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z) { Enabled = false },
                new System.Windows.Forms.ToolStripMenuItem("重做(&R)", null, null, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y) { Enabled = false },
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("删除选中(&D)", null, OnMenuDelete, System.Windows.Forms.Keys.Delete),
                new System.Windows.Forms.ToolStripMenuItem("全选(&A)", null, OnMenuSelectAll, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)
            });

            // 视图菜单
            var viewMenu = new System.Windows.Forms.ToolStripMenuItem("视图(&V)");
            viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                new System.Windows.Forms.ToolStripMenuItem("放大(&I)", null, OnMenuZoomIn, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Add),
                new System.Windows.Forms.ToolStripMenuItem("缩小(&O)", null, OnMenuZoomOut, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Subtract),
                new System.Windows.Forms.ToolStripMenuItem("重置缩放(&R)", null, OnMenuZoomReset, System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("自动布局(&L)", null, OnMenuAutoLayout)
            });

            // 工作流菜单
            var workflowMenu = new System.Windows.Forms.ToolStripMenuItem("工作流(&W)");
            workflowMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                new System.Windows.Forms.ToolStripMenuItem("验证(&V)", null, OnMenuValidate, System.Windows.Forms.Keys.F5),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("添加开始节点", null, (s, e) => AddNodeAtCenter("Start")),
                new System.Windows.Forms.ToolStripMenuItem("添加结束节点", null, (s, e) => AddNodeAtCenter("End")),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("添加条件判断", null, (s, e) => AddNodeAtCenter("ConditionJudge")),
                new System.Windows.Forms.ToolStripMenuItem("添加循环", null, (s, e) => AddNodeAtCenter("CycleBegins")),
                new System.Windows.Forms.ToolStripMenuItem("添加延时等待", null, (s, e) => AddNodeAtCenter("DelayWait"))
            });

            // 帮助菜单
            var helpMenu = new System.Windows.Forms.ToolStripMenuItem("帮助(&H)");
            helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                new System.Windows.Forms.ToolStripMenuItem("使用说明(&H)", null, OnMenuHelp, System.Windows.Forms.Keys.F1),
                new System.Windows.Forms.ToolStripSeparator(),
                new System.Windows.Forms.ToolStripMenuItem("关于(&A)", null, OnMenuAbout)
            });

            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                fileMenu, editMenu, viewMenu, workflowMenu, helpMenu
            });

            // 设置菜单项颜色
            foreach (System.Windows.Forms.ToolStripMenuItem item in menuStrip.Items)
            {
                item.ForeColor = System.Drawing.Color.White;
            }

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        #endregion

        #region 控件字段声明

        private MainUI.LogicalConfiguration.NodeEditor.Controls.WorkflowDesignerPanel _designerPanel;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _zoomLabel;
        private System.Windows.Forms.ToolStripStatusLabel _nodeCountLabel;

        #endregion
    }
}