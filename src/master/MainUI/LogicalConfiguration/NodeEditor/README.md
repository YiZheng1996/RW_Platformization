# STNodeEditor 工作流设计器

基于 **STNodeEditor** 的可视化工作流设计器，专为 WinForms 工业控制应用设计。

## 📦 目录结构

```
STNodeWorkflow/
├── Core/                               # 核心功能
│   ├── WorkflowNodeFactory.cs          # 节点工厂 - 创建和管理节点
│   ├── WorkflowGraphConverter.cs       # 图转换器 - 节点图 ↔ ChildModel
│   ├── NodeConfigAdapter.cs            # 配置适配器 - 节点配置窗体
│   └── WorkflowExecutionVisualizer.cs  # 执行可视化 - 实时状态显示
│
├── Nodes/                              # 节点定义
│   ├── WorkflowNodeBase.cs             # 节点基类
│   ├── SpecialNodes.cs                 # 特殊节点 (开始/结束/注释)
│   ├── ControlFlowNodes.cs             # 控制流节点 (条件/循环/等待)
│   └── OperationNodes.cs               # 操作节点 (延时/PLC/变量)
│
├── Controls/
│   └── WorkflowDesignerPanel.cs        # 设计器面板控件
│
├── Forms/
│   └── WorkflowDesignerForm.cs         # 设计器窗体
│
├── IntegrationExample.cs               # 集成示例代码
└── README.md                           # 本文档
```

## 🚀 快速开始

### 1. 安装 NuGet 包

```bash
# Package Manager Console
Install-Package ST.Library.UI

# 或 .NET CLI
dotnet add package ST.Library.UI
```

### 2. 添加代码文件

将 `STNodeWorkflow` 文件夹复制到你的项目中，调整命名空间。

### 3. 打开设计器

```csharp
using MainUI.LogicalConfiguration.NodeEditor.Forms;

// 方式1: 打开独立窗口
var designerForm = new WorkflowDesignerForm();
designerForm.Show();

// 方式2: 带初始数据
var steps = workflowStateService.GetSteps();
var designerForm = new WorkflowDesignerForm(steps, (result) =>
{
    // 保存回调
    workflowStateService.ClearSteps();
    foreach (var step in result)
    {
        workflowStateService.AddStep(step);
    }
});
designerForm.Show();

// 方式3: 对话框模式
var result = new WorkflowDesignerForm(steps).ShowDialogAndGetResult();
if (result != null)
{
    // 用户点击确定，处理结果
}
```

## 🎨 节点类型

### 特殊节点
| 节点 | StepName | 说明 |
|------|----------|------|
| 🚀 开始 | `Start` | 工作流起点 |
| ✅ 结束 | `End` | 工作流终点 |
| 📝 注释 | `Comment` | 添加说明文字 |

### 控制流节点
| 节点 | StepName | 说明 |
|------|----------|------|
| ❓ 条件判断 | `ConditionJudge` | 根据条件分支 |
| 🔄 循环 | `CycleBegins` | 循环执行步骤 |
| ⏳ 等待稳定 | `Waitingforstability` | 等待值稳定 |

### 操作节点
| 节点 | StepName | 说明 |
|------|----------|------|
| ⏱️ 延时等待 | `DelayWait` | 暂停执行 |
| 📝 变量赋值 | `VariableAssign` | 设置变量值 |
| 📥 读取PLC | `PLCRead` | 从PLC读取 |
| 📤 写入PLC | `PLCWrite` | 向PLC写入 |
| 💬 消息通知 | `MessageNotify` | 发送消息 |
| 📺 实时监控 | `MonitorTool` | 显示监控窗口 |

## 🔧 与现有项目集成

### 1. 处理参数类冲突

如果项目已有 `Parameter_XXX` 类，删除 `OperationNodes.cs` 底部的参数类定义，添加引用：

```csharp
using MainUI.LogicalConfiguration.Parameter;
```

### 2. 注册现有配置窗体

```csharp
// 在程序启动时注册
NodeConfigAdapter.Instance.RegisterFormFactory("DelayWait", node =>
{
    var form = new Form_DelayTime();
    form.Parameter = node.StepParameter as Parameter_DelayTime 
                     ?? new Parameter_DelayTime();
    return form;
});

NodeConfigAdapter.Instance.RegisterFormFactory("ConditionJudge", node =>
{
    var form = new Form_Detection();
    form.Parameter = node.StepParameter as Parameter_Detection 
                     ?? new Parameter_Detection();
    return form;
});

// ... 其他节点
```

### 3. 嵌入到现有窗体

```csharp
public partial class frmLogicalConfiguration : Form
{
    private WorkflowDesignerPanel _nodeDesigner;

    private void InitializeNodeDesigner()
    {
        _nodeDesigner = new WorkflowDesignerPanel
        {
            Dock = DockStyle.Fill
        };

        // 添加到容器面板
        panelDesigner.Controls.Add(_nodeDesigner);

        // 加载现有步骤
        var steps = _workflowStateService.GetSteps();
        _nodeDesigner.LoadFromChildModels(steps);

        // 监听变化
        _nodeDesigner.WorkflowChanged += (s, e) => SyncToWorkflowState();

        // 双击节点打开配置
        _nodeDesigner.NodeDoubleClick += (s, e) =>
        {
            var result = NodeConfigAdapter.Instance.OpenConfigForm(e.Node, this);
            if (result.Success)
            {
                e.Node.RefreshDisplay();
            }
            e.Handled = true;
        };
    }

    private void SyncToWorkflowState()
    {
        var steps = _nodeDesigner.ExportToChildModels();
        _workflowStateService.ClearSteps();
        foreach (var step in steps)
        {
            _workflowStateService.AddStep(step);
        }
    }
}
```

## 📊 执行状态可视化

```csharp
// 创建可视化器
var visualizer = new WorkflowExecutionVisualizer(designerPanel.NodeEditor);

// 开始执行
visualizer.StartExecution();

// 更新节点状态
visualizer.SetNodeExecuting(nodeId);      // 执行中
visualizer.SetNodeCompleted(nodeId);       // 完成
visualizer.SetNodeCompleted(nodeId, false); // 失败
visualizer.SetNodeSkipped(nodeId);         // 跳过

// 停止执行
visualizer.StopExecution();
```

### 与 StepExecutionManager 集成

```csharp
var tracker = new WorkflowExecutionTracker(visualizer);

// 构建映射
tracker.BuildStepMapping(steps, nodes);

// 在步骤执行回调中
stepExecutionManager.StepStarted += (stepNum) => tracker.OnStepStarted(stepNum);
stepExecutionManager.StepCompleted += (stepNum, success) => tracker.OnStepCompleted(stepNum, success);
```

## 🎯 自定义节点

```csharp
[STNode("自定义/我的节点", "工作流设计器", "", "", "自定义节点说明")]
public class MyCustomNode : WorkflowNodeBase
{
    public override string StepName => "MyCustomStep";
    public override string DisplayName => "我的节点";
    public override string CategoryPath => "自定义";
    public override string Description => "这是一个自定义节点";

    // 自定义参数
    public MyCustomParameter Parameter
    {
        get => StepParameter as MyCustomParameter;
        set => StepParameter = value;
    }

    public override string ConfigSummary
    {
        get
        {
            if (Parameter == null) return "未配置";
            return $"配置: {Parameter.SomeValue}";
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        this.Title = "🔧 我的节点";
        this.Size = new Size(180, 70);
    }

    protected override Color GetTitleColor()
    {
        return Color.FromArgb(200, 100, 149, 237);
    }

    protected override void InitializeParameter()
    {
        Parameter = new MyCustomParameter();
    }

    protected override void LoadParameterFromJson(string json)
    {
        Parameter = JsonConvert.DeserializeObject<MyCustomParameter>(json);
    }

    public override Type GetParameterType()
    {
        return typeof(MyCustomParameter);
    }
}

// 注册自定义节点
WorkflowNodeFactory.RegisterNode<MyCustomNode>();
```

## ⌨️ 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+N` | 新建工作流 |
| `Ctrl+O` | 打开工作流 |
| `Ctrl+S` | 保存工作流 |
| `Ctrl+A` | 全选节点 |
| `Delete` | 删除选中节点 |
| `F5` | 验证工作流 |
| `鼠标滚轮` | 缩放画布 |
| `中键拖动` | 移动画布 |
| `双击节点` | 打开配置 |

## 📁 文件格式

### .stn 格式
STNodeEditor 原生格式，包含完整的节点图信息：
- 节点位置和大小
- 连接关系
- 节点参数

### .json 格式
ChildModel 列表格式，与现有系统兼容：
```json
[
  {
    "StepNum": 1,
    "StepName": "DelayWait",
    "StepParameter": { "T": 1000 },
    "Remark": "等待1秒",
    "Status": 0
  }
]
```

## 🔍 验证功能

设计器内置验证功能：
- ✅ 检查开始节点存在
- ✅ 检查结束节点存在  
- ⚠️ 检测孤立节点
- ⚠️ 检测未配置节点
- ❌ 检测循环依赖

## 📝 外观定制

### 节点颜色
```csharp
protected override Color GetTitleColor()
{
    return Color.FromArgb(200, 74, 144, 226);
}
```

### 编辑器样式
```csharp
_nodeEditor.BackColor = Color.FromArgb(30, 30, 32);
_nodeEditor.GridColor = Color.FromArgb(50, 50, 52);
_nodeEditor.Curvature = 0.3f; // 连线曲率
```

### 数据类型颜色
```csharp
_nodeEditor.SetTypeColor(typeof(int), Color.DodgerBlue);
_nodeEditor.SetTypeColor(typeof(string), Color.Yellow);
_nodeEditor.SetTypeColor(typeof(ExecutionFlow), Color.White);
```

## 📋 API 参考

### WorkflowDesignerPanel

| 方法 | 说明 |
|------|------|
| `NewWorkflow()` | 创建新工作流 |
| `LoadWorkflow(path)` | 从文件加载 |
| `SaveWorkflow(path)` | 保存到文件 |
| `LoadFromChildModels(list)` | 从步骤列表加载 |
| `ExportToChildModels()` | 导出为步骤列表 |
| `ValidateWorkflow()` | 验证工作流 |
| `AddNode(stepName, x, y)` | 添加节点 |

### WorkflowGraphConverter

| 方法 | 说明 |
|------|------|
| `ConvertToChildModels()` | 转换为步骤列表 |
| `LoadFromChildModels(list)` | 从步骤列表加载 |
| `AutoLayoutNodes()` | 自动布局 |
| `ValidateGraph()` | 验证图结构 |

### WorkflowNodeFactory

| 方法 | 说明 |
|------|------|
| `CreateNode(stepName)` | 根据名称创建节点 |
| `CreateNodeFromChildModel(model)` | 从模型创建节点 |
| `GetNodesByCategory()` | 按分类获取节点 |
| `RegisterNode<T>()` | 注册自定义节点 |

## 🐛 故障排除

### 节点不显示在树视图
确保节点类有 `[STNode]` 特性，并且已被扫描到。

### 连接无法创建
检查数据类型是否匹配。执行流使用 `ExecutionFlowType`。

### 参数无法保存
确保参数类可序列化（有无参构造函数，属性可读写）。

### 配置窗体不弹出
检查 `NodeConfigAdapter` 是否注册了对应的窗体工厂。

## 📄 许可证

本项目使用 MIT 许可证。STNodeEditor 库遵循其原始许可协议。

---

**作者**: Claude AI Assistant  
**日期**: 2025年12月
