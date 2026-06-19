
using System.Collections.Generic;
using StarProjectDef;

namespace StarProject.Service.SystemOpen
{

    /// 系统开放 关于 自定义条件的 代码部分
    [XLua.Hotfix]
    public partial class SystemOpenManager
    {
        private Dictionary<SystemOpenType, SystemCustomCondition> systemConditions = new();

        /// <summary>
        /// 初始化 对应的 系统开放类型 需要自定义条件检查的 接口
        /// 需要的 都在 里面加
        /// </summary>
        public void InitCustomConditions()
        {
            // 创建 一个 测试的 自定义条件检查 类型
            CreateCustomCondition<TestSystemCustomCondition>();
            // 转职
            CreateCustomCondition<TransJobSystemCustomCondition>();

            // 后续 自定义条件类型 可以按上面的 格式 依次添加

        }

        /// <summary>
        /// 创建 对应 系统 自定义类型的接口
        /// </summary>
        private void CreateCustomCondition<T>() where T : SystemCustomCondition, new()
        {
            T t = new();
            RegisterCustomCondition(t);

            t.ActionOnTriggerCheckSystemCusCondition += TriggerCheckSystemCusCondition;
        }

        private void RegisterCustomCondition(SystemCustomCondition systemCustomCondition)
        {
            if (systemConditions.ContainsKey(systemCustomCondition.systemOpenType))
            {
                systemConditions[systemCustomCondition.systemOpenType] = systemCustomCondition;
            }
            else
            {
                systemConditions.Add(systemCustomCondition.systemOpenType, systemCustomCondition);
            }
        }

        private void TriggerCheckSystemCusCondition(SystemOpenType systemOpenType)
        {
            // 刷新一次系统开放
            RefreshSystemOpen();
        }

        /// <summary>
        /// 检查系统 自己定义的条件检查
        /// </summary>
        /// <param name="systemItemCfg"></param>
        private bool CheckSystemItemCustomCondition(SystemItemData systemItemCfg)
        {
            if (!systemConditions.ContainsKey(systemItemCfg.systemOpenType))
            {
                return true;
            }

            return systemConditions[systemItemCfg.systemOpenType].CheckCondion();
        }


    }
}
