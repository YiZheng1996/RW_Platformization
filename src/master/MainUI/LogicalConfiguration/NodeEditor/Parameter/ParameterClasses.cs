/*
 * ========================================================================
 *     参数类定义（占位符）
 * ========================================================================
 * 
 * 如果你的项目中已经有这些参数类，请删除此文件或调整引用
 * 这里只是为了让配置面板代码能够编译通过
 * 
 */

namespace MainUI.LogicalConfiguration.Parameter
{
   
    /// <summary>
    /// PLC读写参数
    /// </summary>
    public class Parameter_PLCReadWrite
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; } = "Int16";

        /// <summary>
        /// 目标变量（读取时使用）
        /// </summary>
        public string TargetVariable { get; set; }

        /// <summary>
        /// 写入值（写入时使用）
        /// </summary>
        public string WriteValue { get; set; }

        /// <summary>
        /// 是否为读取操作
        /// </summary>
        public bool IsRead { get; set; } = true;
    }

   

    /// <summary>
    /// 等待稳定参数
    /// </summary>
    public class Parameter_WaitStable
    {
        /// <summary>
        /// 监控变量名
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// 波动阈值
        /// </summary>
        public double Threshold { get; set; } = 0.5;

        /// <summary>
        /// 稳定持续时间（毫秒）
        /// </summary>
        public int StableDuration { get; set; } = 2000;

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 30000;
    }

    /// <summary>
    /// 消息通知参数
    /// </summary>
    public class Parameter_MessageNotify
    {
        /// <summary>
        /// 消息类型（信息/警告/错误/成功）
        /// </summary>
        public string MessageType { get; set; } = "信息";

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否等待用户确认
        /// </summary>
        public bool WaitForConfirm { get; set; } = false;
    }
}
