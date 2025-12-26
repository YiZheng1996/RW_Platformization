using MainUI.LogicalConfiguration.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MainUI.LogicalConfiguration.LogicalManager
{
    /// <summary>
    /// 变量管理器
    /// 
    /// 职责：
    /// 1. 查询变量（获取、查找、检查存在性）
    /// 2. 修改变量（添加、更新、删除）
    /// 3. 持久化管理（加载、保存到JSON文件）
    /// 4. 变量分析（冲突检测、赋值追踪）
    /// 
    /// 数据流：
    /// ┌────────────────────────────────────────────────┐
    /// │         GlobalVariableManager (统一入口)              │
    /// │              ↓                                 │
    /// │    WorkflowStateService._variables            │  ← 唯一数据源
    /// └────────────────────────────────────────────────┘
    ///          ↕ (直接读写JSON)
    ///      JSON 文件 (持久化)
    /// </summary>
    public class GlobalVariableManager(
        IWorkflowStateService workflowState,
        ILogger<GlobalVariableManager> logger = null)
    {
        #region 私有字段

        private readonly IWorkflowStateService _workflowState = workflowState ?? throw new ArgumentNullException(nameof(workflowState));
        private bool _isInitialized;
        private bool _hasUnsavedChanges;

        // JSON文件路径（可以通过配置修改）
        private string _jsonFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Configurations",
            "Variables.json"
        );

        #endregion

        #region 属性

        /// <summary>
        /// 是否已初始化（已从文件加载）
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 是否有未保存的更改
        /// </summary>
        public bool HasUnsavedChanges => _hasUnsavedChanges;

        /// <summary>
        /// JSON配置文件路径
        /// </summary>
        public string JsonFilePath
        {
            get => _jsonFilePath;
            set => _jsonFilePath = value;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 变量变更事件
        /// </summary>
        public event EventHandler<VariableChangedEventArgs> VariableChanged;

        #endregion

        #region 1. 查询方法（只读操作）

        /// <summary>
        /// 获取所有变量（包括系统变量）
        /// </summary>
        public IReadOnlyList<VarItem_Enhanced> GetAllVariables()
        {
            return _workflowState.GetAllVariables();
        }

        /// <summary>
        /// 获取用户变量（排除系统变量）
        /// </summary>
        public IReadOnlyList<VarItem_Enhanced> GetUserVariables()
        {
            return _workflowState.GetAllVariables()
                .Where(v => !v.VarName.StartsWith("_sys_"))
                .ToList();
        }

        /// <summary>
        /// 按名称查找变量
        /// </summary>
        public VarItem_Enhanced FindVariable(string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
                return null;

            return _workflowState.FindVariable(varName);
        }

        /// <summary>
        /// 检查变量是否存在
        /// </summary>
        public bool VariableExists(string varName)
        {
            return FindVariable(varName) != null;
        }

        /// <summary>
        /// 按类型过滤变量
        /// </summary>
        public IReadOnlyList<VarItem_Enhanced> GetVariablesByType(string varType)
        {
            return GetAllVariables()
                .Where(v => v.VarType.Equals(varType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        #endregion

        #region 2. 修改方法（写入操作）

        /// <summary>
        /// 添加或更新变量
        /// </summary>
        /// <param name="variable">变量对象</param>
        /// <param name="saveImmediately">是否立即保存到文件（默认false）</param>
        public async Task AddOrUpdateAsync(VarItem_Enhanced variable, bool saveImmediately = false)
        {
            ArgumentNullException.ThrowIfNull(variable);

            var existing = FindVariable(variable.VarName);
            var isNew = existing == null;
            var oldValue = existing?.VarValue?.ToString();

            if (existing != null)
            {
                // 更新现有变量
                existing.VarType = variable.VarType;
                existing.VarValue = variable.VarValue;
                existing.VarText = variable.VarText;
                existing.LastUpdated = DateTime.Now;

                logger?.LogDebug("更新变量: {VarName}, 旧值: {OldValue}, 新值: {NewValue}",
                    variable.VarName, oldValue, variable.VarValue);
            }
            else
            {
                // 添加新变量
                variable.LastUpdated = DateTime.Now;
                _workflowState.AddVariable(variable);

                logger?.LogDebug("添加变量: {VarName}, 值: {Value}", variable.VarName, variable.VarValue);
            }

            _hasUnsavedChanges = true;

            // 可选立即保存
            if (saveImmediately)
            {
                await SaveToFileAsync();
            }

            // 触发事件
            OnVariableChanged(new VariableChangedEventArgs
            {
                ChangeType = isNew ? VariableChangeType.Added : VariableChangeType.Updated,
                Variable = variable,
                OldValue = oldValue,
                NewValue = variable.VarValue?.ToString()
            });
        }

        /// <summary>
        /// 批量添加或更新变量
        /// </summary>
        public async Task AddOrUpdateBatchAsync(IEnumerable<VarItem_Enhanced> variables, bool saveImmediately = false)
        {
            var varList = variables?.ToList();
            if (varList == null || varList.Count == 0)
                return;

            foreach (var variable in varList)
            {
                await AddOrUpdateAsync(variable, saveImmediately: false);
            }

            if (saveImmediately)
            {
                await SaveToFileAsync();
            }

            // 触发批量事件
            OnVariableChanged(new VariableChangedEventArgs
            {
                ChangeType = VariableChangeType.BatchUpdated,
                Variables = varList
            });
        }

        /// <summary>
        /// 更新变量值（仅更新值，不更新类型等其他属性）
        /// </summary>
        public async Task UpdateValueAsync(string varName, object value, string source = null)
        {
            var variable = FindVariable(varName) ?? throw new ArgumentException($"变量不存在: {varName}");
            var oldValue = variable.VarValue?.ToString();
            variable.VarValue = value;
            variable.LastUpdated = DateTime.Now;

            _hasUnsavedChanges = true;

            logger?.LogDebug("更新变量值: {VarName}, 旧值: {OldValue}, 新值: {NewValue}, 来源: {Source}",
                varName, oldValue, value, source ?? "未知");

            // 触发事件
            OnVariableChanged(new VariableChangedEventArgs
            {
                ChangeType = VariableChangeType.ValueChanged,
                Variable = variable,
                OldValue = oldValue,
                NewValue = value?.ToString(),
                Source = source
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除变量
        /// </summary>
        public async Task<bool> RemoveAsync(string varName, bool saveImmediately = false)
        {
            var variable = FindVariable(varName);
            if (variable == null)
            {
                logger?.LogWarning("尝试删除不存在的变量: {VarName}", varName);
                return false;
            }

            var result = _workflowState.RemoveVariable(varName);
            _hasUnsavedChanges = true;

            if (saveImmediately)
            {
                await SaveToFileAsync();
            }

            logger?.LogInformation("删除变量: {VarName}", varName);

            // 触发事件
            OnVariableChanged(new VariableChangedEventArgs
            {
                ChangeType = VariableChangeType.Removed,
                Variable = variable
            });
            return result;
        }

        /// <summary>
        /// 清空所有用户变量（保留系统变量）
        /// </summary>
        public async Task ClearUserVariablesAsync(bool saveImmediately = false)
        {
            _workflowState.ClearUserVariables();
            _hasUnsavedChanges = true;

            if (saveImmediately)
            {
                await SaveToFileAsync();
            }

            logger?.LogInformation("清空所有用户变量");

            // 触发事件
            OnVariableChanged(new VariableChangedEventArgs
            {
                ChangeType = VariableChangeType.Cleared
            });
        }

        #endregion

        #region 3. 持久化方法（直接读写JSON）

        /// <summary>
        /// 从JSON文件加载变量
        /// </summary>
        public async Task LoadFromFileAsync()
        {
            try
            {
                logger?.LogInformation("开始从文件加载变量: {FilePath}", _jsonFilePath);

                // 确保目录存在
                var directory = Path.GetDirectoryName(_jsonFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 如果文件不存在，创建空文件
                if (!File.Exists(_jsonFilePath))
                {
                    logger?.LogWarning("配置文件不存在，创建新文件: {FilePath}", _jsonFilePath);
                    await File.WriteAllTextAsync(_jsonFilePath, "[]");
                    _isInitialized = true;
                    return;
                }

                // 读取JSON文件
                var json = await File.ReadAllTextAsync(_jsonFilePath);
                var variables = JsonConvert.DeserializeObject<List<VarItem>>(json) ?? [];

                // 清空运行时用户变量
                _workflowState.ClearUserVariables();

                // 添加到运行时
                foreach (var varItem in variables)
                {
                    var enhanced = new VarItem_Enhanced
                    {
                        VarName = varItem.VarName,
                        VarValue = varItem.VarValue,
                        VarType = varItem.VarType,
                        VarText = varItem.VarText,
                        LastUpdated = DateTime.Now,
                        IsAssignedByStep = false,
                        AssignmentType = VariableAssignmentType.None,
                        AssignedByStepIndex = -1
                    };
                    _workflowState.AddVariable(enhanced);
                }

                _isInitialized = true;
                _hasUnsavedChanges = false;

                logger?.LogInformation("成功加载 {Count} 个变量", variables.Count);

                // 触发事件
                OnVariableChanged(new VariableChangedEventArgs
                {
                    ChangeType = VariableChangeType.BatchLoaded,
                    Variables = GetUserVariables()
                });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "从文件加载变量失败");
                throw new InvalidOperationException($"加载变量失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存变量到JSON文件
        /// </summary>
        public async Task SaveToFileAsync()
        {
            try
            {
                logger?.LogInformation("开始保存变量到文件: {FilePath}", _jsonFilePath);

                // 确保目录存在
                var directory = Path.GetDirectoryName(_jsonFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 获取用户变量
                var userVariables = GetUserVariables();

                // 转换为基础VarItem
                var varItems = userVariables.Select(v => new VarItem
                {
                    VarName = v.VarName,
                    VarValue = v.VarValue,
                    VarType = v.VarType,
                    VarText = v.VarText
                }).ToList();

                // 序列化为JSON
                var json = JsonConvert.SerializeObject(varItems, Formatting.Indented);

                // 写入文件
                await File.WriteAllTextAsync(_jsonFilePath, json);

                _hasUnsavedChanges = false;

                logger?.LogInformation("成功保存 {Count} 个变量", varItems.Count);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "保存变量到文件失败");
                throw new InvalidOperationException($"保存变量失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 4. 变量分析方法

        /// <summary>
        /// 获取所有已被赋值的变量信息
        /// </summary>
        public List<VariableAssignment> GetVariableAssignments()
        {
            return [.. GetAllVariables()
                .Where(v => v.IsAssignedByStep && !v.VarName.StartsWith("_sys_"))
                .Select(v => new VariableAssignment
                {
                    VariableName = v.VarName,
                    AssignmentDescription = v.AssignmentType switch
                    {
                        VariableAssignmentType.ExpressionCalculation => $"表达式赋值 ({v.VarText})",
                        VariableAssignmentType.PLCRead => $"PLC读取 ({v.VarText})",
                        //VariableAssignmentType.ExcelRead => $"Excel读取 ({v.VarText})",
                        //VariableAssignmentType.DatabaseRead => $"数据库读取 ({v.VarText})",
                        _ => "其他方式"
                    },
                    ExtraInfo = $"类型: {v.VarType}, 当前值: {v.VarValue}"
                })
                .OrderBy(a => a.VariableName)];
        }

        /// <summary>
        /// 检查变量赋值冲突
        /// </summary>
        public VariableConflictInfo CheckVariableConflict(
            string targetVarName,
            int currentStepIndex,
            VariableAssignmentType assignmentType)
        {
            var variable = FindVariable(targetVarName);
            if (variable == null || !variable.IsAssignedByStep)
            {
                return new VariableConflictInfo { HasConflict = false };
            }

            if (variable.AssignedByStepIndex >= 0 && variable.AssignedByStepIndex != currentStepIndex)
            {
                return new VariableConflictInfo
                {
                    HasConflict = true,
                    ConflictStepIndex = variable.AssignedByStepIndex,
                    ConflictStepInfo = $"步骤 {variable.AssignedByStepIndex + 1}",
                    ConflictAssignmentType = variable.AssignmentType
                };
            }

            return new VariableConflictInfo { HasConflict = false };
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 触发变量变更事件
        /// </summary>
        private void OnVariableChanged(VariableChangedEventArgs e)
        {
            try
            {
                VariableChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "触发变量变更事件时发生错误");
            }
        }

        #endregion
    }

    #region 辅助数据类

    /// <summary>
    /// 变量变更事件参数
    /// </summary>
    public class VariableChangedEventArgs : EventArgs
    {
        public VariableChangeType ChangeType { get; set; }
        public VarItem_Enhanced Variable { get; set; }
        public IReadOnlyList<VarItem_Enhanced> Variables { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Source { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 变量变更类型
    /// </summary>
    public enum VariableChangeType
    {
        Added,          // 新增
        Updated,        // 更新（结构属性更新）
        ValueChanged,   // 值变更（仅值变更）
        Removed,        // 删除
        BatchLoaded,    // 批量加载
        BatchUpdated,   // 批量更新
        Cleared         // 清空
    }

    /// <summary>
    /// 变量赋值信息
    /// </summary>
    public class VariableAssignment
    {
        public string VariableName { get; set; }
        public string AssignmentDescription { get; set; }
        public string ExtraInfo { get; set; }
    }

    /// <summary>
    /// 变量冲突信息
    /// </summary>
    public class VariableConflictInfo
    {
        public bool HasConflict { get; set; }

        public int ConflictStepIndex { get; set; } = -1;

        public string ConflictStepInfo { get; set; } = "";

        public VariableAssignmentType ConflictAssignmentType { get; set; } = VariableAssignmentType.None;

        public string ConflictDescription => HasConflict
            ? $"变量已在 {ConflictStepInfo} 被赋值（{ConflictAssignmentType}）"
            : "";
    }

    /// <summary>
    /// 当前步骤信息
    /// </summary>
    public class CurrentStepInfo
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 步骤索引
        /// </summary>
        public int StepIndex { get; set; }

        /// <summary>
        /// 步骤对象
        /// </summary>
        public ChildModel Step { get; set; }

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; }
    }

    #endregion
}