using System;
using System.Drawing;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 端口类型枚举
    /// </summary>
    public enum SocketType
    {
        /// <summary>
        /// 输入端口
        /// </summary>
        Input,

        /// <summary>
        /// 输出端口
        /// </summary>
        Output
    }

    /// <summary>
    /// 节点端口（连接点）
    /// </summary>
    [Serializable]
    public class NodeSocket
    {
        #region 属性

        /// <summary>
        /// 端口唯一标识
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 端口名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 端口类型
        /// </summary>
        public SocketType Type { get; set; }

        /// <summary>
        /// 端口颜色
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// 端口标签（显示在端口旁边）
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 是否为内部端口（如循环体内部连接点）
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// 端口位置（相对于节点）
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// 端口半径
        /// </summary>
        public int Radius { get; set; } = 6;

        /// <summary>
        /// 是否允许多个连接
        /// </summary>
        public bool AllowMultipleConnections { get; set; }

        /// <summary>
        /// 所属节点ID
        /// </summary>
        public Guid OwnerNodeId { get; set; }

        #endregion

        #region 构造函数

        public NodeSocket()
        {
        }

        public NodeSocket(string name, SocketType type)
        {
            Name = name;
            Type = type;
            // 输入端口默认不允许多连接，输出端口允许
            AllowMultipleConnections = type == SocketType.Output;
        }

        public NodeSocket(string name, SocketType type, Color color) : this(name, type)
        {
            Color = color;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取端口的绘制区域
        /// </summary>
        public Rectangle GetBounds()
        {
            return new Rectangle(
                Position.X - Radius,
                Position.Y - Radius,
                Radius * 2,
                Radius * 2);
        }

        /// <summary>
        /// 检查点是否在端口区域内
        /// </summary>
        public bool ContainsPoint(Point point)
        {
            var bounds = GetBounds();
            // 扩大点击区域以便于操作
            bounds.Inflate(4, 4);
            return bounds.Contains(point);
        }

        /// <summary>
        /// 获取连接线的起/终点
        /// </summary>
        public Point GetConnectionPoint()
        {
            return Position;
        }

        #endregion
    }
}
