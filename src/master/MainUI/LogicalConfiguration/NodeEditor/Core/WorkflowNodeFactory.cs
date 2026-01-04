using MainUI.LogicalConfiguration.NodeEditor.Nodes;
using System.Reflection;

namespace MainUI.LogicalConfiguration.NodeEditor.Core
{
    /// <summary>
    /// 节点工厂 - 负责创建和管理工作流节点
    /// </summary>
    public static class WorkflowNodeFactory
    {
        #region 私有字段

        /// <summary>
        /// StepName 到节点类型的映射
        /// </summary>
        private static readonly Dictionary<string, Type> _stepNameToNodeType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 所有节点类型列表
        /// </summary>
        private static readonly List<Type> _allNodeTypes = new List<Type>();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _initialized = false;

        #endregion

        #region 初始化

        /// <summary>
        /// 静态构造函数 - 注册所有节点类型
        /// </summary>
        static WorkflowNodeFactory()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化节点工厂
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            _stepNameToNodeType.Clear();
            _allNodeTypes.Clear();

            // 注册内置节点
            RegisterBuiltInNodes();

            // 扫描程序集中的节点类型
            ScanAssemblyForNodes();

            _initialized = true;
        }

        /// <summary>
        /// 注册内置节点
        /// </summary>
        private static void RegisterBuiltInNodes()
        {
            // 特殊节点
            RegisterNodeType<StartNode>("Start");
            RegisterNodeType<EndNode>("End");
            RegisterNodeType<CommentNode>("Comment");

            // 控制流节点
            RegisterNodeType<ConditionNode>("ConditionJudge");
            RegisterNodeType<LoopNode>("CycleBegins");
            RegisterNodeType<WaitForStableNode>("Waitingforstability");

            // 操作节点
            RegisterNodeType<DelayNode>("DelayWait");
            RegisterNodeType<VariableAssignNode>("VariableAssign");
            RegisterNodeType<ReadPLCNode>("PLCRead");
            RegisterNodeType<WritePLCNode>("PLCWrite");
            RegisterNodeType<MessageNotifyNode>("MessageNotify");
            RegisterNodeType<RealtimeMonitorNode>("MonitorTool");
        }

        /// <summary>
        /// 扫描程序集中的节点类型
        /// </summary>
        private static void ScanAssemblyForNodes()
        {
            try
            {
                // 获取当前程序集
                var assembly = Assembly.GetExecutingAssembly();

                // 查找所有继承自 WorkflowNodeBase 的类型
                var nodeTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(WorkflowNodeBase).IsAssignableFrom(t));

                foreach (var nodeType in nodeTypes)
                {
                    if (!_allNodeTypes.Contains(nodeType))
                    {
                        _allNodeTypes.Add(nodeType);
                    }

                    // 尝试获取 StepName
                    try
                    {
                        var instance = (WorkflowNodeBase)Activator.CreateInstance(nodeType);
                        string stepName = instance.StepName;

                        if (!string.IsNullOrEmpty(stepName) && !_stepNameToNodeType.ContainsKey(stepName))
                        {
                            _stepNameToNodeType[stepName] = nodeType;
                        }
                    }
                    catch
                    {
                        // 忽略无法实例化的类型
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描节点类型失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册节点类型
        /// </summary>
        private static void RegisterNodeType<T>(string stepName) where T : WorkflowNodeBase
        {
            var type = typeof(T);

            if (!_allNodeTypes.Contains(type))
            {
                _allNodeTypes.Add(type);
            }

            if (!_stepNameToNodeType.ContainsKey(stepName))
            {
                _stepNameToNodeType[stepName] = type;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 根据 StepName 创建节点
        /// </summary>
        public static WorkflowNodeBase CreateNode(string stepName)
        {
            if (string.IsNullOrEmpty(stepName))
                return null;

            if (_stepNameToNodeType.TryGetValue(stepName, out Type nodeType))
            {
                try
                {
                    return (WorkflowNodeBase)Activator.CreateInstance(nodeType);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"创建节点失败 [{stepName}]: {ex.Message}");
                }
            }

            // 如果找不到对应类型，创建通用节点
            return CreateGenericNode(stepName);
        }

        /// <summary>
        /// 创建通用节点 (用于未知类型)
        /// </summary>
        private static WorkflowNodeBase CreateGenericNode(string stepName)
        {
            return new GenericNode(stepName);
        }

        /// <summary>
        /// 从 ChildModel 创建节点
        /// </summary>
        public static WorkflowNodeBase CreateNodeFromChildModel(ChildModel model)
        {
            if (model == null)
                return null;

            var node = CreateNode(model.StepName);
            if (node != null)
            {
                node.LoadFromChildModel(model);
            }

            return node;
        }

        /// <summary>
        /// 获取所有已注册的节点类型
        /// </summary>
        public static IReadOnlyList<Type> GetAllNodeTypes()
        {
            return _allNodeTypes.AsReadOnly();
        }

        /// <summary>
        /// 获取所有已注册的 StepName
        /// </summary>
        public static IEnumerable<string> GetAllStepNames()
        {
            return _stepNameToNodeType.Keys;
        }

        /// <summary>
        /// 检查 StepName 是否已注册
        /// </summary>
        public static bool IsStepNameRegistered(string stepName)
        {
            return !string.IsNullOrEmpty(stepName) && _stepNameToNodeType.ContainsKey(stepName);
        }

        /// <summary>
        /// 获取节点分类信息 (用于TreeView)
        /// </summary>
        public static Dictionary<string, List<NodeInfo>> GetNodesByCategory()
        {
            var result = new Dictionary<string, List<NodeInfo>>();

            foreach (var nodeType in _allNodeTypes)
            {
                try
                {
                    var instance = (WorkflowNodeBase)Activator.CreateInstance(nodeType);
                    string category = instance.CategoryPath ?? "其他";

                    if (!result.TryGetValue(category, out List<NodeInfo> value))
                    {
                        value = [];
                        result[category] = value;
                    }

                    value.Add(new NodeInfo
                    {
                        StepName = instance.StepName,
                        DisplayName = instance.DisplayName,
                        Description = instance.Description,
                        NodeType = nodeType
                    });
                }
                catch
                {
                    // 忽略
                }
            }

            return result;
        }

        /// <summary>
        /// 手动注册节点类型
        /// </summary>
        public static void RegisterNode<T>() where T : WorkflowNodeBase, new()
        {
            var type = typeof(T);
            if (!_allNodeTypes.Contains(type))
            {
                _allNodeTypes.Add(type);
            }

            try
            {
                var instance = new T();
                if (!string.IsNullOrEmpty(instance.StepName))
                {
                    _stepNameToNodeType[instance.StepName] = type;
                }
            }
            catch { }
        }

        #endregion
    }

    #region 辅助类型

    /// <summary>
    /// 节点信息
    /// </summary>
    public class NodeInfo
    {
        public string StepName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Type NodeType { get; set; }
    }

    /// <summary>
    /// 通用节点 - 用于未知类型
    /// </summary>
    public class GenericNode(string stepName) : WorkflowNodeBase
    {
        private readonly string _stepName = stepName ?? "Unknown";

        public override string StepName => _stepName;
        public override string DisplayName => _stepName;
        public override string CategoryPath => "其他";
        public override string Description => $"未知节点类型: {_stepName}";

        // 无参构造函数（用于反序列化）
        public GenericNode() : this("Unknown") { }

        protected override void OnCreate()
        {
            base.OnCreate();
            Title = $"❓ {_stepName}";
            TitleColor = Color.FromArgb(200, 128, 128, 128);
        }
    }

    #endregion
}
