/*
 * ============================================================================
 *                    STNodeEditor 工作流设计器 - 项目集成指南
 * ============================================================================
 * 
 * 本文件提供了将 STNodeEditor 工作流设计器集成到现有项目的完整示例和说明
 * 
 * 【目录结构】
 * 
 * MainUI.LogicalConfiguration.NodeEditor/
 * ├── Core/
 * │   ├── WorkflowNodeFactory.cs      - 节点工厂（创建和管理节点）
 * │   └── WorkflowGraphConverter.cs   - 图转换器（节点图 ↔ ChildModel列表）
 * │
 * ├── Nodes/
 * │   ├── WorkflowNodeBase.cs         - 节点基类
 * │   ├── SpecialNodes.cs             - 特殊节点（开始/结束/注释）
 * │   ├── ControlFlowNodes.cs         - 控制流节点（条件/循环/等待稳定）
 * │   └── OperationNodes.cs           - 操作节点（延时/PLC读写/变量赋值）
 * │
 * ├── Controls/
 * │   └── WorkflowDesignerPanel.cs    - 设计器面板控件
 * │
 * └── Forms/
 *     └── WorkflowDesignerForm.cs     - 设计器窗体
 * 
 * ============================================================================
 */

using MainUI.LogicalConfiguration.Forms;
using MainUI.LogicalConfiguration.NodeEditor.Controls;
using MainUI.LogicalConfiguration.NodeEditor.Forms;
using MainUI.LogicalConfiguration.Parameter;
using MainUI.Procedure.DSL.LogicalConfiguration.Forms;

namespace MainUI.LogicalConfiguration.NodeEditor
{
    /// <summary>
    /// 项目集成示例类
    /// </summary>
    public static class IntegrationExample
    {
        #region 示例1: 打开独立的设计器窗口

        /// <summary>
        /// 示例1: 打开独立的设计器窗口
        /// 适用于需要全屏编辑工作流的场景
        /// </summary>
        public static void OpenDesignerWindow(List<ChildModel> existingSteps = null)
        {
            // 创建设计器窗体
            var designerForm = new WorkflowDesignerForm(existingSteps, (models) =>
            {
                // 保存回调 - 当用户点击确定时执行
                Debug.WriteLine($"保存了 {models.Count} 个步骤");

                // 在这里更新你的数据源
                // 例如: _workflowStateService.ClearSteps();
                //       foreach (var model in models) _workflowStateService.AddStep(model);
            });

            // 显示窗体
            designerForm.Show();
        }

        #endregion

        #region 示例2: 以对话框方式获取结果

        /// <summary>
        /// 示例2: 以对话框方式打开，并获取编辑结果
        /// 适用于需要用户确认后才保存的场景
        /// </summary>
        public static List<ChildModel> OpenDesignerDialog(List<ChildModel> existingSteps = null)
        {
            using (var designerForm = new WorkflowDesignerForm(existingSteps))
            {
                var result = designerForm.ShowDialogAndGetResult();

                if (result != null)
                {
                    Debug.WriteLine($"用户确认，返回 {result.Count} 个步骤");
                    return result;
                }

                Debug.WriteLine("用户取消");
                return null;
            }
        }

        #endregion

        #region 示例3: 嵌入到现有窗体

        /// <summary>
        /// 示例3: 将设计器面板嵌入到现有窗体
        /// 适用于需要与其他控件共存的场景
        /// </summary>
        public static void EmbedDesignerInForm(Form parentForm, Panel containerPanel)
        {
            // 创建设计器面板
            var designerPanel = new WorkflowDesignerPanel
            {
                Dock = DockStyle.Fill
            };

            // 绑定事件
            designerPanel.NodeSelected += (s, e) =>
            {
                if (e.Node != null)
                {
                    Debug.WriteLine($"选中节点: {e.Node.DisplayName}");
                }
            };

            designerPanel.NodeDoubleClick += (s, e) =>
            {
                // 自定义双击处理
                if (e.Node != null)
                {
                    Debug.WriteLine($"双击节点: {e.Node.DisplayName}");

                    // 可以在这里打开自定义的配置窗体
                    // OpenCustomConfigForm(e.Node);

                    // 设置为已处理，阻止默认的配置对话框
                    // e.Handled = true;
                }
            };

            designerPanel.WorkflowChanged += (s, e) =>
            {
                Debug.WriteLine("工作流已更改");
            };

            // 添加到容器
            containerPanel.Controls.Clear();
            containerPanel.Controls.Add(designerPanel);

            // 初始化新工作流
            designerPanel.NewWorkflow();
        }

        #endregion

        #region 示例4: 与现有 WorkflowStateService 集成

        /// <summary>
        /// 示例4: 与现有的 WorkflowStateService 集成
        /// </summary>
        public static void IntegrateWithWorkflowState(
            WorkflowDesignerPanel designerPanel,
            Services.IWorkflowStateService workflowState)
        {
            // 从 WorkflowStateService 加载步骤
            var steps = workflowState.GetSteps();
            if (steps != null && steps.Count > 0)
            {
                designerPanel.LoadFromChildModels(steps);
            }

            // 监听工作流变化，同步到 WorkflowStateService
            designerPanel.WorkflowChanged += (s, e) =>
            {
                // 获取当前的步骤列表
                var currentSteps = designerPanel.ExportToChildModels();

                // 同步到 WorkflowStateService
                // 注意: 这里需要根据你的实际实现来调整
                // workflowState.ClearSteps();
                // foreach (var step in currentSteps)
                // {
                //     workflowState.AddStep(step);
                // }
            };
        }

        #endregion

        #region 示例5: 自定义节点配置

        /// <summary>
        /// 示例5: 为节点配置自定义的配置窗体
        /// </summary>
        public static void SetupCustomNodeConfigs(WorkflowDesignerPanel designerPanel)
        {
            designerPanel.NodeDoubleClick += (s, e) =>
            {
                if (e.Node == null) return;

                Form configForm = null;

                // 根据节点类型打开不同的配置窗体
                switch (e.Node.StepName)
                {
                    case "DelayWait":
                        // 使用现有的延时配置窗体
                        configForm = new Form_DelayTime
                        {
                            Parameter = (Parameter_DelayTime)e.Node.StepParameter
                        };
                        break;

                    case "ConditionJudge":
                        // 使用现有的条件配置窗体
                        // configForm = new Form_Detection();
                        // configForm.Parameter = (Parameter_Detection)e.Node.StepParameter;
                        break;

                    case "PLCRead":
                        // 使用现有的PLC读取配置窗体
                        // configForm = new Form_ReadPLC();
                        break;

                    case "PLCWrite":
                        // 使用现有的PLC写入配置窗体
                        // configForm = new Form_WritePLC();
                        break;

                        // ... 其他节点类型
                }

                if (configForm != null)
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        // 更新节点状态
                        e.Node.IsConfigured = true;
                        e.Node.RefreshDisplay();
                    }

                    e.Handled = true; // 阻止默认处理
                }
            };
        }

        #endregion

        #region 示例6: 注册自定义节点类型

        /// <summary>
        /// 示例6: 注册自定义节点类型
        /// </summary>
        public static void RegisterCustomNodes()
        {
            // 注册自定义节点类型
            // WorkflowNodeFactory.RegisterNode<MyCustomNode>();

            // 或者手动添加到映射
            // 在 WorkflowNodeFactory 中的 RegisterBuiltInNodes 方法里添加
        }

        #endregion
    }

    #region 在 frmLogicalConfiguration 中集成的示例代码

    /*
     * ========================================================================
     * 在现有的 frmLogicalConfiguration.cs 中集成设计器的示例代码
     * ========================================================================
     * 
     * 1. 添加引用:
     *    using MainUI.LogicalConfiguration.NodeEditor.Forms;
     *    using MainUI.LogicalConfiguration.NodeEditor.Controls;
     * 
     * 2. 添加成员变量:
     *    private WorkflowDesignerPanel _nodeDesigner;
     * 
     * 3. 添加切换按钮:
     *    private void BtnSwitchToNodeEditor_Click(object sender, EventArgs e)
     *    {
     *        // 获取当前步骤列表
     *        var steps = _workflowStateService.GetSteps();
     *        
     *        // 打开设计器窗体
     *        var designerForm = new WorkflowDesignerForm(steps, (result) =>
     *        {
     *            // 保存结果
     *            _workflowStateService.ClearSteps();
     *            foreach (var step in result)
     *            {
     *                _workflowStateService.AddStep(step);
     *            }
     *            
     *            // 刷新表格显示
     *            RefreshStepTable();
     *        });
     *        
     *        designerForm.Show();
     *    }
     * 
     * 4. 或者嵌入到现有面板:
     *    private void InitializeNodeDesigner()
     *    {
     *        _nodeDesigner = new WorkflowDesignerPanel
     *        {
     *            Dock = DockStyle.Fill
     *        };
     *        
     *        // 添加到某个面板
     *        panelDesigner.Controls.Add(_nodeDesigner);
     *        
     *        // 加载现有步骤
     *        var steps = _workflowStateService.GetSteps();
     *        _nodeDesigner.LoadFromChildModels(steps);
     *        
     *        // 监听变化
     *        _nodeDesigner.WorkflowChanged += (s, e) =>
     *        {
     *            // 同步到 WorkflowStateService
     *            SyncToWorkflowState();
     *        };
     *    }
     *    
     *    private void SyncToWorkflowState()
     *    {
     *        var steps = _nodeDesigner.ExportToChildModels();
     *        _workflowStateService.ClearSteps();
     *        foreach (var step in steps)
     *        {
     *            _workflowStateService.AddStep(step);
     *        }
     *    }
     * 
     * ========================================================================
     */

    #endregion

    #region 安装说明

    /*
     * ========================================================================
     *                           安装和配置说明
     * ========================================================================
     * 
     * 【步骤1】安装 NuGet 包
     * 
     * 在 Package Manager Console 中执行:
     * Install-Package ST.Library.UI
     * 
     * 或者在 .csproj 中添加:
     * <PackageReference Include="ST.Library.UI" Version="2.0.0" />
     * 
     * 
     * 【步骤2】添加代码文件
     * 
     * 将以下文件添加到你的项目:
     * - Nodes/WorkflowNodeBase.cs
     * - Nodes/SpecialNodes.cs
     * - Nodes/ControlFlowNodes.cs
     * - Nodes/OperationNodes.cs
     * - Core/WorkflowNodeFactory.cs
     * - Core/WorkflowGraphConverter.cs
     * - Controls/WorkflowDesignerPanel.cs
     * - Forms/WorkflowDesignerForm.cs
     * 
     * 
     * 【步骤3】调整命名空间
     * 
     * 根据你的项目结构，调整文件中的命名空间
     * 
     * 
     * 【步骤4】处理参数类冲突
     * 
     * 如果你的项目已经有 Parameter_XXX 类:
     * 1. 删除 OperationNodes.cs 底部的参数类定义
     * 2. 添加正确的 using 引用指向现有的参数类
     * 
     * 例如:
     * using MainUI.LogicalConfiguration.Parameter;
     * 
     * 
     * 【步骤5】注册现有的窗体
     * 
     * 在 WorkflowDesignerPanel 的 NodeDoubleClick 事件中
     * 注册你现有的配置窗体（如 Form_DelayTime, Form_Detection 等）
     * 
     * ========================================================================
     */

    #endregion
}
