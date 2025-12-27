using System;
using System.Drawing;
using System.Windows.Forms;
using MainUI.LogicalConfiguration.NodeEditor.Services;

namespace MainUI.LogicalConfiguration.NodeEditor.Controls
{
    /// <summary>
    /// 节点工具箱控件
    /// </summary>
    public class NodeToolboxControl : UserControl
    {
        #region 事件

        /// <summary>
        /// 节点拖动开始事件
        /// </summary>
        public event EventHandler<string> NodeDragStart;

        #endregion

        #region 字段

        private TreeView _treeView;
        private Label _titleLabel;
        private Panel _headerPanel;

        #endregion

        #region 属性

        /// <summary>
        /// 工具箱标题
        /// </summary>
        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        #endregion

        #region 构造函数

        public NodeToolboxControl()
        {
            InitializeComponent();
            LoadNodeCategories();
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            // 设置控件属性
            BackColor = Color.FromArgb(37, 37, 38);
            Padding = new Padding(0);

            // 标题面板
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10, 0, 0, 0)
            };

            _titleLabel = new Label
            {
                Text = "节点工具箱",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _headerPanel.Controls.Add(_titleLabel);

            // 树形视图
            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                Font = new Font("微软雅黑", 9),
                ItemHeight = 26,
                ShowLines = false,
                ShowPlusMinus = true,
                ShowRootLines = false,
                FullRowSelect = true,
                HideSelection = false
            };

            _treeView.ItemDrag += TreeView_ItemDrag;
            _treeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;

            // 添加控件
            Controls.Add(_treeView);
            Controls.Add(_headerPanel);
        }

        /// <summary>
        /// 加载节点分类
        /// </summary>
        private void LoadNodeCategories()
        {
            _treeView.Nodes.Clear();

            var categories = NodeFactory.GetNodeCategories();

            foreach (var category in categories)
            {
                var categoryNode = new TreeNode(category.CategoryName)
                {
                    Tag = category.CategoryKey,
                    ForeColor = Color.FromArgb(200, 200, 200)
                };

                foreach (var nodeType in category.NodeTypes)
                {
                    var typeNode = new TreeNode($"  {nodeType.DisplayName}")
                    {
                        Tag = nodeType.TypeKey,
                        ToolTipText = nodeType.Description,
                        ForeColor = Color.FromArgb(220, 220, 220)
                    };

                    categoryNode.Nodes.Add(typeNode);
                }

                _treeView.Nodes.Add(categoryNode);
            }

            // 展开所有分类
            _treeView.ExpandAll();
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理拖动开始
        /// </summary>
        private void TreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && node.Tag is string nodeType)
            {
                // 不允许拖动分类节点
                if (node.Nodes.Count > 0) return;

                NodeDragStart?.Invoke(this, nodeType);
                DoDragDrop(nodeType, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// 双击节点（可以用于快速添加）
        /// </summary>
        private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is string nodeType && e.Node.Nodes.Count == 0)
            {
                NodeDragStart?.Invoke(this, nodeType);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 刷新工具箱
        /// </summary>
        public void RefreshToolbox()
        {
            LoadNodeCategories();
        }

        /// <summary>
        /// 设置节点可见性
        /// </summary>
        public void SetNodeVisibility(string nodeType, bool visible)
        {
            foreach (TreeNode categoryNode in _treeView.Nodes)
            {
                foreach (TreeNode typeNode in categoryNode.Nodes)
                {
                    if (typeNode.Tag as string == nodeType)
                    {
                        if (visible)
                            typeNode.ForeColor = Color.FromArgb(220, 220, 220);
                        else
                            typeNode.ForeColor = Color.FromArgb(80, 80, 80);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 过滤显示
        /// </summary>
        public void Filter(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadNodeCategories();
                return;
            }

            keyword = keyword.ToLower();
            _treeView.BeginUpdate();

            foreach (TreeNode categoryNode in _treeView.Nodes)
            {
                bool categoryVisible = false;

                foreach (TreeNode typeNode in categoryNode.Nodes)
                {
                    var nodeInfo = NodeFactory.GetNodeTypeInfo(typeNode.Tag as string);
                    bool matches = typeNode.Text.ToLower().Contains(keyword) ||
                                   (nodeInfo?.Description?.ToLower().Contains(keyword) ?? false);

                    typeNode.ForeColor = matches
                        ? Color.FromArgb(220, 220, 220)
                        : Color.FromArgb(60, 60, 60);

                    if (matches) categoryVisible = true;
                }

                categoryNode.ForeColor = categoryVisible
                    ? Color.FromArgb(200, 200, 200)
                    : Color.FromArgb(60, 60, 60);
            }

            _treeView.EndUpdate();
        }

        #endregion
    }
}
