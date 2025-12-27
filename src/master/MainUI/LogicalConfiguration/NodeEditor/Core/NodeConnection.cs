using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 节点之间的连接线
    /// </summary>
    [Serializable]
    public class NodeConnection
    {
        #region 属性

        /// <summary>
        /// 连接唯一标识
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 源节点ID
        /// </summary>
        public Guid SourceNodeId { get; set; }

        /// <summary>
        /// 源端口ID
        /// </summary>
        public Guid SourceSocketId { get; set; }

        /// <summary>
        /// 目标节点ID
        /// </summary>
        public Guid TargetNodeId { get; set; }

        /// <summary>
        /// 目标端口ID
        /// </summary>
        public Guid TargetSocketId { get; set; }

        /// <summary>
        /// 连接线颜色
        /// </summary>
        public Color LineColor { get; set; } = Color.FromArgb(200, 200, 200);

        /// <summary>
        /// 连接线宽度
        /// </summary>
        public float LineWidth { get; set; } = 2f;

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否高亮（鼠标悬停）
        /// </summary>
        public bool IsHighlighted { get; set; }

        /// <summary>
        /// 连接标签（可选）
        /// </summary>
        public string Label { get; set; }

        #endregion

        #region 运行时属性（不序列化）

        /// <summary>
        /// 起始点（运行时计算）
        /// </summary>
        [NonSerialized]
        private Point _startPoint;
        public Point StartPoint
        {
            get => _startPoint;
            set => _startPoint = value;
        }

        /// <summary>
        /// 终止点（运行时计算）
        /// </summary>
        [NonSerialized]
        private Point _endPoint;
        public Point EndPoint
        {
            get => _endPoint;
            set => _endPoint = value;
        }

        #endregion

        #region 绘制方法

        /// <summary>
        /// 绘制连接线
        /// </summary>
        public void Draw(Graphics g)
        {
            if (g == null) return;

            using var pen = CreatePen();
            var path = CreateBezierPath();
            g.DrawPath(pen, path);

            // 绘制箭头
            DrawArrow(g, pen);

            // 如果有标签，绘制标签
            if (!string.IsNullOrEmpty(Label))
            {
                DrawLabel(g);
            }
        }

        /// <summary>
        /// 创建画笔
        /// </summary>
        private Pen CreatePen()
        {
            var color = LineColor;
            var width = LineWidth;

            if (IsSelected)
            {
                color = Color.FromArgb(0, 122, 204);  // 选中时蓝色
                width = 3f;
            }
            else if (IsHighlighted)
            {
                color = Color.FromArgb(255, 255, 100);  // 悬停时黄色
                width = 2.5f;
            }

            return new Pen(color, width)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
        }

        /// <summary>
        /// 创建贝塞尔曲线路径
        /// </summary>
        private GraphicsPath CreateBezierPath()
        {
            var path = new GraphicsPath();

            // 计算控制点（使曲线更平滑）
            int dx = Math.Abs(EndPoint.X - StartPoint.X);
            int offset = Math.Max(50, dx / 2);

            var cp1 = new Point(StartPoint.X + offset, StartPoint.Y);
            var cp2 = new Point(EndPoint.X - offset, EndPoint.Y);

            path.AddBezier(StartPoint, cp1, cp2, EndPoint);

            return path;
        }

        /// <summary>
        /// 绘制箭头
        /// </summary>
        private void DrawArrow(Graphics g, Pen pen)
        {
            // 计算箭头位置（在终点附近）
            var arrowSize = 8;
            var angle = Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);

            var arrowPoint1 = new PointF(
                EndPoint.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                EndPoint.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6));

            var arrowPoint2 = new PointF(
                EndPoint.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                EndPoint.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6));

            using var brush = new SolidBrush(pen.Color);
            var arrowPoints = new PointF[] { EndPoint, arrowPoint1, arrowPoint2 };
            g.FillPolygon(brush, arrowPoints);
        }

        /// <summary>
        /// 绘制标签
        /// </summary>
        private void DrawLabel(Graphics g)
        {
            var midPoint = new Point(
                (StartPoint.X + EndPoint.X) / 2,
                (StartPoint.Y + EndPoint.Y) / 2);

            using var font = new Font("微软雅黑", 8);
            using var brush = new SolidBrush(Color.White);
            using var bgBrush = new SolidBrush(Color.FromArgb(180, 60, 60, 60));

            var size = g.MeasureString(Label, font);
            var rect = new RectangleF(
                midPoint.X - size.Width / 2 - 4,
                midPoint.Y - size.Height / 2 - 2,
                size.Width + 8,
                size.Height + 4);

            g.FillRoundedRectangle(bgBrush, rect, 4);
            g.DrawString(Label, font, brush, midPoint.X - size.Width / 2, midPoint.Y - size.Height / 2);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 检查点是否在连接线上（用于选择）
        /// </summary>
        public bool HitTest(Point point, float tolerance = 5f)
        {
            using var path = CreateBezierPath();
            using var pen = new Pen(Color.Black, tolerance * 2);
            return path.IsOutlineVisible(point, pen);
        }

        /// <summary>
        /// 获取连接线的边界矩形
        /// </summary>
        public Rectangle GetBounds()
        {
            int minX = Math.Min(StartPoint.X, EndPoint.X) - 50;
            int minY = Math.Min(StartPoint.Y, EndPoint.Y) - 10;
            int maxX = Math.Max(StartPoint.X, EndPoint.X) + 50;
            int maxY = Math.Max(StartPoint.Y, EndPoint.Y) + 10;

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        #endregion
    }

    /// <summary>
    /// Graphics 扩展方法
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// 绘制圆角矩形
        /// </summary>
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using var path = CreateRoundedRectangle(rect, radius);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// 绘制圆角矩形边框
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using var path = CreateRoundedRectangle(rect, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
