using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    partial class WorkflowDesignerPanel
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            _leftPanel = new Panel();
            _toolbarPanel = new Panel();
            _toolStrip = new ToolStrip();
            _nodeTreeView = new STNodeTreeView();
            _mainSplitContainer = new SplitContainer();
            _nodeEditor = new STNodeEditor();

            // ★ 新增的控件 ★
            _rightSplitContainer = new SplitContainer();
            _configHostPanel = new NodeConfigHostPanel();
            _propertyGrid = new STNodePropertyGrid();

            _leftPanel.SuspendLayout();
            _toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_mainSplitContainer).BeginInit();
            _mainSplitContainer.Panel1.SuspendLayout();
            _mainSplitContainer.Panel2.SuspendLayout();
            _mainSplitContainer.SuspendLayout();

            // ★ 新增 ★
            ((System.ComponentModel.ISupportInitialize)_rightSplitContainer).BeginInit();
            _rightSplitContainer.Panel1.SuspendLayout();
            _rightSplitContainer.Panel2.SuspendLayout();
            _rightSplitContainer.SuspendLayout();

            SuspendLayout();

            // 
            // _leftPanel
            // 
            _leftPanel.BackColor = Color.FromArgb(45, 45, 48);
            _leftPanel.Controls.Add(_nodeTreeView);
            _leftPanel.Controls.Add(_toolbarPanel);
            _leftPanel.Dock = DockStyle.Left;
            _leftPanel.Location = new Point(0, 0);
            _leftPanel.Name = "_leftPanel";
            _leftPanel.Size = new Size(250, 700);
            _leftPanel.TabIndex = 1;

            // 
            // _toolbarPanel
            // 
            _toolbarPanel.BackColor = Color.FromArgb(45, 45, 48);
            _toolbarPanel.Controls.Add(_toolStrip);
            _toolbarPanel.Dock = DockStyle.Top;
            _toolbarPanel.Location = new Point(0, 0);
            _toolbarPanel.Name = "_toolbarPanel";
            _toolbarPanel.Padding = new Padding(5);
            _toolbarPanel.Size = new Size(250, 300);
            _toolbarPanel.TabIndex = 1;

            // 
            // _toolStrip
            // 
            _toolStrip.BackColor = Color.FromArgb(45, 45, 48);
            _toolStrip.Dock = DockStyle.Fill;
            _toolStrip.ForeColor = Color.White;
            _toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            _toolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            _toolStrip.Location = new Point(5, 5);
            _toolStrip.Name = "_toolStrip";
            _toolStrip.Size = new Size(240, 290);
            _toolStrip.TabIndex = 0;

            // 
            // _nodeTreeView
            // 
            _nodeTreeView.BackColor = Color.FromArgb(35, 35, 35);
            _nodeTreeView.Dock = DockStyle.Fill;
            _nodeTreeView.ForeColor = Color.White;
            _nodeTreeView.InfoButtonColor = Color.DarkCyan;
            _nodeTreeView.ItemBackColor = Color.FromArgb(45, 45, 48);
            _nodeTreeView.ItemHoverColor = Color.FromArgb(60, 60, 60);
            _nodeTreeView.Location = new Point(0, 300);
            _nodeTreeView.MinimumSize = new Size(100, 60);
            _nodeTreeView.Name = "_nodeTreeView";
            _nodeTreeView.ShowFolderCount = true;
            _nodeTreeView.ShowInfoButton = true;
            _nodeTreeView.Size = new Size(250, 400);
            _nodeTreeView.TabIndex = 0;
            _nodeTreeView.TitleColor = Color.FromArgb(127, 37, 37, 38);

            // 
            // _mainSplitContainer
            // 
            _mainSplitContainer.Dock = DockStyle.Fill;
            _mainSplitContainer.Location = new Point(250, 0);
            _mainSplitContainer.Name = "_mainSplitContainer";

            // 
            // _mainSplitContainer.Panel1
            // 
            _mainSplitContainer.Panel1.Controls.Add(_nodeEditor);

            // 
            // _mainSplitContainer.Panel2 - ★ 修改：使用 _rightSplitContainer ★
            // 
            _mainSplitContainer.Panel2.Controls.Add(_rightSplitContainer);
            _mainSplitContainer.Size = new Size(950, 700);
            _mainSplitContainer.SplitterDistance = 750;
            _mainSplitContainer.SplitterWidth = 5;
            _mainSplitContainer.TabIndex = 0;

            // 
            // _nodeEditor
            // 
            _nodeEditor.AllowDrop = true;
            _nodeEditor.BackColor = Color.FromArgb(34, 34, 34);
            _nodeEditor.Curvature = 0.3F;
            _nodeEditor.Dock = DockStyle.Fill;
            _nodeEditor.GridColor = Color.FromArgb(60, 60, 60);
            _nodeEditor.Location = new Point(0, 0);
            _nodeEditor.LocationBackColor = Color.FromArgb(120, 0, 0, 0);
            _nodeEditor.MarkBackColor = Color.FromArgb(180, 0, 0, 0);
            _nodeEditor.MarkForeColor = Color.FromArgb(180, 0, 0, 0);
            _nodeEditor.MinimumSize = new Size(100, 100);
            _nodeEditor.Name = "_nodeEditor";
            _nodeEditor.Size = new Size(750, 700);
            _nodeEditor.TabIndex = 0;

            // 
            // ★ _rightSplitContainer (新增) ★
            // 
            _rightSplitContainer.Dock = DockStyle.Fill;
            _rightSplitContainer.Location = new Point(0, 0);
            _rightSplitContainer.Name = "_rightSplitContainer";
            _rightSplitContainer.Orientation = Orientation.Horizontal;
            _rightSplitContainer.SplitterDistance = 250;
            _rightSplitContainer.SplitterWidth = 4;
            _rightSplitContainer.BackColor = Color.FromArgb(50, 50, 50);
            _rightSplitContainer.Size = new Size(195, 700);
            _rightSplitContainer.TabIndex = 0;

            // 
            // _rightSplitContainer.Panel1 - PropertyGrid
            // 
            _rightSplitContainer.Panel1.Controls.Add(_propertyGrid);
            _rightSplitContainer.Panel1MinSize = 150;

            // 
            // _rightSplitContainer.Panel2 - ConfigHostPanel
            // 
            _rightSplitContainer.Panel2.Controls.Add(_configHostPanel);
            _rightSplitContainer.Panel2MinSize = 200;

            // 
            // _propertyGrid
            // 
            _propertyGrid.BackColor = Color.FromArgb(35, 35, 35);
            _propertyGrid.DescriptionColor = Color.FromArgb(200, 184, 134, 11);
            _propertyGrid.Dock = DockStyle.Fill;
            _propertyGrid.ErrorColor = Color.FromArgb(200, 165, 42, 42);
            _propertyGrid.ForeColor = Color.White;
            _propertyGrid.ItemHoverColor = Color.FromArgb(50, 125, 125, 125);
            _propertyGrid.ItemValueBackColor = Color.FromArgb(80, 80, 80);
            _propertyGrid.Location = new Point(0, 0);
            _propertyGrid.MinimumSize = new Size(120, 50);
            _propertyGrid.Name = "_propertyGrid";
            _propertyGrid.ShowTitle = true;
            _propertyGrid.Size = new Size(195, 250);
            _propertyGrid.TabIndex = 0;
            _propertyGrid.TitleColor = Color.FromArgb(127, 0, 0, 0);

            // 
            // ★ _configHostPanel (新增) ★
            // 
            _configHostPanel.BackColor = Color.FromArgb(40, 40, 40);
            _configHostPanel.Dock = DockStyle.Fill;
            _configHostPanel.Location = new Point(0, 0);
            _configHostPanel.MinimumSize = new Size(180, 200);
            _configHostPanel.Name = "_configHostPanel";
            _configHostPanel.Size = new Size(195, 446);
            _configHostPanel.TabIndex = 0;

            // 
            // WorkflowDesignerPanel
            // 
            Controls.Add(_mainSplitContainer);
            Controls.Add(_leftPanel);
            Name = "WorkflowDesignerPanel";
            Size = new Size(1200, 700);

            _leftPanel.ResumeLayout(false);
            _toolbarPanel.ResumeLayout(false);
            _toolbarPanel.PerformLayout();
            _mainSplitContainer.Panel1.ResumeLayout(false);
            _mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_mainSplitContainer).EndInit();
            _mainSplitContainer.ResumeLayout(false);

            // ★ 新增 ★
            _rightSplitContainer.Panel1.ResumeLayout(false);
            _rightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_rightSplitContainer).EndInit();
            _rightSplitContainer.ResumeLayout(false);

            ResumeLayout(false);
        }

        #endregion

        #region 控件声明

        private Panel _leftPanel;
        private Panel _toolbarPanel;
        private ToolStrip _toolStrip;
        private STNodeTreeView _nodeTreeView;
        private SplitContainer _mainSplitContainer;
        private STNodeEditor _nodeEditor;
        private STNodePropertyGrid _propertyGrid;

        // ★ 新增的控件 ★
        private SplitContainer _rightSplitContainer;
        private NodeConfigHostPanel _configHostPanel;

        #endregion
    }
}
