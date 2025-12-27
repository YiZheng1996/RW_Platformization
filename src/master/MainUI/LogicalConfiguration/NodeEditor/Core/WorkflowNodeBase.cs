using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 节点执行状态
    /// </summary>
    public enum NodeExecutionStatus
    {
        /// <summary>
        /// 待执行
        /// </summary>
        Pending,

        /// <summary>
        /// 执行中
        /// </summary>
        Running,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已跳过
        /// </summary>
        Skipped
    }

    /// <summary>
    /// 工作流节点基类
    /// </summary>
    [Serializable]
    public abstract class WorkflowNodeBase
    {
        #region 常量

        /// <summary>
        /// 默认节点宽度
        /// </summary>
        protected const int DefaultWidth = 160;

        /// <summary>
        /// 默认节点高度
        /// </summary>
        protected const int DefaultHeight = 60;

        /// <summary>
        /// 标题栏高度
        /// </summary>
        protected const int TitleHeight = 24;

        /// <summary>
        /// 端口间距
        /// </summary>
        protected const int SocketSpacing = 20;

        /// <summary>
        /// 端口边距
        /// </summary>
        protected const int SocketMargin = 12;

        #endregion

        #region 核心属性

        /// <summary>
        /// 节点唯一标识
        /// </summary>
        public Guid NodeId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 节点类型标识（对应 StepName）
        /// </summary>
        public abstract string NodeType { get; }

        /// <summary>
        /// 节点显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 节点备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 步骤参数
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public NodeExecutionStatus ExecutionStatus { get; set; } = NodeExecutionStatus.Pending;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; }

        #endregion

        #region 位置和尺寸

        /// <summary>
        /// 节点位置
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// 节点宽度
        /// </summary>
        public int Width { get; set; } = DefaultWidth;

        /// <summary>
        /// 节点高度
        /// </summary>
        public int Height { get; set; } = DefaultHeight;

        /// <summary>
        /// 获取节点边界
        /// </summary>
        public Rectangle Bounds => new Rectangle(Location, new Size(Width, Height));

        #endregion

        #region 端口定义

        /// <summary>
        /// 输入端口集合
        /// </summary>
        public List<NodeSocket> InputSockets { get; } = new();

        /// <summary>
        /// 输出端口集合
        /// </summary>
        public List<NodeSocket> OutputSockets { get; } = new();

        /// <summary>
        /// 获取所有端口
        /// </summary>
        public IEnumerable<NodeSocket> AllSockets => InputSockets.Concat(OutputSockets);

        #endregion

        #region 选择状态

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否高亮（鼠标悬停）
        /// </summary>
        public bool IsHighlighted { get; set; }

        #endregion

        #region 视觉属性（子类实现）

        /// <summary>
        /// 节点背景色
        /// </summary>
        public abstract Color NodeColor { get; }

        /// <summary>
        /// 节点图标名称
        /// </summary>
        public abstract string IconName { get; }

        /// <summary>
        /// 获取参数预览文本
        /// </summary>
        public abstract string GetParameterPreview();

        #endregion

        #region 构造函数

        protected WorkflowNodeBase()
        {
            // 更新所有端口的所属节点ID
            foreach (var socket in AllSockets)
            {
                socket.OwnerNodeId = NodeId;
            }
        }

        #endregion

        #region 端口位置计算

        /// <summary>
        /// 更新所有端口的位置
        /// </summary>
        public void UpdateSocketPositions()
        {
            // 计算输入端口位置（左侧）
            int inputY = TitleHeight + SocketMargin;
            foreach (var socket in InputSockets.Where(s => !s.IsInternal))
            {
                socket.Position = new Point(Location.X, Location.Y + inputY);
                socket.OwnerNodeId = NodeId;
                inputY += SocketSpacing;
            }

            // 计算输出端口位置（右侧）
            int outputY = TitleHeight + SocketMargin;
            foreach (var socket in OutputSockets.Where(s => !s.IsInternal))
            {
                socket.Position = new Point(Location.X + Width, Location.Y + outputY);
                socket.OwnerNodeId = NodeId;
                outputY += SocketSpacing;
            }
        }

        /// <summary>
        /// 根据名称获取端口
        /// </summary>
        public NodeSocket GetSocket(string name)
        {
            return AllSockets.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// 根据ID获取端口
        /// </summary>
        public NodeSocket GetSocketById(Guid id)
        {
            return AllSockets.FirstOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// 获取指定位置的端口
        /// </summary>
        public NodeSocket GetSocketAt(Point point)
        {
            return AllSockets.FirstOrDefault(s => s.ContainsPoint(point));
        }

        #endregion

        #region 绘制方法

        /// <summary>
        /// 绘制节点
        /// </summary>
        public virtual void Draw(Graphics g)
        {
            if (g == null) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 更新端口位置
            UpdateSocketPositions();

            // 绘制阴影
            DrawShadow(g);

            // 绘制节点主体
            DrawBody(g);

            // 绘制标题栏
            DrawTitle(g);

            // 绘制内容
            DrawContent(g);

            // 绘制端口
            DrawSockets(g);

            // 绘制状态指示器
            DrawStatusIndicator(g);

            // 如果被选中，绘制选中边框
            if (IsSelected)
            {
                DrawSelectionBorder(g);
            }

            // 如果被禁用，绘制禁用遮罩
            if (IsDisabled)
            {
                DrawDisabledOverlay(g);
            }
        }

        /// <summary>
        /// 绘制阴影
        /// </summary>
        protected virtual void DrawShadow(Graphics g)
        {
            var shadowOffset = 4;
            var shadowRect = new Rectangle(
                Location.X + shadowOffset,
                Location.Y + shadowOffset,
                Width,
                Height);

            using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
            g.FillRoundedRectangle(shadowBrush, shadowRect, 8);
        }

        /// <summary>
        /// 绘制节点主体
        /// </summary>
        protected virtual void DrawBody(Graphics g)
        {
            var bodyRect = new RectangleF(Location.X, Location.Y, Width, Height);
            var bodyColor = Color.FromArgb(50, 50, 55);

            if (IsHighlighted)
            {
                bodyColor = Color.FromArgb(60, 60, 65);
            }

            using var bodyBrush = new SolidBrush(bodyColor);
            g.FillRoundedRectangle(bodyBrush, bodyRect, 8);

            // 绘制边框
            var borderColor = Color.FromArgb(80, 80, 85);
            if (IsHighlighted)
            {
                borderColor = Color.FromArgb(100, 100, 105);
            }

            using var borderPen = new Pen(borderColor, 1);
            g.DrawRoundedRectangle(borderPen, bodyRect, 8);
        }

        /// <summary>
        /// 绘制标题栏
        /// </summary>
        protected virtual void DrawTitle(Graphics g)
        {
            var titleRect = new RectangleF(Location.X, Location.Y, Width, TitleHeight);

            // 绘制标题栏背景（带圆角的上半部分）
            using var path = new GraphicsPath();
            float radius = 8;
            path.AddArc(titleRect.X, titleRect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(titleRect.Right - radius * 2, titleRect.Y, radius * 2, radius * 2, 270, 90);
            path.AddLine(titleRect.Right, titleRect.Y + radius, titleRect.Right, titleRect.Bottom);
            path.AddLine(titleRect.Right, titleRect.Bottom, titleRect.X, titleRect.Bottom);
            path.AddLine(titleRect.X, titleRect.Bottom, titleRect.X, titleRect.Y + radius);
            path.CloseFigure();

            using var titleBrush = new SolidBrush(NodeColor);
            g.FillPath(titleBrush, path);

            // 绘制标题文字
            using var titleFont = new Font("微软雅黑", 9, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);

            var titleText = DisplayName ?? NodeType;
            var textSize = g.MeasureString(titleText, titleFont);
            var textX = Location.X + (Width - textSize.Width) / 2;
            var textY = Location.Y + (TitleHeight - textSize.Height) / 2;

            g.DrawString(titleText, titleFont, textBrush, textX, textY);
        }

        /// <summary>
        /// 绘制内容区域
        /// </summary>
        protected virtual void DrawContent(Graphics g)
        {
            var preview = GetParameterPreview();
            if (string.IsNullOrEmpty(preview)) return;

            using var font = new Font("微软雅黑", 8);
            using var brush = new SolidBrush(Color.FromArgb(180, 180, 180));

            var contentRect = new RectangleF(
                Location.X + 8,
                Location.Y + TitleHeight + 4,
                Width - 16,
                Height - TitleHeight - 8);

            var format = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.LineLimit
            };

            g.DrawString(preview, font, brush, contentRect, format);
        }

        /// <summary>
        /// 绘制端口
        /// </summary>
        protected virtual void DrawSockets(Graphics g)
        {
            foreach (var socket in AllSockets.Where(s => !s.IsInternal))
            {
                DrawSocket(g, socket);
            }
        }

        /// <summary>
        /// 绘制单个端口
        /// </summary>
        protected virtual void DrawSocket(Graphics g, NodeSocket socket)
        {
            var bounds = socket.GetBounds();
            var color = socket.Color;

            // 绘制端口圆形
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, bounds);

            // 绘制边框
            using var pen = new Pen(Color.FromArgb(100, 100, 100), 1);
            g.DrawEllipse(pen, bounds);

            // 绘制标签
            if (!string.IsNullOrEmpty(socket.Label))
            {
                using var font = new Font("微软雅黑", 7);
                using var textBrush = new SolidBrush(Color.FromArgb(150, 150, 150));

                float labelX, labelY;
                if (socket.Type == SocketType.Input)
                {
                    labelX = bounds.Right + 4;
                    labelY = bounds.Y + bounds.Height / 2 - 6;
                }
                else
                {
                    var size = g.MeasureString(socket.Label, font);
                    labelX = bounds.X - size.Width - 4;
                    labelY = bounds.Y + bounds.Height / 2 - 6;
                }

                g.DrawString(socket.Label, font, textBrush, labelX, labelY);
            }
        }

        /// <summary>
        /// 绘制状态指示器
        /// </summary>
        protected virtual void DrawStatusIndicator(Graphics g)
        {
            Color statusColor = ExecutionStatus switch
            {
                NodeExecutionStatus.Running => Color.FromArgb(255, 193, 7),    // 黄色
                NodeExecutionStatus.Completed => Color.FromArgb(40, 167, 69),  // 绿色
                NodeExecutionStatus.Failed => Color.FromArgb(220, 53, 69),     // 红色
                NodeExecutionStatus.Skipped => Color.FromArgb(108, 117, 125),  // 灰色
                _ => Color.Transparent
            };

            if (statusColor == Color.Transparent) return;

            // 在右上角绘制状态点
            var indicatorSize = 10;
            var indicatorRect = new Rectangle(
                Location.X + Width - indicatorSize - 4,
                Location.Y + 4,
                indicatorSize,
                indicatorSize);

            using var brush = new SolidBrush(statusColor);
            g.FillEllipse(brush, indicatorRect);

            // 运行中状态添加动画效果（闪烁边框）
            if (ExecutionStatus == NodeExecutionStatus.Running)
            {
                using var pen = new Pen(Color.FromArgb(255, 255, 200), 2);
                g.DrawEllipse(pen, indicatorRect);
            }
        }

        /// <summary>
        /// 绘制选中边框
        /// </summary>
        protected virtual void DrawSelectionBorder(Graphics g)
        {
            var borderRect = new RectangleF(
                Location.X - 2,
                Location.Y - 2,
                Width + 4,
                Height + 4);

            using var pen = new Pen(Color.FromArgb(0, 122, 204), 2);
            g.DrawRoundedRectangle(pen, borderRect, 10);
        }

        /// <summary>
        /// 绘制禁用遮罩
        /// </summary>
        protected virtual void DrawDisabledOverlay(Graphics g)
        {
            var overlayRect = new RectangleF(Location.X, Location.Y, Width, Height);

            using var brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            g.FillRoundedRectangle(brush, overlayRect, 8);

            // 绘制禁用图标
            using var font = new Font("Segoe UI Symbol", 16);
            using var textBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
            var text = "⊘";
            var textSize = g.MeasureString(text, font);
            g.DrawString(text, font, textBrush,
                Location.X + (Width - textSize.Width) / 2,
                Location.Y + (Height - textSize.Height) / 2);
        }

        #endregion

        #region 交互方法

        /// <summary>
        /// 检查点是否在节点内
        /// </summary>
        public bool ContainsPoint(Point point)
        {
            return Bounds.Contains(point);
        }

        /// <summary>
        /// 检查点是否在标题栏内（用于拖动）
        /// </summary>
        public bool IsInTitleBar(Point point)
        {
            var titleRect = new Rectangle(Location.X, Location.Y, Width, TitleHeight);
            return titleRect.Contains(point);
        }

        /// <summary>
        /// 移动节点
        /// </summary>
        public void Move(int deltaX, int deltaY)
        {
            Location = new Point(Location.X + deltaX, Location.Y + deltaY);
            UpdateSocketPositions();
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetLocation(Point location)
        {
            Location = location;
            UpdateSocketPositions();
        }

        #endregion

        #region 复制

        /// <summary>
        /// 克隆节点（不包含连接）
        /// </summary>
        public abstract WorkflowNodeBase Clone();

        /// <summary>
        /// 复制基础属性
        /// </summary>
        protected void CopyBasicProperties(WorkflowNodeBase target)
        {
            target.DisplayName = DisplayName;
            target.Remark = Remark;
            target.Location = new Point(Location.X + 20, Location.Y + 20);
            target.Width = Width;
            target.Height = Height;
            target.IsDisabled = IsDisabled;
            // 注意：不复制 NodeId，新节点需要新的ID
        }

        #endregion
    }
}
