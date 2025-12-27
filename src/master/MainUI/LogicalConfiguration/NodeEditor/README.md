# NodeEditor 可视化工作流编辑器

## 📁 文件结构

```
NodeEditor/
├── Core/                          # 核心基础类
│   ├── NodeSocket.cs              # 节点端口类
│   ├── NodeConnection.cs          # 节点连接类
│   ├── WorkflowNodeBase.cs        # 节点基类
│   └── ParameterModels.cs         # 参数模型定义
│
├── Nodes/                         # 具体节点实现
│   ├── ControlNodes.cs            # 控制节点（开始/结束）
│   ├── LogicNodes.cs              # 逻辑节点（条件/循环/延时等）
│   ├── PLCNodes.cs                # PLC通信节点
│   └── DataNodes.cs               # 数据操作节点
│
├── Controls/                      # UI控件
│   ├── NodeEditorControl.cs       # 节点编辑器主控件
│   ├── NodeToolboxControl.cs      # 工具箱控件
│   └── NodePropertyPanel.cs       # 属性面板控件
│
├── Services/                      # 服务类
│   ├── NodeFactory.cs             # 节点工厂
│   ├── WorkflowDocument.cs        # 工作流文档模型
│   ├── NodeExecutionAdapter.cs    # 执行适配器
│   └── ServiceInterfaces.cs       # 服务接口定义
│
├── Converters/                    # 转换器
│   └── WorkflowConverter.cs       # Node ↔ ChildModel 转换
│
└── Forms/                         # 窗体
    └── frmNodeWorkflowDesigner.cs # 主设计器窗体
```

## 🚀 快速开始

### 1. 添加引用

将 `NodeEditor` 文件夹复制到项目的 `LogicalConfiguration` 目录下。

### 2. 添加命名空间引用

```csharp
using MainUI.LogicalConfiguration.NodeEditor.Core;
using MainUI.LogicalConfiguration.NodeEditor.Controls;
using MainUI.LogicalConfiguration.NodeEditor.Services;
using MainUI.LogicalConfiguration.NodeEditor.Forms;
```

### 3. 打开设计器窗体

```csharp
// 在主界面中添加按钮打开节点编辑器
private void btnOpenNodeEditor_Click(object sender, EventArgs e)
{
    var workflowState = new WorkflowStateService(
        modelTypeName: "产品类型",
        modelName: "产品型号",
        itemName: "测试项目"
    );
    
    using var designer = new frmNodeWorkflowDesigner(workflowState);
    designer.ShowDialog();
}
```

## 📝 使用说明

### 节点操作

| 操作 | 方法 |
|------|------|
| 添加节点 | 从左侧工具箱拖拽到画布 |
| 删除节点 | 选中后按 Delete 键 |
| 移动节点 | 拖拽节点标题栏 |
| 多选节点 | Ctrl + 点击 或 框选 |
| 连接节点 | 从输出端口拖拽到输入端口 |
| 编辑参数 | 双击节点 或 在右侧属性面板编辑 |

### 视图操作

| 操作 | 方法 |
|------|------|
| 平移视图 | 中键/右键拖拽 |
| 缩放视图 | 鼠标滚轮 |
| 适应视图 | 点击工具栏"适应"按钮 |
| 重置视图 | 点击工具栏"重置"按钮 |

### 快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+S | 保存工作流 |
| Ctrl+Z | 撤销 |
| Ctrl+Y | 重做 |
| Ctrl+A | 全选 |
| Ctrl+C | 复制 |
| Ctrl+V | 粘贴 |
| Delete | 删除选中 |
| Escape | 取消选择 |

## 🎨 节点类型

### 控制节点
- **开始节点** - 工作流入口点（绿色）
- **结束节点** - 工作流终点（红色）

### 逻辑节点
- **延时等待** - 等待指定时间（灰色）
- **条件判断** - 条件分支，True/False两个出口（橙色）
- **循环** - 循环执行子步骤（紫色）
- **等待稳定** - 等待数值稳定（深蓝灰）
- **跳出循环** - Break（深红色）
- **继续循环** - Continue（深绿色）

### 通信节点
- **读取PLC** - 从PLC读取数据（蓝色）
- **写入PLC** - 向PLC写入数据（紫色）

### 数据节点
- **变量赋值** - 设置变量值（青色）
- **变量定义** - 定义新变量（深青色）
- **消息通知** - 显示消息（黄色）

## 🔧 扩展开发

### 添加自定义节点

1. 继承 `WorkflowNodeBase` 基类：

```csharp
[Serializable]
public class MyCustomNode : WorkflowNodeBase
{
    public override string NodeType => "MyCustom";
    public override Color NodeColor => Color.FromArgb(100, 150, 200);
    public override string IconName => "icon_custom.png";

    public MyCustomNode()
    {
        DisplayName = "我的自定义节点";
        InputSockets.Add(new NodeSocket("In", SocketType.Input));
        OutputSockets.Add(new NodeSocket("Out", SocketType.Output));
    }

    public override string GetParameterPreview()
    {
        return "自定义预览文本";
    }

    public override WorkflowNodeBase Clone()
    {
        var clone = new MyCustomNode();
        CopyBasicProperties(clone);
        return clone;
    }
}
```

2. 注册到节点工厂：

```csharp
NodeFactory.RegisterNodeType("MyCustom", () => new MyCustomNode());
```

### 自定义参数配置窗体

实现 `IFormService` 接口：

```csharp
public class MyFormService : IFormService
{
    public Form CreateParameterForm(string nodeType, IWorkflowStateService workflowState)
    {
        return nodeType switch
        {
            "ReadPLC" => new Form_ReadPLC(workflowState),
            "WritePLC" => new Form_WritePLC(workflowState),
            "ConditionJudge" => new Form_Detection(workflowState),
            _ => null
        };
    }
}
```

## ⚠️ 注意事项

1. **参数类型兼容** - 确保 `ParameterModels.cs` 中的参数类与现有系统一致
2. **JSON序列化** - 需要安装 `Newtonsoft.Json` NuGet包
3. **日志支持** - 可选依赖 `Microsoft.Extensions.Logging`

## 📦 依赖项

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

## 🔄 与现有系统集成

本节点编辑器设计为与现有 `LogicalConfiguration` 系统兼容：

- `ChildModel` 保持不变
- `Parameter_*` 参数类保持不变  
- 通过 `WorkflowConverter` 实现双向转换
- 可与现有表格编辑器并存

---

如有问题，请参考代码注释或联系开发团队。
