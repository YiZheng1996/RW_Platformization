using System;
using System.Drawing;
using System.Windows.Forms;
using MainUI.LogicalConfiguration.NodeEditor.Core;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 节点属性面板控件
    /// </summary>
    public class NodePropertyPanel : UserControl
    {
        #region 事件

        /// <summary>
        /// 参数修改事件
        /// </summary>
        public event EventHandler ParameterChanged;

        /// <summary>
        /// 编辑参数请求事件
        /// </summary>
        public event EventHandler<WorkflowNodeBase> EditParameterRequested;

        #endregion

        #region 字段

        private WorkflowNodeBase _selectedNode;
        private Panel _headerPanel;
        private Label _titleLabel;
        private Label _nodeTypeLabel;
        private PropertyGrid _propertyGrid;
        private Panel _buttonPanel;
        private Button _editButton;
        private TextBox _remarkTextBox;
        private Label _remarkLabel;

        #endregion

        #region 属性

        /// <summary>
        /// 当前选中的节点
        /// </summary>
        public WorkflowNodeBase SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                UpdateDisplay();
            }
        }

        #endregion

        #region 构造函数

        public NodePropertyPanel()
        {
            InitializeComponent();
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(37, 37, 38);
            Padding = new Padding(0);

            // 标题面板
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            _titleLabel = new Label
            {
                Text = "节点属性",
                Location = new Point(10, 8),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };

            _nodeTypeLabel = new Label
            {
                Text = "未选中节点",
                Location = new Point(10, 32),
                AutoSize = true,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("微软雅黑", 9)
            };

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_nodeTypeLabel);

            // 备注区域
            var remarkPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10, 5, 10, 5)
            };

            _remarkLabel = new Label
            {
                Text = "备注：",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("微软雅黑", 9)
            };

            _remarkTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("微软雅黑", 9)
            };
            _remarkTextBox.TextChanged += RemarkTextBox_TextChanged;

            remarkPanel.Controls.Add(_remarkTextBox);
            remarkPanel.Controls.Add(_remarkLabel);

            // 属性网格
            _propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                ViewBackColor = Color.FromArgb(45, 45, 48),
                ViewForeColor = Color.White,
                LineColor = Color.FromArgb(60, 60, 65),
                CategoryForeColor = Color.FromArgb(200, 200, 200),
                CategorySplitterColor = Color.FromArgb(60, 60, 65),
                HelpVisible = true,
                HelpBackColor = Color.FromArgb(45, 45, 48),
                HelpForeColor = Color.FromArgb(180, 180, 180),
                ToolbarVisible = false,
                PropertySort = PropertySort.Categorized
            };
            _propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;

            // 按钮面板
            _buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            _editButton = new Button
            {
                Text = "编辑参数...",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Cursor = Cursors.Hand
            };
            _editButton.FlatAppearance.BorderSize = 0;
            _editButton.Click += EditButton_Click;

            _buttonPanel.Controls.Add(_editButton);

            // 添加控件
            Controls.Add(_propertyGrid);
            Controls.Add(remarkPanel);
            Controls.Add(_buttonPanel);
            Controls.Add(_headerPanel);
        }

        #endregion

        #region 显示更新

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (_selectedNode == null)
            {
                _nodeTypeLabel.Text = "未选中节点";
                _propertyGrid.SelectedObject = null;
                _remarkTextBox.Text = "";
                _remarkTextBox.Enabled = false;
                _editButton.Enabled = false;
                return;
            }

            // 更新标题
            _nodeTypeLabel.Text = $"{_selectedNode.DisplayName} ({_selectedNode.NodeType})";

            // 更新备注
            _remarkTextBox.Enabled = true;
            _remarkTextBox.Text = _selectedNode.Remark ?? "";

            // 更新属性网格
            _propertyGrid.SelectedObject = _selectedNode.Parameter;

            // 更新编辑按钮
            _editButton.Enabled = _selectedNode.Parameter != null;

            // 更新标题颜色
            _nodeTypeLabel.ForeColor = _selectedNode.NodeColor;
        }

        #endregion

        #region 事件处理

        private void RemarkTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                _selectedNode.Remark = _remarkTextBox.Text;
                ParameterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ParameterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                EditParameterRequested?.Invoke(this, _selectedNode);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshDisplay()
        {
            _propertyGrid.Refresh();
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            SelectedNode = null;
        }

        #endregion
    }
}
