using System;
using System.Drawing;
using System.Windows.Forms;
using ST.Library.UI.NodeEditor;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    partial class WorkflowDesignerPanel
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _leftPanel = new Panel();
            _nodeTreeView = new STNodeTreeView();
            _toolbarPanel = new Panel();
            _toolStrip = new ToolStrip();
            _mainSplitContainer = new SplitContainer();
            _nodeEditor = new STNodeEditor();
            _propertyGrid = new STNodePropertyGrid();
            _leftPanel.SuspendLayout();
            _toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_mainSplitContainer).BeginInit();
            _mainSplitContainer.Panel1.SuspendLayout();
            _mainSplitContainer.Panel2.SuspendLayout();
            _mainSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // _leftPanel
            // 
            _leftPanel.BackColor = Color.FromArgb(35, 35, 35);
            _leftPanel.Controls.Add(_nodeTreeView);
            _leftPanel.Controls.Add(_toolbarPanel);
            _leftPanel.Dock = DockStyle.Left;
            _leftPanel.Location = new Point(0, 0);
            _leftPanel.Name = "_leftPanel";
            _leftPanel.Size = new Size(250, 700);
            _leftPanel.TabIndex = 0;
            // 
            // _nodeTreeView
            // 
            _nodeTreeView.AllowDrop = true;
            _nodeTreeView.BackColor = Color.FromArgb(35, 35, 35);
            _nodeTreeView.Dock = DockStyle.Fill;
            _nodeTreeView.FolderCountColor = Color.FromArgb(40, 255, 255, 255);
            _nodeTreeView.ForeColor = Color.FromArgb(220, 220, 220);
            _nodeTreeView.ItemBackColor = Color.FromArgb(45, 45, 45);
            _nodeTreeView.ItemHoverColor = Color.FromArgb(50, 125, 125, 125);
            _nodeTreeView.Location = new Point(0, 300);
            _nodeTreeView.MinimumSize = new Size(100, 60);
            _nodeTreeView.Name = "_nodeTreeView";
            _nodeTreeView.ShowFolderCount = true;
            _nodeTreeView.Size = new Size(250, 400);
            _nodeTreeView.TabIndex = 0;
            _nodeTreeView.TextBoxColor = Color.FromArgb(30, 30, 30);
            _nodeTreeView.TitleColor = Color.FromArgb(60, 60, 60);
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
            // _mainSplitContainer.Panel2
            // 
            _mainSplitContainer.Panel2.Controls.Add(_propertyGrid);
            _mainSplitContainer.Size = new Size(950, 700);
            _mainSplitContainer.SplitterDistance = 766;
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
            _nodeEditor.Size = new Size(766, 700);
            _nodeEditor.TabIndex = 0;
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
            _propertyGrid.Size = new Size(179, 700);
            _propertyGrid.TabIndex = 0;
            _propertyGrid.TitleColor = Color.FromArgb(127, 0, 0, 0);
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
            ResumeLayout(false);
        }


        private void AddToolbarButtons()
        {
            // 设置工具栏样式
            _toolStrip.BackColor = Color.FromArgb(45, 45, 48);
            _toolStrip.ForeColor = Color.White;
            //_toolStrip.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable());

            // 新建
            var btnNew = new ToolStripButton("新建工作流", null, OnNewClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,  // 垂直工具栏按钮宽度
                Height = 35,
                ToolTipText = "新建工作流 (Ctrl+N)"
            };

            // 打开
            var btnOpen = new ToolStripButton("打开工作流", null, OnOpenClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "打开工作流 (Ctrl+O)"
            };

            // 保存
            var btnSave = new ToolStripButton("保存工作流", null, OnSaveClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "保存工作流 (Ctrl+S)"
            };

            _toolStrip.Items.Add(btnNew);
            _toolStrip.Items.Add(btnOpen);
            _toolStrip.Items.Add(btnSave);
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 验证
            var btnValidate = new ToolStripButton("验证工作流", null, OnValidateClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "验证工作流"
            };

            // 自动布局
            var btnAutoLayout = new ToolStripButton("自动布局", null, OnAutoLayoutClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "自动排列节点"
            };

            _toolStrip.Items.Add(btnValidate);
            _toolStrip.Items.Add(btnAutoLayout);

            // 新增：缩放控制按钮
            _toolStrip.Items.Add(new ToolStripSeparator());

            // 缩放显示标签
            var zoomLabelItem = new ToolStripLabel("缩放: 100%")
            {
                Name = "_zoomLabelItem",
                AutoSize = false,
                Width = 230,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White
            };
            _toolStrip.Items.Add(zoomLabelItem);

            var btnZoomIn = new ToolStripButton("放大", null, OnZoomInClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "放大 (Ctrl+加号)"
            };

            var btnZoomOut = new ToolStripButton("缩小", null, OnZoomOutClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "缩小 (Ctrl+减号)"
            };

            var btnZoomReset = new ToolStripButton("重置缩放", null, OnZoomResetClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 230,
                Height = 35,
                ToolTipText = "重置缩放 (Ctrl+0)"
            };

            _toolStrip.Items.Add(btnZoomIn);
            _toolStrip.Items.Add(btnZoomOut);
            _toolStrip.Items.Add(btnZoomReset);
        }

        #endregion

        #region 控件字段声明

        private SplitContainer _mainSplitContainer;
        private SplitContainer _rightSplitContainer;
        private STNodeEditor _nodeEditor;
        private STNodeTreeView _nodeTreeView;
        private STNodePropertyGrid _propertyGrid;
        private Panel _toolbarPanel;
        private Panel _leftPanel;
        private ToolStrip _toolStrip;

        #endregion
    }
}