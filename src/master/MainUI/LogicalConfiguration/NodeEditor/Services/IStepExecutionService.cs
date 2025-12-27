using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainUI.LogicalConfiguration.NodeEditor.Services
{
    /// <summary>
    /// 步骤执行服务接口
    /// </summary>
    public interface IStepExecutionService
    {
        /// <summary>
        /// 执行步骤
        /// </summary>
        Task<StepExecutionResult> ExecuteStepAsync(ChildModel step, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行工作流
        /// </summary>
        Task<WorkflowExecutionResult> ExecuteWorkflowAsync(List<ChildModel> steps, CancellationToken cancellationToken = default);

        /// <summary>
        /// 步骤执行状态变化事件
        /// </summary>
        event EventHandler<StepStatusChangedEventArgs> StepStatusChanged;
    }

    /// <summary>
    /// 步骤执行结果
    /// </summary>
    public class StepExecutionResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public object ResultData { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public static StepExecutionResult Successful(object data = null)
        {
            return new StepExecutionResult { Success = true, ResultData = data };
        }

        public static StepExecutionResult Failed(string message)
        {
            return new StepExecutionResult { Success = false, ErrorMessage = message };
        }
    }

    /// <summary>
    /// 工作流执行结果
    /// </summary>
    public class WorkflowExecutionResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int ExecutedSteps { get; set; }
        public int TotalSteps { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public List<StepExecutionResult> StepResults { get; set; } = new();
    }


    /// <summary>
    /// 步骤状态变化事件参数
    /// </summary>
    public class StepStatusChangedEventArgs : EventArgs
    {
        public int StepNum { get; set; }
        public string StepName { get; set; }
        public StepStatus Status { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 步骤状态
    /// </summary>
    public enum StepStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped
    }
}
